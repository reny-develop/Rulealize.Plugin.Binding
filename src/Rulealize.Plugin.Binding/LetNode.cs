// Copyright (c) 2026 Reny
// Licensed under the Apache License, Version 2.0.

using System.Collections.Immutable;
using System.Text.Json;
using Rulealize.Abstraction.Building;
using Rulealize.Abstraction.Evaluation;
using Rulealize.Abstraction.Node;
using Rulealize.Abstraction.Value;

namespace Rulealize.Plugin.Binding
{
    /// <summary>Binds names to values over a body.</summary>
    /// <remarks>
    /// <para>
    /// Binding is sequential, not simultaneous: each value expression is built and
    /// evaluated with the earlier names already in scope. Othello's flip detection depends
    /// on it, binding a ray and then immediately walking that ray:
    /// </para>
    /// <code>
    /// "bind": {
    ///   "ray": { "op": "grid.ray", … },
    ///   "run": { "op": "seq.takeWhile", "source": "@ray", … }
    /// }
    /// </code>
    /// <para>
    /// Under simultaneous binding that has to be written as two nested <c>bind.let</c>
    /// nodes, and rule sets fill up with nesting that says nothing.
    /// </para>
    /// <para>
    /// Order comes from the document. JSON says object keys are unordered, but the parser
    /// preserves them and this node treats their order in the text as the declaration
    /// order.
    /// </para>
    /// <para>
    /// Each value is evaluated once, however many times the name is read. That is the other
    /// half of what naming buys.
    /// </para>
    /// </remarks>
    internal sealed class LetNode(
        ImmutableArray<ExpressionNode> values,
        ImmutableArray<LocalSlot> slots,
        ExpressionNode body) : ExpressionNode
    {
        public static ExpressionNode Build(INodeBuildContext context)
        {
            JsonElement element = context.GetRequiredProperty("bind");
            if (element.ValueKind != JsonValueKind.Object)
            {
                throw context.Error("bind", "must be an object mapping names to expressions.");
            }

            ImmutableArray<ExpressionNode>.Builder values = ImmutableArray.CreateBuilder<ExpressionNode>();
            ImmutableArray<LocalSlot>.Builder slots = ImmutableArray.CreateBuilder<LocalSlot>();
            ExpressionNode body;

            using (context.Scope.BeginScope())
            {
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    // Built before the name is declared, so a binding can see the ones
                    // before it but not itself. That rules out recursive bindings.
                    values.Add(context.BuildExpression(property.Value, $"bind/{property.Name}"));
                    slots.Add(context.Scope.Declare(property.Name));
                }

                if (values.Count == 0)
                {
                    throw context.Error("bind", "must not be empty; a let that binds nothing is just its body.");
                }

                body = context.RequireExpression("in");
            }

            return new LetNode(values.ToImmutable(), slots.ToImmutable(), body);
        }

        public override RuleValue Evaluate(IEvaluationContext context)
        {
            IEvaluationContext scope = context;
            for (int i = 0; i < values.Length; i++)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                scope = scope.Bind(slots[i], values[i].Evaluate(scope));
            }

            return body.Evaluate(scope);
        }
    }
}

// Copyright (c) 2026 Reny
// Licensed under the Apache License, Version 2.0.

using Rulealize.Abstraction.Building;
using Rulealize.Abstraction.Evaluation;
using Rulealize.Abstraction.Nodes;
using Rulealize.Abstraction.Plugins;
using Rulealize.Abstraction.Values;

namespace Rulealize.Plugin.Binding
{
    /// <summary>Reads a local binding.</summary>
    /// <remarks>
    /// <para>
    /// The name is resolved while the rule set is built, not looked up while it runs. Local
    /// scope is decided entirely by the shape of the document, so nothing is gained by
    /// deferring it and two things are lost: a reference to a name nothing declared would
    /// become a surprise on the forty-first candidate of a <c>GetValidInputs</c> call
    /// rather than a build error, and every read would cost a dictionary probe instead of
    /// an indexed access.
    /// </para>
    /// <para>
    /// The innermost declaration wins, so an inner binding shadows an outer one of the same
    /// name.
    /// </para>
    /// </remarks>
    internal sealed class LocalNode(LocalSlot slot) : ExpressionNode
    {
        public static ExpressionNode Build(INodeBuildContext context) =>
            Resolve(context, context.RequireString("name"), "name");

        /// <summary>Resolves a local name against the scope in force here.</summary>
        /// <param name="context">The surrounding build state.</param>
        /// <param name="name">The name, without any shorthand prefix.</param>
        /// <param name="propertyName">The property to blame, or null to blame the node.</param>
        /// <returns>The node.</returns>
        public static ExpressionNode Resolve(IBuildContext context, string name, string? propertyName)
        {
            if (!context.Scope.TryResolve(name, out LocalSlot slot))
            {
                string message = $"'{name}' is not bound here. Locals come from bind.let, from the 'as' of a "
                    + "sequence operation, or from a definition's parameters.";

                throw propertyName is null
                    ? context.Error(message)
                    : context.Error(propertyName, message);
            }

            return new LocalNode(slot);
        }

        public override RuleValue Evaluate(IEvaluationContext context) => context.GetLocal(slot);
    }

    /// <summary>Expands <c>"@name"</c> into the node <c>bind.local</c> would have built.</summary>
    internal sealed class LocalSugarExpander : ISugarExpander
    {
        public ExpressionNode Expand(IBuildContext context, string text)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(text);

            string name = text[1..];
            if (name.Length == 0)
            {
                throw context.Error("'@' on its own does not name a local.");
            }

            return LocalNode.Resolve(context, name, propertyName: null);
        }
    }
}

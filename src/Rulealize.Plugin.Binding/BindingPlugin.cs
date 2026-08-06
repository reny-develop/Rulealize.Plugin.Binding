// Copyright (c) 2026 Reny
// Licensed under the Apache License, Version 2.0.

using Rulealize.Abstraction.Plugins;

namespace Rulealize.Plugin.Binding
{
    /// <summary>
    /// Scoped local bindings over the <c>bind</c> namespace, and the <c>@</c> shorthand.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Naming a value keeps a rule set from repeating an expression, and it also states
    /// something the evaluator can use: two references to one name are known from the
    /// syntax to be the same value, where two copies of an expression are not.
    /// </para>
    /// <para>
    /// This plugin owns the vocabulary for <em>reading</em> a local. Introducing one is
    /// spread across whoever needs to: <c>bind.let</c> here, the <c>as</c> of the sequence
    /// operations there, a definition's parameters elsewhere. The scope machinery itself
    /// belongs to the runtime, which is how plugins that have never heard of each other
    /// end up sharing a name.
    /// </para>
    /// </remarks>
    public sealed class BindingPlugin : IRulealizePlugin
    {
        /// <inheritdoc />
        public PluginManifest Manifest { get; } =
            new("Rulealize.Plugin.Binding", new Version(1, 0, 0), "bind", '@');

        /// <inheritdoc />
        public void Register(IPluginRegistry registry)
        {
            ArgumentNullException.ThrowIfNull(registry);

            registry.AddExpression("let", LetNode.Build);
            registry.AddExpression("local", LocalNode.Build);
            registry.AddSugar(new LocalSugarExpander());
        }
    }
}

# Rulealize.Plugin.Binding

Scoped local bindings for [Rulealize](https://github.com/reny-develop/Rulealize) rule sets.

| | |
| --- | --- |
| Plugin id | `Rulealize.Plugin.Binding` |
| Namespace | `bind` |
| Reserved prefix | `@` |
| Depends on | `Rulealize.Abstraction` |

Naming a value keeps a rule set from repeating an expression. It also states something the
evaluator can act on: two references to one name are known from the syntax to be the same
value, where two copies of an expression are not.

Independent of `Rulealize.Plugin.Branch`. Bindings without branches make sense, and so
does the reverse.

## Operations

```jsonc
{ "op": "bind.let", "bind": { "<name>": <expr>, … }, "in": <expr> }

{ "op": "bind.local", "name": "<name>" }     // shorthand: "@<name>"
```

## Binding is sequential

Each value expression is built and evaluated with the earlier names already in scope, so a
later binding can see an earlier one but not the reverse. Othello's flip detection depends
on it:

```jsonc
"bind": {
  "ray": { "op": "grid.ray", "grid": "$board", "from": "@at", "dir": "@dir" },
  "run": { "op": "seq.takeWhile", "source": "@ray", … }
}
```

Under simultaneous binding that has to become two nested `bind.let` nodes, and rule sets
fill up with nesting that carries no meaning.

Order comes from the document. JSON says object keys are unordered; the parser preserves
them anyway, and this plugin treats their order in the text as the declaration order.

A binding cannot see itself — its value is built before its name is declared — so there is
no recursive `let`.

Each value is evaluated once, however many times the name is read. Whether a binding that
is never read gets evaluated at all is left open; every node is pure, so the only
difference is cost.

## Who introduces a binding, and who reads one

This plugin owns reading. Introducing is spread across whoever needs it:

| Introduced by | Key | Visible in |
| --- | --- | --- |
| `bind.let` | each key of `bind` | the later bindings and `in` |
| the sequence operations | `as` | that node's predicate or projection |
| `def.call` | the callee's `params` | the definition's body only |

`seq.takeWhile` introduces a name that `bind.local` reads, and neither plugin references
the other. What connects them is the scope machinery, which belongs to the runtime — both
reach it through `Rulealize.Abstraction`.

Definition bodies are hygienic: they cannot see the caller's locals at all. Arguments are
the only way in.

## Unbound names are build errors

Local scope follows the shape of the document, so a name that nothing declared is caught
when the rule set is built, not when it runs. This matters more than it sounds:
`GetValidInputs` evaluates a guard against every candidate in a domain, and a fault that
first appears on the forty-first candidate is a fault that reaches production.

Resolving early also means reading a local is an indexed access rather than a dictionary
probe.

Shadowing is allowed — an inner binding hides an outer one of the same name — but
declaring one name twice in the same scope is not.

## Building

`Rulealize.Abstraction` is not on nuget.org yet, so `NuGet.config` points at a folder
feed. Produce it from the abstraction repository first:

```
dotnet pack path\to\Rulealize.Abstraction\src\Rulealize.Abstraction -c Release -o path\to\LocalNuGet
```

with `LocalNuGet` a sibling of this repository. Then `dotnet build`.

## License

Apache-2.0.

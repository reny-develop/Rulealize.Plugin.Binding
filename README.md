# Rulealize.Plugin.Binding

Scoped local bindings for [Rulealize](https://github.com/reny-develop/Rulealize) rule sets.

| | |
| --- | --- |
| Plugin id | `Rulealize.Plugin.Binding` |
| Namespace | `bind` |
| Reserved prefix | `@` |
| Depends on | `Rulealize.Abstraction` |
| Specification | [doc/specification.md](doc/specification.md) |

`bind.let` introduces names; `bind.local` reads one, with `"@name"` as its shorthand.
Independent of `Rulealize.Plugin.Branch` — bindings without branches make sense, and so
does the reverse.

Naming a value keeps a rule set from repeating an expression. It also states something the
evaluator can act on: two references to one name are known from the syntax to be the same
value, where two copies of an expression are not.

The part worth knowing before reading the specification is that this plugin owns only
*reading* a binding. Introducing one is spread across whoever needs it — `bind.let`, the
sequence operations' `as`, a definition's parameters — so `seq.takeWhile` introduces a name
that `bind.local` reads while neither plugin references the other. What connects them is
scope machinery that belongs to `Rulealize.Abstraction`, which both already depend on.

## Building

`dotnet build`. `Rulealize.Abstraction` restores from nuget.org like any other package, so
this repository builds on its own.

## License

Apache-2.0.

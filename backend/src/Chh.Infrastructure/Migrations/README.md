# Chh.Infrastructure / Migrations

EF Core code-first migrations. Naming: `<Timestamp>_<PascalCaseDescription>`
(`.claude/rules/db-standards.md` §4). Every migration must implement `Down()`.

Generated via `.claude/skills/ef-migration-skill/SKILL.md`; the startup project
for `dotnet ef` is `src/Chh.Api`.

Always generate migrations with `dotnet ef migrations add` (or the skill above) —
never write a `Migration`-derived class by hand. `dotnet ef` also emits the paired
`.Designer.cs` (carrying the `[DbContext]`/`[Migration]` attributes that associate
the migration with `ChhDbContext`) and updates `ChhDbContextModelSnapshot.cs`. A
migration missing its `.Designer.cs` is invisible to `Database.MigrateAsync()` —
EF reports "already up to date" and silently never creates the table.

`dotnet ef`'s generated template puts `/// <inheritdoc />` on the migration class
and its `Up()`/`Down()` — there's nothing to inherit from (`Migration` doesn't
declare virtual members with doc comments), so replace those with real one-line
summaries after generating.

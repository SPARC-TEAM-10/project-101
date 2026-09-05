# Chh.Infrastructure / Migrations

EF Core code-first migrations. Naming: `<Timestamp>_<PascalCaseDescription>`
(`.claude/rules/db-standards.md` §4). Every migration must implement `Down()`.

Generated via `.claude/skills/ef-migration-skill/SKILL.md`; the startup project
for `dotnet ef` is `src/Chh.Api`.

Empty by design — no schema exists yet.

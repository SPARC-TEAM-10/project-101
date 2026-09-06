# Chh.Infrastructure / Persistence

`ChhDbContext` + `IEntityTypeConfiguration<T>` classes, plus the AES-256 EF Core
value converters required for health-screening flags and date of birth
(`.claude/rules/db-standards.md` §3).

Single `DbContext` for the whole service — no multi-tenancy, no second context.

Empty by design.

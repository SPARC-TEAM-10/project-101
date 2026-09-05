# Chh.Domain / Enums

Domain enums (e.g. role, blood group, request status). Persisted as
`varchar(50)` via `.HasConversion<string>()` per `.claude/rules/db-standards.md` §2b.

Empty by design.

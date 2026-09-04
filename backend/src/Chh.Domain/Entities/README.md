# Chh.Domain / Entities

EF Core domain entities (e.g. `IndividualProfile`, `Facility`, `BloodRequest`,
`OtpRequest`). Naming, key strategy (`Guid Id`), PostgreSQL type mapping, and
PII-at-rest rules are defined in `.claude/rules/db-standards.md`.

Empty by design — entities are added by the module ticket that needs them.

# Rules: SQL Database Design Guidelines (Community Health Hub — CHH)

> Status: IN PROGRESS — authoritative for all DB development in this project.
> This project is a single PostgreSQL database for one backend service — no
> multi-tenancy, no partitioning, no per-microservice repeats.

---

## 1. Naming Conventions

Follows DOTNET-RULES Part 2 §1 directly — one consistent convention, no
mixed casing:

| Element | Convention | Example |
|---------|-----------|---------|
| Table names | PascalCase, **singular** | `BloodRequest`, `Facility`, `OtpRequest` |
| Column names | PascalCase | `MobileNumber`, `CreatedAtUtc` |
| Primary key column | Always named `Id` | `Id` |
| Index names | `IX_{TableName}_{ColumnName}` | `IX_BloodRequest_RequesterId` |
| Foreign key constraints | `FK_{TableName}_{ReferencedTableName}_{ColumnName}` | `FK_EventRsvp_Event_EventId` |
| Abbreviations | Avoid unless industry-standard | `FacilityId` NOT `FacId` |

---

## 2. General Design Principles

### 2a — Surrogate Keys
- Use **`UUID`** as primary keys — named `Id`, C# `Guid`
- PostgreSQL default generation: `gen_random_uuid()` (built into PostgreSQL
  13+ via `pgcrypto`/`pgcrypto` is no longer required from PG13 on — confirm
  the target PostgreSQL 16 has it available by default; if not, enable
  `pgcrypto`). This is the PostgreSQL equivalent of DOTNET-RULES Part 2 §2's
  `NEWSEQUENTIALID()` — that function is SQL-Server-only, do not use it here.
- **Never** use natural keys (mobile number, email, license number) as PKs
- **Avoid compound primary keys** unless absolutely required (e.g. a pure join table like `EventRsvp(EventId, UserId)`)

### 2b — Column Data Types & Sizes (PostgreSQL)

This table supersedes DOTNET-RULES Part 2 §6, which is written for SQL
Server — apply the same design principles (explicit sizes, explicit
nullability, no `VARCHAR(MAX)`-equivalent without justification) with these
PostgreSQL types:

| C# Type | PostgreSQL Type | EF Core Configuration |
|---|---|---|
| `Guid` | `uuid` | Default |
| `string` (required) | `varchar(n)` | `.HasMaxLength(n).IsRequired()` |
| `string?` | `varchar(n)` NULL | `.HasMaxLength(n)` |
| `decimal` | `numeric(18,6)` | `.HasPrecision(18, 6)` |
| `DateTime` (UTC) / `DateTimeOffset` | `timestamptz` | Default |
| `DateOnly` | `date` | `.HasColumnType("date")` |
| `TimeOnly` | `time` | `.HasColumnType("time")` |
| `bool` | `boolean` | `.HasDefaultValue(false)` |
| `enum` | `varchar(50)` | `.HasConversion<string>().HasMaxLength(50)` |
| `byte[]` (binary) | `bytea` — avoid; store large binaries (facility license files) in blob storage and keep only a URL/reference here | — |

- Nullability always explicit — no implicit nullability
- Never use unbounded `text` for a column that has a known reasonable max length

### 2c — Connection String Naming

- The primary application database connection string is always named `DefaultConnection`
  in `ConnectionStrings` (`appsettings.json`) — the standard ASP.NET Core convention, not
  a service-specific name like `ChhDatabase`. Secondary connection strings (e.g.
  `HangfireDatabase`) keep a descriptive name.

---

## 3. PII and Health Data

> Every table holding user-submitted personal or health-screening content is
> subject to this section. Treat deviations as Critical review findings.

CHH's sensitive fields (per PRD §8 Data Dictionary):

- **Health screening flags** — `IsChronicIllness`, `HasRecentSurgery`,
  `IsInfectiousDisease`, `IsUnderweight` on `IndividualProfile`. Encrypt
  these at rest using AES-256 via an EF Core value converter (not
  `varchar` plaintext). The derived `ReceiverOnly` boolean (true if any
  restriction flag is set, per PRD §7 CHH-F02 AC2) may be stored unencrypted
  — it's an operational flag, not raw health detail.
- **Date of birth** — encrypt at rest (same value-converter approach).
- **Facility license documents** — store in blob storage, not `bytea`; the
  DB holds only the blob URL/reference. Access to the document is gated to
  System Admin role (PRD §4 Role & Permission Matrix — only Admin verifies
  facilities).
- **Mobile number** — the login identifier; not a PK (see §2a), but treat as
  PII for logging purposes (see `.claude/rules/api-standards.md` §8 — mask
  all but the last 2 digits in any log line).
- No employee-exit-style "every read of another user's record" audit table
  is required for CHH — there's no PRD requirement for it. Instead, audit
  the specific actions the PRD actually calls out: facility verification
  decisions (`FacilityVerification`: who approved/rejected, when, previous
  status) and blood-request creation (`BloodRequest.RequesterId`,
  `CreatedAtUtc` already cover this — no separate audit table needed).
- No mandatory `RetentionExpiresUtc` / retention-deletion job is required at
  this time — the PRD does not specify a data retention policy. Use standard
  soft delete (`IsDeleted`, per DOTNET-RULES Part 2 §4) for user-initiated
  deletions. Revisit if a compliance requirement emerges.

---

## 4. Schema Changes

> See DOTNET-RULES Part 2 §8 for migration practices (build-before-migrate,
> review requirements, `Down()` implementation) — followed as-is, no
> CHH-specific additions needed since this is a single service with a single
> `DbContext` and `Migrations` folder.

### Migration Naming
- Format: `<Timestamp>_<PascalCaseDescription>`
- Example: `20260902120000_AddOtpRequestTable`

---

## 5. Security Guidelines

| Concern | Rule |
|---------|------|
| Health-screening / DOB PII | Encrypt at rest via EF Core value converter (AES-256) — see §3 |
| Facility license documents | Store in blob storage, not inline `bytea`; gate access to System Admin |
| DB access | Connection string via Azure Key Vault; Azure AD Managed Identity if hosted on Azure App Service — never credentials committed to config files |
| SQL injection | EF Core parameterised queries by default — never raw SQL with string concatenation |
| Encryption at rest | Enable disk/volume-level encryption on the PostgreSQL instance (Azure Database for PostgreSQL: encryption at rest is on by default) |
| Retention | No mandatory job at this time (see §3) — standard soft delete only |
| Access logging | Audit facility verification decisions and blood-request lifecycle changes (see §3) — not a blanket per-read audit log |

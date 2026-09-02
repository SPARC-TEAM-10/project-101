# Rules: API Development Standards (Community Health Hub — CHH)

> Status: IN PROGRESS — authoritative for all API development in this project.
> RFC 7807 error format and pagination rules are carried over from the
> shipped `api-standards.md` template unchanged (they're good defaults).
> Contract-first rules and the resource/domain content below are specific
> to CHH. This project is a single monorepo, single backend service — no
> multi-tenancy, no shared-package cross-repo concerns.

---

## 1. API Design Standards

### RESTful Principles — HTTP Methods

| Method | Use |
|--------|-----|
| `GET` | Retrieve data |
| `POST` | Create a resource |
| `PUT` | Update a resource (full replace) |
| `PATCH` | Partial update |
| `DELETE` | Remove a resource |

### URI Design

| Rule | Good | Bad |
|------|------|-----|
| Use nouns, not verbs | `/blood-requests`, `/events/123` | `/getBloodRequests`, `/createEvent` |
| Use **plural nouns** for collections | `/facilities`, `/health-screenings` | `/facility`, `/health-screening` |
| Use hierarchical URIs for relationships | `/facilities/123/contacts` | `/getContactsByFacility?id=123` |
| Use **hyphens** for multi-word segments | `/blood-requests` | `/bloodRequests`, `/blood_requests` |
| No file extensions in URIs | `/events/123` | `/events/123.json` |
| URL versioning | `/api/v1/blood-requests` | `/blood-requests?version=1` |

> Exact resource names above are illustrative — the real set of resources
> is whatever exists in `contracts/chh-api.v1.yaml` (see §3). Don't add a
> resource here that isn't in the contract yet. Known CHH-F01 endpoints
> (already scoped in the Jira task breakdown, still to be added to the
> contract): `POST /api/v1/auth/otp/request`, `POST /api/v1/auth/otp/verify`.

### Statelessness
Every request must contain all information needed to process it. Session
state is the JWT issued on OTP verification (1-hour expiry per CHH-F01 AC3)
— no other stored client context between requests.

### Data Exchange Format
JSON only — all request and response bodies.

---

## 2. HTTP Status Codes

> See DOTNET-RULES Part 1 §8 for the full status code mapping. CHH-specific
> requirement: `POST` endpoints that create a resource must return `201 Created`
> with a `Location` header pointing to the new resource. The two OTP
> endpoints are the exception — they don't create a addressable resource, so
> `200 OK` with the response body is correct there.

---

## 3. Contract-First Rule (hard)

- No controller action may be added, renamed, or have its request/response
  shape changed unless the change exists first in `contracts/chh-api.v1.yaml`.
- DTOs in `Chh.Application/Dtos/` are generated from that file. Hand-editing
  a generated DTO is a **Critical** review finding.
- Breaking a published operation requires a new path version (`/v2/...`),
  never an in-place change.
- The frontend Coding Agent builds against this same contract file — it is
  the single source of truth both sides depend on. If a frontend ticket
  needs a field the backend doesn't expose yet, that's a contract change to
  flag, not something to work around client-side.

### DTO Naming (post-generation)
- `{Entity}Dto` for responses, `Create{Entity}Request` / `Update{Entity}Request` for inputs
- **Never expose database entities directly** in API responses — the generated
  DTO is the only shape that crosses the wire

---

## 4. API Documentation — Swagger / Swashbuckle

- Use latest stable `Swashbuckle.AspNetCore` NuGet package
- Register Swagger generator and UI middleware in `Program.cs`
- Use XML Comments (`///`) on all controllers, actions, and models
- Enable XML documentation generation in project settings
- **Restrict Swagger UI to Development environment only:**

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

---

## 5. Security Standards

### Authentication & Authorisation
- **OTP-first, not enterprise SSO.** Login is mobile number + 6-digit OTP
  (CHH-F01) — no OAuth2/OIDC identity provider. On successful OTP
  verification, issue a JWT (1-hour expiry, per CHH-F01 AC3) carrying the
  user's `RoleID` (Guest / Individual / Hospital Admin / NGO / System Admin
  — see PRD §4 Role & Permission Matrix).
- Use `[Authorize]` on all protected endpoints — see DOTNET-RULES Part 1 §7
  for JWT Bearer implementation. Use policy-based authorization for the
  role checks in PRD §4 (e.g. only Hospital Admin / System Admin may hit
  inventory-management endpoints).
- `POST /api/v1/auth/otp/request` and `POST /api/v1/auth/otp/verify` are the
  only endpoints that may carry `[AllowAnonymous]` — every other endpoint
  requires a valid JWT, down to Guest-tier read endpoints (Guest still
  authenticates via OTP per PRD §3, it just gets a limited-permission role).

### Secure Configuration
- **Never** store secrets in `appsettings.json` or environment variables
- Use **Azure Key Vault** for all secret storage — this includes the SMS
  gateway API key, Firebase Cloud Messaging server key, and the maps API key,
  not just JWT signing keys

### Input Validation
- Use **FluentValidation** — see DOTNET-RULES Part 1 §5 for implementation
- Enforce HTTPS for all communication
- **Encrypt health-screening PII at rest** (chronic illness, recent surgery,
  infectious disease, underweight flags — PRD §8 Data Dictionary) using
  AES-256 via an EF Core value converter — see db-standards.md §3 (PII and
  Health Data) for the exact columns and encryption approach.

---

## 6. Performance Optimisation

### Asynchronous Programming
> See DOTNET-RULES Part 1 §9 for async/await rules.

### Pagination & Filtering — kept as-is, unchanged
- **Never return large datasets** — always paginate
- Pagination query params: `page` and `pageSize`
- Example: `GET /api/v1/events?page=1&pageSize=50`
- Return `PagedResponse<T>` — see DOTNET-RULES Part 1 §12 for the standard wrapper type

### Database Call Optimisation
> Use `AsNoTracking()` for all read-only queries — see DOTNET-RULES Part 2 §9.

### Proximity / Notification Performance
- Per PRD §9 (NFR): proximity calculation and notification dispatch must
  complete in under 5 seconds. Notification fan-out to matched donors runs
  as a Hangfire background job, not inline in the request that creates the
  blood request or event — the creating endpoint returns as soon as the
  request/event is persisted, not after dispatch completes.

---

## 7. Error Handling — kept as-is, unchanged

- Use **`Hellang.Middleware.ProblemDetails`** NuGet package for RFC 7807-compliant error responses
- Map custom exceptions to appropriate HTTP status codes:

```csharp
builder.Services.AddProblemDetails(options =>
{
    options.IncludeExceptionDetails = (_, __) => env.IsDevelopment();
    options.Map<NotImplementedException>(_ => new StatusCodeProblemDetails(501));
    options.Map<ChhValidationException>(ex => new ValidationProblemDetails(ex.Failures)
    {
        Status = StatusCodes.Status400BadRequest
    });
    options.Map<ChhException>(ex => MapChhExceptionToProblemDetails(ex));
});
app.UseExceptionHandler(); // Required to activate IExceptionHandler
```

- Return structured error responses with clear error codes and messages
- Log all errors for debugging and monitoring
- **Error response format:** RFC 7807 `ProblemDetails` — not custom error schemas
- OTP-specific: `POST /api/v1/auth/otp/verify` returning "Invalid OTP" is a
  normal validation failure (422), not logged as an error — see CHH-F01 AC2.

---

## 8. Logging

- Use **Serilog** with structured logging
- Configure via `appsettings.json` Serilog block
- Enrich with: `FromLogContext`, `WithThreadId`, `WithProcessId`, `WithMachineName`
- Export logs to **Azure App Insights** (APM tool)

### Log Levels

| Level | Purpose |
|-------|---------|
| `Debug` | Debugging only — lowest level |
| `Information` | Normal operation records |
| `Warning` | Potential issues |
| `Error` | Errors preventing correct functioning |
| `Fatal` | Critical errors causing crash |

- **Never log sensitive information** — no OTP codes, JWTs, mobile numbers
  in full (mask all but last 2 digits if a mobile number must appear in a
  log line), or health-screening data.
- Log format: `[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}`

---

## 9. Date and Time Handling

> See DOTNET-RULES Part 1 §13 for the full date/time type mapping including
> `DateOnly`, `TimeOnly`, `DateTimeOffset`, and `datetime2`/`timestamptz`
> (this project uses PostgreSQL — see db-standards.md for the PostgreSQL
> column type equivalents). CHH-specific usage examples:

### Use `DateOnly` when:
- Date of birth: `DateOnly DateOfBirth` (Individual Registration, PRD §8)
- Event start/end date component: `DateOnly EventDate`

### Use `DateTimeOffset` / `timestamptz` when:
- `CreatedAtUtc`, `UpdatedAtUtc` on every entity
- `OtpRequestedAtUtc`, `OtpExpiresAtUtc` (OTP resend timer, CHH-F01)
- `SessionExpiresAtUtc` (1-hour JWT session, CHH-F01 AC3)
- `VerifiedAtUtc` (facility document verification timestamp, CHH-F03)

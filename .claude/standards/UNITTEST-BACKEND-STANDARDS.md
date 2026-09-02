# Standards: .NET Unit & Integration Testing (CHH Backend)

> Authoritative for all backend test writing in this project. Read in full before writing any tests.
>
> **This document covers:** CHH backend testing philosophy, what to test per layer, and coverage thresholds.
> **For how to write tests** (AAA structure, xUnit attributes, Moq patterns, FluentAssertions, code examples, test project layout, NuGet packages, anti-patterns), see **DOTNET-RULES.md Part 3** — those rules are binding here and are not repeated.

---

## Testing Philosophy

- **Test behavior, not implementation.** Test what the system produces (HTTP response, DB state) — not internal method calls or private attributes.
- **Test at the right level.** Controller tests verify the full contract. Service tests verify business rules. Repository tests verify data access. Do not duplicate coverage across layers.
- **Mock at the boundary.** For Service tests, mock the Repository. For Controller tests, either mock the Service or use the full stack with a real test database.
- **Prefer real over fake where practical.** Repository tests must hit a real test database (in-memory or containerized), not a mocked ORM. Controller integration tests use a real HTTP client against the real app.
- **No test logic.** No `if`, `for`, `while`, or `try/catch` inside test methods.
- **One Act per test.** One HTTP call or one method call per test.

---

---

## What to Test

### Controller Layer (unit tests)

- **Happy path** — valid input returns the correct `CreatedAtActionResult` / `OkObjectResult` status code and response body
- **Validation error** — invalid input returns the correct error with field-level detail
- **Auth guard** — unauthenticated request returns 401; insufficient permissions returns 403 (integration test)
- **Not found** — service throws `NotFoundException` → controller propagates (middleware handles it)
- **Business rule violation** — service throws domain exception → correct error status returned
- **Response schema** — all required fields are present and correctly typed
- **Pagination / list** — correct `Items`, `Total`, `Page`, `PageSize` (if applicable)

### Service Layer (unit tests — all dependencies mocked)

- **Happy path** — returns mapped response DTO
- **Not found** — repository returns `null` → service throws `NotFoundException`
- **Duplicate detected** — service throws `ConflictException`
- **Access control failure** — service throws `ForbiddenException`
- **Cache hit** — returns cached result without calling repository (if caching is used)
- **Cache miss** — calls repository then writes to cache
- **Mutation invalidates cache** — cache invalidation called after a write
- **Every business rule guard** — each condition throws the correct domain exception
- **Repository mutation** — `AddAsync` / `UpdateAsync` called exactly once with the correct entity

### Repository Layer (integration tests — real EF Core in-memory DB)

- **Happy path** — `GetByIdAsync` returns correct entity when present
- **Not found** — `GetByIdAsync` returns `null` for a missing ID
- **Persist** — `AddAsync` + `SaveChangesAsync` persists entity and sets generated fields
- **Soft-delete filter** — global query filter excludes soft-deleted records
- **Relationships** — related navigation properties are correctly loaded (not lazy)
- **Pagination** — returns correct `Items` and `Total` for given `Page` and `PageSize`
- **Filter queries** — returns only matching rows

### Cache

- **Cache hit** — returns the cached value when the key exists
- **Cache miss** — returns `null` when the key is absent
- **Cache set** — stores correct data with the correct TTL
- **Cache invalidation** — deletes the correct key(s)
- **Deserialization failure** — returns `null` (not throws) on malformed cached data

### FluentValidation (unit tests — no mocks)

- **Valid input** — `IsValid` is `true`
- **Each invalid field** — `IsValid` is `false` with the correct `PropertyName`
- **Boundary values** — 0, -1, max+1 for numeric rules

---

---

## Coverage Thresholds

| Layer | Minimum Lines | Minimum Branches |
|---|---|---|
| Services | 90% | 85% |
| Repositories | 85% | 80% |
| Controllers | 85% | 80% |
| Validators | 100% | 100% |
| Domain entities | 90% | 85% |
| Exception handlers | 85% | 80% |

Commands:
```
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:Html
```
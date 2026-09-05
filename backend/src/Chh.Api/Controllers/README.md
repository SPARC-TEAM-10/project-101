# Chh.Api / Controllers

Entry layer. Rules that apply to every file added here:

- **Contract-first (`.claude/rules/api-standards.md` §3):** no action may exist
  unless it is already in `contracts/chh-api.v1.yaml`. That file does not exist
  in the repo yet — it is a prerequisite for the first real controller.
- **Layer isolation (`backend/CLAUDE.md`):** controllers must never reference
  `DbContext`, repository types, or `Microsoft.EntityFrameworkCore.*`. Delegate
  to an Application service.
- `[Authorize]` by default; only `POST /api/v1/auth/otp/request` and
  `POST /api/v1/auth/otp/verify` may be `[AllowAnonymous]`.
- XML `///` comments on every controller and action (feeds Swagger).

Empty by design. The `/health` endpoint is a minimal API in `Program.cs`, not a
controller, because it is not part of the versioned API contract.

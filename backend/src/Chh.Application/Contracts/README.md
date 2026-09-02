# Chh.Application / Contracts

Service and repository interfaces (e.g. `IOtpService`, `IBloodRequestRepository`,
`ISmsGatewayClient`). Implementations live in `Chh.Infrastructure` and are bound
in DI from `Chh.Api/Extensions`.

Note: `ISmsGatewayClient` must stay provider-agnostic — the SMS provider is an
open PRD question (see `backend/CLAUDE.md` Tech Stack).

Empty by design.

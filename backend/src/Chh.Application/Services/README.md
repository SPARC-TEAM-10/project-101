# Chh.Application / Services

Logic layer — OTP issuance/verification, role redirection, proximity matching,
notification dispatch, domain rules. Services are the only layer allowed to
call repositories; controllers must never touch `DbContext` or EF Core types
(see `backend/CLAUDE.md` — Layer isolation rule).

Empty by design.

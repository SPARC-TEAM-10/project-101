# Project standards

> Fill in the `[PLACEHOLDER]` sections once the idea and stack are locked.
> Keep this file lean — every line here is loaded into every subagent call,
> for every module. Stack-specific detail belongs in the nested CLAUDE.md
> files below, not here.

## Project

- **Idea**: Community Health Hub (CHH) — a mobile-first web platform
  connecting blood donors, seekers, hospitals, and NGOs: OTP-first login,
  real-time proximity alerts for blood requests and events, hospital
  inventory/resource visibility, and facility verification. Source of
  truth: PRD-CHH-v2.2 (Confluence) and the Jira `CHH` project.
- **Modules**:
  - `backend` — ASP.NET Core 8 Web API + PostgreSQL, single service (see `backend/CLAUDE.md`)
  - `frontend` — React + TypeScript + Vite, mobile-first responsive web (see `frontend/CLAUDE.md`)

## Repo structure

This is a monorepo: one git history, backend and frontend live as plain
subfolders (no git submodules). Each has its own nested `CLAUDE.md` with
stack-specific standards — Claude Code auto-loads whichever is closest to
the files an agent is touching, so a backend coder agent never loads
frontend-only rules and vice versa.

- `backend/CLAUDE.md` — backend stack, coding standards, backend-only DoD
- `frontend/CLAUDE.md` — frontend stack, coding standards, frontend-only DoD

The Architect agent creates these nested files (and the folders) when it
locks the tech stack per module — see `.claude/agents/architect.md`.

## Coder pipeline for backend/frontend modules

The generic `coder-{module}` stage referenced elsewhere in this file (and
in `docs/AGENTIC_SDLC.md`) is, for this project, implemented by a richer
sub-pipeline: `.claude/agents/orchestrator.md`, driving a
Startup → Knowledge → Planning → Coding → Code Review → Unittest → PR flow
with its own developer/lead approval gates and Confluence-published
implementation plans.

- **It supersedes `coder-backend-template.md` / `coder-frontend-template.md`**
  for backend and frontend module tickets. Once the Architect creates a
  module's Jira ticket, hand it to the orchestrator (paste the Jira URL, or
  `/task <TICKET_ID>`) rather than copying a generic `coder-<module>` agent.
- The orchestrator's own **Code Review Agent** and **Unittest Agent** stages
  fulfill the role of the macro `reviewer.md` / `tester.md` agents for these
  tickets — the macro Reviewer/Tester are **not** separately invoked for
  backend/frontend module work.
- The macro `reviewer.md` / `tester.md` and the generic `coder-template.md`
  remain the path for any module **outside** the backend/frontend split
  (e.g. a standalone infra module).
- Concrete agent files: `backend-*-agent.md` (backend side) and
  `frontend-*-agent.md` (frontend side) in `.claude/agents/`; `startup-agent`
  and `pr-agent` are shared across both sides. Full roster and behavior:
  `backend/CLAUDE.md` §Agent Directory (the orchestrator's entry point doc).

## Shared coding standards (apply to every module)

- Commit message format: `<type>(<module>): <short summary>`
  (types: feat, fix, refactor, test, docs, chore)
- Cross-module interfaces (API contracts, shared types) are documented in
  Confluence by the Architect and must not change unilaterally — see each
  coder agent's "what not to do."

## Definition of done (checked by the Reviewer agent)

A module/ticket is "done" only when all of the following are true:

- [ ] Matches the acceptance criteria in the linked Jira ticket / Confluence page
- [ ] Passes lint/format checks (enforced by hook, not manual)
- [ ] Passes its own test suite
- [ ] No unresolved Reviewer comments on the ticket
- [ ] Public functions/interfaces documented with a one-line docstring/comment
- [ ] If the module exposes or consumes a cross-module interface, it matches
      the documented contract exactly (no silent drift between backend/frontend)

Module-specific additions to this checklist (e.g. "matches OpenAPI spec",
"passes accessibility lint") live in that module's nested CLAUDE.md.

## Context engineering rules

- **Global context (this file)**: stable, low-churn, shared by every module.
  Only updated via a deliberate Decisions Log entry — never grows by
  accumulation. Backend/frontend-specific standards go in the nested files
  instead of being added here.
- **Per-module context (nested CLAUDE.md)**: stable per module, only loaded
  by agents working inside that module's folder.
- **Per-feature context**: lives in the module's Jira ticket + linked
  Confluence page. Only the coder agent working that ticket loads it.
- **Compaction**: when a module is marked done, the Reviewer agent (or a
  dedicated compaction step) writes a 2-3 line summary into the Decisions
  Log below. Future agents read the summary, not the full ticket history.

## Decisions log

> One entry per architectural decision or completed module. Keep each entry
> to 2-3 lines. This is what keeps global context small as the project grows.

- **2026-09-02 — Pipeline reconciliation**: Locked Idea as Community Health
  Hub (CHH, PRD-CHH-v2.2); replaced backend content that had been copied in
  from an unrelated prior template ("KTA"). Simplified the orchestrator
  sub-pipeline from multi-repo/microservices assumptions (shared NuGet repo,
  sibling cloned repos) down to this repo's actual single-monorepo shape,
  and dropped the RCA bugfix workflow (no bug tickets exist yet — add back
  when needed). Added `frontend-*-agent.md` counterparts (React + TS + Vite)
  alongside the existing `backend-*-agent.md` set, and a design-readiness
  check in the Planning Agent for tickets labeled `needs-design`.
- **2026-09-05 — Deployment targets locked**: Frontend deploys to **Vercel**
  (git-push-to-deploy from the `frontend/` folder); backend deploys to
  **AWS** (App Runner/Elastic Beanstalk + RDS PostgreSQL). Split-cloud, not
  all-AWS — chosen for Vercel's faster Vite/React deploy experience. Backend
  CORS must allow the Vercel origin(s); see `backend/CLAUDE.md` and
  `frontend/CLAUDE.md` for the per-side detail.
- **2026-09-08 — CHH-73 interim SystemAdmin mechanism settled**: Two
  independent fixes for the same PR review comments (hardcoded admin mobile
  number → real mechanism) landed on the same branch — one adding
  `IndividualProfile.IsAdmin`, the other a separate `AdminUser` table. Kept
  `IndividualProfile.IsAdmin` (see `.claude/rules/api-standards.md` §5); the
  `AdminUser` table/repository were removed. Routing (the global
  `RoutePrefixConvention` + controller-naming coupling — see
  `.claude/rules/api-standards.md` §1) was untouched by this decision.
- **2026-09-09 — Codebase-scan caching + pipeline drift check added**:
  Codebase Analysis Agent now caches known Service/Repository/Controller
  classes in `.claude/backend-symbol-map.md`, keyed to `codebaseRef`, and
  only re-scans git-diffed files (falls back to a full scan if the map is
  missing/stale or >25% of tracked files changed — never trusts cache for
  anything the diff flags as changed). **Correction, same day:** the map is
  git-ignored, not committed — it's a derived, disposable artifact, and
  committing it would create merge conflicts between concurrent feature
  branches instead of just triggering a cheap rebuild. Added a maintainer-run
  `pipeline-regression-skill` plus `.claude/golden-tickets.md`
  (CHH-8/35/38) to catch silent drift in agent `.md` files after edits —
  advisory only, not part of the per-ticket workflow.
- **2026-09-09 — Git safety guardrails added**: New `.claude/rules/git-safety.md`
  forbids `--force` pushes, `reset --hard`, `clean -f`, force-deleting
  branches, amending pushed commits, rebase, and `--no-verify` across the
  whole pipeline (not just the coder), and restates that a reviewed GitHub
  PR merge is the only path onto `gitBaseBranch`. Wired in via Orchestrator
  Rule 13 and referenced from `backend-coding-agent.md`,
  `frontend-coding-agent.md`, `pr-agent.md`, `git-branch-skill.md`, and
  `github-pr-skill.md`.

## Non-goals / out of scope

- Live GPS tracking of moving ambulances
- Active sleep tracking (UI placeholder only — no functional tracking)
- Medical diagnosis or clinical advice
- Multi-tenant / multi-repo infrastructure (this is a single-team, single-repo build)

# Project standards

> Fill in the `[PLACEHOLDER]` sections once the idea and stack are locked.
> Keep this file lean — every line here is loaded into every subagent call,
> for every module. Stack-specific detail belongs in the nested CLAUDE.md
> files below, not here.

## Project

- **Idea**: [PLACEHOLDER — one-line description]
- **Modules**: [PLACEHOLDER — top-level list, e.g. backend, frontend]

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

<!-- Example:
- **2026-08-31 — Auth module**: JWT-based, tokens in httpOnly cookies.
  See Confluence page "Auth design" for full rationale.
-->

## Non-goals / out of scope

- [PLACEHOLDER — anything explicitly not being built, to prevent scope drift]

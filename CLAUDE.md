# Project standards

> Fill in the `[PLACEHOLDER]` sections once the idea and stack are locked.
> Keep this file lean — every line here is loaded into every subagent call.

## Project

- **Idea**: [PLACEHOLDER — one-line description]
- **Tech stack**: [PLACEHOLDER — languages, frameworks, DB]
- **Modules**: [PLACEHOLDER — list, e.g. auth, api, frontend]

## Coding standards

- Language/style guide: [PLACEHOLDER — e.g. PEP8 / Airbnb JS style]
- Linter/formatter: [PLACEHOLDER — e.g. eslint + prettier, ruff]
- Test framework: [PLACEHOLDER — e.g. pytest, jest]
- Commit message format: `<type>(<module>): <short summary>`
  (types: feat, fix, refactor, test, docs, chore)

## Definition of done (checked by the Reviewer agent)

A module/ticket is "done" only when all of the following are true:

- [ ] Matches the acceptance criteria in the linked Jira ticket / Confluence page
- [ ] Passes lint/format checks (enforced by hook, not manual)
- [ ] Passes its own test suite
- [ ] No unresolved Reviewer comments on the ticket
- [ ] Public functions/interfaces documented with a one-line docstring/comment

## Context engineering rules

- **Global context (this file)**: stable, low-churn. Only updated via a
  deliberate Decisions Log entry — never grows by accumulation.
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

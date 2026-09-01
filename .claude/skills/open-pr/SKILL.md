---
name: open-pr
description: Open a pull request for a completed module, following this repo's PR conventions. Use once a coder agent's ticket has passed Review and its tests, before final integration.
---

# Open a PR

> Generic across backend/frontend — the git/gh mechanics don't depend on
> stack. Fill in `[PLACEHOLDER]`s once repo conventions (branch naming,
> required reviewers, CI checks) are decided.

## Preconditions

This skill uses the `gh` CLI, not MCP — check it's authenticated before
anything else: run `gh auth status`. If it reports not logged in, stop and
tell the user: "GitHub CLI isn't authenticated. Run `gh auth login`, then
retry." Do not attempt `git push`/`gh pr create` against an unauthenticated
`gh` — it fails mid-way and can leave a pushed branch with no PR opened.

## Steps

1. Confirm the module's Jira ticket is in "Review" (or later) status and the
   relevant test skill (`run-backend-tests` / `run-frontend-tests`) has
   passed — do not open a PR on failing tests.
2. Branch naming: `[PLACEHOLDER — e.g. <module>/<ticket-key>-short-desc]`
3. Commit using the format from `CLAUDE.md`: `<type>(<module>): <summary>`
4. Push and open the PR via `gh pr create`, with:
   - Title: ticket key + short summary
   - Body: what changed, link to the Jira ticket, link to the interface
     contract if this module exposes/consumes one
   - `[PLACEHOLDER — required reviewers/labels/checks, if any]`
5. Link the PR back to the Jira ticket (comment with the PR URL).

## What NOT to do

- Do not open a PR before tests pass — see the relevant `run-*-tests` skill.
- Do not force-push or bypass CI checks to get a PR to a green state.

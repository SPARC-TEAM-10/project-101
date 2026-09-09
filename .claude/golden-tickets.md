# Golden Tickets — Pipeline Regression Baseline

> Used by `.claude/skills/pipeline-regression-skill/SKILL.md` to detect
> silent drift in the orchestrator pipeline after an `.claude/agents/*.md`,
> `.claude/skills/*.md`, or `CLAUDE.md`/`rules` edit.
>
> **These checkpoint values are recorded by hand** from the real,
> already-completed plans/PRs for each ticket below — they are not
> generated automatically, and must not be filled in by guessing. If a
> value below is unknown, leave it as `TBD` rather than inventing one;
> the regression skill should skip a `TBD` checkpoint rather than treat it
> as a false baseline.
>
> When an agent's behavior legitimately changes on purpose, update the
> affected checkpoint(s) here deliberately — otherwise the regression
> skill will flag permanent false drift forever.

---

## CHH-8

- **One-line description**: TBD — fill in from the Jira ticket summary.
- Design Status surfaced: TBD
- Migration Plan section present: TBD
- IssueType resolved: TBD
- Confluence LLD found: TBD

## CHH-35

- **One-line description**: TBD — fill in from the Jira ticket summary (donor accept/decline, per `main` merge history).
- Design Status surfaced: TBD
- Migration Plan section present: TBD
- IssueType resolved: TBD
- Confluence LLD found: TBD

## CHH-38

- **One-line description**: Event creation (POST `/api/v1/events` + frontend form).
- Design Status surfaced: TBD
- Migration Plan section present: TBD
- IssueType resolved: TBD
- Confluence LLD found: TBD

---

## Filling this in

For each ticket above, open its actual approved implementation plan
(Confluence, or the plan as it appeared in the conversation/PR history)
and record what the plan actually contained at the time:

- **Design Status surfaced** — did the plan include a `### Design Status`
  section (per `backend-planning-agent.md` / `frontend-planning-agent.md`)?
  `Yes` only if the ticket had `needs-design` or a blank/TBC design
  reference; `No` (i.e. correctly omitted) if the ticket had a confirmed
  design reference.
- **Migration Plan section present** — did the plan include a
  `### 11. Migration Plan` section? `Yes` if the ticket added/modified an
  entity; `No` (correctly omitted) otherwise.
- **IssueType resolved** — the `IssueType` the Knowledge Agent recorded
  (`Story` / `Bug` / `Task`), which drove the `feature/`
  vs `bugfix/` branch prefix.
- **Confluence LLD found** — whether the Knowledge Agent's output cited an
  actual LLD page (`Yes`), or reported it as a gap (`No`).

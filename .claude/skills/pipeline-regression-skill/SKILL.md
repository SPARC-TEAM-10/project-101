---
name: pipeline-regression-skill
description: Maintainer-run check for silent drift in the orchestrator pipeline after editing an agent .md file, a skill, or CLAUDE.md/rules. Replays Knowledge + Planning against a golden ticket and diffs structural checkpoints. Advisory only — not part of the per-ticket workflow.
---

# Pipeline regression check

This is a smoke test for the **pipeline itself**, not for a ticket's code.
It exists because editing an agent `.md` file has no other safety net — a
required gate (e.g. the Planning Agent's Design Status check or Migration
Plan section) could silently stop being surfaced, and nothing would catch
it until a real ticket went wrong.

## When to use

Run manually, by a maintainer, after editing any of:
- `.claude/agents/*.md`
- `.claude/skills/*/SKILL.md`
- root `CLAUDE.md` or `.claude/rules/*.md`

Run it **before** trusting the change on a real ticket. It is never
invoked automatically by the Orchestrator and never gates a real task.

## Steps

1. Read `.claude/golden-tickets.md`. Ask the developer which golden ticket
   to use (default: the first entry in the file). Skip any checkpoint
   currently recorded as `TBD` for that ticket rather than treating it as
   a baseline.
2. Determine `Side` (backend/frontend) for the chosen ticket the same way
   the Orchestrator does (component/label, or ask if ambiguous).
3. Re-run the Knowledge Agent, then the Planning Agent, for that ticket in
   **dry-run mode**:
   - No branch creation (do not invoke the Git Branch Skill).
   - No Confluence writes (`createConfluencePage` / `updateConfluencePage`)
     — stop as soon as the Planning Agent presents the plan in
     conversation, before Gate 1's "Publish to Confluence?" prompt.
   - No Jira writes — already globally disabled for the whole pipeline
     (see `orchestrator.md` Rule 10); this just confirms none are
     attempted here either.
   - Tell both agents explicitly, in the invocation, that this is a
     dry-run regression check with no side effects, so they don't wait on
     approval gates that assume a real task.
4. From the fresh Planning Agent output, extract the same checkpoints
   recorded in `.claude/golden-tickets.md`:
   - Is a `### Design Status` section present?
   - Is a `### 11. Migration Plan` section present?
   - What `IssueType` did the Knowledge Agent resolve?
   - Did the Knowledge Agent report a real Confluence LLD, or a gap?
5. Print a table comparing each non-`TBD` golden checkpoint against what
   this run produced:

   ```
   Checkpoint                        Golden      This run    Result
   Design Status surfaced            No          No          MATCH
   Migration Plan section present    Yes         No          DRIFT
   IssueType resolved                Story       Story       MATCH
   Confluence LLD found              Yes         Yes         MATCH
   ```

6. Report the table to the user. Do not edit any agent file, do not retry
   automatically, do not block anything — this skill only reports.

## Notes

- **Surface markers only.** This catches a gate/section silently
  disappearing or a field resolving to the wrong value — it cannot detect
  content-quality drift (e.g., the Design Status section present but with
  garbled or wrong reasoning inside it). Treat a MATCH as "structurally
  intact," not as "content verified" — a human should still read the plan
  when the underlying agent file changed meaningfully.
- **Golden values rot if not maintained.** If an agent's behavior changes
  on purpose (a deliberate improvement), update the affected checkpoint(s)
  in `.claude/golden-tickets.md` in the same change — otherwise this skill
  will report DRIFT forever on a checkpoint that's actually correct now.
- **`TBD` checkpoints are not baselines.** Fill them in from real history
  before relying on this skill for that ticket/checkpoint combination.

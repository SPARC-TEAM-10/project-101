# Rules: Git Safety (Community Health Hub — CHH)

> Status: authoritative for every agent in the orchestrator pipeline that
> runs a `git` or `gh` command — Coding Agent, PR Agent, Git Branch Skill,
> GitHub PR Skill, and any rework/re-review loop. This file exists because
> those agents follow a written rulebook rather than implicit judgment;
> "don't do anything destructive" needs to be an explicit, checkable rule
> here, the same as any other standard in this project.

---

## 1. Forbidden commands (hard rule — no exceptions, no "just this once")

No agent in this pipeline may ever run:

| Command | Why |
|---|---|
| `git push --force` / `--force-with-lease` / `-f` | Overwrites remote history — can destroy another developer's pushed work with no recovery path |
| `git reset --hard` | Discards uncommitted work with no recovery path |
| `git clean -f` / `-fd` / `-fx` | Permanently deletes untracked files |
| `git branch -D` / `git push origin --delete <branch>` | Force-deletes a branch, bypassing the "unmerged changes" safety check |
| `git commit --amend` on a commit that has already been pushed | Rewrites published history — the next push would require a force-push, which is itself forbidden |
| `git rebase` (any form) | This repo's convention is merge commits (see `git-branch-skill.md` and `pr-agent.md`) — rebase is never used to reconcile a feature branch with its base |
| `--no-verify` / `--no-gpg-sign` / any flag that skips a hook or signing check | Hooks and signing exist for a reason; a failing hook is a problem to fix, not bypass |
| `git checkout -- .` / `git restore .` / `git restore --staged .` (unscoped, no path list) | Discards uncommitted changes broadly instead of the specific file the agent meant to touch |

If a task seems to require one of these, that is a signal to **stop and ask
the developer**, not to proceed. There is no scenario in the normal
pipeline (Coding, Rework, Code Review, Unittest, PR) that legitimately
needs any command on this list.

---

## 2. Never push to or commit on the base branch

- `gitBaseBranch` (from `project_config.md`, or `BaseBranchOverride` for
  `/dev` runs) is read-only from every agent's perspective except via a
  reviewed PR merge on GitHub itself.
- No agent ever runs `git push origin <BaseBranch>`, `git commit` while
  `<BaseBranch>` is the active branch, or any operation that writes to
  `<BaseBranch>` directly. This is already enforced as a hard blocker for
  *coding* in `git-branch-skill.md` §"Hard Rule — Never Code on the Base
  Branch" — this rule extends the same restriction to every git-writing
  operation in the pipeline, not just file edits.
- The **only** path from a feature/bugfix branch to `<BaseBranch>` is the
  reviewed GitHub pull request raised by the PR Agent (`github-pr-skill.md`)
  and merged by a human on GitHub.

---

## 3. Merging *into* a feature branch (base has moved ahead)

Already the documented behavior in `git-branch-skill.md` Step 2a and
`pr-agent.md` step 4 — restated here as the binding rule other agents must
also follow:

- Never merge automatically. Report the incoming commits and ask the
  developer for explicit confirmation first.
- Only `git merge origin/<BaseBranch> --no-edit` after confirmation —
  never rebase.
- Resolve conflicts against the actual source of truth for each file
  (check history, contract, or Confluence — never default to blindly
  keeping "ours" or "theirs").
- Re-run the build/test suite after any merge before treating it as safe.

---

## 4. Escalate instead of working around

If a git command fails, returns an unexpected result, or the agent is
tempted to reach for a forbidden command to "fix" a messy state (detached
HEAD, diverged branch, dirty working tree it didn't expect):

- **Stop.** Do not retry with a different flag, do not silently pick a
  destructive fix, and do not continue past the failure.
- Report the exact error and the current `git status` to the Orchestrator
  and the developer, and wait for guidance.

This mirrors the existing error-handling rule in `git-branch-skill.md`
("Do not retry silently. Surface the error immediately.") — extended to
every agent that touches git, not just that skill.

---

## 5. Staging discipline

- Always stage files explicitly by name (`git add <file1> <file2> ...`).
  Never `git add .` or `git add -A` — already required in
  `backend-coding-agent.md` / `frontend-coding-agent.md`; restated here as
  a general rule for any future agent that commits code, since an
  unscoped add can silently capture secrets, stray local config, or
  unrelated in-progress files.
- Verify `git status --porcelain` is clean of tracked-file changes after
  every commit — if anything unexpected remains staged or modified, stop
  and report rather than committing it anyway.

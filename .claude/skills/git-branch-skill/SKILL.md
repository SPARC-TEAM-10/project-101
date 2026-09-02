---
agent: git-branch-skill
attached_to: coding-agent
---

# Git Branch Skill

Creates or checks out a feature branch before any code is written. Called by the Coding Agent as Gate 3 of its PRE-CONDITIONS.

---

## Modes

This skill operates in two modes depending on the caller:

| Mode | Caller | Purpose |
|---|---|---|
| `Create` | Coding Agent (Gate 3) | Create or check out the feature branch before any file operation |
| `Validate` | Startup Agent (Step 4.2) | Check whether a named branch exists locally or on the remote without creating anything |

---

## Trigger Point

The Coding Agent MUST invoke this skill in **`Create` mode** during Gate 3, before any file operation, passing the data below from the approved plan context.

The Startup Agent invokes this skill in **`Validate` mode** during Step 4.2 to verify the user-entered git base branch exists.

---

## Input

| Parameter | Type | Required | Description |
|---|---|---|---|
| `TicketId` | string | No | Jira ticket ID (e.g. `US-123`). Omit if no ticket. |
| `Description` | string | Yes | Short feature description — lowercase, hyphen-separated, ≤ 5 words (e.g. `add-user-profile-page`) |
| `BaseBranchOverride` | string | No | When provided, use this as the base branch instead of `gitBaseBranch` from `project_config.md`. Set by the Orchestrator for `/dev` runs (e.g. `release/0.17.2`). |
| `BranchPrefixOverride` | string | No | When provided, use this as the branch prefix instead of `featureBranchPrefix` from `project_config.md`. Set by the Orchestrator for `/dev` runs (e.g. `bugfix/`). |

### Branch Name Rules

- If `BranchPrefixOverride` is provided, use it as the prefix; otherwise read `featureBranchPrefix` from `project_config.md` (default: `feature/`).
- Ticket available → `<Prefix><TicketId>-<Description>` (e.g. `feature/CHH-8-mobile-entry-otp` or `bugfix/CHH-8-fix-otp-timer`)
- No ticket → `<Prefix><Description>` (e.g. `feature/add-user-profile-page`)

---

## How to Invoke — Validate Mode

Run the following to check whether a branch exists locally or on the remote:

```bash
git show-ref --verify --quiet refs/heads/<BranchName> \
  || git ls-remote --heads origin <BranchName> | grep -q .
```

- Exit 0 (found) → return `Status: Found`
- Exit non-zero (not found) → return `Status: NotFound` — this is a warning, not a hard block; see startup-agent Step 4.2 for the follow-up prompt

No branch is created or checked out in Validate mode.

---

## Hard Rule — Never Code on the Base Branch

**Coding on the base branch is strictly forbidden and is a hard blocker.**

If at any point during this skill — or during coding — `git branch --show-current` returns the resolved base branch name (i.e. `BaseBranchOverride` if set, otherwise `gitBaseBranch` from `project_config.md`), stop immediately and do not write any file. Report: *"Active branch is `<BaseBranch>`. All coding must be on a feature/bugfix branch. This is a hard blocker."*

---

## How to Invoke — Create Mode

**Step 1 — Check out the base branch and pull latest:**

```bash
git checkout <BaseBranch>
git pull origin <BaseBranch>
```

Gate: if either command exits non-zero, set `Status: Failed` and stop — do not create the feature branch on a stale or dirty base.

`BaseBranch` is resolved as follows: use `BaseBranchOverride` if provided by the caller; otherwise read `gitBaseBranch` from `project_config.md` (default `main`).

**Step 2 — Create or check out the feature branch:**

```bash
git show-ref --verify --quiet refs/heads/<BranchName> \
  && git checkout <BranchName> \
  || git checkout -b <BranchName>
```

**Step 3 — Verify the active branch is the feature branch (hard gate):**

```bash
git branch --show-current
```

- Output must equal `<BranchName>` exactly.
- If output is `<BaseBranch>` or anything other than `<BranchName>` → set `Status: Failed`, stop, and report: *"Branch switch did not complete — still on `<current>`. Coding blocked."*

Only after Step 3 confirms the correct branch, persist the branch name to memory:

Update `project_config.md` in the project memory directory — add or overwrite the `featureBranch` field:

```
featureBranch: <BranchName>
```

---

## Single-Repo Scope

This project is one monorepo (`backend/` and `frontend/` as plain
subfolders, one git history — see `.claude/repository-index.md` for the
module map). This skill creates **exactly one** feature branch per task, in
this repo, regardless of whether the task's Scope of Change touches
`backend/`, `frontend/`, or (rarely) both. There is no per-repo loop and no
shared-package repo to branch separately.

---

## Output

| Field | Description |
|---|---|
| `BranchName` | The full branch name created or checked out (Create mode only) |
| `Status` | `Created` \| `CheckedOut` \| `Found` \| `NotFound` \| `Failed` |
| `Error` | Populated only on `Failed` — the raw git error message |

---

## Error Handling

- Non-zero git exit code → set `Status: Failed`, report `Error`, **do not proceed**. Coding Agent must STOP and notify Orchestrator.
- Do not retry silently. Surface the error immediately.

---

## Required Tools

| Tool | Purpose |
|---|---|
| Bash | Run `git checkout` / `git show-ref` commands |

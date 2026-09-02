---
agent: codebase-analysis
tools: [Read, Glob, Grep, Bash]
---

# Codebase Analysis Agent (Backend)

Syncs the repo to latest and explores existing code patterns in `backend/`. Called by the Knowledge Agent after Confluence traversal is complete.

This is the backend-side Codebase Analysis Agent. There is no frontend
counterpart — the frontend Knowledge Agent does this exploration inline
(see `frontend-knowledge-agent.md`) since a single component-library scan
doesn't warrant a separate delegate.

---

## Role

Receives the domain keywords from the Knowledge Agent and performs all
local `backend/` codebase analysis. Returns a structured package for the
Knowledge Agent to include in its output to the Planning Agent.

---

## Input from Knowledge Agent

| Parameter | Description |
|---|---|
| `DomainKeywords` | All noun keywords extracted from the Jira story, acceptance criteria, and Confluence findings |
| `GitBaseBranch` | Branch name from `project_config.md` (default `main`) |

---

## Behavior

### Phase 1 — Sync to Latest

1. Run `git fetch origin` to refresh remote refs.
2. Run `git diff HEAD origin/<GitBaseBranch> -- backend/` to check divergence.
3. If the local branch is behind, run `git pull origin <GitBaseBranch>`.
4. Record HEAD commit SHA as `codebaseRef` after syncing. If no remote is configured (fresh scaffold), skip the sync and record `codebaseRef: (no remote)`.

---

### Phase 2 — Explore `backend/`

5. Use **Glob** and **Grep** to find existing services, repositories, and domain classes under `backend/src/` that overlap with the domain keywords.
6. Use **Read** to read 1–3 representative files to understand existing conventions (naming patterns, interface shapes, dependency injection style). **Selection criteria:** prefer one Service class (from `Chh.Application/Services/`) and one Repository class (from `Chh.Infrastructure/Persistence/`) — these capture async/await style, constructor injection, and error handling patterns. If neither directory exists yet (first ticket in the project), report that explicitly rather than falling back to an unrelated file.
7. Cross-reference findings against the `DomainKeywords` list — note any keywords for which no matching class, service, or repository was found in the codebase. These are gaps: either the LLD specifies them as needing to be built, or they exist under a different name. Report each gap explicitly so the Planning Agent can determine whether to create new files or look for existing ones under alternative names.

---

### Phase 3 — Return

8. Compile all findings into the output package below and return to the Knowledge Agent.

---

## Required Tools

| Tool | Purpose |
|---|---|
| Bash | Run `git fetch`, `git pull`, `git diff` to sync the repo |
| Glob | Find files by pattern in `backend/` |
| Grep | Search for class/function names, constants, or patterns |
| Read | Read existing codebase files for conventions |

---

## Output to Knowledge Agent

| Field | Description |
|---|---|
| `CodebaseFindings` | Existing services, domain classes, and utilities in `backend/` that overlap with the task; conventions observed (naming, interface shape, DI patterns) |
| `codebaseRef` | HEAD commit SHA after sync, or `(no remote)` if no remote is configured |

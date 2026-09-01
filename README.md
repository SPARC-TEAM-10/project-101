# hackathon-agent-scaffold

Idea-agnostic Claude Code agentic SDLC scaffold. Build this *before* the idea
is locked. Everything in here is process, not product — nothing references a
specific tech stack, domain, or module until you fill in the placeholders.

Full technical documentation of how this system works — pipeline, agent
roster, skills, hooks, and the reasoning behind each — lives in
[`docs/AGENTIC_SDLC.md`](docs/AGENTIC_SDLC.md). **Keep it updated** whenever
you change an agent, skill, hook, or rule file.

## What's in here

```
.claude/
  agents/
    planner.md                    full — turns requirements into a spec + acceptance criteria
    architect.md                   full — breaks spec into independent modules, creates tickets
    reviewer.md                     full — diffs coder output against spec + standards
    tester.md                        full — runs tests, routes failures back to the right coder
    coder-backend-template.md   template — copy per backend module
    coder-frontend-template.md   template — copy per frontend module
    coder-template.md            template — generic fallback for modules outside backend/frontend
  skills/
    run-backend-tests/SKILL.md   run the backend test suite — used by coder/reviewer/tester
    run-frontend-tests/SKILL.md   run the frontend test suite — used by coder/reviewer/tester
    open-pr/SKILL.md             open a PR following repo conventions
  settings.json                 hooks config: lint/format on file edit (dispatch by path),
                                 test gate on Jira ticket transition (currently inert placeholder)
CLAUDE.md                        shared standards skeleton + empty Decisions Log
backend/CLAUDE.md                backend stack/standards skeleton (nested — only loaded in backend/)
frontend/CLAUDE.md               frontend stack/standards skeleton (nested — only loaded in frontend/)
```

This is set up as a **monorepo**: one git history, `backend/` and
`frontend/` as plain subfolders (not git submodules). Root `CLAUDE.md`
stays lean and shared; each side's stack-specific detail lives in its own
nested `CLAUDE.md`, which Claude Code auto-loads only when an agent is
working inside that folder.

Four levers shape agent behavior in this scaffold: **rules** (`CLAUDE.md`
files — standards agents read), **tools** (the `tools:` allowlist in each
agent's frontmatter — what an agent may call), **skills** (`.claude/skills/`
— reusable, invokable procedures like running tests or opening a PR, so
every agent does these the same way instead of improvising), and **hooks**
(`settings.json` — automatic actions on events like file edits or ticket
transitions).

## How to use this at the hackathon

1. **Before the idea is locked**: work in this repo. Finish wiring the
   Atlassian MCP server, test the hooks, review the four full agents as a
   team so everyone knows what each one does.
2. **When the idea locks**:
   - Copy `.claude/`, `CLAUDE.md`, `backend/CLAUDE.md`, and
     `frontend/CLAUDE.md` into your real project repo root.
   - Fill in the placeholders in `CLAUDE.md` (idea, top-level module list)
     and in `backend/CLAUDE.md` / `frontend/CLAUDE.md` (stack, coding
     standards) — the Architect agent does this as part of its process.
   - For each module identified by the Architect agent, copy
     `coder-backend-template.md` or `coder-frontend-template.md` (matching
     its side) to `coder-<module>.md` and fill in its placeholders. Use the
     generic `coder-template.md` only for modules outside the backend/
     frontend split (e.g. infra).
   - Module folders under `backend/` and `frontend/` are created by the
     Architect so each coder agent has a real place to write into.
   - Fill in the real test commands in `.claude/skills/run-backend-tests/SKILL.md`
     and `run-frontend-tests/SKILL.md`, and wire the `PreToolUse` hook in
     `settings.json` to actually run them and block the transition on failure.
3. **Start the pipeline**: Planner → Architect → parallel Coders → Reviewer
   → Tester, per the flow described in each agent file.

## Design principles baked into this scaffold

- **Token efficiency**: each subagent has its own isolated context — no
  agent re-reads the full project history. `CLAUDE.md` stays lean; per-module
  detail lives in Jira/Confluence, not in global context.
- **Minimal drift**: `CLAUDE.md` is the single contract every agent is
  measured against. The Reviewer agent enforces it before anything merges.
- **Minimal rework**: Reviewer runs before Integrator, Tester routes failures
  to the specific coder agent only — not a full pipeline re-run.
- **Parallel + collaborative work**: one coder agent per module, each scoped
  to its own files and its own Jira ticket, so multiple team members (and
  agents) can work concurrently without stepping on each other.
- **Backend/frontend isolation**: nested `CLAUDE.md` files mean a frontend
  coder agent never loads backend-only standards and vice versa; explicit
  interface contracts (written by the Architect) are the only thing that
  crosses the boundary, which keeps the two sides from drifting apart.

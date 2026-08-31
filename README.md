# hackathon-agent-scaffold

Idea-agnostic Claude Code agentic SDLC scaffold. Build this *before* the idea
is locked. Everything in here is process, not product — nothing references a
specific tech stack, domain, or module until you fill in the placeholders.

## What's in here

```
.claude/
  agents/
    planner.md          full — turns requirements into a spec + acceptance criteria
    architect.md         full — breaks spec into independent modules, creates tickets
    reviewer.md           full — diffs coder output against spec + standards
    tester.md              full — runs tests, routes failures back to the right coder
    coder-template.md   template — copy + fill in placeholders once modules are known
  settings.json         hooks config (lint/format on file edit)
CLAUDE.md                standards skeleton + empty Decisions Log
```

## How to use this at the hackathon

1. **Before the idea is locked**: work in this repo. Finish wiring the
   Atlassian MCP server, test the hooks, review the four full agents as a
   team so everyone knows what each one does.
2. **When the idea locks**:
   - Copy `.claude/` and `CLAUDE.md` into your real project repo root.
   - Fill in the placeholders in `CLAUDE.md` (tech stack, module list).
   - For each module identified by the Architect agent, copy
     `coder-template.md` to `coder-<module>.md` and fill in its placeholders.
   - Create your module folders (`frontend/`, `backend/`, etc.) so each
     coder agent has a real place to write into.
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

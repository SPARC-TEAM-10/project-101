# Design Links

Single source of truth for where each story's UI design lives. The Designer
agent appends one row per story after explicit user approval — never before.

**This file is a log, not the connection to Jira.** The actual handoff
that unblocks a story is a `Design Reference` field the Designer agent sets
on that story's **Confluence page** — Jira comments are disabled repo-wide
(`.claude/agents/orchestrator.md` Rule 10) and the frontend Planning
Agent's design-readiness gate reads Confluence, not Jira comments (see
`frontend-knowledge-agent.md` / `frontend-planning-agent.md`).

## Canvas

CHH uses **one shared Claude Design canvas** for the whole app. A
`/design-story` run drafts on a disposable per-story scratch artifact
(`design/drafts/<KEY>.dc.html`) first; only after explicit approval does it
add the story's artboard(s) to this canonical canvas and republish to the
same URL below — it never publishes a draft here directly, and it never
creates a new canonical artifact per story. Tool: **Claude Design** (Figma
is currently blocked by an MCP limit issue — see `.claude/agents/designer.md`
for the fallback rule).

- **Canvas URL**: https://claude.ai/code/artifact/6b185d14-a32d-4647-a2b7-7366b07c2b75?via=auto_preview
- **Seeded with**: Landing Page (built manually, outside the `/design-story` flow)

## Story → Artboard log

| Story (Jira key) | Artboard(s) added | Tool | URL | Date |
|---|---|---|---|---|
| — | Landing Page | Claude Design | (canvas URL above) | pre-`/design` |
| CHH-8 | Mobile Entry (mobile), Mobile Entry — Web | Claude Design | (canvas URL above) | 2026-09-03 |
| CHH-9 | OTP Verify (mobile), OTP Verify — Web | Claude Design | (canvas URL above) | 2026-09-03 |
| CHH-11 | New User / Guest (mobile), New User / Guest — Web | Claude Design | (canvas URL above) | 2026-09-03 |

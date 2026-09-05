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
- **Pages**: the canvas has two canvas-editor pages — **App** (every screen, one flat pan/zoom layout) and **Components** (standalone, non-flow UI pieces like the blood-drip loader and toast — added here, not inside a specific screen, when a component doesn't belong to one flow).

## Story → Artboard log

| Story (Jira key) | Artboard(s) added | Tool | URL | Date |
|---|---|---|---|---|
| — | Landing Page | Claude Design | (canvas URL above) | pre-`/design` |
| CHH-8 | Mobile Entry (mobile), Mobile Entry — Web | Claude Design | (canvas URL above) | 2026-09-03 |
| CHH-9 | OTP Verify (mobile), OTP Verify — Web | Claude Design | (canvas URL above) | 2026-09-03 |
| CHH-11 | New User / Guest (mobile), New User / Guest — Web | Claude Design | (canvas URL above) | 2026-09-03 |
| CHH-12 (US-CHH-002-01/02/03 — no Jira sub-tickets yet) | Registration (mobile), Registration — Web (disabled), Registration — Web (enabled) | Claude Design | (canvas URL above, App page) | 2026-09-03 |
| — | Feedback Components (blood-drip loader, top-center toast) | Claude Design | (canvas URL above, Components page) | 2026-09-03 |
| CHH-26 (US-CHH-003-01) | Facility Registration — Step 1 Details (mobile+web), Step 2 Contacts (web) | Claude Design | (canvas URL above, App page) | 2026-09-03 |
| CHH-27 (US-CHH-003-02) | Facility Registration — Step 3 Licence Upload (web); Facility Upload & Submit — Loader States, Facility Verification Toasts | Claude Design | (canvas URL above, App + Components pages) | 2026-09-03 |
| CHH-28 (US-CHH-003-03) | Verification Status — Facility Dashboard (mobile+web) | Claude Design | (canvas URL above, App page) | 2026-09-03 |
| CHH-33 (US-CHH-004-01) | Blood Request Form (mobile+web) | Claude Design | (canvas URL above, App page) | 2026-09-04 |
| CHH-34 (US-CHH-004-03) | Donor Notification Center (mobile+web) | Claude Design | (canvas URL above, App + Components pages) | 2026-09-04 |
| CHH-35 (US-CHH-004-04) | Request Details / Accept — Decline (mobile+web) | Claude Design | (canvas URL above, App page) | 2026-09-04 |
| CHH-36 (US-CHH-004-05) | Requester Dashboard (mobile+web) | Claude Design | (canvas URL above, App page) | 2026-09-04 |
| — | Proximity Alerts — Push/In-app/SMS previews and toasts | Claude Design | (canvas URL above, Components page) | 2026-09-04 |
| CHH-37 (US-CHH-005-01/02/03/04/07/08) | Create an event, Events near you, Event details/RSVP, Manage/edit/cancel event, Mark attendance, Event attendance dashboard (mobile+web) | Claude Design | (canvas URL above, App + Components pages) | 2026-09-05 |
| CHH-68 (US-CHH-001-01/02/03) | Emergency services hub — search, Facility details, No results/location denied/load failed empty states (mobile+web) | Claude Design | (canvas URL above, App page) | 2026-09-05 |
| CHH-72 (US-CHH-001-01/02/03/04) | Pending verification queue, Facility review + document viewer, Approve/reject dialogs, User management + suspend, Suspended sign-in, Decision notices — plus two proposed (not yet story-backed) screens: Add an organisation directly, Bulk upload (mobile+web) | Claude Design | (canvas URL above, App + Components pages) | 2026-09-05 |

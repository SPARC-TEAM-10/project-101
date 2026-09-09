---
agent: gap-flag-skill
attached_to: backend-knowledge-agent, frontend-knowledge-agent, backend-planning-agent, frontend-planning-agent
---

# Gap Flag Skill

Flags a story gap — an ambiguity, scope question, or undocumented decision that only the BA/requester can resolve — to the Jira ticket's reporter, via a Confluence footer comment and a Jira comment. Called by the Knowledge Agent or Planning Agent (backend or frontend) whenever a Gaps entry or Open Questions row is `Owner: BA`.

This is the sole permitted use of Jira comment-posting in this pipeline — see Orchestrator Rule 10's narrow exception in `orchestrator.md`.

---

## Universal Rule (applies to every invocation)

**Always ask before flagging.** The calling agent must present a confirmation prompt and wait for the developer to reply `Yes` (case-insensitive) before invoking this skill:

> "This looks like it needs BA/product input, not something we can resolve here: '\<Question\>'. Flag it to \<BA display name\> on `<TicketId>` (Confluence comment + Jira comment)? Reply `Yes` to flag, or tell me how to resolve it instead."

Any reply other than an unambiguous `Yes` is treated as the developer supplying the resolution directly — record it as the answer in the Gaps list / Open Questions table and do **not** invoke this skill.

---

## Trigger Point

Invoked by the Knowledge Agent or Planning Agent (backend or frontend) whenever a Gaps entry (Knowledge Agent) or an Open Questions row (Planning Agent) has `Owner: BA`, and the developer has replied `Yes` to the confirmation prompt above. Never invoked for `Owner: Developer` entries. Exactly one invocation per distinct gap — check `Flagged?` (Planning Agent's Open Questions table) before re-invoking on a plan revision; only re-flag if the question's wording materially changed.

---

## Input

| Parameter | Type | Required | Description |
|---|---|---|---|
| `TicketId` | string | Yes | Jira issue key the gap was raised against |
| `Question` | string | Yes | The gap/question text, verbatim from the Gaps list or Open Questions row |
| `WhyBlocking` | string | Yes | Why it's blocking (or, if `Blocking?` is No, why it still needs BA input before the plan is finalized) |
| `RaisedByStage` | string | Yes | `"Knowledge Agent"` or `"Planning Agent"`, plus `Side` (`Backend`/`Frontend`) |
| `LldPageId` / `ConfluenceUrl` | string | No | The plan/LLD Confluence page ID or URL already resolved this run — used as the comment's parent page and as the "full context" pointer. If both are null, fall back to the story's own Confluence page (same CQL resolution the Knowledge Agent already performs) |

---

## Steps

### Step 1 — Resolve the BA

```
mcp__claude_ai_Atlassian__getJiraIssue
  issueKey: <TicketId>
```

Read `fields.reporter.accountId` and `fields.reporter.displayName`. If the reporter is null/missing, ask the developer: *"No reporter found on `<TicketId>` — who should this be flagged to?"*, then resolve via:

```
mcp__claude_ai_Atlassian__lookupJiraAccountId
```

Do not guess an identity.

### Step 2 — Resolve the target Confluence page

Use `LldPageId`/`ConfluenceUrl` if provided. Otherwise resolve the story's own Confluence page via CQL (same pattern as `confluence-publish-skill` Step 1). If no Confluence page can be found at all, skip the Confluence half and proceed Jira-only — note this in the output; do not block the flag on a missing page.

### Step 3 — Post the Confluence footer comment

Only after the developer has confirmed `Yes`:

```
mcp__claude_ai_Atlassian__createConfluenceFooterComment
  pageId: <resolved page id>
  body: <formatted comment body, with @mention of reporter accountId>
```

### Step 4 — Post the Jira comment

```
mcp__claude_ai_Atlassian__addCommentToJiraIssue
  issueKey: <TicketId>
  comment: <formatted comment body, without @mention markup — plain text>
```

### Step 5 — Return result

Return `Status`/`ConfluenceCommentUrl`/`JiraCommentUrl` to the calling agent. The calling agent records the URL(s) in the `Flagged?` column of the Open Questions table (Planning Agent) or against the Gaps entry (Knowledge Agent, carried forward into the plan) so this exact gap is never re-flagged on a plan revision.

---

## Comment Body Format

Confluence (includes the `@`-mention line):

```
⚠️ Gap flagged for BA input — <TicketId>

Stage: <RaisedByStage> (<Side>)
Gap: <Question>
Why it's blocking: <WhyBlocking>

Full context: <link to plan/LLD Confluence page, or "no plan page yet — see ticket description">

@<BA display name>, could you clarify this before implementation proceeds?
```

Jira (plain text — Jira's `[~accountId:...]` mention syntax differs from Confluence's and triggers a different notification path, so this is spelled out instead of using markup):

```
⚠️ Gap flagged for BA input — <TicketId>

Stage: <RaisedByStage> (<Side>)
Gap: <Question>
Why it's blocking: <WhyBlocking>

Full context: <link to plan/LLD Confluence page, or "no plan page yet — see ticket description">

Reporter (<BA display name>) — please clarify before implementation proceeds.
```

---

## Output

| Field | Description |
|---|---|
| `Status` | `Flagged` \| `PartiallyFlagged` (one write failed) \| `Skipped` (developer declined) \| `Failed` |
| `ConfluenceCommentUrl` | URL of the posted footer comment, or `null` if skipped/failed |
| `JiraCommentUrl` | Jira comment permalink, or `null` |
| `BaAccountId` / `BaDisplayName` | The resolved reporter identity used |
| `Errors` | Populated on any failure — which half failed and why |

---

## Error Handling

- If the Confluence write fails but the Jira write succeeds (or vice versa): `Status: PartiallyFlagged`, surface which half failed. Do not retry automatically — let the calling agent decide whether to retry or proceed.
- Failure of this skill must not block plan generation or coding. It only leaves the corresponding Open Questions row unresolved — visible to the Tech Lead at Gate 1/Gate 2 sign-off.

---

## Required Tools

| Tool | Purpose |
|---|---|
| `mcp__claude_ai_Atlassian__getJiraIssue` | Resolve reporter (BA) identity |
| `mcp__claude_ai_Atlassian__lookupJiraAccountId` | Fallback BA resolution if reporter missing |
| `mcp__claude_ai_Atlassian__createConfluenceFooterComment` | Post the @mention comment on the story/LLD page |
| `mcp__claude_ai_Atlassian__addCommentToJiraIssue` | Post the plain comment on the ticket |
| `mcp__claude_ai_Atlassian__searchConfluenceUsingCql` | Fallback resolution of the story's Confluence page if no `LldPageId`/`ConfluenceUrl` was passed |

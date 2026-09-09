---
agent: confluence-publish-skill
attached_to: planning-agent, qa-execution-agent
---

# Confluence Publish Skill

Publishes or updates a page in Confluence, as a child of a resolved parent page. Called by the Planning Agent after the developer has approved the plan at Gate 1 and confirmed they are ready to publish (parent: the Epic's LLD page). Also called by the standalone QA Execution Agent (`.claude/agents/qa-execution-agent.md`) to publish a QA Execution Report after a post-merge test run (parent: the Feature's QA Test Case Design page — passed explicitly via `ParentPageId`, skipping LLD resolution entirely).

---

## Universal Rule (applies to every invocation)

**Always ask before publishing.** The calling agent must present a confirmation prompt and wait for the developer to reply `Yes` (case-insensitive) before invoking this skill — whether it is the first publish or a subsequent update. The exact prompt depends on context:

- **First publish:** "Ready to publish this plan to Confluence for lead review? Reply `Yes` to publish, or tell me what else to change."
- **Update (refinement round):** "Ready to publish this update to Confluence? Reply `Yes` to update the page, or continue refining."

Any reply other than `Yes` → treat it as a further refinement, apply feedback, re-present the updated plan, and ask again. **Never invoke this skill without receiving `Yes` first.**

---

## Trigger Point

The Planning Agent MUST invoke this skill only after both of the following are true:
- The developer has typed `PlanApproved` at Gate 1 (or `LeadApproved` at Gate 2 for updates)
- The developer has replied `Yes` to the publish confirmation prompt above

---

## Input

| Parameter | Type | Required | Description |
|---|---|---|---|
| `StoryId` | string | Yes | Jira story/ticket key (e.g. `US-123`), or a Feature ID (e.g. `CHH-F04`) for QA Execution Report calls |
| `PlanContent` | string | Yes | Full page body content to publish — the implementation plan for the Planning Agent; the QA Execution Report body for the QA Execution Agent; other callers pass their own content under this same field |
| `LldPageId` | string | No | Confluence page ID of the Epic's LLD page — use this when the Knowledge Agent found it. If null, and `ParentPageId` is also null, the skill resolves it via CQL. **Planning Agent only.** |
| `ParentPageId` | string | No | Confluence page ID to publish directly under, skipping LLD resolution entirely (Step 1). Use this when the caller has already resolved its own parent page — e.g. the QA Execution Agent passes the Feature's QA Test Case Design page ID here. Takes precedence over `LldPageId` if both are somehow set. |
| `PageTitle` | string | No | Full page title to use verbatim. If omitted, defaults to `"{StoryId} - Implementation Plan"` (Planning Agent's existing behavior, unchanged). Other callers should always pass this explicitly — e.g. the QA Execution Agent passes `"{FeatureId} - QA Execution Report (Run {N}, {Date})"`. |
| `SpaceKey` | string | Yes | Confluence space key (e.g. `CHH`) |
| `EpicKeywords` | string | No | Keywords from the Epic summary — used in the CQL fallback if both `LldPageId` and `ParentPageId` are null. **Planning Agent only.** |
| `ExistingPageId` | string | No | If the page was already published in a prior round, pass its ID here to update rather than create. QA Execution Reports are point-in-time records and should never pass this — each run creates a new page so execution history isn't overwritten. |

---

## Page Title Format

Default (used when `PageTitle` is not provided — this is the Planning Agent's existing behavior, unchanged):

```
{StoryId} - Implementation Plan
```

Example: `US-123 - Implementation Plan`

When `PageTitle` is provided, it is used verbatim instead — e.g. the QA Execution Agent passes `CHH-F04 - QA Execution Report (Run 1, 2026-09-08)`.

---

## Steps

### Step 1 — Resolve the parent page

If `ParentPageId` is provided, use it directly as the parent and skip to Step 2 — this is the path the QA Execution Agent always takes (it resolves the Feature's QA Test Case Design page itself before calling this skill). No LLD lookup is performed in this case.

Otherwise (Planning Agent path): if `LldPageId` is provided, skip to Step 2.

If neither is provided, find the Epic's LLD page via CQL (try in order, stop at first result):

```
# Option A — by title keyword scoped to the space
mcp__claude_ai_Atlassian__searchConfluenceUsingCql
  cql: space = "<SpaceKey>" AND title ~ "LLD" AND text ~ "<EpicKeywords>" AND type = page ORDER BY lastmodified DESC
  limit: 5

# Option B — broad title search in the space
mcp__claude_ai_Atlassian__searchConfluenceUsingCql
  cql: space = "<SpaceKey>" AND title ~ "LLD" AND type = page ORDER BY lastmodified DESC
  limit: 10
```

- Pick the page whose title most closely matches the Epic domain keywords
- If two or more pages match equally well, surface all candidates to the user: *"Multiple LLD pages found — which is the correct parent? Please confirm:"* then list each candidate's title and URL. Wait for the user to confirm one before proceeding.
- Record `resolvedLldPageId` from the confirmed result
- If no LLD page is found: surface the error to the user, display the plan content in the conversation, and ask the user to provide the LLD page ID or URL manually before retrying

### Step 2 — Publish or update the page

**If `ExistingPageId` is null (first publish):**

```
mcp__claude_ai_Atlassian__createConfluencePage
  spaceKey: <SpaceKey>
  parentId: <ParentPageId or LldPageId or resolvedLldPageId>
  title: <PageTitle, or "<StoryId> - Implementation Plan" if not provided>
  body: <PlanContent>
```

**If `ExistingPageId` is provided (refinement round — update existing page):**

```
mcp__claude_ai_Atlassian__updateConfluencePage
  pageId: <ExistingPageId>
  title: <PageTitle, or "<StoryId> - Implementation Plan" if not provided>
  body: <PlanContent>
```

### Step 3 — Return result

Return the page URL and page ID to the Planning Agent.

---

## Output

| Field | Description |
|---|---|
| `Status` | `Published` \| `Updated` \| `Failed` |
| `PageId` | Confluence page ID (new or existing) |
| `PageUrl` | Full Confluence URL of the published page |
| `ParentPageId` | The page ID actually used as the parent (whatever was passed in, or the resolved LLD page) — available to callers for audit or logging; not required downstream |
| `Error` | Populated only on `Failed` — include the reason and whether the LLD lookup failed or the page creation failed |

---

## Error Handling

- If the LLD page cannot be resolved via CQL and the user does not provide it manually (Planning Agent path only — callers that pass `ParentPageId` directly skip this risk entirely): set `Status: Failed`, surface the error, and hold — do **not** proceed to the approval gate until the page is published
- If `createConfluencePage` or `updateConfluencePage` fails: set `Status: Failed`, display the error and the Confluence URL that was attempted, and ask the user whether to retry or skip

---

## Required Tools

| Tool | Purpose |
|---|---|
| `mcp__claude_ai_Atlassian__searchConfluenceUsingCql` | Find the LLD parent page when `LldPageId` is not provided |
| `mcp__claude_ai_Atlassian__createConfluencePage` | Publish the implementation plan as a new child page |
| `mcp__claude_ai_Atlassian__updateConfluencePage` | Update the plan page during refinement rounds |
---
agent: confluence-publish-skill
attached_to: planning-agent
---

# Confluence Publish Skill

Publishes or updates the implementation plan as a child page of the Epic's LLD page in Confluence. Called by the Planning Agent after the developer has approved the plan at Gate 1 and confirmed they are ready to publish.

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
| `StoryId` | string | Yes | Jira story key (e.g. `US-123`) |
| `PlanContent` | string | Yes | Full implementation plan content to publish as the page body |
| `LldPageId` | string | No | Confluence page ID of the Epic's LLD page — use this when the Knowledge Agent found it. If null, the skill resolves it via CQL. |
| `SpaceKey` | string | Yes | Confluence space key (e.g. `CHH`) |
| `EpicKeywords` | string | No | Keywords from the Epic summary — used in the CQL fallback if `LldPageId` is null |
| `ExistingPageId` | string | No | If the page was already published in a prior round, pass its ID here to update rather than create |

---

## Page Title Format

```
{StoryId} - Implementation Plan
```

Example: `US-123 - Implementation Plan`

---

## Steps

### Step 1 — Resolve the LLD parent page

If `LldPageId` is provided, skip to Step 2.

If `LldPageId` is null, find the Epic's LLD page via CQL (try in order, stop at first result):

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
  parentId: <LldPageId or resolvedLldPageId>
  title: "<StoryId> - Implementation Plan"
  body: <PlanContent>
```

**If `ExistingPageId` is provided (refinement round — update existing page):**

```
mcp__claude_ai_Atlassian__updateConfluencePage
  pageId: <ExistingPageId>
  title: "<StoryId> - Implementation Plan"
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
| `ParentPageId` | The LLD page ID used as the parent — available to callers for audit or logging; not required downstream |
| `Error` | Populated only on `Failed` — include the reason and whether the LLD lookup failed or the page creation failed |

---

## Error Handling

- If the LLD page cannot be resolved and the user does not provide it manually: set `Status: Failed`, surface the error, and hold — do **not** proceed to the approval gate until the page is published
- If `createConfluencePage` or `updateConfluencePage` fails: set `Status: Failed`, display the error and the Confluence URL that was attempted, and ask the user whether to retry or skip

---

## Required Tools

| Tool | Purpose |
|---|---|
| `mcp__claude_ai_Atlassian__searchConfluenceUsingCql` | Find the LLD parent page when `LldPageId` is not provided |
| `mcp__claude_ai_Atlassian__createConfluencePage` | Publish the implementation plan as a new child page |
| `mcp__claude_ai_Atlassian__updateConfluencePage` | Update the plan page during refinement rounds |
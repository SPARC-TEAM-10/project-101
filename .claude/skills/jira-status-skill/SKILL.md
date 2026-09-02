---
agent: jira-status-skill
attached_to: coding-agent, unittest-agent
---

# Do not use this skill

Transitions a Jira issue to a target status. Called explicitly by agents at defined workflow gates — never auto-triggered.

---

## Trigger Points

| Caller | Status Target | When |
|---|---|---|
| Coding Agent | `In Progress` | Immediately before writing the first line of code |
| Test Agent | `Done` | After the test suite is fully green and the Test Quality Checklist passes |

---

## Input

| Parameter | Type | Required | Description |
|---|---|---|---|
| `TicketId` | string | Yes | Jira issue key (e.g. `US-123`) |
| `TargetStatus` | string | Yes | Human-readable status name (e.g. `In Progress`, `Done`) |

---

## Steps

### Step 1 — Discover available transitions

```
mcp__claude_ai_Atlassian__getTransitionsForJiraIssue
  issueKey: <TicketId>
```

Scan the response for a transition whose `name` matches `TargetStatus` (case-insensitive). Extract its `id`.

### Step 2 — Apply the transition

```
mcp__claude_ai_Atlassian__transitionJiraIssue
  issueKey: <TicketId>
  transitionId: <id from Step 1>
```

---

## Output

| Field | Description |
|---|---|
| `Status` | `Transitioned` \| `AlreadyInStatus` \| `Failed` |
| `TicketId` | The issue key that was acted on |
| `TargetStatus` | The status that was requested |
| `Error` | Populated only on `Failed` — the raw API error message |

---

## Error Handling

- No matching transition found → `Status: Failed`. Do not call `transitionJiraIssue`.
- API error → `Status: Failed`. Workflow continues — status failure must not block coding or PR creation.
- Already in target status → `Status: AlreadyInStatus`. Not an error.

---

## Required Tools

| Tool | Purpose |
|---|---|
| `mcp__claude_ai_Atlassian__getTransitionsForJiraIssue` | List available transitions |
| `mcp__claude_ai_Atlassian__transitionJiraIssue` | Apply the chosen transition |

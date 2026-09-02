---
agent: jira-comment-skill
attached_to: unittest-agent
---

# Do not use this skill

Adds a comment to a Jira issue. Called by the Test Agent after the ticket is transitioned to Done, to record the Confluence LLD link.

---

## Trigger Point

The Test Agent MUST invoke this skill after all of the following are true:
- Test suite is fully green
- Test Quality Checklist passes
- Jira ticket has been transitioned to Done (via Jira Status Skill)

---

## Input

| Parameter | Type | Required | Description |
|---|---|---|---|
| `TicketId` | string | Yes | Jira issue key (e.g. `US-123`) |
| `ConfluenceUrl` | string | No | Confluence LLD page URL — omit if not available |
| `ConfluenceTitle` | string | No | Human-readable title of the LLD page |
| `AdditionalNotes` | string | No | Any extra context to append to the comment body |

---

## Comment Body Format

```
✅ Implementation complete and tests passed.

LLD: [<ConfluenceTitle>](<ConfluenceUrl>)

<AdditionalNotes if provided>
```

If `ConfluenceUrl` is absent, omit the LLD line entirely.

---

## Steps

```
mcp__claude_ai_Atlassian__addCommentToJiraIssue
  issueKey: <TicketId>
  comment: <formatted comment body>
```

---

## Output

| Field | Description |
|---|---|
| `Status` | `Added` \| `Failed` |
| `TicketId` | The issue key that was commented on |
| `Error` | Populated only on `Failed` |

---

## Error Handling

- Failure must not block the GitHub PR Skill — the workflow continues.

---

## Required Tools

| Tool | Purpose |
|---|---|
| `mcp__claude_ai_Atlassian__addCommentToJiraIssue` | Post the comment to the Jira issue |

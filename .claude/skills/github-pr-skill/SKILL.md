---
agent: github-pr-skill
attached_to: unittest-agent, coding-agent
---

# GitHub PR Skill

Pushes a branch to the remote and opens a pull request on GitHub using the `gh` CLI. **This skill must never be invoked before the developer has seen the full PR draft and replied `Approved` in the conversation.** This rule applies to every caller — Unittest Agent (main feature PR) and Coding Agent (shared repo PR).

---

## Universal Rules (apply to every caller)

- **Always ask before raising.** The calling agent must present the full PR draft (title, summary, test plan) in conversation and wait for the developer to reply `PRApproved` (case-insensitive). Any other reply → refine the draft and ask again. No limit on rounds.
- **Always share the PR URL.** After `gh pr create` succeeds, the calling agent must immediately post the full `PrUrl` in the conversation. Never silently discard it.

---

## Trigger Point

The calling agent MUST invoke this skill only after all of the following are true:
- Any prerequisite checks specific to the caller have passed (e.g., green test suite for Unittest Agent; clean `dotnet build` for Coding Agent)
- **Developer has replied `PRApproved` to the PR draft presented in the conversation**

---

## Input

| Parameter | Type | Required | Description |
|---|---|---|---|
| `BranchName` | string | Yes | The feature branch to push |
| `BaseBranch` | string | Yes | Target branch for the PR — from project config (default `main`) |
| `TicketId` | string | No | Jira ticket ID prepended to the PR title |
| `Title` | string | Yes | Short PR title ≤ 70 characters |
| `Summary` | string | Yes | 1–3 bullet points describing what the PR does |
| `TestPlan` | string | Yes | Bulleted checklist of what was tested |
| `Coverage` | string | No | Coverage table (layer, actual % lines, actual % branches, threshold, ✅/❌) — included in PR body when provided |
| `ConfluenceUrl` | string | No | Confluence LLD page URL — included in PR body if present |
| `JiraBaseUrl` | string | No | Atlassian base URL (e.g. `https://yourorg.atlassian.net`) — read from the `atlassianBaseUrl` field in `project_config.md` (written by the Startup Agent); omit the Jira line from the PR body if the field is null or absent |

---

## How to Invoke

```bash
git push -u origin <BranchName>

gh pr create \
  --title "<TicketId> <Title>" \
  --base "<BaseBranch>" \
  --body "$(cat <<'EOF'
## Summary
<Summary>

## Test plan
<TestPlan>

## Coverage
<Coverage table — omit this entire section if Coverage parameter was not provided>

## References
<ConfluenceUrl line — omit if ConfluenceUrl not provided: "LLD: <ConfluenceUrl>">
<Jira line — omit if TicketId not provided: "Jira: <JiraBaseUrl>/browse/<TicketId>" where JiraBaseUrl is read from project_config.md; omit entire line if not configured>

🤖 Generated with [Claude Code](https://claude.ai/claude-code)
EOF
)"
```

---

## Output

| Field | Description |
|---|---|
| `PrUrl` | The full GitHub PR URL |
| `Status` | `Created` \| `Failed` |
| `Error` | Populated only on `Failed` |

---

## Error Handling

- `git push` fails → `Status: Failed`, do not attempt `gh pr create`. Report the push error to the developer.
- `gh pr create` fails → `Status: Failed`. Branch is already pushed — do not re-push on retry.
- PR already exists for this branch → report the existing PR URL from the error message; set `Status: Created`.

---

## Required Tools

| Tool | Purpose |
|---|---|
| Bash | Run `git push` and `gh pr create` |
---
agent: notify-skill
attached_to: all-agents
---

# Notify Skill

Sends a cross-platform system-level desktop toast **and** a phone push notification summarising an agent's outcome. Each agent invokes this skill at the end of its Behavior steps.

---

## Trigger Points

| Event | Status to pass |
|---|---|
| Knowledge Agent completes or is blocked | `Completed` or `Blocked` |
| Planning Agent produces an implementation plan (awaiting approval) | `Completed` |
| Coding Agent finishes implementation | `Completed` or `Failed` |
| Code Review Agent returns Go / No-Go | `Completed` or `Blocked` |
| Unittest Agent achieves green suite or is blocked | `Completed` or `Blocked` |
| Any agent errors out or cannot proceed | `Failed` |

---

## Input

| Parameter | Type | Description |
|---|---|---|
| `AgentName` | string | Canonical name of the invoking agent (see list below) |
| `Status` | enum | `Completed` \| `Blocked` \| `Failed` |
| `Summary` | string | 1–2 sentence plain-English summary — no markdown, no bullet points |

### Canonical Agent Names
- `Startup Agent`
- `Knowledge Agent`
- `Codebase Analysis Agent` (backend only)
- `Planning Agent`
- `Coding Agent`
- `Code Review Agent`
- `Unittest Agent`
- `PR Agent`

---

## How to Invoke

### Step 1 — System Toast (cross-platform)

The script lives at `scripts/notify.sh` in the project root. Run it via Bash:

```bash
bash scripts/notify.sh "<AgentName>" "<Status>" "<Summary>"
```

The script detects the OS automatically and sends the appropriate toast:

| OS | Mechanism | Requirement |
|---|---|---|
| **Windows** | PowerShell `NotifyIcon` balloon tip | None — built-in |
| **macOS** | `osascript` notification centre | None — built-in |
| **Linux** | `notify-send` | `libnotify-bin` (`apt install libnotify-bin`) |

> **If the script is missing:** The file must exist at `scripts/notify.sh` relative to the project root. If it is absent, Step 1 will fail — proceed to Step 2 (PushNotification) and note the missing script to the developer once (not on every notification call).

If the script exits non-zero, log the error but **do not block** — proceed to Step 2.

### Step 2 — Phone Push

After the system toast (regardless of its outcome), call `PushNotification`:

```
message: "<AgentName> — <Status>: <Summary>"
status: "proactive"
```

Keep the combined message under 200 characters. If the call fails, log and continue.

---

## Summary Authoring Rules

1. **One or two sentences maximum.**
2. **Past tense** — state what was done, not what will happen.
3. **For `Completed`:** state the key output (e.g., "Implementation plan ready for review. 6 files planned across 3 layers.").
4. **For `Blocked`:** state the blocker and what is needed (e.g., "Confluence unreachable. Falling back to local context.").
5. **For `Failed`:** state what failed and the error in plain terms (e.g., "Build error in OrderService.cs line 42.").
6. **No markdown, no bullet points, no quotes** — plain text only.

### Examples

```
Knowledge Agent — Completed: Jira ticket fetched, 3 Confluence pages loaded, 2 gaps noted.
Planning Agent — Completed: Implementation plan ready for review. 6 files planned across 3 layers.
Coding Agent — Completed: 4 files created, 2 modified. Branch US-123-add-user-profile.
Code Review Agent — Blocked: 2 critical findings in OrderService.cs. Rework required.
Unittest Agent — Completed: 24 tests passing, 91% coverage. PR opened.
```

---

## Error Handling

- Script exits non-zero → log the error, proceed to Step 2
- `PushNotification` fails → log the error, continue the workflow
- Neither failure blocks the workflow — notifications are informational only

---

## Required Tools

| Tool | Purpose |
|---|---|
| Bash | Run `scripts/notify.sh` for system-level desktop toast |
| `PushNotification` | Send terminal + phone push notification |

---
name: Project Configuration
description: User-confirmed project configuration — feature branch prefix, git base branch, and Atlassian details
type: project
---

## Project Configuration

| Setting | Value |
|---|---|
| Developer Name | SPARC Team 10 |
| Atlassian Base URL | https://experionglobal.atlassian.net |
| Confluence Space Key | ~7120202ff158e18c6b4bfe8ed2513c67e775d7 (personal space — see note below) |
| Feature Branch Prefix | feature/ |
| Branch Naming Convention | feature/{jira-ticket-id}-{short-description} (bugfix/ for Bug tickets or `/dev` runs) |
| Git Base Branch | main |
| Feature Branch | feature/CHH-8-mobile-entry-otp |

## How to Apply

- **Developer Name** — used by the Planning Agent as the `Author` field in every implementation plan; sourced from Atlassian authentication at startup
- **Atlassian Base URL** — used by the GitHub PR Skill to generate the Jira issue link in PR bodies; null means the Jira line is omitted from PR bodies
- **Confluence Space Key** — used by the Knowledge Agent and Confluence Publish Skill as the `space` filter in CQL fallback searches. **Note:** this project's Confluence content (PRD, CHH-F01 story page) lives in a personal space (`~7120202ff158e18c6b4bfe8ed2513c67e775d7`, owned by SPARC Team 10), not a team space keyed `CHH` — CQL searches scoped with `space = "CHH"` will find nothing; use this key as-is or omit the space filter and rely on `ancestor`/keyword scoping instead.
- **Feature Branch Prefix** — all feature branches must start with this prefix; used by the Git Branch Skill when creating branches for every task
- **Branch Naming Convention** — the full pattern for feature branch names
- **Git Base Branch** — used by the Git Branch Skill as the branch-from target when creating feature branches; confirmed to exist locally and on `origin` (github.com/SPARC-TEAM-10/hackathon-agent-scaffold)
- **Feature Branch** — the active feature branch for the current task; written by the Git Branch Skill after branch creation; read by the PR Agent and Unittest Agent. Overwritten each time the Git Branch Skill runs for a new task.

---
name: run-backend-tests
description: Run the backend test suite. Use before transitioning a backend ticket to Review, and whenever verifying a backend change didn't break existing tests.
---

# Run backend tests

> Fill in the real command once the backend stack is locked (see
> `backend/CLAUDE.md`). This skill exists so every agent — coder, reviewer,
> tester — runs tests the *same* way instead of each guessing a command.

## Steps

1. `cd backend`
2. Run the test command: `[PLACEHOLDER — e.g. pytest -q, npm test, go test ./...]`
3. Report pass/fail and, on failure, the specific failing test names/files —
   don't just report "tests failed."

## When to use

- Coder agent: before transitioning a ticket to "Review"
- Tester agent: as the primary verification step for a backend module
- Reviewer agent: to confirm a coder's "tests pass" claim before approving

## Notes

- If the suite needs setup (env vars, a test DB, fixtures), document the
  exact steps here once known — don't leave agents to rediscover it.

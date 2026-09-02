---
name: run-frontend-tests
description: Run the frontend test suite. Use before transitioning a frontend ticket to Review, and whenever verifying a frontend change didn't break existing tests.
---

# Run frontend tests

Frontend stack is locked (see `frontend/CLAUDE.md`): React + TypeScript +
Vite / Vitest + Testing Library. This skill exists so every agent — coder,
reviewer, tester — runs tests the *same* way instead of each guessing a
command.

## Steps

1. `cd frontend`
2. Run the test command: `npm run test -- --run` (Vitest in CI/non-watch mode)
3. Report pass/fail and, on failure, the specific failing test names/files —
   don't just report "tests failed."

## When to use

- Coder agent: before transitioning a ticket to "Review"
- Tester agent: as the primary verification step for a frontend module
- Reviewer agent: to confirm a coder's "tests pass" claim before approving

## Notes

- If the suite needs setup (env vars, mock server, browser deps), document
  the exact steps here once known — don't leave agents to rediscover it.

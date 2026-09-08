import { createContext, useContext, useMemo, useState, type ReactNode } from "react";

// Matches backend/src/Chh.Domain/Constants/RoleConstants.cs — Hospital/NGO/Admin aren't
// resolvable yet (no Facility/Admin entities exist), so those roles aren't issued.
export type Role = "Individual" | "Guest";

export interface AuthSession {
  token: string;
  role: Role;
  expiresAtUtc: string;
}

interface AuthContextValue {
  session: AuthSession | null;
  setSession: (session: AuthSession) => void;
  clearSession: () => void;
}

const AuthContext = createContext<AuthContextValue | null>(null);

// Persisted in localStorage (by explicit product decision, 2026-09-07) so a page refresh doesn't
// drop the session — the JWT now lives 24h (JwtOptions.AccessTokenLifetimeMinutes), long enough
// that losing it on every refresh would be a real annoyance. This is a deliberate reversal of an
// earlier "in-memory only, XSS exposure" decision documented in frontend/CLAUDE.md's Auth row
// (updated alongside this file) — an XSS payload can read localStorage synchronously, same as it
// could read this module's in-memory state via the same execution context, so the practical
// difference is mainly how long a stolen token stays valid, not whether it's stealable at all.
const STORAGE_KEY = "chh.auth.session";

function readStoredSession(): AuthSession | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) {
      return null;
    }
    const parsed = JSON.parse(raw) as AuthSession;
    if (!parsed.token || !parsed.role || !parsed.expiresAtUtc) {
      return null;
    }
    return parsed;
  } catch {
    // Private browsing, storage disabled, or corrupted JSON — fall back to a logged-out session
    // rather than throwing during app startup.
    return null;
  }
}

function writeStoredSession(session: AuthSession | null): void {
  try {
    if (session) {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
    } else {
      localStorage.removeItem(STORAGE_KEY);
    }
  } catch {
    // Storage unavailable (e.g. private browsing quota) — the session still works for this tab
    // via React state, it just won't survive a refresh. Not worth surfacing to the user.
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSessionState] = useState<AuthSession | null>(readStoredSession);

  const value = useMemo<AuthContextValue>(
    () => ({
      session,
      setSession: (next: AuthSession) => {
        writeStoredSession(next);
        setSessionState(next);
      },
      clearSession: () => {
        writeStoredSession(null);
        setSessionState(null);
      },
    }),
    [session],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return ctx;
}

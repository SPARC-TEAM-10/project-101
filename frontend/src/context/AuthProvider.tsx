import { createContext, useContext, useMemo, useState, type ReactNode } from "react";

// Placeholder until the backend's role enum is confirmed (see CHH-8 plan §11 Open Questions).
// CHH-10 (Role-Based Redirection) should replace this with the real role union.
export type Role = string;

export interface AuthSession {
  token: string;
  role: Role;
}

interface AuthContextValue {
  session: AuthSession | null;
  setSession: (session: AuthSession) => void;
  clearSession: () => void;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSessionState] = useState<AuthSession | null>(null);

  const value = useMemo<AuthContextValue>(
    () => ({
      session,
      setSession: setSessionState,
      clearSession: () => setSessionState(null),
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

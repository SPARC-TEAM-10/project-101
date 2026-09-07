import { createContext, useContext, useMemo, useState, type ReactNode } from "react";

// Matches backend/src/Chh.Domain/Constants/RoleConstants.cs — Hospital/NGO aren't resolvable
// yet (no Facility-owner account concept exists), so those two roles aren't issued. SystemAdmin
// is issued via the CHH-F07 interim shortcut (a hardcoded mobile number) — see RoleConstants.cs.
export type Role = "Individual" | "Guest" | "SystemAdmin";

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

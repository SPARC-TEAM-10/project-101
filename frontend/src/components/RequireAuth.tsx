import type { ReactNode } from "react";
import { Navigate } from "react-router-dom";

import { useAuth, type Role } from "../context/AuthProvider";

interface RequireAuthProps {
  children: ReactNode;
  roles?: Role[];
}

export function RequireAuth({ children, roles }: RequireAuthProps) {
  const { session } = useAuth();

  if (!session) {
    return <Navigate to="/login" replace />;
  }

  // AC3 (CHH-10): a session past its 1-hour token lifetime is treated as logged-out.
  if (new Date(session.expiresAtUtc).getTime() <= Date.now()) {
    return <Navigate to="/login" replace />;
  }

  if (roles && !roles.includes(session.role)) {
    return <Navigate to="/login" replace />;
  }

  return <>{children}</>;
}

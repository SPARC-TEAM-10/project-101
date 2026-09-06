import type { ReactNode } from "react";
import { Navigate } from "react-router-dom";

import { useAuth, type Role } from "../context/AuthProvider";

interface RequireAuthProps {
  children: ReactNode;
  roles?: Role[];
}

// Not wired to any route in CHH-8 — exported for CHH-10 (Role-Based Redirection)
// to attach to real dashboard routes once JWT/role data exists.
export function RequireAuth({ children, roles }: RequireAuthProps) {
  const { session } = useAuth();

  if (!session) {
    return <Navigate to="/login" replace />;
  }

  if (roles && !roles.includes(session.role)) {
    return <Navigate to="/login" replace />;
  }

  return <>{children}</>;
}

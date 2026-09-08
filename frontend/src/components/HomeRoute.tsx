import { Navigate } from "react-router-dom";

import { useAuth } from "../context/AuthProvider";
import { LandingPage } from "../pages/LandingPage";
import { DASHBOARD_ROUTE_BY_ROLE } from "../pages/auth/RoleRedirectPage";

/**
 * The root route ("/"). A logged-in user should never see the marketing landing page again —
 * their dashboard is effectively "home" once authenticated. Only an unauthenticated (or expired)
 * session sees the real LandingPage.
 */
export function HomeRoute() {
  const { session } = useAuth();

  if (session && new Date(session.expiresAtUtc).getTime() > Date.now()) {
    return <Navigate to={DASHBOARD_ROUTE_BY_ROLE[session.role]} replace />;
  }

  return <LandingPage />;
}

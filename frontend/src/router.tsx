import { createBrowserRouter } from "react-router-dom";

import { LandingPage } from "./pages/LandingPage";
import { MobileEntryPage } from "./pages/auth/MobileEntryPage";
import { OtpVerificationPage } from "./pages/auth/OtpVerificationPage";
import { RoleRedirectPage } from "./pages/auth/RoleRedirectPage";
import { GuestPlaceholderPage } from "./pages/GuestPlaceholderPage";
import { IndividualDashboardStubPage } from "./pages/dashboard/IndividualDashboardStubPage";
import { GuestDashboardStubPage } from "./pages/dashboard/GuestDashboardStubPage";
import { NewUserGuestDecisionPage } from "./pages/onboarding/NewUserGuestDecisionPage";
import { RegisterStubPage } from "./pages/onboarding/RegisterStubPage";
import { RequireAuth } from "./components/RequireAuth";

export const router = createBrowserRouter([
  { path: "/", element: <LandingPage /> },
  { path: "/login", element: <MobileEntryPage /> },
  { path: "/otp-verify", element: <OtpVerificationPage /> },
  { path: "/redirecting", element: <RoleRedirectPage /> },
  {
    path: "/dashboard/individual",
    element: (
      <RequireAuth roles={["Individual"]}>
        <IndividualDashboardStubPage />
      </RequireAuth>
    ),
  },
  {
    path: "/dashboard/guest",
    element: (
      <RequireAuth roles={["Guest"]}>
        <GuestDashboardStubPage />
      </RequireAuth>
    ),
  },
  {
    path: "/welcome",
    element: (
      <RequireAuth roles={["Guest"]}>
        <NewUserGuestDecisionPage />
      </RequireAuth>
    ),
  },
  {
    path: "/register",
    element: (
      <RequireAuth roles={["Guest"]}>
        <RegisterStubPage />
      </RequireAuth>
    ),
  },
  { path: "/guest", element: <GuestPlaceholderPage /> },
]);

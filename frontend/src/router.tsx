import { createBrowserRouter } from "react-router-dom";

import { HomeRoute } from "./components/HomeRoute";
import { RequireAuth } from "./components/RequireAuth";
import { MobileEntryPage } from "./pages/auth/MobileEntryPage";
import { OtpVerificationPage } from "./pages/auth/OtpVerificationPage";
import { RoleRedirectPage } from "./pages/auth/RoleRedirectPage";
import { GuestPlaceholderPage } from "./pages/GuestPlaceholderPage";
import { BloodRequestFormModal } from "./pages/bloodRequest/BloodRequestFormModal";
import { RequesterMatchDashboardPage } from "./pages/bloodRequest/RequesterMatchDashboardPage";
import { FacilityRegistrationPage } from "./pages/facility/FacilityRegistrationPage";
import { IndividualDashboardPage } from "./pages/dashboard/IndividualDashboardPage";
import { GuestDashboardStubPage } from "./pages/dashboard/GuestDashboardStubPage";
import { FacilityDashboardPage } from "./pages/dashboard/FacilityDashboardPage";
import { ProfilePage } from "./pages/profile/ProfilePage";
import { NotificationsPage } from "./pages/notifications/NotificationsPage";
import { NewUserGuestDecisionPage } from "./pages/onboarding/NewUserGuestDecisionPage";
import { RoleSelectionPage } from "./pages/onboarding/RoleSelectionPage";
import { RegisterStubPage } from "./pages/onboarding/RegisterStubPage";

export const router = createBrowserRouter([
  { path: "/", element: <HomeRoute /> },
  { path: "/login", element: <MobileEntryPage /> },
  { path: "/otp-verify", element: <OtpVerificationPage /> },
  { path: "/redirecting", element: <RoleRedirectPage /> },
  {
    path: "/dashboard/individual",
    element: (
      <RequireAuth roles={["Individual"]}>
        <IndividualDashboardPage />
      </RequireAuth>
    ),
  },
  {
    path: "/profile",
    element: (
      <RequireAuth roles={["Individual"]}>
        <ProfilePage />
      </RequireAuth>
    ),
  },
  {
    path: "/notifications",
    element: (
      <RequireAuth roles={["Individual"]}>
        <NotificationsPage />
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
        <RoleSelectionPage />
      </RequireAuth>
    ),
  },
  {
    path: "/register/individual",
    element: (
      <RequireAuth roles={["Guest"]}>
        <RegisterStubPage />
      </RequireAuth>
    ),
  },
  { path: "/guest", element: <GuestPlaceholderPage /> },
  // Intentionally unguarded: registering is what makes a mobile number resolve to the
  // Hospital/Ngo role in the first place (CHH-10) — gating this page behind that role would
  // make a facility's first-ever registration impossible.
  { path: "/facility/register", element: <FacilityRegistrationPage /> },
  {
    path: "/dashboard/facility",
    element: (
      <RequireAuth roles={["Hospital", "Ngo"]}>
        <FacilityDashboardPage />
      </RequireAuth>
    ),
  },
  {
    path: "/blood-requests/new",
    element: (
      <RequireAuth>
        <BloodRequestFormModal />
      </RequireAuth>
    ),
  },
  {
    // Unrestricted role (both Guest and Individual can create a blood request and need to view
    // its match status — CHH-36's guest-vs-registered donor-list visibility rule is applied
    // server-side, not gated at the route).
    path: "/blood-requests/:id/matches",
    element: (
      <RequireAuth>
        <RequesterMatchDashboardPage />
      </RequireAuth>
    ),
  },
]);

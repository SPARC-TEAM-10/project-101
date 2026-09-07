import { createBrowserRouter } from "react-router-dom";

import { RequireAuth } from "./components/RequireAuth";
import { LandingPage } from "./pages/LandingPage";
import { MobileEntryPage } from "./pages/auth/MobileEntryPage";
import { OtpVerificationPage } from "./pages/auth/OtpVerificationPage";
import { RoleRedirectPage } from "./pages/auth/RoleRedirectPage";
import { GuestPlaceholderPage } from "./pages/GuestPlaceholderPage";
import { BloodRequestFormModal } from "./pages/bloodRequest/BloodRequestFormModal";
import { FacilityRegistrationPage } from "./pages/facility/FacilityRegistrationPage";
import { IndividualDashboardPage } from "./pages/dashboard/IndividualDashboardPage";
import { GuestDashboardStubPage } from "./pages/dashboard/GuestDashboardStubPage";

export const router = createBrowserRouter([
  { path: "/", element: <LandingPage /> },
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
    path: "/dashboard/guest",
    element: (
      <RequireAuth roles={["Guest"]}>
        <GuestDashboardStubPage />
      </RequireAuth>
    ),
  },
  { path: "/guest", element: <GuestPlaceholderPage /> },
  // Unguarded for now — Hospital/NGO role isn't issued yet, see AuthProvider.tsx and
  // CHH-78's Implementation Plan §8 for the tracked follow-up to add a real RequireAuth gate.
  { path: "/facility/register", element: <FacilityRegistrationPage /> },
  {
    path: "/blood-requests/new",
    element: (
      <RequireAuth>
        <BloodRequestFormModal />
      </RequireAuth>
    ),
  },
]);

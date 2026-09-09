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
import { CreateEventPage } from "./pages/events/CreateEventPage";
import { EventDiscoveryPage } from "./pages/events/EventDiscoveryPage";
import { EventDetailPage } from "./pages/events/EventDetailPage";
import { MyEventsPage } from "./pages/events/MyEventsPage";
import { EventManagePage } from "./pages/events/EventManagePage";
import { MarkAttendancePage } from "./pages/events/MarkAttendancePage";
import { IndividualDashboardPage } from "./pages/dashboard/IndividualDashboardPage";
import { GuestDashboardStubPage } from "./pages/dashboard/GuestDashboardStubPage";
import { PendingVerificationsPage } from "./pages/admin/PendingVerificationsPage";
import { UsersPage } from "./pages/admin/UsersPage";
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
  {
    path: "/admin",
    element: (
      <RequireAuth roles={["SystemAdmin"]}>
        <PendingVerificationsPage />
      </RequireAuth>
    ),
  },
  {
    path: "/admin/users",
    element: (
      <RequireAuth roles={["SystemAdmin"]}>
        <UsersPage />
      </RequireAuth>
    ),
  },
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
  {
    // Facility-verification gate is enforced server-side (403 if not Verified) — this route only
    // checks the role, matching the pattern established for /dashboard/facility (CHH-28).
    path: "/events/new",
    element: (
      <RequireAuth roles={["Hospital", "Ngo"]}>
        <CreateEventPage />
      </RequireAuth>
    ),
  },
  {
    // CHH-41's manage-event entry point — ownership itself is enforced server-side (403 if the
    // caller's facility doesn't own the event), matching /events/new's pattern.
    path: "/events/mine",
    element: (
      <RequireAuth roles={["Hospital", "Ngo"]}>
        <MyEventsPage />
      </RequireAuth>
    ),
  },
  {
    path: "/events/:id/manage",
    element: (
      <RequireAuth roles={["Hospital", "Ngo"]}>
        <EventManagePage />
      </RequireAuth>
    ),
  },
  {
    // CHH-44's manual attendance marking — ownership is enforced server-side (403 if the caller's
    // facility doesn't own the event), matching /events/:id/manage's pattern.
    path: "/events/:id/attendance",
    element: (
      <RequireAuth roles={["Hospital", "Ngo"]}>
        <MarkAttendancePage />
      </RequireAuth>
    ),
  },
  {
    // Spec's User Role is explicitly "Individual" (US-CHH-005-02) — Hospital/Ngo/Guest aren't
    // discovery's intended audience per the story.
    path: "/events",
    element: (
      <RequireAuth roles={["Individual"]}>
        <EventDiscoveryPage />
      </RequireAuth>
    ),
  },
  {
    // Detail is open to any authenticated role (GET /events/{id} has no role gate) — RSVP/Cancel
    // actions themselves are gated to Individual server-side and hidden client-side for other
    // roles (CHH-40/US-CHH-005-03).
    path: "/events/:id",
    element: (
      <RequireAuth>
        <EventDetailPage />
      </RequireAuth>
    ),
  },
]);

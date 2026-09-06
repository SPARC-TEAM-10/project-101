import { createBrowserRouter } from "react-router-dom";

import { RequireAuth } from "./components/RequireAuth";
import { LandingPage } from "./pages/LandingPage";
import { MobileEntryPage } from "./pages/auth/MobileEntryPage";
import { OtpVerificationPage } from "./pages/auth/OtpVerificationPage";
import { VerifiedStubPage } from "./pages/auth/VerifiedStubPage";
import { GuestPlaceholderPage } from "./pages/GuestPlaceholderPage";
import { BloodRequestFormModal } from "./pages/bloodRequest/BloodRequestFormModal";

export const router = createBrowserRouter([
  { path: "/", element: <LandingPage /> },
  { path: "/login", element: <MobileEntryPage /> },
  { path: "/otp-verify", element: <OtpVerificationPage /> },
  { path: "/verified", element: <VerifiedStubPage /> },
  { path: "/guest", element: <GuestPlaceholderPage /> },
  {
    path: "/blood-requests/new",
    element: (
      <RequireAuth>
        <BloodRequestFormModal />
      </RequireAuth>
    ),
  },
]);

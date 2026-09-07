import { useNavigate } from "react-router-dom";

import { AuthSplitLayout } from "../../components/AuthSplitLayout";

// Role Selection — the missing step between "Create Account" (CHH-11) and a registration form.
// CHH-11 AC3 only ever named CHH-F02 (Individual) as the destination, but the PRD also requires
// Facility registration (CHH-F03) for Hospital/NGO, and nothing chose between them. Proposed and
// designed earlier (design/drafts/CHH-11-role-select/); built now on direct request. No Jira
// story of its own yet — CHH-11's AC3 should be reworded (or a new story cut) to reflect this.
export function RoleSelectionPage() {
  const navigate = useNavigate();

  return (
    <AuthSplitLayout imageSrc="/images/auth-otp-verify.png" imageAlt="">
      <div className="relative flex min-h-screen flex-col bg-sand px-7 pt-16 font-sans text-ink">
        <a
          href="/welcome"
          onClick={(e) => {
            e.preventDefault();
            navigate("/welcome");
          }}
          className="absolute left-5 top-5 z-10 inline-flex items-center gap-1.5 text-sm font-semibold text-ink-2 underline underline-offset-2 hover:text-ink"
        >
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
            <path d="m15 18-6-6 6-6" />
          </svg>
          Back
        </a>

        <h1 className="mb-2 text-[26px] font-extrabold tracking-tight">Choose an account type</h1>
        <p className="mb-10 max-w-[34ch] text-[14.5px] leading-relaxed text-ink-2">
          This decides which details we ask for next.
        </p>

        <div className="flex max-w-[460px] flex-col gap-3">
          <button
            type="button"
            onClick={() => navigate("/register/individual")}
            className="flex items-start gap-3.5 rounded-md border border-line bg-cream p-4 text-left shadow-[var(--e1)]"
          >
            <span className="flex h-11 w-11 flex-none items-center justify-center rounded-full bg-clay-tint text-clay-deep">
              <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.75} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                <circle cx="12" cy="8" r="3.6" />
                <path d="M4.8 20c.6-3.7 3.6-5.6 7.2-5.6s6.6 1.9 7.2 5.6" />
              </svg>
            </span>
            <span className="flex min-w-0 flex-1 flex-col gap-0.5">
              <b className="text-[17px] font-bold tracking-tight">Individual</b>
              <span className="text-[13px] text-ink-2">Donate or request blood, and get alerts near you.</span>
            </span>
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.75} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true" className="mt-1.5 flex-none text-ink-3">
              <path d="m9 6 6 6-6 6" />
            </svg>
          </button>

          <button
            type="button"
            onClick={() => navigate("/facility/register")}
            className="flex items-start gap-3.5 rounded-md border border-line bg-cream p-4 text-left shadow-[var(--e1)]"
          >
            <span className="flex h-11 w-11 flex-none items-center justify-center rounded-md bg-clay-tint text-clay-deep">
              <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.75} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                <path d="M4 20V7.5l8-3.5 8 3.5V20" />
                <path d="M12 9v5M9.5 11.5h5" />
                <path d="M4 20h16" />
              </svg>
            </span>
            <span className="flex min-w-0 flex-1 flex-col gap-0.5">
              <b className="text-[17px] font-bold tracking-tight">Hospital</b>
              <span className="text-[13px] text-ink-2">Manage blood inventory and answer requests.</span>
              <span className="mt-0.5 flex items-start gap-1.5 text-[12.5px] leading-tight text-ink-3">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.9} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true" className="mt-0.5 flex-none">
                  <path d="M12 3.5 5 6v5.5c0 4 3 7.4 7 9 4-1.6 7-5 7-9V6l-7-2.5Z" />
                </svg>
                Needs a licence. An admin verifies it before you go live.
              </span>
            </span>
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.75} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true" className="mt-1.5 flex-none text-ink-3">
              <path d="m9 6 6 6-6 6" />
            </svg>
          </button>

          <button
            type="button"
            onClick={() => navigate("/facility/register")}
            className="flex items-start gap-3.5 rounded-md border border-line bg-cream p-4 text-left shadow-[var(--e1)]"
          >
            <span className="flex h-11 w-11 flex-none items-center justify-center rounded-md bg-clay-tint text-clay-deep">
              <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.75} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                <circle cx="9" cy="8.5" r="3" />
                <circle cx="16.5" cy="10" r="2.4" />
                <path d="M3.5 19c.5-3.2 2.9-4.9 5.5-4.9 2.2 0 4.2 1.2 5.1 3.4" />
                <path d="M14.6 19h5.9c-.2-2.3-1.7-3.6-3.7-3.6" />
              </svg>
            </span>
            <span className="flex min-w-0 flex-1 flex-col gap-0.5">
              <b className="text-[17px] font-bold tracking-tight">NGO</b>
              <span className="text-[13px] text-ink-2">Run donation drives and wellness events.</span>
              <span className="mt-0.5 flex items-start gap-1.5 text-[12.5px] leading-tight text-ink-3">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.9} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true" className="mt-0.5 flex-none">
                  <path d="M12 3.5 5 6v5.5c0 4 3 7.4 7 9 4-1.6 7-5 7-9V6l-7-2.5Z" />
                </svg>
                Needs registration documents. An admin verifies them.
              </span>
            </span>
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.75} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true" className="mt-1.5 flex-none text-ink-3">
              <path d="m9 6 6 6-6 6" />
            </svg>
          </button>
        </div>
      </div>
    </AuthSplitLayout>
  );
}

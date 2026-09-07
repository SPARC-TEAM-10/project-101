import { useNavigate } from "react-router-dom";

// "Welcome" choice screen (CHH-11) — shown when role === "Guest", i.e. a verified mobile
// number with no completed registration (backend's RoleConstants.cs definition maps exactly
// to CHH-11 AC1's "not currently in the database").
export function NewUserGuestDecisionPage() {
  const navigate = useNavigate();

  return (
    <div className="flex min-h-screen flex-col bg-sand px-7 pt-16 font-sans text-ink">
      <div className="mx-auto mb-5 flex h-14 w-14 items-center justify-center rounded-full bg-clay-tint text-clay-deep">
        <svg width="26" height="26" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.6} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
          <circle cx="9" cy="8.5" r="3.2" />
          <path d="M3.2 19.5a5.8 5.8 0 0 1 11.6 0" />
          <path d="M16 5.6a3.2 3.2 0 0 1 0 5.9M17.5 14.4a5.8 5.8 0 0 1 3.3 5.1" />
        </svg>
      </div>
      <h1 className="mb-2 text-center text-[26px] font-extrabold tracking-tight">Welcome</h1>
      <p className="mb-10 text-center text-[14.5px] leading-relaxed text-ink-2">
        We don&apos;t recognize this number yet. How would you like to continue?
      </p>

      <div className="flex flex-col gap-4">
        <button
          type="button"
          onClick={() => navigate("/register")}
          className="flex flex-col gap-1.5 rounded-md border-[1.5px] border-transparent bg-clay p-6 text-left text-white shadow-[var(--e1)]"
        >
          <span className="flex items-center gap-3">
            <span className="flex h-9.5 w-9.5 flex-none items-center justify-center rounded-full bg-white/[.18]">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.75} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                <circle cx="12" cy="8.5" r="3.6" />
                <path d="M4.8 20a7.2 7.2 0 0 1 14.4 0" />
              </svg>
            </span>
            <b className="text-[17px] font-extrabold tracking-tight">Create Account</b>
          </span>
          <span className="text-[13px] opacity-85">Full access to donate, request and track responses.</span>
        </button>

        <button
          type="button"
          onClick={() => navigate("/dashboard/guest")}
          className="flex flex-col gap-1.5 rounded-md border-[1.5px] border-line-strong bg-transparent p-6 text-left text-ink"
        >
          <span className="flex items-center gap-3">
            <span className="flex h-9.5 w-9.5 flex-none items-center justify-center rounded-full bg-blood-tint text-blood-deep">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.75} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                <path d="M12 3.2c3.4 4 6 6.9 6 10a6 6 0 0 1-12 0c0-3.1 2.6-6 6-10Z" />
              </svg>
            </span>
            <b className="text-[17px] font-extrabold tracking-tight">Emergency Guest Access</b>
          </span>
          <span className="text-[13px] opacity-85">Search &amp; request only, no account needed.</span>
        </button>
      </div>
    </div>
  );
}

import { useNavigate } from "react-router-dom";

/**
 * Notification center (CHH-81 follow-up). CHH-34 (Real-Time Donor Notification) isn't built
 * yet — this page has nothing to list, but exists as a real destination for the bell icon
 * instead of doing nothing, and is where CHH-34's feed will render once it lands.
 */
export function NotificationsPage() {
  const navigate = useNavigate();

  return (
    <div className="min-h-screen bg-sand font-sans text-ink">
      <div className="flex h-[58px] items-center gap-3 border-b border-line bg-cream px-4 lg:px-8">
        <button
          type="button"
          onClick={() => navigate("/dashboard/individual")}
          aria-label="Back to dashboard"
          className="flex h-9 w-9 items-center justify-center rounded-full text-ink-2 transition-colors hover:bg-sand-2 hover:text-ink"
        >
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.9} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
            <path d="M19 12H5M11 6l-6 6 6 6" />
          </svg>
        </button>
        <b className="text-[15px] font-bold">Notifications</b>
      </div>

      <div className="mx-auto max-w-xl px-4 py-10">
        <div className="flex flex-col items-center rounded-md border border-dashed border-line-strong bg-cream px-4 py-10 text-center">
          <div className="mb-3 flex h-[68px] w-[68px] items-center justify-center rounded-full bg-sand-2 text-ink-3">
            <svg width="30" height="30" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.6} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
              <path d="M18 8a6 6 0 1 0-12 0c0 6-2 7-2 7h16s-2-1-2-7" />
              <path d="M10.5 20a1.8 1.8 0 0 0 3 0" />
            </svg>
          </div>
          <h3 className="mb-1 text-base font-bold text-ink">Nothing to read yet</h3>
          <p className="max-w-[36ch] text-[13px] leading-[19px] text-ink-2">
            Requests matching your blood group and area will land here as soon as they come in.
          </p>
          <span className="mt-3 rounded-full bg-sand-2 px-[11px] py-[5px] text-[11.5px] font-semibold text-ink-3">
            Arrives with proximity alerts · CHH-34
          </span>
        </div>
      </div>
    </div>
  );
}

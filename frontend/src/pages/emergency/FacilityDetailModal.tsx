import { useAuth } from "../../context/AuthProvider";
import { useFacilityDetail } from "../../features/emergency/useFacilityDetail";

function mapUrl(latitude: number, longitude: number): string {
  return `https://www.google.com/maps/search/?api=1&query=${latitude},${longitude}`;
}

/**
 * Facility detail modal (CHH-70/US-CHH-001-02, Epic CHH-68): name, category, full address, and
 * contacts with click-to-call. "View on Map" links out to Google Maps rather than an embedded
 * map — no maps API key is provisioned yet (see the CHH-F06 Technical Design's Frontend notes).
 */
export function FacilityDetailModal({ facilityId, onClose }: { facilityId: string; onClose: () => void }) {
  const { session } = useAuth();
  const { facility, isLoading, isError } = useFacilityDetail(session?.token, facilityId);

  return (
    <div className="fixed inset-0 z-20 flex items-end justify-center bg-black/40 sm:items-center" role="dialog" aria-modal="true">
      <div className="flex max-h-[85vh] w-full max-w-md flex-col overflow-y-auto rounded-t-lg bg-cream p-5 sm:rounded-lg">
        <div className="mb-3 flex items-center justify-between">
          <b className="text-[16px] font-bold text-ink">Facility details</b>
          <button
            type="button"
            onClick={onClose}
            aria-label="Close"
            className="flex h-8 w-8 items-center justify-center rounded-full text-ink-2 transition-colors hover:bg-sand-2 hover:text-ink"
          >
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} strokeLinecap="round" aria-hidden="true">
              <path d="M18 6 6 18M6 6l12 12" />
            </svg>
          </button>
        </div>

        {isLoading && <p className="text-sm text-ink-2">Loading…</p>}

        {isError && (
          <div className="flex flex-col gap-3">
            <p className="text-sm text-error">Couldn&apos;t load this facility.</p>
            <button
              type="button"
              onClick={onClose}
              className="flex h-11 w-full items-center justify-center rounded-md border-[1.5px] border-line-strong text-[14px] font-semibold text-ink transition-colors hover:bg-sand-2"
            >
              Close
            </button>
          </div>
        )}

        {facility && (
          <div className="flex flex-col gap-3">
            <div>
              <b className="text-[18px] font-extrabold tracking-tight text-ink">{facility.facilityName}</b>
              <p className="mt-0.5 text-[12.5px] font-semibold text-ink-3">{facility.category}</p>
            </div>

            <p className="text-[13.5px] leading-relaxed text-ink-2">{facility.address}</p>

            {facility.latitude != null && facility.longitude != null && (
              <a
                href={mapUrl(facility.latitude, facility.longitude)}
                target="_blank"
                rel="noreferrer"
                className="flex h-11 w-full items-center justify-center rounded-md border-[1.5px] border-line-strong text-[14px] font-semibold text-clay-deep transition-colors hover:bg-sand-2"
              >
                View on Map
              </a>
            )}

            {facility.contacts.length > 0 && (
              <div className="flex flex-col gap-2 border-t border-line pt-3">
                <b className="text-[13px] font-bold text-ink">Contact</b>
                {facility.contacts.map((contact) => (
                  <div key={contact.mobile} className="flex items-center justify-between gap-2">
                    <div className="min-w-0">
                      <p className="truncate text-[13.5px] font-semibold text-ink">{contact.name}</p>
                      <p className="text-[12px] text-ink-3">{contact.designation}</p>
                    </div>
                    <a
                      href={`tel:${contact.mobile}`}
                      aria-label={`Call ${contact.name}`}
                      className="shrink-0 rounded-full bg-leaf-tint px-3 py-1.5 text-[13px] font-semibold text-leaf"
                    >
                      Call
                    </a>
                  </div>
                ))}
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
}

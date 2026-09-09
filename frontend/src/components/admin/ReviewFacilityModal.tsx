import { useState } from "react";

import { resolveDocumentUrl, type FacilityDto } from "../../api/adminApi";
import { useReviewFacility } from "../../features/admin/useReviewFacility";
import { CheckIcon, FileIcon, FileWarnIcon, XIcon } from "./icons";

interface ReviewFacilityModalProps {
  accessToken: string | undefined;
  facility: FacilityDto;
  onClose: () => void;
  onReviewed: () => void;
}

const MIN_REJECTION_REASON_LENGTH = 3;

// CHH-74/US-CHH-001-02 (view document / missing-document warning) + CHH-75/US-CHH-001-03
// (approve/reject with mandatory rejection reason) — the "Review" action PendingVerificationsPage
// previously rendered disabled.
export function ReviewFacilityModal({ accessToken, facility, onClose, onReviewed }: ReviewFacilityModalProps) {
  const [mode, setMode] = useState<"view" | "reject">("view");
  const [rejectionReason, setRejectionReason] = useState("");
  const { submit, isPending } = useReviewFacility(accessToken);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const hasDocument = Boolean(facility.licenseDocumentUrl);
  const reasonTooShort = rejectionReason.trim().length < MIN_REJECTION_REASON_LENGTH;

  async function handleApprove() {
    setErrorMessage(null);
    const result = await submit(facility.id, "Approve");
    if (result.ok) {
      onReviewed();
    } else {
      setErrorMessage(result.error.message);
    }
  }

  async function handleReject() {
    if (reasonTooShort) return;
    setErrorMessage(null);
    const result = await submit(facility.id, "Reject", rejectionReason.trim());
    if (result.ok) {
      onReviewed();
    } else {
      setErrorMessage(result.error.message);
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-end justify-center bg-ink/40 p-0 md:items-center md:p-4">
      <div className="flex max-h-[90vh] w-full flex-col overflow-y-auto rounded-t-xl bg-cream shadow-lg md:max-w-md md:rounded-xl">
        <div className="flex items-start gap-3 border-b border-line p-5">
          <div className="flex-1">
            <b className="block text-[17px] font-bold leading-5">{facility.facilityName}</b>
            <small className="text-[12.5px] text-ink-2">{facility.address}</small>
          </div>
          <button
            type="button"
            onClick={onClose}
            aria-label="Close"
            className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full text-ink-2 hover:bg-sand-2"
          >
            <XIcon />
          </button>
        </div>

        <div className="flex flex-col gap-4 p-5">
          <div>
            <span className="mb-1.5 block text-[11px] font-bold uppercase tracking-wider text-ink-2">
              Licence document
            </span>
            {hasDocument ? (
              <a
                href={resolveDocumentUrl(facility.licenseDocumentUrl!)}
                target="_blank"
                rel="noopener noreferrer"
                className="flex h-11 items-center gap-2 rounded-md border border-clay-line bg-clay-tint px-3.5 text-[13.5px] font-bold text-clay-deep"
              >
                <FileIcon />
                View document
              </a>
            ) : (
              <div className="flex h-11 items-center gap-2 rounded-md border border-blood-tint bg-error-tint px-3.5 text-[13.5px] font-bold text-error">
                <FileWarnIcon />
                Document missing
              </div>
            )}
          </div>

          {mode === "view" && (
            <>
              {errorMessage && <p className="text-[13px] font-semibold text-error">{errorMessage}</p>}
              <div className="flex gap-2.5">
                <button
                  type="button"
                  onClick={handleApprove}
                  disabled={!hasDocument || isPending}
                  title={!hasDocument ? "Document missing" : undefined}
                  className="flex h-12 flex-1 items-center justify-center gap-2 rounded-md bg-go text-[15px] font-bold text-white transition-colors hover:bg-go-hover disabled:cursor-not-allowed disabled:opacity-50"
                >
                  <CheckIcon />
                  Approve
                </button>
                <button
                  type="button"
                  onClick={() => setMode("reject")}
                  disabled={isPending}
                  className="flex h-12 flex-1 items-center justify-center gap-2 rounded-md bg-error text-[15px] font-bold text-white disabled:cursor-not-allowed disabled:opacity-50"
                >
                  <XIcon />
                  Reject
                </button>
              </div>
            </>
          )}

          {mode === "reject" && (
            <div className="flex flex-col gap-2.5">
              <label htmlFor="rejection-reason" className="text-[13px] font-bold text-ink-2">
                Reason for rejection
              </label>
              <textarea
                id="rejection-reason"
                value={rejectionReason}
                onChange={(e) => setRejectionReason(e.target.value)}
                rows={3}
                placeholder="e.g. Licence document expired, illegible, or details don't match the registration."
                className="rounded-md border border-line-strong bg-sand px-3 py-2 text-sm text-ink"
              />
              {errorMessage && <p className="text-[13px] font-semibold text-error">{errorMessage}</p>}
              <div className="flex gap-2.5">
                <button
                  type="button"
                  onClick={() => setMode("view")}
                  disabled={isPending}
                  className="h-12 flex-1 rounded-md border border-line-strong text-[15px] font-bold text-ink"
                >
                  Back
                </button>
                <button
                  type="button"
                  onClick={handleReject}
                  disabled={reasonTooShort || isPending}
                  className="h-12 flex-1 rounded-md bg-error text-[15px] font-bold text-white disabled:cursor-not-allowed disabled:opacity-50"
                >
                  Confirm rejection
                </button>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

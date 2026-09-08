import { useState } from "react";

import type { AdminUserDto } from "../../api/adminApi";
import { useSuspendUser } from "../../features/admin/useSuspendUser";
import { BanIcon, XIcon } from "./icons";

interface SuspendUserModalProps {
  accessToken: string | undefined;
  user: AdminUserDto;
  onClose: () => void;
  onSuspended: () => void;
}

const MIN_REASON_LENGTH = 3;

// CHH-76/US-CHH-001-04 AC1 — suspend a user account with a mandatory reason.
export function SuspendUserModal({ accessToken, user, onClose, onSuspended }: SuspendUserModalProps) {
  const [reason, setReason] = useState("");
  const { submit, isPending } = useSuspendUser(accessToken);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const reasonTooShort = reason.trim().length < MIN_REASON_LENGTH;

  async function handleConfirm() {
    if (reasonTooShort) return;
    setErrorMessage(null);
    const result = await submit(user.id, reason.trim());
    if (result.ok) {
      onSuspended();
    } else {
      setErrorMessage(result.error.message);
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-end justify-center bg-ink/40 p-0 md:items-center md:p-4">
      <div className="flex w-full flex-col overflow-y-auto rounded-t-xl bg-cream shadow-lg md:max-w-md md:rounded-xl">
        <div className="flex items-start gap-3 border-b border-line p-5">
          <div className="flex-1">
            <b className="block text-[17px] font-bold leading-5">Suspend {user.fullName}</b>
            <small className="text-[12.5px] text-ink-2">{user.mobileNumber}</small>
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

        <div className="flex flex-col gap-3.5 p-5">
          <p className="text-[13px] leading-relaxed text-ink-2">
            This blocks their next OTP login attempt and invalidates any session they currently have open.
          </p>
          <label htmlFor="suspension-reason" className="text-[13px] font-bold text-ink-2">
            Reason for suspension
          </label>
          <textarea
            id="suspension-reason"
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            rows={3}
            placeholder="e.g. Repeated no-shows after accepting blood requests."
            className="rounded-md border border-line-strong bg-sand px-3 py-2 text-sm text-ink"
          />
          {errorMessage && <p className="text-[13px] font-semibold text-error">{errorMessage}</p>}
          <div className="flex gap-2.5">
            <button
              type="button"
              onClick={onClose}
              disabled={isPending}
              className="h-12 flex-1 rounded-md border border-line-strong text-[15px] font-bold text-ink"
            >
              Cancel
            </button>
            <button
              type="button"
              onClick={handleConfirm}
              disabled={reasonTooShort || isPending}
              className="flex h-12 flex-1 items-center justify-center gap-2 rounded-md bg-error text-[15px] font-bold text-white disabled:cursor-not-allowed disabled:opacity-50"
            >
              <BanIcon />
              Suspend account
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}

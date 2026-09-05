import { useNavigate } from "react-router-dom";

import { useOtpRequest } from "../../features/auth/useOtpRequest";

export function MobileEntryPage() {
  const navigate = useNavigate();
  const { mobileNumber, setMobileNumber, isValid, touched, markTouched, submit, isPending, error } =
    useOtpRequest();

  const showInvalid = touched && !isValid;

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    const result = await submit();
    if (result.ok && result.data) {
      navigate("/otp-verify", {
        state: {
          mobileNumber,
          maskedMobileNumber: result.data.maskedMobileNumber,
          otpExpiresAtUtc: result.data.otpExpiresAtUtc,
          resendAvailableAtUtc: result.data.resendAvailableAtUtc,
        },
      });
    }
  }

  const ctaEnabled = isValid && !isPending;

  return (
    <div className="flex min-h-screen flex-col bg-sand font-sans text-ink">
      <div className="px-7 pt-14">
        <div className="mb-3.5 flex h-13 w-13 items-center justify-center rounded-full bg-blood text-white">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
            <path d="M12 3.2c3.4 4 6 6.9 6 10a6 6 0 0 1-12 0c0-3.1 2.6-6 6-10Z" />
          </svg>
        </div>
        <div className="text-sm font-bold text-ink-2">Community Health Hub</div>
      </div>

      <div className="flex flex-1 flex-col justify-center px-7">
        <h1 className="mb-2 text-[26px] font-extrabold tracking-tight">Enter your mobile number</h1>
        <p className="mb-8 max-w-[30ch] text-[14.5px] leading-relaxed text-ink-2">
          We&apos;ll send a 6-digit code to verify it&apos;s you.
        </p>

        <form onSubmit={handleSubmit} noValidate>
          <div className="mb-5 flex flex-col gap-2">
            <label htmlFor="mobile-number" className="sr-only">
              Mobile number
            </label>
            <div className="relative">
              <svg
                className="pointer-events-none absolute left-4 top-1/2 -translate-y-1/2 text-ink-3"
                width="20"
                height="20"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth={1.75}
                strokeLinecap="round"
                strokeLinejoin="round"
                aria-hidden="true"
              >
                <path d="M8 4.5H5.8A1.8 1.8 0 0 0 4 6.5c0 7.7 5.8 13.5 13.5 13.5a1.8 1.8 0 0 0 2-1.8V16l-4-1.5-2 2a13 13 0 0 1-5-5l2-2L8 4.5Z" />
              </svg>
              <input
                id="mobile-number"
                type="tel"
                inputMode="numeric"
                placeholder="10-digit mobile number"
                value={mobileNumber}
                onChange={(e) => setMobileNumber(e.target.value)}
                onBlur={markTouched}
                aria-invalid={showInvalid}
                aria-describedby="mobile-number-hint"
                className={`h-[54px] w-full rounded-sm border-[1.5px] bg-cream pl-12 pr-4 text-base text-ink transition-colors ${
                  showInvalid
                    ? "border-error"
                    : isValid
                      ? "border-clay"
                      : "border-line-strong"
                }`}
              />
            </div>
            <div
              id="mobile-number-hint"
              className={`flex min-h-[17px] items-center gap-1.5 text-xs ${
                showInvalid ? "text-error" : "text-ink-3"
              }`}
            >
              {showInvalid && (
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                  <path d="M12 4.5 21 19.5H3L12 4.5Z" />
                  <path d="M12 10v4M12 16.8v.1" />
                </svg>
              )}
              {showInvalid
                ? "Please enter a valid 10-digit mobile number"
                : error?.status === 422
                  ? error.message
                  : ""}
            </div>
          </div>

          {error && error.status !== 422 && (
            <div className="mb-5 rounded-sm border border-error bg-error-tint px-4 py-3 text-sm text-error">
              {error.message}
            </div>
          )}

          <button
            type="submit"
            disabled={!ctaEnabled}
            className={`h-[54px] w-full rounded-md text-base font-semibold transition-colors ${
              ctaEnabled
                ? "bg-clay text-white hover:bg-clay-hover"
                : "cursor-not-allowed bg-sand-2 text-ink-off"
            }`}
          >
            {isPending ? "Sending…" : "Get OTP"}
          </button>
        </form>
      </div>

      <div className="px-7 pb-10 text-center text-xs text-ink-off">
        By continuing you agree this device isn&apos;t shared for medical alerts.
      </div>
    </div>
  );
}

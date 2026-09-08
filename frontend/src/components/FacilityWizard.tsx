import { useNavigate } from "react-router-dom";

import { useAuth } from "../context/AuthProvider";
import { useToast } from "../context/ToastProvider";
import { useFacilityLicenseUpload } from "../features/facility/useFacilityLicenseUpload";
import { useFacilityRegistration } from "../features/facility/useFacilityRegistration";
import { ALLOWED_FILE_TYPES } from "../lib/validation/facilityUploadValidation";
import { FACILITY_CATEGORIES, MAX_CONTACTS, type FacilityCategory } from "../lib/validation/facilitySchemas";

const STEP_META: Record<"details" | "contacts" | "upload", { no: string; widthPct: number; title: string }> = {
  details: { no: "1", widthPct: 33, title: "Facility details" },
  contacts: { no: "2", widthPct: 66, title: "Contacts" },
  upload: { no: "3", widthPct: 100, title: "Licence document" },
};

function FieldError({ message }: { message?: string }) {
  if (!message) return null;
  return (
    <span className="flex items-start gap-1.5 text-[12.5px] leading-tight text-error">
      <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2.2} strokeLinecap="round" strokeLinejoin="round" className="mt-0.5 flex-none" aria-hidden="true">
        <path d="M12 4.5 21 19.5H3L12 4.5Z" />
        <path d="M12 10v4M12 16.8v.1" />
      </svg>
      {message}
    </span>
  );
}

function Hint({ children }: { children: React.ReactNode }) {
  return <span className="text-[12.5px] leading-tight text-ink-3">{children}</span>;
}

export function FacilityWizard() {
  const navigate = useNavigate();
  const { session } = useAuth();
  const toast = useToast();
  const {
    step,
    facilityId,
    details,
    setDetailsField,
    detailsErrors,
    detailsTouched,
    goToContacts,
    goBack,
    contacts,
    addContact,
    removeContact,
    setContactField,
    contactErrors,
    duplicateMobileIndex,
    contactsTouched,
    isPending,
    error,
    submit,
  } = useFacilityRegistration(session?.token);

  const {
    file,
    status: uploadStatus,
    progressPct,
    errorMessage: uploadErrorMessage,
    selectFile,
    retry: retryUpload,
    isUploaded,
  } = useFacilityLicenseUpload(session?.token, facilityId);

  const meta = STEP_META[step];
  const showDetailsErrors = detailsTouched;
  const showContactsErrors = contactsTouched;

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    const result = await submit();
    if (!result.ok && result.error) {
      toast.error(result.error.message);
    }
  }

  function handleFileInputChange(e: React.ChangeEvent<HTMLInputElement>) {
    const selected = e.target.files?.[0];
    if (selected) {
      void selectFile(selected);
    }
    e.target.value = "";
  }

  function handleSubmitForVerification() {
    toast.success("Submitted for verification.");
    navigate("/");
  }

  return (
    <div className="flex min-h-screen flex-col bg-sand font-sans text-ink">
      <header className="flex h-[58px] flex-none items-center gap-2.5 border-b border-line bg-cream px-3 md:h-16 md:px-8">
        <button
          type="button"
          onClick={() => navigate("/")}
          aria-label="Back to home"
          className="flex h-10 w-10 items-center justify-center rounded-full text-ink-2 transition-colors hover:bg-sand-2"
        >
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.75} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
            <path d="M19 12H5M11 6l-6 6 6 6" />
          </svg>
        </button>
        <b className="flex-1 text-center text-[15px] font-bold md:text-left md:text-base">Register facility</b>
        <span className="hidden w-10 md:block" aria-hidden="true" />
      </header>

      <div className="flex flex-none justify-center border-b border-line bg-cream">
        <div className="w-full max-w-2xl px-4 pb-3 pt-3.5 md:px-8">
          <div className="mb-2 flex items-baseline justify-between">
            <span className="text-sm font-bold">{meta.title}</span>
            <span className="text-xs text-ink-3 [font-variant-numeric:tabular-nums]">Step {meta.no} of 3</span>
          </div>
          <div className="h-1.5 overflow-hidden rounded-full bg-sand-2">
            <div
              className="h-full rounded-full bg-clay transition-[width] duration-200"
              style={{ width: `${meta.widthPct}%` }}
            />
          </div>
        </div>
      </div>

      <form onSubmit={handleSubmit} noValidate className="w-full max-w-2xl flex-1 self-center flex flex-col gap-5 px-4 py-5 md:px-8 md:py-8">
        {step === "details" && (
          <>
            <p className="text-[13px] leading-relaxed text-ink-2">
              Tell us about the facility. You can come back to a saved draft at any time.
            </p>

            <div className="flex flex-col gap-1.5">
              <label htmlFor="facility-name" className="text-sm font-semibold text-ink-2">
                Facility name <i className="not-italic text-error">*</i>
              </label>
              <input
                id="facility-name"
                type="text"
                value={details.facilityName ?? ""}
                onChange={(e) => setDetailsField("facilityName", e.target.value)}
                placeholder="Registered name of the hospital or NGO"
                aria-invalid={showDetailsErrors && !!detailsErrors.facilityName}
                aria-describedby="facility-name-hint"
                className={`h-[50px] rounded-sm border-[1.5px] bg-cream px-4 text-base outline-none transition-colors focus:border-clay ${
                  showDetailsErrors && detailsErrors.facilityName ? "border-error" : "border-line-strong"
                }`}
              />
              <span id="facility-name-hint">
                {showDetailsErrors && detailsErrors.facilityName ? (
                  <FieldError message={detailsErrors.facilityName[0]} />
                ) : (
                  <Hint>Required. Use the name printed on your licence — an admin checks the two match.</Hint>
                )}
              </span>
            </div>

            <div className="flex flex-col gap-1.5">
              <label htmlFor="facility-category" className="text-sm font-semibold text-ink-2">
                Category <i className="not-italic text-error">*</i>
              </label>
              <select
                id="facility-category"
                value={details.category ?? ""}
                onChange={(e) => setDetailsField("category", e.target.value as FacilityCategory)}
                aria-invalid={showDetailsErrors && !!detailsErrors.category}
                aria-describedby="facility-category-hint"
                className={`h-[50px] rounded-sm border-[1.5px] bg-cream px-4 text-base outline-none transition-colors focus:border-clay ${
                  showDetailsErrors && detailsErrors.category ? "border-error" : "border-line-strong"
                }`}
              >
                <option value="" disabled>
                  Select a category
                </option>
                {FACILITY_CATEGORIES.map((cat) => (
                  <option key={cat} value={cat}>
                    {cat}
                  </option>
                ))}
              </select>
              <span id="facility-category-hint">
                {showDetailsErrors && detailsErrors.category ? (
                  <FieldError message={detailsErrors.category[0]} />
                ) : (
                  <Hint>Required. This sets what your facility can publish.</Hint>
                )}
              </span>
            </div>

            <div className="flex flex-col gap-1.5">
              <label htmlFor="license-number" className="text-sm font-semibold text-ink-2">
                Licence number <i className="not-italic text-error">*</i>
              </label>
              <input
                id="license-number"
                type="text"
                value={details.licenseNumber ?? ""}
                onChange={(e) => setDetailsField("licenseNumber", e.target.value)}
                placeholder="KL-HOSP-000000"
                aria-invalid={showDetailsErrors && !!detailsErrors.licenseNumber}
                aria-describedby="license-number-hint"
                className={`h-[50px] rounded-sm border-[1.5px] bg-cream px-4 font-mono text-base outline-none transition-colors focus:border-clay ${
                  showDetailsErrors && detailsErrors.licenseNumber ? "border-error" : "border-line-strong"
                }`}
              />
              <span id="license-number-hint">
                {showDetailsErrors && detailsErrors.licenseNumber ? (
                  <FieldError message={detailsErrors.licenseNumber[0]} />
                ) : (
                  <Hint>Required. Letters and numbers, as printed on the licence.</Hint>
                )}
              </span>
            </div>

            <div className="flex flex-col gap-1.5">
              <label htmlFor="facility-address" className="text-sm font-semibold text-ink-2">
                Address <i className="not-italic text-error">*</i>
              </label>
              <textarea
                id="facility-address"
                value={details.address ?? ""}
                onChange={(e) => setDetailsField("address", e.target.value)}
                placeholder="Building, street, area, city, PIN code"
                aria-invalid={showDetailsErrors && !!detailsErrors.address}
                aria-describedby="facility-address-hint"
                className={`h-[88px] resize-y rounded-sm border-[1.5px] bg-cream px-4 py-3 text-base leading-relaxed outline-none transition-colors focus:border-clay ${
                  showDetailsErrors && detailsErrors.address ? "border-error" : "border-line-strong"
                }`}
              />
              <span id="facility-address-hint">
                {showDetailsErrors && detailsErrors.address ? (
                  <FieldError message={detailsErrors.address[0]} />
                ) : (
                  <Hint>Required. This is the fixed address donors are routed to — it is not tracked or updated automatically.</Hint>
                )}
              </span>
            </div>

            <div className="flex justify-end pt-2">
              <button
                type="button"
                onClick={goToContacts}
                className="flex h-12 items-center justify-center gap-2 rounded-md bg-clay px-6 text-[15px] font-semibold text-white transition-colors hover:bg-clay-hover"
              >
                Continue to contacts
              </button>
            </div>
          </>
        )}

        {step === "contacts" && (
          <>
            <p className="text-[13px] leading-relaxed text-ink-2">
              Add up to three people. A donor responding to this facility sees the primary contact.
            </p>

            <div className="flex flex-col gap-4">
              {contacts.map((contact, i) => {
                const errs = contactErrors[i] ?? {};
                const isDuplicate = showContactsErrors && duplicateMobileIndex === i;
                const flagged = showContactsErrors && (!!errs.mobile || isDuplicate);
                return (
                  <div
                    key={i}
                    className={`flex flex-col gap-3 rounded-md border-[1.5px] bg-cream p-4 ${
                      flagged ? "border-error" : "border-line"
                    }`}
                  >
                    <div className="flex items-center gap-2">
                      <span className="flex h-[22px] items-center rounded-full bg-clay-tint px-2.5 text-[11.5px] font-bold text-clay-deep">
                        {i + 1}
                      </span>
                      <b className="text-[13.5px] font-bold">{i === 0 ? "Primary contact" : `Contact ${i + 1}`}</b>
                      {i > 0 && (
                        <button
                          type="button"
                          onClick={() => removeContact(i)}
                          aria-label={`Remove contact ${i + 1}`}
                          className="ml-auto flex h-[34px] w-[34px] items-center justify-center rounded-full text-ink-3 transition-colors hover:bg-sand-2 hover:text-error"
                        >
                          <svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.9} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                            <path d="M4 7h16M9.5 7V5h5v2M6.5 7l1 12.5h9L17.5 7" />
                          </svg>
                        </button>
                      )}
                    </div>

                    <div className="flex flex-col gap-1.5">
                      <label htmlFor={`contact-${i}-name`} className="text-sm font-semibold text-ink-2">
                        Name <i className="not-italic text-error">*</i>
                      </label>
                      <input
                        id={`contact-${i}-name`}
                        type="text"
                        value={contact.name}
                        onChange={(e) => setContactField(i, "name", e.target.value)}
                        placeholder="Full name"
                        className={`h-[50px] rounded-sm border-[1.5px] bg-sand px-3.5 text-base outline-none transition-colors focus:border-clay ${
                          showContactsErrors && errs.name ? "border-error" : "border-line"
                        }`}
                      />
                      {showContactsErrors && <FieldError message={errs.name?.[0]} />}
                    </div>

                    <div className="flex flex-col gap-1.5">
                      <label htmlFor={`contact-${i}-designation`} className="text-sm font-semibold text-ink-2">
                        Designation <i className="not-italic text-error">*</i>
                      </label>
                      <input
                        id={`contact-${i}-designation`}
                        type="text"
                        value={contact.designation}
                        onChange={(e) => setContactField(i, "designation", e.target.value)}
                        placeholder="e.g. Blood bank officer"
                        className={`h-[50px] rounded-sm border-[1.5px] bg-sand px-3.5 text-base outline-none transition-colors focus:border-clay ${
                          showContactsErrors && errs.designation ? "border-error" : "border-line"
                        }`}
                      />
                      {showContactsErrors && <FieldError message={errs.designation?.[0]} />}
                    </div>

                    <div className="flex flex-col gap-1.5">
                      <label htmlFor={`contact-${i}-mobile`} className="text-sm font-semibold text-ink-2">
                        Mobile <i className="not-italic text-error">*</i>
                      </label>
                      <div className="flex">
                        <span className="flex h-[50px] items-center rounded-l-sm border-[1.5px] border-r-0 border-line bg-sand-2 px-3 font-mono text-[15px] text-ink-2">
                          +91
                        </span>
                        <input
                          id={`contact-${i}-mobile`}
                          type="tel"
                          inputMode="numeric"
                          value={contact.mobile}
                          onChange={(e) => setContactField(i, "mobile", e.target.value)}
                          placeholder="10 digits"
                          className={`h-[50px] w-full rounded-r-sm border-[1.5px] bg-sand px-3.5 font-mono text-base outline-none transition-colors focus:border-clay ${
                            flagged ? "border-error" : "border-line"
                          }`}
                        />
                      </div>
                      {showContactsErrors && isDuplicate && (
                        <FieldError
                          message={`Contact ${
                            contacts.findIndex((c) => c.mobile === contact.mobile) + 1
                          } already uses this number. Enter a different one.`}
                        />
                      )}
                      {showContactsErrors && !isDuplicate && <FieldError message={errs.mobile?.[0]} />}
                      {!showContactsErrors && <Hint>10 digits, no spaces.</Hint>}
                    </div>
                  </div>
                );
              })}
            </div>

            <button
              type="button"
              onClick={addContact}
              disabled={contacts.length >= MAX_CONTACTS}
              className="flex h-12 w-full items-center justify-center gap-2 rounded-md border-[1.5px] border-dashed border-clay-line text-[14.5px] font-semibold text-clay transition-colors hover:bg-clay-tint disabled:cursor-not-allowed disabled:border-line-strong disabled:text-ink-off disabled:hover:bg-transparent"
            >
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                <path d="M12 5v14M5 12h14" />
              </svg>
              {contacts.length >= MAX_CONTACTS ? "Three contacts is the maximum" : "Add another contact"}
            </button>

            {error && (
              <div className="rounded-sm border border-error bg-error-tint px-4 py-3 text-sm text-error">
                {error.message}
              </div>
            )}

            <div className="flex items-center justify-between pt-2">
              <button
                type="button"
                onClick={goBack}
                className="flex h-12 items-center justify-center rounded-md border-[1.5px] border-line-strong px-5 text-[15px] font-semibold text-ink transition-colors hover:bg-sand-2"
              >
                Back
              </button>
              <button
                type="submit"
                disabled={isPending}
                className={`flex h-12 items-center justify-center gap-2 rounded-md px-6 text-[15px] font-semibold transition-colors ${
                  isPending ? "cursor-not-allowed bg-sand-2 text-ink-off" : "bg-clay text-white hover:bg-clay-hover"
                }`}
              >
                {isPending ? "Saving…" : "Continue to licence"}
              </button>
            </div>
          </>
        )}

        {step === "upload" && (
          <>
            <p className="text-[13px] leading-relaxed text-ink-2">
              An admin compares this document against the facility name and licence number you entered. Only admins can open it.
            </p>

            {(uploadStatus === "empty" || uploadStatus === "invalid") && (
              <div className="flex flex-col items-center gap-2.5 rounded-md border-2 border-dashed border-line-strong bg-cream px-6 py-8 text-center">
                <span className="flex h-14 w-14 items-center justify-center rounded-full bg-sand-2 text-ink-3">
                  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.7} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                    <path d="M12 16V4M12 4 7.5 8.5M12 4l4.5 4.5" />
                    <path d="M4 15v3.5A1.5 1.5 0 0 0 5.5 20h13a1.5 1.5 0 0 0 1.5-1.5V15" />
                  </svg>
                </span>
                <b className="text-[15.5px] font-bold">Add your licence</b>
                <span className="text-[12.5px] text-ink-3 [font-variant-numeric:tabular-nums]">PDF, JPG or PNG · up to 5 MB</span>
                <label
                  htmlFor="license-file-input"
                  className="mt-1 flex h-11 cursor-pointer items-center justify-center rounded-sm border-[1.5px] border-line-strong bg-sand px-[18px] text-sm font-semibold text-ink transition-colors hover:border-clay hover:text-clay"
                >
                  Choose a file
                </label>
                <input
                  id="license-file-input"
                  type="file"
                  accept={ALLOWED_FILE_TYPES.join(",")}
                  onChange={handleFileInputChange}
                  className="sr-only"
                />
                {uploadStatus === "invalid" && <FieldError message={uploadErrorMessage ?? undefined} />}
              </div>
            )}

            {(uploadStatus === "uploading" || uploadStatus === "uploaded" || uploadStatus === "networkFailed") && file && (
              <div
                className={`flex gap-3.5 rounded-md border p-4 ${
                  uploadStatus === "networkFailed" ? "border-error bg-error-tint" : uploadStatus === "uploaded" ? "border-leaf bg-leaf-tint" : "border-line bg-cream"
                }`}
              >
                <span className="flex h-12 w-10 flex-none items-center justify-center rounded-sm border border-line bg-sand text-ink-3">
                  <svg width="19" height="19" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.7} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                    <path d="M14 3H7a1.5 1.5 0 0 0-1.5 1.5v15A1.5 1.5 0 0 0 7 21h10a1.5 1.5 0 0 0 1.5-1.5V7.5Z" />
                    <path d="M14 3v4.5h4.5" />
                  </svg>
                </span>
                <div className="flex min-w-0 flex-1 flex-col gap-1.5">
                  <div className="flex items-baseline gap-2.5">
                    <b className="truncate text-[13.5px] font-bold">{file.name}</b>
                    <span className="flex-none text-xs text-ink-3 [font-variant-numeric:tabular-nums]">
                      {(file.size / (1024 * 1024)).toFixed(1)} MB
                    </span>
                  </div>
                  {uploadStatus !== "uploaded" && (
                    <div
                      role="progressbar"
                      aria-label="Upload progress"
                      aria-valuenow={progressPct}
                      aria-valuemin={0}
                      aria-valuemax={100}
                      className="h-1.5 overflow-hidden rounded-full bg-sand-2"
                    >
                      <div
                        className={`h-full rounded-full transition-[width] duration-200 ${uploadStatus === "networkFailed" ? "bg-error" : "bg-clay"}`}
                        style={{ width: `${progressPct}%` }}
                      />
                    </div>
                  )}
                  <span
                    className={`text-xs [font-variant-numeric:tabular-nums] ${
                      uploadStatus === "networkFailed" ? "text-error" : uploadStatus === "uploaded" ? "font-semibold text-leaf" : "text-ink-2"
                    }`}
                  >
                    {uploadStatus === "uploading" && `Uploading — ${progressPct}%`}
                    {uploadStatus === "uploaded" && "Uploaded"}
                    {uploadStatus === "networkFailed" && (uploadErrorMessage ?? "Upload failed.")}
                  </span>
                  {uploadStatus === "networkFailed" && (
                    <button
                      type="button"
                      onClick={() => void retryUpload()}
                      className="flex h-9 w-fit items-center gap-1.5 rounded-sm border-[1.5px] border-line-strong px-3.5 text-[13px] font-semibold text-ink transition-colors hover:bg-sand-2"
                    >
                      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                        <path d="M20 11a8 8 0 1 0-2.3 6.3" />
                        <path d="M20 5v6h-6" />
                      </svg>
                      Retry upload
                    </button>
                  )}
                </div>
              </div>
            )}

            <div className="flex items-center justify-between pt-2">
              <button
                type="button"
                onClick={goBack}
                className="flex h-12 items-center justify-center rounded-md border-[1.5px] border-line-strong px-5 text-[15px] font-semibold text-ink transition-colors hover:bg-sand-2"
              >
                Back
              </button>
              <button
                type="button"
                onClick={handleSubmitForVerification}
                disabled={!isUploaded}
                className={`flex h-12 items-center justify-center gap-2 rounded-md px-6 text-[15px] font-semibold transition-colors ${
                  isUploaded ? "bg-clay text-white hover:bg-clay-hover" : "cursor-not-allowed bg-sand-2 text-ink-off"
                }`}
              >
                Submit for verification
              </button>
            </div>
          </>
        )}
      </form>
    </div>
  );
}

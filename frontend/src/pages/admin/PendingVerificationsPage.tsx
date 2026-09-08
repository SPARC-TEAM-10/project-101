import { useState } from "react";

import { AdminShell } from "../../components/admin/AdminShell";
import { CalendarIcon, ChevronLeftIcon, ChevronRightIcon, CloudOffIcon, FileIcon, FileWarnIcon, InboxIcon, RetryIcon } from "../../components/admin/icons";
import { useAuth } from "../../context/AuthProvider";
import { DEFAULT_PAGE_SIZE, usePendingFacilities } from "../../features/admin/usePendingFacilities";
import type { FacilityDto } from "../../api/adminApi";

function facilityInitials(name: string): string {
  const words = name.trim().split(/\s+/);
  return ((words[0]?.[0] ?? "") + (words[1]?.[0] ?? "")).toUpperCase();
}

function formatRegisteredDate(isoDate: string): string {
  return new Intl.DateTimeFormat("en-GB", { day: "numeric", month: "short", year: "numeric" }).format(
    new Date(isoDate),
  );
}

function LicenceBadge({ licenseDocumentUrl }: { licenseDocumentUrl: string | null }) {
  if (!licenseDocumentUrl) {
    return (
      <span className="inline-flex items-center gap-1.5 rounded-full border border-blood-tint bg-error-tint px-2.5 py-1 text-[12.5px] font-bold text-error">
        <FileWarnIcon />
        Missing
      </span>
    );
  }
  const extension = licenseDocumentUrl.split(".").pop()?.toUpperCase().slice(0, 4) ?? "File";
  return (
    <span className="inline-flex items-center gap-1.5 rounded-full border border-clay-line bg-clay-tint px-2.5 py-1 text-[12.5px] font-bold text-clay-deep">
      <FileIcon />
      {extension}
    </span>
  );
}

function DesktopRow({ facility }: { facility: FacilityDto }) {
  const missingDoc = !facility.licenseDocumentUrl;
  return (
    <tr className="border-b border-line last:border-0">
      <td className="h-[66px] px-4.5">
        <div className="flex items-center gap-3">
          <span className="flex h-9.5 w-9.5 shrink-0 items-center justify-center rounded-md bg-clay-tint text-[13px] font-extrabold text-clay-deep">
            {facilityInitials(facility.facilityName)}
          </span>
          <span>
            <b className="block text-[15px] font-bold leading-5">{facility.facilityName}</b>
            <small className="text-[12.5px] text-ink-2">{facility.address}</small>
          </span>
        </div>
      </td>
      <td className="h-[66px] px-4.5">
        <span className="inline-flex items-center rounded-full border border-line bg-sand-2 px-2.5 py-1 text-[12.5px] font-bold text-ink-2">
          {facility.category}
        </span>
      </td>
      <td className="h-[66px] px-4.5 text-sm tabular-nums">{formatRegisteredDate(facility.createdAtUtc)}</td>
      <td className="h-[66px] px-4.5">
        <LicenceBadge licenseDocumentUrl={facility.licenseDocumentUrl} />
      </td>
      <td className="h-[66px] px-4.5 text-right">
        <button
          type="button"
          disabled
          title="Facility review ships in a later story (CHH-74/75)"
          className={`h-9 cursor-not-allowed rounded-[10px] px-4 text-[13px] font-bold text-white opacity-60 ${
            missingDoc ? "bg-sand-2 text-ink-off" : "bg-clay"
          }`}
        >
          Review
        </button>
      </td>
    </tr>
  );
}

function MobileCard({ facility }: { facility: FacilityDto }) {
  return (
    <div className="flex flex-col gap-2.5 rounded-md border border-line bg-cream p-3.5 shadow-sm">
      <div className="flex items-start gap-2.5">
        <span className="flex h-[42px] w-[42px] shrink-0 items-center justify-center rounded-md bg-clay-tint text-[13px] font-extrabold text-clay-deep">
          {facilityInitials(facility.facilityName)}
        </span>
        <span>
          <b className="block text-base font-bold leading-5">{facility.facilityName}</b>
          <small className="text-[12.5px] text-ink-2">{facility.address}</small>
        </span>
      </div>
      <div className="flex flex-wrap gap-1.5">
        <span className="inline-flex items-center gap-1.5 rounded-full border border-line bg-sand-2 px-2.5 py-1 text-[12.5px] font-bold text-ink-2">
          {facility.category}
        </span>
        <span className="inline-flex items-center gap-1.5 rounded-full border border-line bg-sand-2 px-2.5 py-1 text-[12.5px] font-bold tabular-nums text-ink-2">
          <CalendarIcon />
          {formatRegisteredDate(facility.createdAtUtc)}
        </span>
        <LicenceBadge licenseDocumentUrl={facility.licenseDocumentUrl} />
      </div>
      <button
        type="button"
        disabled
        title="Facility review ships in a later story (CHH-74/75)"
        className="h-12 w-full cursor-not-allowed rounded-md bg-sand-2 text-[15px] font-bold text-ink-off opacity-70"
      >
        Review registration
      </button>
    </div>
  );
}

// CHH-73/US-CHH-001-01 — the "Pending verification" tab of the Admin Command Center. Matches
// the design canvas's AdminQueueWeb/AdminQueueMobile artboards' 4-state machine.
export function PendingVerificationsPage() {
  const { session } = useAuth();
  const [page, setPage] = useState(1);
  const { status, items, totalPages, totalCount, refetch } = usePendingFacilities(session?.token, page);

  return (
    <AdminShell activeTab="pending" pendingCount={status === "loading" ? undefined : totalCount}>
      <div className="flex min-h-0 flex-1 flex-col gap-3.5 p-4 md:p-7">
        <div className="flex min-h-0 flex-1 flex-col overflow-hidden rounded-md border border-line bg-cream shadow-sm">
          {status === "loading" && (
            <>
              {/* Desktop skeleton */}
              <table className="hidden w-full border-collapse md:table">
                <tbody>
                  {[1, 2, 3, 4, 5, 6].map((n) => (
                    <tr key={n} className="border-b border-line">
                      <td className="h-[66px] px-4.5">
                        <div className="flex items-center gap-3">
                          <span className="h-9.5 w-9.5 shrink-0 rounded-md bg-sand-2" />
                          <span className="flex flex-col gap-1.5">
                            <span className="h-3.5 w-[190px] rounded-sm bg-sand-2" />
                            <span className="h-2.5 w-[104px] rounded-sm bg-sand-2" />
                          </span>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
              {/* Mobile skeleton */}
              <div className="flex flex-col gap-3 p-4 md:hidden">
                {[1, 2, 3, 4].map((n) => (
                  <div key={n} className="flex flex-col gap-2.5 rounded-md border border-line bg-cream p-3.5">
                    <div className="flex items-center gap-3">
                      <span className="h-[42px] w-[42px] shrink-0 rounded-md bg-sand-2" />
                      <span className="flex flex-1 flex-col gap-2">
                        <span className="h-3.5 w-3/4 rounded-sm bg-sand-2" />
                        <span className="h-2.5 w-2/5 rounded-sm bg-sand-2" />
                      </span>
                    </div>
                    <span className="h-12 w-full rounded-md bg-sand-2" />
                  </div>
                ))}
              </div>
            </>
          )}

          {status === "error" && (
            <div className="flex flex-1 flex-col items-center justify-center gap-3 p-10 text-center">
              <span className="flex h-[68px] w-[68px] items-center justify-center rounded-full bg-error-tint text-error">
                <CloudOffIcon />
              </span>
              <b className="text-[17px] font-bold">The verification queue didn&apos;t load.</b>
              <p className="max-w-[46ch] text-sm leading-relaxed text-ink-2">
                The connection dropped before the list arrived. Nothing has changed — no facility was approved or
                rejected.
              </p>
              <button
                type="button"
                onClick={() => refetch()}
                className="flex h-12 items-center gap-2 rounded-md bg-clay px-5 text-[15px] font-bold text-white"
              >
                <RetryIcon />
                Try again
              </button>
            </div>
          )}

          {status === "empty" && (
            <div className="flex flex-1 flex-col items-center justify-center gap-3 p-10 text-center">
              <span className="flex h-[68px] w-[68px] items-center justify-center rounded-full bg-sand-2 text-ink-3">
                <InboxIcon />
              </span>
              <b className="text-[17px] font-bold">No pending verifications at this time.</b>
              <p className="max-w-[46ch] text-sm leading-relaxed text-ink-2">
                Every facility that has registered has been reviewed. New registrations land here the moment they
                submit a licence.
              </p>
            </div>
          )}

          {status === "list" && (
            <>
              <table className="hidden w-full border-collapse md:table">
                <thead>
                  <tr className="border-b border-line bg-sand-2">
                    <th className="h-11 px-4.5 text-left text-[11px] font-bold uppercase tracking-wider text-ink-2">
                      Facility
                    </th>
                    <th className="h-11 px-4.5 text-left text-[11px] font-bold uppercase tracking-wider text-ink-2">
                      Category
                    </th>
                    <th className="h-11 px-4.5 text-left text-[11px] font-bold uppercase tracking-wider text-ink-2">
                      Registered
                    </th>
                    <th className="h-11 px-4.5 text-left text-[11px] font-bold uppercase tracking-wider text-ink-2">
                      Licence
                    </th>
                    <th className="h-11 px-4.5" />
                  </tr>
                </thead>
                <tbody>
                  {items.map((facility) => (
                    <DesktopRow key={facility.id} facility={facility} />
                  ))}
                </tbody>
              </table>

              <div className="flex flex-col gap-3 p-4 md:hidden">
                <div className="flex items-baseline gap-2">
                  <b className="text-[13px] font-bold text-ink-2">Oldest first</b>
                  <span className="ml-auto text-[12.5px] tabular-nums text-ink-3">
                    {(page - 1) * DEFAULT_PAGE_SIZE + 1}–{Math.min(page * DEFAULT_PAGE_SIZE, totalCount)} of{" "}
                    {totalCount}
                  </span>
                </div>
                {items.map((facility) => (
                  <MobileCard key={facility.id} facility={facility} />
                ))}
              </div>
            </>
          )}

          {status === "list" && totalPages > 1 && (
            <div className="flex shrink-0 items-center gap-3 border-t border-line bg-sand px-4.5 py-3">
              <span className="hidden text-[13px] font-semibold text-ink-2 md:inline">
                Showing {(page - 1) * DEFAULT_PAGE_SIZE + 1}–{Math.min(page * DEFAULT_PAGE_SIZE, totalCount)} of{" "}
                {totalCount}
              </span>
              <div className="ml-auto flex items-center gap-1.5">
                <button
                  type="button"
                  onClick={() => setPage((p) => Math.max(1, p - 1))}
                  disabled={page <= 1}
                  aria-label="Previous page"
                  className="flex h-9 min-w-9 items-center justify-center rounded-[10px] border border-line bg-cream text-ink-2 disabled:cursor-not-allowed disabled:opacity-50"
                >
                  <ChevronLeftIcon />
                </button>
                <span className="px-2 text-[13.5px] font-bold tabular-nums text-ink-2">
                  Page {page} of {totalPages}
                </span>
                <button
                  type="button"
                  onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                  disabled={page >= totalPages}
                  aria-label="Next page"
                  className="flex h-9 min-w-9 items-center justify-center rounded-[10px] border border-line bg-cream text-ink-2 disabled:cursor-not-allowed disabled:opacity-50"
                >
                  <ChevronRightIcon />
                </button>
              </div>
            </div>
          )}
        </div>
        <p className="text-[12.5px] leading-relaxed text-ink-3">
          Only System Admins can open this screen. Every approval and rejection is recorded against your account
          with the reason you give.
        </p>
      </div>
    </AdminShell>
  );
}

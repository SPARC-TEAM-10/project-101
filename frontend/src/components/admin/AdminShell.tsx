import type { ReactNode } from "react";
import { Link } from "react-router-dom";

import { BellIcon, ShieldIcon, UsersIcon } from "./icons";

export type AdminTab = "pending" | "verified" | "rejected" | "users";

interface AdminShellProps {
  activeTab: AdminTab;
  pendingCount?: number;
  children: ReactNode;
}

const TAB_LABELS: Record<Exclude<AdminTab, "users">, string> = {
  pending: "Pending verification",
  verified: "Verified",
  rejected: "Rejected",
};

// Sidebar (desktop, matching AdminQueueWeb.dc.html) + appbar/bottomnav (mobile, matching
// AdminQueueMobile.dc.html) shell for the whole Admin Command Center epic (CHH-72). "Verified"/
// "Rejected" in-page tabs still have no backing endpoint (only CHH-75's approve/reject action
// exists, not a list-by-status view) so they render disabled; "Verifications" and "Users" are both
// functional top-level nav destinations (CHH-73/CHH-76).
export function AdminShell({ activeTab, pendingCount, children }: AdminShellProps) {
  const onUsersPage = activeTab === "users";
  return (
    <div className="flex min-h-screen flex-col bg-sand font-sans text-ink md:flex-row">
      {/* Desktop sidebar */}
      <aside className="hidden w-[248px] shrink-0 flex-col gap-1 border-r border-line bg-sand-2 p-3.5 md:flex">
        <div className="flex items-center gap-2.5 px-2 pb-5 pt-2">
          <span className="flex h-8.5 w-8.5 shrink-0 items-center justify-center rounded-[10px] bg-clay text-white">
            <ShieldIcon />
          </span>
          <span>
            <b className="block text-[15px] font-extrabold tracking-tight">Community Health Hub</b>
            <span className="block text-xs font-semibold text-ink-2">Admin command center</span>
          </span>
        </div>
        <Link
          to="/admin"
          aria-current={onUsersPage ? undefined : "page"}
          className={`flex h-11 items-center gap-2.5 rounded-md px-3 text-sm font-bold ${
            onUsersPage ? "text-ink-2 hover:bg-sand" : "bg-cream text-clay shadow-sm"
          }`}
        >
          <ShieldIcon />
          Verifications
          {typeof pendingCount === "number" && (
            <span className="ml-auto rounded-full bg-clay-tint px-2 py-0.5 text-xs font-bold text-clay-deep">
              {pendingCount}
            </span>
          )}
        </Link>
        <Link
          to="/admin/users"
          aria-current={onUsersPage ? "page" : undefined}
          className={`flex h-11 items-center gap-2.5 rounded-md px-3 text-sm font-bold ${
            onUsersPage ? "bg-cream text-clay shadow-sm" : "text-ink-2 hover:bg-sand"
          }`}
        >
          <UsersIcon />
          Users
        </Link>
        <div className="mt-auto flex items-center gap-2.5 border-t border-line-strong pt-3.5">
          <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-clay-tint text-[13px] font-extrabold text-clay-deep">
            SA
          </span>
          <span>
            <b className="block text-[13px] font-bold">System Admin</b>
            <small className="block text-[11.5px] font-semibold text-ink-2">Signed in</small>
          </span>
        </div>
      </aside>

      <div className="flex min-w-0 flex-1 flex-col">
        {/* Mobile appbar */}
        <div className="flex h-[58px] shrink-0 items-center gap-2.5 border-b border-line bg-cream px-4 md:hidden">
          <h1 className="flex-1 text-[17px] font-extrabold tracking-tight">{onUsersPage ? "Users" : "Verifications"}</h1>
          <span className="flex h-8.5 w-8.5 shrink-0 items-center justify-center rounded-full bg-clay-tint text-xs font-extrabold text-clay-deep">
            SA
          </span>
        </div>

        {/* Desktop topbar */}
        <div className="hidden h-[78px] shrink-0 items-center gap-3.5 border-b border-line bg-cream px-7 md:flex">
          <h1 className="m-0 text-[28px] font-extrabold leading-[34px] tracking-tight">
            {onUsersPage ? "Users" : "Facility verifications"}
          </h1>
          {!onUsersPage && activeTab === "pending" && typeof pendingCount === "number" && (
            <span className="rounded-full bg-amber-tint px-2.5 py-1.5 text-[12.5px] font-bold text-amber">
              {pendingCount} waiting
            </span>
          )}
          <div className="ml-auto flex items-center gap-2.5">
            <button
              type="button"
              disabled
              title="Notification log ships in a later story"
              className="flex h-[42px] cursor-not-allowed items-center gap-2 rounded-md border-[1.5px] border-line-strong bg-transparent px-4 text-[13.5px] font-bold text-ink opacity-60"
            >
              <BellIcon />
              Notification log
            </button>
          </div>
        </div>

        {/* Tab bar — shared markup across breakpoints. Only meaningful on the Verifications page. */}
        {!onUsersPage && (
          <div className="flex shrink-0 gap-5 overflow-x-auto border-b border-line bg-cream px-4 md:gap-6.5 md:px-7">
            {(["pending", "verified", "rejected"] as const).map((tab) => {
              const isOn = tab === activeTab;
              const isDisabled = tab !== "pending";
              return (
                <div
                  key={tab}
                  role={isDisabled ? undefined : "tab"}
                  aria-selected={isOn}
                  title={isDisabled ? "Coming in a later story" : undefined}
                  className={`flex h-[46px] shrink-0 items-center gap-1.5 whitespace-nowrap border-b-[2.5px] text-sm font-semibold md:h-12 ${
                    isOn
                      ? "border-clay font-bold text-ink"
                      : `border-transparent text-ink-3 ${isDisabled ? "cursor-not-allowed opacity-60" : ""}`
                  }`}
                >
                  {TAB_LABELS[tab]}
                  {tab === "pending" && typeof pendingCount === "number" && (
                    <span
                      className={`rounded-full px-1.5 py-px text-[11.5px] font-bold ${
                        isOn ? "bg-clay-tint text-clay-deep" : "bg-sand-2 text-ink-2"
                      }`}
                    >
                      {pendingCount}
                    </span>
                  )}
                </div>
              );
            })}
          </div>
        )}

        <main className="flex min-h-0 flex-1 flex-col">{children}</main>

        {/* Mobile bottom nav */}
        <nav className="flex h-[66px] shrink-0 items-center border-t border-line bg-cream px-1.5 md:hidden">
          <Link
            to="/admin"
            className={`flex flex-1 flex-col items-center gap-1 text-[11px] font-bold ${onUsersPage ? "text-ink-3" : "text-clay"}`}
          >
            <ShieldIcon className="h-5 w-5" />
            Verify
          </Link>
          <Link
            to="/admin/users"
            className={`flex flex-1 flex-col items-center gap-1 text-[11px] font-bold ${onUsersPage ? "text-clay" : "text-ink-3"}`}
          >
            <UsersIcon className="h-5 w-5" />
            Users
          </Link>
          <span
            className="flex flex-1 cursor-not-allowed flex-col items-center gap-1 text-[11px] font-semibold text-ink-3 opacity-60"
            title="Activity log ships in a later story"
          >
            <BellIcon className="h-5 w-5" />
            Activity
          </span>
        </nav>
      </div>
    </div>
  );
}

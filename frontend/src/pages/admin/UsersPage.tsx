import { useState } from "react";

import { AdminShell } from "../../components/admin/AdminShell";
import { SuspendUserModal } from "../../components/admin/SuspendUserModal";
import { BanIcon, ChevronLeftIcon, ChevronRightIcon, CloudOffIcon, InboxIcon, RetryIcon, SearchIcon } from "../../components/admin/icons";
import { useAuth } from "../../context/AuthProvider";
import type { AdminUserDto } from "../../api/adminApi";
import { DEFAULT_PAGE_SIZE, useAdminUsers } from "../../features/admin/useAdminUsers";

function userInitials(name: string): string {
  const words = name.trim().split(/\s+/);
  return ((words[0]?.[0] ?? "") + (words[1]?.[0] ?? "")).toUpperCase();
}

function StatusBadge({ user }: { user: AdminUserDto }) {
  if (user.accountStatus === "Suspended") {
    return (
      <span
        title={user.suspensionReason ?? undefined}
        className="inline-flex items-center gap-1.5 rounded-full border border-blood-tint bg-error-tint px-2.5 py-1 text-[12.5px] font-bold text-error"
      >
        Suspended
      </span>
    );
  }
  return (
    <span className="inline-flex items-center gap-1.5 rounded-full border border-go bg-go-tint px-2.5 py-1 text-[12.5px] font-bold text-go-deep">
      Active
    </span>
  );
}

function UserRow({ user, onSuspend }: { user: AdminUserDto; onSuspend: (user: AdminUserDto) => void }) {
  const isSuspended = user.accountStatus === "Suspended";
  return (
    <tr className="border-b border-line last:border-0">
      <td className="h-[66px] px-4.5">
        <div className="flex items-center gap-3">
          <span className="flex h-9.5 w-9.5 shrink-0 items-center justify-center rounded-md bg-clay-tint text-[13px] font-extrabold text-clay-deep">
            {userInitials(user.fullName)}
          </span>
          <span>
            <b className="block text-[15px] font-bold leading-5">{user.fullName}</b>
            <small className="text-[12.5px] text-ink-2">{user.mobileNumber}</small>
          </span>
        </div>
      </td>
      <td className="h-[66px] px-4.5 text-sm">{user.bloodGroup}</td>
      <td className="h-[66px] px-4.5">
        <StatusBadge user={user} />
      </td>
      <td className="h-[66px] px-4.5 text-right">
        <button
          type="button"
          onClick={() => onSuspend(user)}
          disabled={isSuspended}
          className="inline-flex h-9 items-center gap-1.5 rounded-[10px] bg-error px-4 text-[13px] font-bold text-white disabled:cursor-not-allowed disabled:opacity-40"
        >
          <BanIcon />
          {isSuspended ? "Suspended" : "Suspend"}
        </button>
      </td>
    </tr>
  );
}

// CHH-76/US-CHH-001-04 — the Users tab of the Admin Command Center: search by mobile number or
// name (UI Notes), then suspend an account with a mandatory reason (AC1).
export function UsersPage() {
  const { session } = useAuth();
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [suspendingUser, setSuspendingUser] = useState<AdminUserDto | null>(null);
  const { status, items, totalPages, totalCount, refetch } = useAdminUsers(session?.token, search, page);

  return (
    <AdminShell activeTab="users">
      <div className="flex min-h-0 flex-1 flex-col gap-3.5 p-4 md:p-7">
        <div className="relative">
          <span className="pointer-events-none absolute left-3.5 top-1/2 -translate-y-1/2 text-ink-3">
            <SearchIcon />
          </span>
          <input
            type="search"
            value={search}
            onChange={(e) => {
              setSearch(e.target.value);
              setPage(1);
            }}
            placeholder="Search by mobile number or name"
            aria-label="Search users"
            className="h-12 w-full rounded-md border border-line-strong bg-cream pl-10 pr-3.5 text-sm text-ink placeholder:text-ink-3 md:max-w-sm"
          />
        </div>

        <div className="flex min-h-0 flex-1 flex-col overflow-hidden rounded-md border border-line bg-cream shadow-sm">
          {status === "loading" && (
            <div className="flex flex-col gap-3 p-4">
              {[1, 2, 3, 4].map((n) => (
                <div key={n} className="flex items-center gap-3">
                  <span className="h-9.5 w-9.5 shrink-0 rounded-md bg-sand-2" />
                  <span className="flex flex-col gap-1.5">
                    <span className="h-3.5 w-[190px] rounded-sm bg-sand-2" />
                    <span className="h-2.5 w-[104px] rounded-sm bg-sand-2" />
                  </span>
                </div>
              ))}
            </div>
          )}

          {status === "error" && (
            <div className="flex flex-1 flex-col items-center justify-center gap-3 p-10 text-center">
              <span className="flex h-[68px] w-[68px] items-center justify-center rounded-full bg-error-tint text-error">
                <CloudOffIcon />
              </span>
              <b className="text-[17px] font-bold">The user list didn&apos;t load.</b>
              <p className="max-w-[46ch] text-sm leading-relaxed text-ink-2">
                The connection dropped before the list arrived. Nothing has changed.
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
              <b className="text-[17px] font-bold">
                {search.trim() ? "No users match that search." : "No individual accounts registered yet."}
              </b>
            </div>
          )}

          {status === "list" && (
            <table className="w-full border-collapse">
              <thead>
                <tr className="border-b border-line bg-sand-2">
                  <th className="h-11 px-4.5 text-left text-[11px] font-bold uppercase tracking-wider text-ink-2">User</th>
                  <th className="h-11 px-4.5 text-left text-[11px] font-bold uppercase tracking-wider text-ink-2">
                    Blood group
                  </th>
                  <th className="h-11 px-4.5 text-left text-[11px] font-bold uppercase tracking-wider text-ink-2">Status</th>
                  <th className="h-11 px-4.5" />
                </tr>
              </thead>
              <tbody>
                {items.map((user) => (
                  <UserRow key={user.id} user={user} onSuspend={setSuspendingUser} />
                ))}
              </tbody>
            </table>
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
      </div>

      {suspendingUser && (
        <SuspendUserModal
          accessToken={session?.token}
          user={suspendingUser}
          onClose={() => setSuspendingUser(null)}
          onSuspended={() => {
            setSuspendingUser(null);
            refetch();
          }}
        />
      )}
    </AdminShell>
  );
}

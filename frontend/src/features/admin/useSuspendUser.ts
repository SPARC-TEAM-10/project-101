import { useMutation } from "@tanstack/react-query";

import { suspendUser, type AdminUserDto } from "../../api/adminApi";
import { ApiError } from "../../api/httpClient";

export interface SuspendUserError {
  status: number | null;
  message: string;
}

function toSuspendError(err: unknown): SuspendUserError {
  if (err instanceof ApiError) {
    return { status: err.status, message: err.problem.detail ?? "Couldn't suspend this account. Try again." };
  }
  return { status: null, message: "Couldn't suspend this account. Try again." };
}

// CHH-76/US-CHH-001-04 AC1 — suspend a user account with a mandatory reason. The caller (Users
// page's suspend modal) owns refetching the user list on success.
export function useSuspendUser(accessToken: string | undefined) {
  const mutation = useMutation<AdminUserDto, unknown, { userId: string; reason: string }>({
    mutationFn: ({ userId, reason }) => {
      if (!accessToken) {
        throw new Error("Not authenticated");
      }
      return suspendUser(accessToken, userId, reason);
    },
  });

  async function submit(userId: string, reason: string) {
    try {
      const data = await mutation.mutateAsync({ userId, reason });
      return { ok: true as const, data };
    } catch (err) {
      return { ok: false as const, error: toSuspendError(err) };
    }
  }

  return {
    submit,
    reset: mutation.reset,
    isPending: mutation.isPending,
  };
}

import { useQuery, useQueryClient, useMutation } from "@tanstack/react-query";

import { getBloodRequestMatchStatus, updateBloodRequestRadius } from "../../api/bloodRequestApi";
import { ApiError } from "../../api/httpClient";

// 15s poll — this is the "live-ish" dashboard the requester is expected to actively watch while
// waiting for donors to respond, so a shorter interval than the notification center's 30s.
const POLL_INTERVAL_MS = 15_000;

function queryKey(accessToken: string | undefined, bloodRequestId: string) {
  return ["blood-requests", bloodRequestId, "matches", accessToken] as const;
}

/**
 * Fetches a requester's own request's match/response status (CHH-36), polling while the request
 * is still active, and exposes an increase-radius mutation (AC4).
 */
export function useBloodRequestMatchStatus(accessToken: string | undefined, bloodRequestId: string) {
  const queryClient = useQueryClient();

  const query = useQuery({
    queryKey: queryKey(accessToken, bloodRequestId),
    queryFn: () => getBloodRequestMatchStatus(accessToken!, bloodRequestId),
    enabled: Boolean(accessToken) && Boolean(bloodRequestId),
    refetchInterval: (q) => (q.state.data?.status === "Matching" ? POLL_INTERVAL_MS : false),
  });

  const increaseRadiusMutation = useMutation({
    mutationFn: (newRadiusKm: number) => {
      if (!accessToken) throw new Error("Not authenticated");
      return updateBloodRequestRadius(accessToken, bloodRequestId, newRadiusKm);
    },
    onSuccess: (result) => {
      queryClient.setQueryData(queryKey(accessToken, bloodRequestId), result);
    },
  });

  function increaseRadiusErrorMessage(): string | null {
    const err = increaseRadiusMutation.error;
    if (!err) return null;
    return err instanceof ApiError ? (err.problem.detail ?? "Couldn't update the radius.") : "Couldn't update the radius.";
  }

  return {
    status: query.data,
    isLoading: query.isLoading,
    isError: query.isError,
    increaseRadius: increaseRadiusMutation.mutateAsync,
    isIncreasingRadius: increaseRadiusMutation.isPending,
    increaseRadiusError: increaseRadiusErrorMessage(),
  };
}

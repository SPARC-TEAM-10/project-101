import { useMutation } from "@tanstack/react-query";

import { reviewFacility, type FacilityDto, type FacilityVerificationDecision } from "../../api/adminApi";
import { ApiError } from "../../api/httpClient";

export interface ReviewFacilityError {
  status: number | null;
  message: string;
}

function toReviewError(err: unknown): ReviewFacilityError {
  if (err instanceof ApiError) {
    return { status: err.status, message: err.problem.detail ?? "Couldn't submit the review. Try again." };
  }
  return { status: null, message: "Couldn't submit the review. Try again." };
}

// CHH-75/US-CHH-001-03 — approve/reject a pending facility. The caller (PendingVerificationsPage's
// review modal) owns refetching the pending list on success; this hook only owns the mutation.
export function useReviewFacility(accessToken: string | undefined) {
  const mutation = useMutation<FacilityDto, unknown, { facilityId: string; decision: FacilityVerificationDecision; rejectionReason?: string }>({
    mutationFn: ({ facilityId, decision, rejectionReason }) => {
      if (!accessToken) {
        throw new Error("Not authenticated");
      }
      return reviewFacility(accessToken, facilityId, decision, rejectionReason);
    },
  });

  async function submit(facilityId: string, decision: FacilityVerificationDecision, rejectionReason?: string) {
    try {
      const data = await mutation.mutateAsync({ facilityId, decision, rejectionReason });
      return { ok: true as const, data };
    } catch (err) {
      return { ok: false as const, error: toReviewError(err) };
    }
  }

  return {
    submit,
    reset: mutation.reset,
    isPending: mutation.isPending,
  };
}

import { useQuery } from "@tanstack/react-query";

import { getMyBloodRequests, type BloodRequestDto } from "../../api/bloodRequestApi";
import { ApiError } from "../../api/httpClient";
import { getMyProfile, type IndividualProfileDto } from "../../api/individualApi";

export interface IndividualDashboardData {
  profile: IndividualProfileDto | null;
  needsRegistration: boolean;
  requests: BloodRequestDto[];
  activeRequest: BloodRequestDto | null;
}

function isActive(request: BloodRequestDto): boolean {
  return request.status === "Matching" && new Date(request.expiresAtUtc).getTime() > Date.now();
}

export function useIndividualDashboard(accessToken: string | undefined) {
  const profileQuery = useQuery({
    queryKey: ["individual", "me", accessToken],
    queryFn: () => getMyProfile(accessToken!),
    enabled: Boolean(accessToken),
    // A 404 here means "not registered yet," not a transient failure — retrying wastes a request.
    retry: (failureCount, error) => error instanceof ApiError && error.status === 404 ? false : failureCount < 2,
  });

  const requestsQuery = useQuery({
    queryKey: ["blood-requests", "mine", accessToken],
    queryFn: () => getMyBloodRequests(accessToken!),
    enabled: Boolean(accessToken),
  });

  const needsRegistration = profileQuery.error instanceof ApiError && profileQuery.error.status === 404;
  const requests = requestsQuery.data?.items ?? [];
  const activeRequest = requests.find(isActive) ?? null;

  const data: IndividualDashboardData = {
    profile: profileQuery.data ?? null,
    needsRegistration,
    requests,
    activeRequest,
  };

  return {
    data,
    isLoading: profileQuery.isLoading || requestsQuery.isLoading,
    // A 404 profile is a normal, handled state (needsRegistration), not an error to surface.
    isError: (profileQuery.isError && !needsRegistration) || requestsQuery.isError,
  };
}

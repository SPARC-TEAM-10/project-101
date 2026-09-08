import { useQuery } from "@tanstack/react-query";

import { getFacilityById, type PublicFacilityDto } from "../../api/facilityApi";

export interface UseFacilityDetailResult {
  facility: PublicFacilityDto | undefined;
  isLoading: boolean;
  isError: boolean;
}

/**
 * Fetches one facility's public detail (CHH-70/US-CHH-001-02, Epic CHH-68) for the detail modal.
 * Only enabled while a facility id is selected, so closing the modal doesn't leave a stale fetch
 * running.
 */
export function useFacilityDetail(accessToken: string | undefined, facilityId: string | null): UseFacilityDetailResult {
  const query = useQuery({
    queryKey: ["facilities", "detail", facilityId],
    queryFn: () => getFacilityById(accessToken, facilityId!),
    enabled: Boolean(accessToken && facilityId),
    retry: false,
  });

  return {
    facility: query.data,
    isLoading: query.isLoading,
    isError: query.isError,
  };
}

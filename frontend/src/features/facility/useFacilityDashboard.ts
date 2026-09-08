import { useQuery } from "@tanstack/react-query";

import { getMyFacility, type FacilityDto } from "../../api/facilityApi";

export function useFacilityDashboard(accessToken: string | undefined) {
  const query = useQuery({
    queryKey: ["facility", "me", accessToken],
    queryFn: () => getMyFacility(accessToken!),
    enabled: Boolean(accessToken),
  });

  return {
    facility: query.data as FacilityDto | undefined,
    isLoading: query.isLoading,
    isError: query.isError,
  };
}

import { useQuery } from "@tanstack/react-query";

import { getPendingFacilities, type FacilityDto } from "../../api/adminApi";

export const DEFAULT_PAGE_SIZE = 20;

export type PendingFacilitiesStatus = "loading" | "error" | "empty" | "list";

export interface UsePendingFacilitiesResult {
  status: PendingFacilitiesStatus;
  items: FacilityDto[];
  page: number;
  totalPages: number;
  totalCount: number;
  refetch: () => void;
}

// Matches the design canvas's AdminQueueWeb/Mobile artboards' explicit state machine
// (list/empty/loading/error) rather than exposing raw TanStack Query flags to the page.
export function usePendingFacilities(
  accessToken: string | undefined,
  page: number,
  pageSize: number = DEFAULT_PAGE_SIZE,
): UsePendingFacilitiesResult {
  const query = useQuery({
    queryKey: ["admin", "facilities", "pending", page, pageSize],
    queryFn: () => getPendingFacilities(accessToken!, page, pageSize),
    enabled: Boolean(accessToken),
    retry: false,
  });

  let status: PendingFacilitiesStatus;
  if (query.isPending) {
    status = "loading";
  } else if (query.isError) {
    status = "error";
  } else if ((query.data?.items.length ?? 0) === 0) {
    status = "empty";
  } else {
    status = "list";
  }

  return {
    status,
    items: query.data?.items ?? [],
    page: query.data?.page ?? page,
    totalPages: query.data?.totalPages ?? 1,
    totalCount: query.data?.totalCount ?? 0,
    refetch: () => query.refetch(),
  };
}

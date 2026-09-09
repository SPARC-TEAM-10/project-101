import { useQuery } from "@tanstack/react-query";

import { searchAdminUsers, type AdminUserDto } from "../../api/adminApi";

export const DEFAULT_PAGE_SIZE = 20;

export type AdminUsersStatus = "loading" | "error" | "empty" | "list";

export interface UseAdminUsersResult {
  status: AdminUsersStatus;
  items: AdminUserDto[];
  page: number;
  totalPages: number;
  totalCount: number;
  refetch: () => void;
}

// CHH-76/US-CHH-001-04 — the Users tab's search/list, mirroring usePendingFacilities' explicit
// state machine rather than exposing raw TanStack Query flags to the page.
export function useAdminUsers(
  accessToken: string | undefined,
  search: string,
  page: number,
  pageSize: number = DEFAULT_PAGE_SIZE,
): UseAdminUsersResult {
  const query = useQuery({
    queryKey: ["admin", "users", search, page, pageSize],
    queryFn: () => searchAdminUsers(accessToken!, search, page, pageSize),
    enabled: Boolean(accessToken),
    retry: false,
  });

  let status: AdminUsersStatus;
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

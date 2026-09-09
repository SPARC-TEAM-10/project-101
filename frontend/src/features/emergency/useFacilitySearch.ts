import { useEffect, useState } from "react";
import { useQuery } from "@tanstack/react-query";

import { searchFacilities, type EmergencySearchCategory, type PublicFacilityDto } from "../../api/facilityApi";
import { useGeolocation } from "../shared/useGeolocation";

export type FacilitySearchStatus = "loading" | "error" | "empty" | "list";

export interface UseFacilitySearchResult {
  q: string;
  setQ: (value: string) => void;
  category: EmergencySearchCategory | undefined;
  setCategory: (value: EmergencySearchCategory | undefined) => void;
  geolocationStatus: ReturnType<typeof useGeolocation>["status"];
  results: PublicFacilityDto[];
  status: FacilitySearchStatus;
  clearFilters: () => void;
  refetch: () => void;
}

// Debounced to avoid firing a request on every keystroke while typing the search box (CHH-69 AC2).
const DEBOUNCE_MS = 300;

function useDebouncedValue<T>(value: T, delayMs: number): T {
  const [debounced, setDebounced] = useState(value);

  useEffect(() => {
    const timeout = setTimeout(() => setDebounced(value), delayMs);
    return () => clearTimeout(timeout);
  }, [value, delayMs]);

  return debounced;
}

/**
 * Drives the Emergency Services Hub search (CHH-69/US-CHH-001-01, Epic CHH-68). Composes
 * useGeolocation for proximity sorting (AC3) — coordinates are optional; the search still works
 * on q/category alone when location is denied (CHH-71 AC2).
 */
export function useFacilitySearch(accessToken: string | undefined): UseFacilitySearchResult {
  const [q, setQ] = useState("");
  const [category, setCategory] = useState<EmergencySearchCategory | undefined>(undefined);
  const geolocation = useGeolocation();
  const debouncedQ = useDebouncedValue(q, DEBOUNCE_MS);

  useEffect(() => {
    geolocation.request();
    // Runs once on mount — CHH-71 AC2 requires the denied/manual-entry prompt to appear as soon
    // as the Hub opens, not only after an explicit user action.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const query = useQuery({
    queryKey: ["facilities", "search", debouncedQ, category, geolocation.coordinates],
    queryFn: () =>
      searchFacilities(accessToken, {
        q: debouncedQ || undefined,
        category,
        latitude: geolocation.coordinates?.latitude,
        longitude: geolocation.coordinates?.longitude,
      }),
    enabled: Boolean(accessToken),
    retry: false,
  });

  let status: FacilitySearchStatus;
  if (query.isPending) {
    status = "loading";
  } else if (query.isError) {
    status = "error";
  } else if ((query.data?.items.length ?? 0) === 0) {
    status = "empty";
  } else {
    status = "list";
  }

  function clearFilters() {
    setQ("");
    setCategory(undefined);
  }

  return {
    q,
    setQ,
    category,
    setCategory,
    geolocationStatus: geolocation.status,
    results: query.data?.items ?? [],
    status,
    clearFilters,
    refetch: () => query.refetch(),
  };
}

import { useEffect, useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";

import { searchEvents, type EventSummaryDto } from "../../api/eventApi";
import type { EventType } from "../../lib/validation/eventSchemas";
import { useGeolocation } from "../shared/useGeolocation";
import { useVenueGeocoding } from "./useVenueGeocoding";

export const MIN_RADIUS_KM = 5;
export const MAX_RADIUS_KM = 100;
export const DEFAULT_RADIUS_KM = 15;

export type DiscoveryViewMode = "list" | "map";

export function useEventDiscovery(accessToken: string | undefined) {
  const [radiusKm, setRadiusKm] = useState(DEFAULT_RADIUS_KM);
  const [eventType, setEventType] = useState<EventType | undefined>(undefined);
  const [viewMode, setViewMode] = useState<DiscoveryViewMode>("list");
  const [hideFullEvents, setHideFullEvents] = useState(false);
  const [manualLocationText, setManualLocationText] = useState("");

  const geolocation = useGeolocation();
  const manualGeocoding = useVenueGeocoding(manualLocationText);

  // Auto-request device location once on mount — Discovery.dc.html's "list" state has no visible
  // "detect location" affordance; DiscoveryWeb.dc.html's "gpsDenied" state is what renders when
  // this fails, not a state the user opts into.
  useEffect(() => {
    geolocation.request();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const isGpsDenied = geolocation.status === "denied" || geolocation.status === "unavailable";
  const coordinates = geolocation.coordinates ?? manualGeocoding.coordinates;

  const query = useQuery({
    queryKey: ["events", "search", coordinates, radiusKm, eventType, accessToken],
    queryFn: () =>
      searchEvents(accessToken, {
        latitude: coordinates!.latitude,
        longitude: coordinates!.longitude,
        radiusKm,
        eventType,
      }),
    enabled: Boolean(coordinates),
  });

  const events: EventSummaryDto[] = query.data ?? [];
  const visibleEvents = useMemo(
    () => (hideFullEvents ? events.filter((e) => e.spotsRemaining > 0) : events),
    [events, hideFullEvents],
  );

  return {
    radiusKm,
    setRadiusKm: (km: number) => setRadiusKm(Math.min(MAX_RADIUS_KM, Math.max(MIN_RADIUS_KM, km))),
    eventType,
    setEventType,
    viewMode,
    setViewMode,
    hideFullEvents,
    setHideFullEvents,
    manualLocationText,
    setManualLocationText,
    geolocation,
    isGpsDenied,
    coordinates,
    events: visibleEvents,
    isLoading: query.isLoading,
    isError: query.isError,
    hasSearched: Boolean(coordinates),
  };
}

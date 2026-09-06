import { useCallback, useState } from "react";

export interface Coordinates {
  latitude: number;
  longitude: number;
}

export type GeolocationStatus = "idle" | "locating" | "resolved" | "denied" | "unavailable";

export interface UseGeolocationResult {
  coordinates: Coordinates | null;
  status: GeolocationStatus;
  request: () => void;
}

// Uses the browser's Geolocation API directly — no maps/geocoding provider is configured yet
// (backend/CLAUDE.md's Tech Stack row: "Maps / Geo API — not yet decided"), and this needs no
// API key. "denied"/"unavailable" is this story's Edge Case ("Requester location coordinates
// cannot be resolved").
export function useGeolocation(): UseGeolocationResult {
  const [coordinates, setCoordinates] = useState<Coordinates | null>(null);
  const [status, setStatus] = useState<GeolocationStatus>("idle");

  const request = useCallback(() => {
    if (!("geolocation" in navigator)) {
      setStatus("unavailable");
      return;
    }

    setStatus("locating");
    navigator.geolocation.getCurrentPosition(
      (position) => {
        setCoordinates({
          latitude: position.coords.latitude,
          longitude: position.coords.longitude,
        });
        setStatus("resolved");
      },
      () => {
        setStatus("denied");
      },
      { enableHighAccuracy: false, timeout: 10_000 },
    );
  }, []);

  return { coordinates, status, request };
}

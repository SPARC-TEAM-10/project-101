import { useCallback, useEffect, useRef, useState } from "react";

export interface VenueCoordinates {
  latitude: number;
  longitude: number;
}

export type VenueGeocodingStatus = "idle" | "resolving" | "resolved" | "notFound" | "error";

// Forward-geocodes a typed venue address to coordinates via OpenStreetMap's free Nominatim
// /search endpoint — the mirror image of useGeolocation.ts's reverse-geocoding call, same
// no-API-key posture (see that file's doc comment on why no maps provider is configured yet).
async function forwardGeocode(address: string): Promise<VenueCoordinates | null> {
  const res = await fetch(
    `https://nominatim.openstreetmap.org/search?format=jsonv2&q=${encodeURIComponent(address)}&limit=1`,
    { headers: { Accept: "application/json" } },
  );
  if (!res.ok) {
    return null;
  }
  const data = (await res.json()) as Array<{ lat: string; lon: string }>;
  const first = data[0];
  if (!first) {
    return null;
  }
  return { latitude: Number(first.lat), longitude: Number(first.lon) };
}

/**
 * Resolves venue coordinates from a typed address (CreateEvent.dc.html: "Resolved from the
 * address"), debounced so it doesn't fire on every keystroke, with a "Move pin" escape hatch
 * (setManualCoordinates) that stops auto-resolution from overwriting a manual placement.
 */
export function useVenueGeocoding(address: string) {
  const [coordinates, setCoordinatesState] = useState<VenueCoordinates | null>(null);
  const [status, setStatus] = useState<VenueGeocodingStatus>("idle");
  const isManualRef = useRef(false);
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  useEffect(() => {
    if (debounceRef.current) {
      clearTimeout(debounceRef.current);
    }
    if (isManualRef.current) {
      return;
    }
    const trimmed = address.trim();
    if (trimmed.length < MIN_VENUE_ADDRESS_LENGTH_FOR_LOOKUP) {
      setStatus("idle");
      return;
    }

    setStatus("resolving");
    debounceRef.current = setTimeout(() => {
      forwardGeocode(trimmed)
        .then((coords) => {
          if (coords) {
            setCoordinatesState(coords);
            setStatus("resolved");
          } else {
            setStatus("notFound");
          }
        })
        .catch(() => setStatus("error"));
    }, DEBOUNCE_MS);

    return () => {
      if (debounceRef.current) {
        clearTimeout(debounceRef.current);
      }
    };
  }, [address]);

  const setManualCoordinates = useCallback((coords: VenueCoordinates) => {
    isManualRef.current = true;
    setCoordinatesState(coords);
    setStatus("resolved");
  }, []);

  return { coordinates, status, setManualCoordinates };
}

const DEBOUNCE_MS = 800;
const MIN_VENUE_ADDRESS_LENGTH_FOR_LOOKUP = 10;

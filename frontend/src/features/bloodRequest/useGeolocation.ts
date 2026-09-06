import { useCallback, useState } from "react";

export interface Coordinates {
  latitude: number;
  longitude: number;
}

export type GeolocationStatus = "idle" | "locating" | "resolved" | "denied" | "unavailable";

export interface UseGeolocationResult {
  coordinates: Coordinates | null;
  status: GeolocationStatus;
  addressLabel: string | null;
  isResolvingAddress: boolean;
  request: () => void;
}

// Reverse-geocodes resolved coordinates to a human-readable area/pincode via OpenStreetMap's
// free Nominatim API — no API key needed, no maps provider decision required yet
// (backend/CLAUDE.md's Tech Stack row: "Maps / Geo API — not yet decided"). Nominatim's usage
// policy caps this at ~1 request/second and asks for a descriptive header; fine for this
// hackathon's traffic, but revisit if this ever needs to scale (their policy explicitly
// discourages heavy production use of the free endpoint).
async function reverseGeocode(latitude: number, longitude: number): Promise<string | null> {
  try {
    // accept-language=en forces English place names — Nominatim otherwise returns the name in
    // whatever language OSM has tagged locally (e.g. Malayalam in Kerala), which isn't usable
    // as free-text the backend/validator expects to render back in English contexts.
    const res = await fetch(
      `https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat=${latitude}&lon=${longitude}&accept-language=en`,
      { headers: { Accept: "application/json", "Accept-Language": "en" } },
    );
    if (!res.ok) {
      return null;
    }
    const data = (await res.json()) as {
      address?: {
        suburb?: string;
        neighbourhood?: string;
        city?: string;
        town?: string;
        village?: string;
        state_district?: string;
        postcode?: string;
      };
    };
    const area = data.address?.suburb ?? data.address?.neighbourhood;
    const city = data.address?.city ?? data.address?.town ?? data.address?.village ?? data.address?.state_district;
    const namePart = [area, city].filter(Boolean).join(", ");
    // Prefer the resolved name alone — only fall back to the bare pincode when no name at all
    // could be resolved, rather than always appending both.
    if (namePart) {
      return namePart;
    }
    return data.address?.postcode ?? null;
  } catch {
    return null;
  }
}

// Uses the browser's Geolocation API directly for coordinates, then Nominatim for a readable
// label — no paid maps/geocoding provider is configured yet. "denied"/"unavailable" is this
// story's Edge Case ("Requester location coordinates cannot be resolved").
export function useGeolocation(): UseGeolocationResult {
  const [coordinates, setCoordinates] = useState<Coordinates | null>(null);
  const [status, setStatus] = useState<GeolocationStatus>("idle");
  const [addressLabel, setAddressLabel] = useState<string | null>(null);
  const [isResolvingAddress, setIsResolvingAddress] = useState(false);

  const request = useCallback(() => {
    if (!("geolocation" in navigator)) {
      setStatus("unavailable");
      return;
    }

    setStatus("locating");
    navigator.geolocation.getCurrentPosition(
      (position) => {
        const coords = {
          latitude: position.coords.latitude,
          longitude: position.coords.longitude,
        };
        setCoordinates(coords);
        setStatus("resolved");

        setIsResolvingAddress(true);
        reverseGeocode(coords.latitude, coords.longitude)
          .then(setAddressLabel)
          .finally(() => setIsResolvingAddress(false));
      },
      () => {
        setStatus("denied");
      },
      { enableHighAccuracy: false, timeout: 10_000 },
    );
  }, []);

  return { coordinates, status, addressLabel, isResolvingAddress, request };
}

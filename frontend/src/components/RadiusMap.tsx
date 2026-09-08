import { useEffect } from "react";
import { Circle, MapContainer, TileLayer, useMap } from "react-leaflet";

// Recenters/refits the map whenever the request location or radius changes — MapContainer's
// own `center`/`zoom` props are only read on first mount, not on later re-renders.
function MapViewSync({ center, radiusKm }: { center: [number, number]; radiusKm: number }) {
  const map = useMap();

  useEffect(() => {
    const radiusMeters = radiusKm * 1000;
    const circleBounds = [
      [center[0] - radiusMeters / 111_320, center[1] - radiusMeters / (111_320 * Math.cos((center[0] * Math.PI) / 180))],
      [center[0] + radiusMeters / 111_320, center[1] + radiusMeters / (111_320 * Math.cos((center[0] * Math.PI) / 180))],
    ] as [[number, number], [number, number]];
    map.fitBounds(circleBounds, { padding: [16, 16] });
  }, [map, center, radiusKm]);

  return null;
}

/**
 * Real interactive map (OpenStreetMap tiles via Leaflet — no paid maps API key needed, same
 * no-key posture as the existing Nominatim reverse-geocoding call) showing the request's
 * resolved location and its search radius, replacing the earlier schematic concentric-circle
 * placeholder (CHH-33/US-CHH-004-01 UI Notes: "Show a map preview of the selected radius").
 */
export function RadiusMap({ latitude, longitude, radiusKm }: { latitude: number; longitude: number; radiusKm: number }) {
  const center: [number, number] = [latitude, longitude];

  return (
    <div className="h-48 w-full overflow-hidden rounded-lg border border-line">
      <MapContainer center={center} zoom={12} scrollWheelZoom={false} className="h-full w-full" attributionControl={false}>
        <TileLayer url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" />
        <Circle center={center} radius={radiusKm * 1000} pathOptions={{ color: "#d32f2f", fillColor: "#d32f2f", fillOpacity: 0.12, weight: 2 }} />
        <Circle center={center} radius={80} pathOptions={{ color: "#d32f2f", fillColor: "#d32f2f", fillOpacity: 1, weight: 0 }} />
        <MapViewSync center={center} radiusKm={radiusKm} />
      </MapContainer>
    </div>
  );
}

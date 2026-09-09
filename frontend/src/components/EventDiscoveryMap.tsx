import { useEffect, useMemo } from "react";
import { Circle, MapContainer, Marker, TileLayer, useMap } from "react-leaflet";
import L from "leaflet";

import type { EventSummaryDto } from "../api/eventApi";

function MapViewSync({ center, radiusKm }: { center: [number, number]; radiusKm: number }) {
  const map = useMap();

  useEffect(() => {
    const radiusMeters = radiusKm * 1000;
    const circleBounds = [
      [center[0] - radiusMeters / 111_320, center[1] - radiusMeters / (111_320 * Math.cos((center[0] * Math.PI) / 180))],
      [center[0] + radiusMeters / 111_320, center[1] + radiusMeters / (111_320 * Math.cos((center[0] * Math.PI) / 180))],
    ] as [[number, number], [number, number]];
    map.fitBounds(circleBounds, { padding: [24, 24] });
  }, [map, center, radiusKm]);

  return null;
}

// Blue dot for "you are here" — matches DiscoveryWeb.dc.html's .me marker.
const ME_ICON = L.divIcon({
  className: "",
  html: '<span style="display:block;width:16px;height:16px;border-radius:999px;background:#3B6EA5;border:3px solid #FDFAF4;box-shadow:0 0 0 6px rgba(59,110,165,.16)"></span>',
  iconSize: [16, 16],
  iconAnchor: [8, 8],
});

// Same inline SVG pin as VenuePinMap, non-draggable here — one per discovered event.
const EVENT_PIN_ICON = L.divIcon({
  className: "",
  html:
    '<svg width="28" height="28" viewBox="0 0 24 24" fill="#B06134" xmlns="http://www.w3.org/2000/svg">' +
    '<path d="M12 22s-7-5.2-7-11a7 7 0 1 1 14 0c0 5.8-7 11-7 11Z"/>' +
    '<circle cx="12" cy="10.6" r="2.6" fill="#FDFAF4"/></svg>',
  iconSize: [28, 28],
  iconAnchor: [14, 26],
});

/**
 * Map view for event discovery (CHH-39/US-CHH-005-02 UI Notes: "Toggle between List and Map
 * view") — extends RadiusMap.tsx's halo-circle pattern (search radius) with one pin per event,
 * reusing VenuePinMap's inline-SVG-icon approach (non-draggable this time).
 */
export function EventDiscoveryMap({
  latitude,
  longitude,
  radiusKm,
  events,
}: {
  latitude: number;
  longitude: number;
  radiusKm: number;
  events: EventSummaryDto[];
}) {
  const center = useMemo<[number, number]>(() => [latitude, longitude], [latitude, longitude]);

  return (
    <div className="h-[360px] w-full overflow-hidden rounded-lg border border-line">
      <MapContainer center={center} zoom={12} scrollWheelZoom={false} className="h-full w-full" attributionControl={false}>
        <TileLayer url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" />
        <Circle center={center} radius={radiusKm * 1000} pathOptions={{ color: "#B06134", fillColor: "#B06134", fillOpacity: 0.06, weight: 1.5, dashArray: "4 4" }} />
        <Marker position={center} icon={ME_ICON} />
        {events.map((event) => (
          <Marker key={event.id} position={[event.latitude, event.longitude]} icon={EVENT_PIN_ICON} />
        ))}
        <MapViewSync center={center} radiusKm={radiusKm} />
      </MapContainer>
    </div>
  );
}

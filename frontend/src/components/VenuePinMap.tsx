import { useEffect, useMemo, useRef } from "react";
import { MapContainer, Marker, TileLayer, useMap, useMapEvents } from "react-leaflet";
import L from "leaflet";

function MapCenterSync({ center }: { center: [number, number] }) {
  const map = useMap();
  const first = useRef(true);

  useEffect(() => {
    // First render only recenters silently (MapContainer's own center prop is mount-only) — a
    // later address-driven move still animates, but doesn't fight a drag the user just did.
    map.setView(center, map.getZoom() < 13 ? 15 : map.getZoom(), { animate: !first.current });
    first.current = false;
  }, [map, center]);

  return null;
}

function ClickToMove({ onMove }: { onMove: (lat: number, lng: number) => void }) {
  useMapEvents({
    click(e) {
      onMove(e.latlng.lat, e.latlng.lng);
    },
  });
  return null;
}

// Inline SVG pin as a DivIcon — avoids Leaflet's default marker image path breaking under Vite's
// bundler (a well-known react-leaflet gotcha), and matches the design's clay pin exactly.
const PIN_ICON = L.divIcon({
  className: "",
  html:
    '<svg width="32" height="32" viewBox="0 0 24 24" fill="#B06134" xmlns="http://www.w3.org/2000/svg">' +
    '<path d="M12 22s-7-5.2-7-11a7 7 0 1 1 14 0c0 5.8-7 11-7 11Z"/>' +
    '<circle cx="12" cy="10.6" r="2.6" fill="#FDFAF4"/></svg>',
  iconSize: [32, 32],
  iconAnchor: [16, 30],
});

/**
 * Draggable pin picker for an event venue (CHH-38/US-CHH-005-01) — the "Move pin" affordance in
 * CreateEvent.dc.html/CreateEventWeb.dc.html. Reuses RadiusMap.tsx's no-API-key OSM tile setup;
 * unlike that component, this shows one movable marker rather than a fixed radius circle.
 */
export function VenuePinMap({
  latitude,
  longitude,
  onMove,
}: {
  latitude: number;
  longitude: number;
  onMove: (latitude: number, longitude: number) => void;
}) {
  const center = useMemo<[number, number]>(() => [latitude, longitude], [latitude, longitude]);

  return (
    <div className="h-44 w-full overflow-hidden rounded-lg border border-line sm:h-[180px]">
      <MapContainer center={center} zoom={15} scrollWheelZoom={false} className="h-full w-full" attributionControl={false}>
        <TileLayer url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" />
        <Marker
          position={center}
          icon={PIN_ICON}
          draggable
          eventHandlers={{
            dragend: (e) => {
              const marker = e.target as L.Marker;
              const pos = marker.getLatLng();
              onMove(pos.lat, pos.lng);
            },
          }}
        />
        <ClickToMove onMove={onMove} />
        <MapCenterSync center={center} />
      </MapContainer>
    </div>
  );
}

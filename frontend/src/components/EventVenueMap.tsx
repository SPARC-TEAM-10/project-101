import { useMemo } from "react";
import { MapContainer, Marker, TileLayer } from "react-leaflet";
import L from "leaflet";

// Same icons as EventDiscoveryMap.tsx — kept local rather than shared since each file's icon is a
// module-level singleton with a hardcoded style; not worth an abstraction for two small SVGs.
const ME_ICON = L.divIcon({
  className: "",
  html: '<span style="display:block;width:16px;height:16px;border-radius:999px;background:#3B6EA5;border:3px solid #FDFAF4;box-shadow:0 0 0 6px rgba(59,110,165,.16)"></span>',
  iconSize: [16, 16],
  iconAnchor: [8, 8],
});

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
 * Small read-only venue map for the event detail page (CHH-40, EventRsvp.dc.html's "before"
 * state) — a single event pin, plus a "me" dot when the caller's own coordinates are known.
 * Deliberately Marker-only (no Circle) — EventDiscoveryMap's jsdom/Leaflet Circle gap doesn't
 * apply here since this view never needs a radius halo.
 */
export function EventVenueMap({
  latitude,
  longitude,
  myLatitude,
  myLongitude,
}: {
  latitude: number;
  longitude: number;
  myLatitude?: number;
  myLongitude?: number;
}) {
  const eventPosition = useMemo<[number, number]>(() => [latitude, longitude], [latitude, longitude]);
  const hasMyPosition = myLatitude !== undefined && myLongitude !== undefined;
  const center = hasMyPosition ? ([myLatitude!, myLongitude!] as [number, number]) : eventPosition;

  return (
    <div className="h-[110px] w-full overflow-hidden rounded-lg border border-line lg:h-44">
      <MapContainer center={center} zoom={12} scrollWheelZoom={false} dragging={false} className="h-full w-full" attributionControl={false}>
        <TileLayer url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" />
        <Marker position={eventPosition} icon={EVENT_PIN_ICON} />
        {hasMyPosition && <Marker position={[myLatitude!, myLongitude!]} icon={ME_ICON} />}
      </MapContainer>
    </div>
  );
}

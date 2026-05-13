"use client";

import L from "leaflet";
import { useEffect, useMemo } from "react";
import { CircleMarker, MapContainer, Marker, Polyline, Popup, TileLayer, useMap } from "react-leaflet";

import type { LocationDto } from "@/lib/api/types";

// Leaflet's default marker icons reference image files via relative paths,
// which Webpack/Turbopack can't resolve. Point them at the unpkg CDN once.
L.Icon.Default.mergeOptions({
  iconRetinaUrl: "https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png",
  iconUrl: "https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png",
  shadowUrl: "https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png",
});

interface DeviceMapProps {
  latest: LocationDto;
  history?: LocationDto[];
}

export default function DeviceMap({ latest, history }: DeviceMapProps) {
  const center: [number, number] = [latest.latitude, latest.longitude];

  const trail = useMemo<[number, number][]>(() => {
    if (!history || history.length === 0) return [];
    // History comes newest-first from the API; reverse for chronological polyline.
    return [...history]
      .reverse()
      .map((location) => [location.latitude, location.longitude] as [number, number]);
  }, [history]);

  return (
    <MapContainer
      center={center}
      zoom={16}
      className="h-full w-full"
      style={{ height: "100%", width: "100%" }}
      scrollWheelZoom
    >
      <InvalidateOnResize />
      <TileLayer
        attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
        url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
      />

      {/* Accuracy radius around the latest fix */}
      <CircleMarker
        center={center}
        radius={Math.max(20, Math.min(80, latest.accuracy * 1.5))}
        pathOptions={{ color: "#3b82f6", fillColor: "#3b82f6", fillOpacity: 0.15, weight: 1 }}
      />

      {trail.length > 1 && (
        <Polyline
          positions={trail}
          pathOptions={{ color: "#3b82f6", weight: 3, opacity: 0.6, dashArray: "4 6" }}
        />
      )}

      <Marker position={center}>
        <Popup>
          <div className="text-xs space-y-0.5">
            <div className="font-medium">Last known location</div>
            <div>Lat: {latest.latitude.toFixed(6)}</div>
            <div>Lng: {latest.longitude.toFixed(6)}</div>
            <div>Accuracy: {latest.accuracy.toFixed(0)} m</div>
          </div>
        </Popup>
      </Marker>

      <Recenter center={center} />
    </MapContainer>
  );
}

/** Smoothly re-centres the map whenever the latest location changes. */
function Recenter({ center }: { center: [number, number] }) {
  const map = useMap();
  useEffect(() => {
    map.flyTo(center, map.getZoom(), { duration: 0.8 });
  }, [center, map]);
  return null;
}

/**
 * Forces Leaflet to recompute its viewport size on mount and whenever the
 * container resizes. Without this the map paints into a 0×0 / 256×256 box
 * when its parent's final layout isn't settled at first render.
 */
function InvalidateOnResize() {
  const map = useMap();
  useEffect(() => {
    const container = map.getContainer();
    const observer = new ResizeObserver(() => map.invalidateSize());
    observer.observe(container);

    // One immediate kick after mount in case the observer's first callback
    // fires before tiles have had a chance to lay out.
    map.invalidateSize();
    return () => observer.disconnect();
  }, [map]);
  return null;
}

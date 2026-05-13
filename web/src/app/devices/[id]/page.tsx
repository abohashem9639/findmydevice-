"use client";

import dynamic from "next/dynamic";
import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { useCallback, useEffect, useState } from "react";

import { ApiError } from "@/lib/api/client";
import { devicesApi } from "@/lib/api/devices";
import { locationsApi } from "@/lib/api/locations";
import type { DeviceDto, LocationDto } from "@/lib/api/types";
import { useAuth } from "@/lib/auth/context";
import { formatLastSeen } from "@/lib/format/time";

const REFRESH_INTERVAL_MS = 15_000;

// Leaflet touches `window` at import time, so the component must be client-only.
const DeviceMap = dynamic(() => import("@/components/device-map"), {
  ssr: false,
  loading: () => (
    <div className="flex h-full items-center justify-center text-sm text-zinc-500">
      Loading map…
    </div>
  ),
});

export default function DeviceDetailPage() {
  const router = useRouter();
  const params = useParams<{ id: string }>();
  const deviceId = params.id;

  const { isHydrating, isAuthenticated, logout } = useAuth();

  const [device, setDevice] = useState<DeviceDto | null>(null);
  const [latest, setLatest] = useState<LocationDto | null>(null);
  const [history, setHistory] = useState<LocationDto[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [isInitialLoading, setIsInitialLoading] = useState(true);

  useEffect(() => {
    if (!isHydrating && !isAuthenticated) {
      router.replace("/login");
    }
  }, [isHydrating, isAuthenticated, router]);

  const loadAll = useCallback(
    async (showLoading: boolean) => {
      if (showLoading) setIsInitialLoading(true);
      try {
        const [deviceData, latestData, historyData] = await Promise.all([
          devicesApi.getById(deviceId),
          locationsApi.getLatest(deviceId),
          locationsApi.getHistory(deviceId, 100),
        ]);
        setDevice(deviceData);
        setLatest(latestData);
        setHistory(historyData);
        setError(null);
      } catch (err) {
        if (err instanceof ApiError) {
          if (err.status === 401) {
            logout();
            router.replace("/login");
            return;
          }
          if (err.status === 404) {
            setError("Device not found.");
            return;
          }
        }
        setError(err instanceof Error ? err.message : "Failed to load device.");
      } finally {
        if (showLoading) setIsInitialLoading(false);
      }
    },
    [deviceId, logout, router],
  );

  // Initial load + polling refresh.
  useEffect(() => {
    if (!isAuthenticated || !deviceId) return;
    void loadAll(true);
    const interval = setInterval(() => loadAll(false), REFRESH_INTERVAL_MS);
    return () => clearInterval(interval);
  }, [isAuthenticated, deviceId, loadAll]);

  if (isHydrating || !isAuthenticated) {
    return <CenteredMessage>Loading…</CenteredMessage>;
  }

  if (isInitialLoading) {
    return <CenteredMessage>Loading device…</CenteredMessage>;
  }

  if (error || !device) {
    return (
      <main className="mx-auto max-w-4xl px-6 py-10">
        <BackLink />
        <div className="mt-6 rounded-md border border-red-300 bg-red-50 dark:border-red-800 dark:bg-red-950 px-4 py-3 text-sm text-red-700 dark:text-red-300">
          {error ?? "Device not found."}
        </div>
      </main>
    );
  }

  return (
    <main className="mx-auto max-w-5xl px-6 py-8">
      <BackLink />

      <header className="mt-4 flex flex-col gap-2 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <div className="flex items-center gap-2 flex-wrap">
            <span
              aria-hidden
              className={`inline-block h-2.5 w-2.5 rounded-full ${
                device.isOnline ? "bg-emerald-500" : "bg-zinc-400"
              }`}
            />
            <h1 className="text-2xl font-semibold tracking-tight">{device.name}</h1>
            {device.isLocationEnabled === false && (
              <span className="ml-2 inline-flex items-center rounded-full bg-amber-100 dark:bg-amber-950 px-2.5 py-0.5 text-xs font-medium text-amber-800 dark:text-amber-300">
                Location disabled on phone
              </span>
            )}
          </div>
          <p className="mt-1 text-sm text-zinc-500">
            {[device.manufacturer, device.model].filter(Boolean).join(" ")}
            {device.osVersion && ` · ${device.platform} ${device.osVersion}`}
          </p>
        </div>

        <div className="text-sm text-zinc-500">
          Last seen: {formatLastSeen(device.lastSeenAtUtc)}
        </div>
      </header>

      <StatGrid device={device} latest={latest} />

      <section className="mt-6 rounded-xl border border-zinc-200 dark:border-zinc-800 overflow-hidden bg-zinc-50 dark:bg-zinc-900">
        <div className="h-[420px] sm:h-[500px]">
          {latest ? (
            <DeviceMap latest={latest} history={history} />
          ) : (
            <div className="flex h-full items-center justify-center text-sm text-zinc-500">
              No location reported yet. Start tracking from the device.
            </div>
          )}
        </div>
      </section>

      {latest && (
        <section className="mt-4 grid grid-cols-2 gap-4 text-xs text-zinc-500 sm:grid-cols-4">
          <Detail label="Latitude" value={latest.latitude.toFixed(6)} />
          <Detail label="Longitude" value={latest.longitude.toFixed(6)} />
          <Detail label="Accuracy" value={`${latest.accuracy.toFixed(0)} m`} />
          {typeof latest.altitude === "number" && (
            <Detail label="Altitude" value={`${latest.altitude.toFixed(0)} m`} />
          )}
        </section>
      )}

      <p className="mt-6 text-xs text-zinc-400">
        Auto-refreshing every {REFRESH_INTERVAL_MS / 1000} seconds.
      </p>
    </main>
  );
}

// ─────────────────────────────────────────────────────────────────────────────

function StatGrid({ device, latest }: { device: DeviceDto; latest: LocationDto | null }) {
  // Prefer the cached device-level fields (refreshed by every heartbeat) over
  // the latest location record (only refreshed when GPS is on).
  const battery = device.lastBatteryLevel ?? latest?.batteryLevel ?? null;
  const charging = device.lastIsCharging ?? latest?.isCharging ?? null;
  const network = device.lastNetworkType ?? latest?.networkType ?? null;

  return (
    <section className="mt-6 grid grid-cols-2 gap-3 sm:grid-cols-4">
      <Stat
        label="Battery"
        value={
          typeof battery === "number"
            ? `${battery}%${charging ? " ⚡" : ""}`
            : "—"
        }
      />
      <Stat
        label="Network"
        value={network ? network.toUpperCase() : "—"}
      />
      <Stat
        label="Last fix"
        value={latest ? formatLastSeen(latest.recordedAtUtc) : "Never"}
      />
      <Stat
        label="Last seen"
        value={formatLastSeen(device.lastSeenAtUtc)}
      />
    </section>
  );
}

function Stat({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-lg border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 px-4 py-3">
      <div className="text-xs uppercase tracking-wide text-zinc-400">{label}</div>
      <div className="mt-1 text-sm font-medium">{value}</div>
    </div>
  );
}

function Detail({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <div className="uppercase tracking-wide text-zinc-400">{label}</div>
      <div className="mt-0.5 font-mono text-sm text-zinc-700 dark:text-zinc-300">{value}</div>
    </div>
  );
}

function BackLink() {
  return (
    <Link
      href="/devices"
      className="inline-flex items-center text-sm text-zinc-500 hover:text-zinc-700 dark:hover:text-zinc-300"
    >
      ← Back to devices
    </Link>
  );
}

function CenteredMessage({ children }: { children: React.ReactNode }) {
  return (
    <div className="flex min-h-screen items-center justify-center">
      <p className="text-sm text-zinc-500">{children}</p>
    </div>
  );
}

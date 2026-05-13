"use client";

import { useRouter } from "next/navigation";
import { useCallback, useEffect, useState } from "react";

import { ApiError } from "@/lib/api/client";
import { devicesApi } from "@/lib/api/devices";
import type { DeviceDto } from "@/lib/api/types";
import { useAuth } from "@/lib/auth/context";
import { DeviceCard } from "@/components/device-card";

export default function DevicesPage() {
  const router = useRouter();
  const { isHydrating, isAuthenticated, user, logout } = useAuth();

  const [devices, setDevices] = useState<DeviceDto[] | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [deletingId, setDeletingId] = useState<string | null>(null);

  useEffect(() => {
    if (!isHydrating && !isAuthenticated) {
      router.replace("/login");
    }
  }, [isHydrating, isAuthenticated, router]);

  const load = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      const list = await devicesApi.getMine();
      setDevices(list);
    } catch (err) {
      if (err instanceof ApiError && err.status === 401) {
        logout();
        router.replace("/login");
        return;
      }
      setError(err instanceof Error ? err.message : "Failed to load devices.");
    } finally {
      setIsLoading(false);
    }
  }, [logout, router]);

  useEffect(() => {
    if (!isAuthenticated) return;
    void load();
  }, [isAuthenticated, load]);

  async function handleDelete(id: string) {
    setDeletingId(id);
    try {
      await devicesApi.delete(id);
      setDevices((current) => current?.filter((d) => d.id !== id) ?? null);
    } catch (err) {
      if (err instanceof ApiError && err.status === 401) {
        logout();
        router.replace("/login");
        return;
      }
      setError(err instanceof Error ? err.message : "Failed to unlink device.");
    } finally {
      setDeletingId(null);
    }
  }

  function handleSignOut() {
    logout();
    router.replace("/login");
  }

  if (isHydrating || !isAuthenticated) {
    return <CenteredMessage>Loading…</CenteredMessage>;
  }

  return (
    <main className="mx-auto max-w-4xl px-6 py-10">
      <header className="flex flex-col gap-4 border-b border-zinc-200 dark:border-zinc-800 pb-6 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">My Devices</h1>
          <p className="mt-1 text-sm text-zinc-500">
            Signed in as{" "}
            <span className="font-medium text-zinc-700 dark:text-zinc-300">
              {user?.displayName ?? user?.email}
            </span>
          </p>
        </div>

        <div className="flex items-center gap-2">
          <button
            onClick={load}
            disabled={isLoading}
            className="rounded-md border border-zinc-300 dark:border-zinc-700 px-3 py-1.5 text-sm hover:bg-zinc-100 dark:hover:bg-zinc-800 disabled:opacity-50"
          >
            {isLoading ? "Refreshing…" : "Refresh"}
          </button>
          <button
            onClick={handleSignOut}
            className="rounded-md border border-zinc-300 dark:border-zinc-700 px-3 py-1.5 text-sm hover:bg-zinc-100 dark:hover:bg-zinc-800"
          >
            Sign out
          </button>
        </div>
      </header>

      <section className="mt-8">
        {error && (
          <div className="mb-4 rounded-md border border-red-300 bg-red-50 dark:border-red-800 dark:bg-red-950 px-4 py-3 text-sm text-red-700 dark:text-red-300">
            {error}
            <button
              onClick={load}
              className="ml-3 font-medium underline underline-offset-2"
            >
              Retry
            </button>
          </div>
        )}

        {isLoading && !devices && <CenteredMessage>Loading devices…</CenteredMessage>}

        {!isLoading && devices?.length === 0 && <EmptyState />}

        {devices && devices.length > 0 && (
          <div className="grid grid-cols-1 gap-3 md:grid-cols-2">
            {devices.map((device) => (
              <DeviceCard
                key={device.id}
                device={device}
                onDelete={handleDelete}
                isDeleting={deletingId === device.id}
              />
            ))}
          </div>
        )}
      </section>
    </main>
  );
}

function EmptyState() {
  return (
    <div className="rounded-xl border border-dashed border-zinc-300 dark:border-zinc-700 p-12 text-center">
      <h2 className="text-base font-semibold">No devices yet</h2>
      <p className="mt-2 text-sm text-zinc-500">
        Install the FindMyDevice app on your phone and register it from there to start tracking.
      </p>
    </div>
  );
}

function CenteredMessage({ children }: { children: React.ReactNode }) {
  return (
    <div className="flex min-h-[40vh] items-center justify-center">
      <p className="text-sm text-zinc-500">{children}</p>
    </div>
  );
}

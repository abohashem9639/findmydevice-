"use client";

import Link from "next/link";

import type { DeviceDto } from "@/lib/api/types";
import { formatLastSeen } from "@/lib/format/time";

interface DeviceCardProps {
  device: DeviceDto;
  onDelete: (id: string) => void;
  isDeleting?: boolean;
}

export function DeviceCard({ device, onDelete, isDeleting }: DeviceCardProps) {
  const subtitle = [device.manufacturer, device.model]
    .filter(Boolean)
    .join(" ");
  const osLabel = device.osVersion ? `${device.platform} ${device.osVersion}` : device.platform;

  return (
    <article className="group rounded-xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 transition hover:shadow-sm">
      <div className="flex items-start justify-between gap-4 p-5">
        <Link href={`/devices/${device.id}`} className="flex-1 min-w-0">
          <div className="flex items-center gap-2">
            <span
              aria-hidden
              className={`inline-block h-2.5 w-2.5 rounded-full ${
                device.isOnline ? "bg-emerald-500" : "bg-zinc-400"
              }`}
            />
            <h3 className="truncate text-base font-semibold">{device.name}</h3>
          </div>

          {subtitle && (
            <p className="mt-1 truncate text-sm text-zinc-500">{subtitle}</p>
          )}

          {device.isLocationEnabled === false && (
            <span className="mt-2 inline-flex items-center rounded-full bg-amber-100 dark:bg-amber-950 px-2 py-0.5 text-[10px] font-medium text-amber-800 dark:text-amber-300">
              Location disabled on phone
            </span>
          )}

          <dl className="mt-3 grid grid-cols-2 gap-x-4 gap-y-1 text-xs text-zinc-500">
            <div>
              <dt className="font-medium text-zinc-400">Platform</dt>
              <dd>{osLabel}</dd>
            </div>
            <div>
              <dt className="font-medium text-zinc-400">Last seen</dt>
              <dd>{formatLastSeen(device.lastSeenAtUtc)}</dd>
            </div>
          </dl>
        </Link>

        <button
          type="button"
          onClick={() => {
            if (window.confirm(`Unlink ${device.name}?`)) {
              onDelete(device.id);
            }
          }}
          disabled={isDeleting}
          className="rounded-md border border-zinc-300 dark:border-zinc-700 px-2.5 py-1 text-xs text-zinc-600 dark:text-zinc-300 hover:bg-zinc-100 dark:hover:bg-zinc-800 disabled:opacity-50"
          aria-label="Unlink device"
        >
          {isDeleting ? "…" : "Unlink"}
        </button>
      </div>
    </article>
  );
}

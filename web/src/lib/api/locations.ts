import { apiFetch } from "./client";
import type { LocationDto } from "./types";

export const locationsApi = {
  getLatest: async (deviceId: string): Promise<LocationDto | null> => {
    // Backend returns 204 No Content when there are no locations yet.
    const result = await apiFetch<LocationDto | undefined>(
      `api/v1/Devices/${deviceId}/Locations/latest`,
      { method: "GET" },
    );
    return result ?? null;
  },

  getHistory: (deviceId: string, limit = 100) =>
    apiFetch<LocationDto[]>(
      `api/v1/Devices/${deviceId}/Locations?limit=${limit}`,
      { method: "GET" },
    ),
};

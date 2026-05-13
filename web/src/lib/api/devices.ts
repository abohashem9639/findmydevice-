import { apiFetch } from "./client";
import type { DeviceDto } from "./types";

export const devicesApi = {
  getMine: () => apiFetch<DeviceDto[]>("api/v1/Devices", { method: "GET" }),

  getById: (id: string) =>
    apiFetch<DeviceDto>(`api/v1/Devices/${id}`, { method: "GET" }),

  delete: (id: string) =>
    apiFetch<void>(`api/v1/Devices/${id}`, { method: "DELETE" }),
};

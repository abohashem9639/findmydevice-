// DTOs mirroring the .NET backend. Field names match the JSON casing the
// backend produces (camelCase by default in ASP.NET Core).

// ─── Auth ───────────────────────────────────────────────────────────────

export interface SignupRequest {
  email: string;
  password: string;
  confirmPassword: string;
  displayName?: string | null;
  language?: string | null;
}

export interface SigninRequest {
  email: string;
  password: string;
}

export interface RefreshRequest {
  refreshToken: string;
}

export interface UserDto {
  id: string;
  email: string;
  displayName?: string | null;
  profileImage?: string | null;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresInSeconds: number;
  tokenType: string;
  user: UserDto;
}

// ─── Devices ────────────────────────────────────────────────────────────

export interface DeviceDto {
  id: string;
  name: string;
  manufacturer?: string | null;
  model?: string | null;
  platform: string;
  osVersion?: string | null;
  hasFcmToken: boolean;
  createdAtUtc: string;
  lastSeenAtUtc?: string | null;
  isOnline: boolean;
  lastBatteryLevel?: number | null;
  lastIsCharging?: boolean | null;
  lastNetworkType?: string | null;
  /** Null until phone reports for the first time. False = phone deliberately disabled location. */
  isLocationEnabled?: boolean | null;
}

// ─── Locations ──────────────────────────────────────────────────────────

export interface LocationDto {
  id: string;
  deviceId: string;
  latitude: number;
  longitude: number;
  accuracy: number;
  altitude?: number | null;
  speed?: number | null;
  bearing?: number | null;
  recordedAtUtc: string;
  receivedAtUtc: string;
  batteryLevel?: number | null;
  isCharging?: boolean | null;
  networkType?: string | null;
}

// ─── Errors ─────────────────────────────────────────────────────────────

export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
}

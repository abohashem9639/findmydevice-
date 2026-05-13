import type { AuthResponse, UserDto } from "@/lib/api/types";

const STORAGE_KEY = "findmydevice.session";

export interface StoredSession {
  accessToken: string;
  refreshToken: string;
  expiresAtEpochMillis: number;
  user: UserDto;
}

const isBrowser = () => typeof window !== "undefined";

export function saveSession(response: AuthResponse): StoredSession {
  const session: StoredSession = {
    accessToken: response.accessToken,
    refreshToken: response.refreshToken,
    expiresAtEpochMillis: Date.now() + response.expiresInSeconds * 1000,
    user: response.user,
  };

  if (isBrowser()) {
    window.localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
  }
  return session;
}

export function loadSession(): StoredSession | null {
  if (!isBrowser()) return null;
  const raw = window.localStorage.getItem(STORAGE_KEY);
  if (!raw) return null;
  try {
    return JSON.parse(raw) as StoredSession;
  } catch {
    return null;
  }
}

export function clearSession(): void {
  if (!isBrowser()) return;
  window.localStorage.removeItem(STORAGE_KEY);
}

/** Treat as expired 30s before actual expiry to avoid edge-case 401s. */
export function isAccessTokenExpired(session: StoredSession): boolean {
  return Date.now() >= session.expiresAtEpochMillis - 30_000;
}

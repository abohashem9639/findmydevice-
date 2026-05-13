import type { ProblemDetails } from "./types";
import { loadSession } from "@/lib/auth/storage";

const BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:8000/";

const PUBLIC_PATHS = [
  "api/v1/Auth/signup",
  "api/v1/Auth/signin",
  "api/v1/Auth/refresh",
  "api/v1/Auth/forgot-password",
  "api/v1/Auth/reset-password",
];

export class ApiError extends Error {
  readonly status: number;
  readonly problem: ProblemDetails | null;

  constructor(status: number, problem: ProblemDetails | null, fallbackMessage: string) {
    super(problem?.detail ?? problem?.title ?? fallbackMessage);
    this.status = status;
    this.problem = problem;
  }
}

type RequestOptions = Omit<RequestInit, "body"> & {
  body?: unknown;
};

/**
 * Thin fetch wrapper that:
 *  - prepends the base URL
 *  - attaches Bearer token from local storage on authenticated paths
 *  - serialises JSON bodies
 *  - parses RFC 7807 ProblemDetails error responses into typed ApiError
 */
export async function apiFetch<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const url = new URL(path.replace(/^\//, ""), BASE_URL).toString();
  const headers = new Headers(options.headers ?? {});
  headers.set("Accept", "application/json");

  if (options.body !== undefined) {
    headers.set("Content-Type", "application/json");
  }

  if (!isPublicPath(path)) {
    const session = loadSession();
    if (session?.accessToken) {
      headers.set("Authorization", `Bearer ${session.accessToken}`);
    }
  }

  const response = await fetch(url, {
    ...options,
    headers,
    body: options.body !== undefined ? JSON.stringify(options.body) : undefined,
  });

  if (response.status === 204) {
    return undefined as T;
  }

  const text = await response.text();
  const json = text ? safeJsonParse(text) : null;

  if (!response.ok) {
    throw new ApiError(response.status, json as ProblemDetails | null, `HTTP ${response.status}`);
  }

  return json as T;
}

function isPublicPath(path: string): boolean {
  const normalised = path.replace(/^\//, "");
  return PUBLIC_PATHS.some((p) => normalised.startsWith(p));
}

function safeJsonParse(text: string): unknown {
  try {
    return JSON.parse(text);
  } catch {
    return null;
  }
}

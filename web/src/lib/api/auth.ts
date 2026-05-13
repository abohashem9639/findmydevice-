import { apiFetch } from "./client";
import type {
  AuthResponse,
  RefreshRequest,
  SigninRequest,
  SignupRequest,
} from "./types";

export const authApi = {
  signup: (request: SignupRequest) =>
    apiFetch<AuthResponse>("api/v1/Auth/signup", {
      method: "POST",
      body: request,
    }),

  signin: (request: SigninRequest) =>
    apiFetch<AuthResponse>("api/v1/Auth/signin", {
      method: "POST",
      body: request,
    }),

  refresh: (request: RefreshRequest) =>
    apiFetch<AuthResponse>("api/v1/Auth/refresh", {
      method: "POST",
      body: request,
    }),
};

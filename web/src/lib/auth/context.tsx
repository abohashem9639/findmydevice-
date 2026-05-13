"use client";

import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react";

import { authApi } from "@/lib/api/auth";
import type {
  AuthResponse,
  SigninRequest,
  SignupRequest,
  UserDto,
} from "@/lib/api/types";
import {
  clearSession,
  loadSession,
  saveSession,
  type StoredSession,
} from "./storage";

interface AuthContextValue {
  /** `null` until we hydrate from storage on mount. */
  session: StoredSession | null;
  /** True while we are reading from storage on first mount. */
  isHydrating: boolean;
  isAuthenticated: boolean;
  user: UserDto | null;
  signin: (request: SigninRequest) => Promise<void>;
  signup: (request: SignupRequest) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<StoredSession | null>(null);
  const [isHydrating, setIsHydrating] = useState(true);

  // Hydrate from localStorage on first mount (client-only).
  useEffect(() => {
    setSession(loadSession());
    setIsHydrating(false);
  }, []);

  const persistFromResponse = useCallback((response: AuthResponse) => {
    const stored = saveSession(response);
    setSession(stored);
  }, []);

  const signin = useCallback(
    async (request: SigninRequest) => {
      const response = await authApi.signin(request);
      persistFromResponse(response);
    },
    [persistFromResponse],
  );

  const signup = useCallback(
    async (request: SignupRequest) => {
      const response = await authApi.signup(request);
      persistFromResponse(response);
    },
    [persistFromResponse],
  );

  const logout = useCallback(() => {
    clearSession();
    setSession(null);
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({
      session,
      isHydrating,
      isAuthenticated: session !== null,
      user: session?.user ?? null,
      signin,
      signup,
      logout,
    }),
    [session, isHydrating, signin, signup, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error("useAuth must be used inside <AuthProvider>");
  }
  return ctx;
}

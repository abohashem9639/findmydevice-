"use client";

import { useRouter } from "next/navigation";
import { useEffect } from "react";

import { useAuth } from "@/lib/auth/context";

export default function HomePage() {
  const router = useRouter();
  const { isHydrating, isAuthenticated } = useAuth();

  useEffect(() => {
    if (isHydrating) return;
    router.replace(isAuthenticated ? "/devices" : "/login");
  }, [isHydrating, isAuthenticated, router]);

  return (
    <div className="flex min-h-screen items-center justify-center">
      <p className="text-sm text-gray-500">Loading…</p>
    </div>
  );
}

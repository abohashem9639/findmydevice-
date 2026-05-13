"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect, useState, type FormEvent } from "react";

import { ApiError } from "@/lib/api/client";
import { useAuth } from "@/lib/auth/context";

const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

export default function SignupPage() {
  const router = useRouter();
  const { isHydrating, isAuthenticated, signup } = useAuth();

  const [displayName, setDisplayName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!isHydrating && isAuthenticated) {
      router.replace("/devices");
    }
  }, [isHydrating, isAuthenticated, router]);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const validation = validate();
    if (validation) {
      setError(validation);
      return;
    }

    setIsLoading(true);
    setError(null);
    try {
      await signup({
        email: email.trim(),
        password,
        confirmPassword,
        displayName: displayName.trim() || null,
      });
      router.replace("/devices");
    } catch (err) {
      setError(messageForError(err));
    } finally {
      setIsLoading(false);
    }
  }

  function validate(): string | null {
    if (!email.trim()) return "Email is required.";
    if (!EMAIL_REGEX.test(email.trim())) return "Invalid email format.";
    if (password.length < 6) return "Password must be at least 6 characters.";
    if (password !== confirmPassword) return "Passwords do not match.";
    return null;
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-zinc-50 dark:bg-zinc-950 px-4 py-8">
      <div className="w-full max-w-sm rounded-2xl bg-white dark:bg-zinc-900 p-8 shadow-sm">
        <h1 className="text-2xl font-semibold">Create account</h1>
        <p className="mt-1 text-sm text-zinc-500">
          Sign up to start tracking your devices.
        </p>

        <form className="mt-6 space-y-4" onSubmit={handleSubmit} noValidate>
          <Field
            id="displayName"
            label="Display name (optional)"
            value={displayName}
            onChange={setDisplayName}
            autoComplete="name"
            disabled={isLoading}
          />
          <Field
            id="email"
            label="Email"
            type="email"
            value={email}
            onChange={setEmail}
            autoComplete="email"
            disabled={isLoading}
          />
          <Field
            id="password"
            label="Password"
            type="password"
            value={password}
            onChange={setPassword}
            autoComplete="new-password"
            disabled={isLoading}
          />
          <Field
            id="confirmPassword"
            label="Confirm password"
            type="password"
            value={confirmPassword}
            onChange={setConfirmPassword}
            autoComplete="new-password"
            disabled={isLoading}
          />

          {error && (
            <p className="text-sm text-red-600 dark:text-red-400">{error}</p>
          )}

          <button
            type="submit"
            disabled={isLoading}
            className="w-full rounded-md bg-blue-600 px-3 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-60"
          >
            {isLoading ? "Creating…" : "Create account"}
          </button>
        </form>

        <p className="mt-6 text-center text-sm text-zinc-500">
          Already have an account?{" "}
          <Link href="/login" className="font-medium text-blue-600 hover:underline">
            Sign in
          </Link>
        </p>
      </div>
    </div>
  );
}

interface FieldProps {
  id: string;
  label: string;
  value: string;
  onChange: (value: string) => void;
  type?: string;
  autoComplete?: string;
  disabled?: boolean;
}

function Field({ id, label, value, onChange, type = "text", autoComplete, disabled }: FieldProps) {
  return (
    <div>
      <label htmlFor={id} className="block text-sm font-medium">
        {label}
      </label>
      <input
        id={id}
        type={type}
        autoComplete={autoComplete}
        value={value}
        onChange={(event) => onChange(event.target.value)}
        disabled={disabled}
        className="mt-1 w-full rounded-md border border-zinc-300 dark:border-zinc-700 bg-transparent px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
      />
    </div>
  );
}

function messageForError(err: unknown): string {
  if (err instanceof ApiError) {
    switch (err.status) {
      case 409:
        return "An account with this email already exists.";
      case 400:
        return "Please check your input.";
      default:
        return err.message || "Sign-up failed.";
    }
  }
  return "No connection. Check your network and try again.";
}

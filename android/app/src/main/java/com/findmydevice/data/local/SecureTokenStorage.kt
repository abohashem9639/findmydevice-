package com.findmydevice.data.local

import com.findmydevice.domain.model.AuthSession

/**
 * Persistent, encrypted storage for the authenticated session.
 * Implementations must keep tokens encrypted at rest (Android Keystore-backed).
 */
interface SecureTokenStorage {
    fun saveSession(session: AuthSession)
    fun getSession(): AuthSession?
    fun clear()
}

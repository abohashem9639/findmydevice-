package com.findmydevice.domain.model

data class AuthTokens(
    val accessToken: String,
    val refreshToken: String,
    val expiresAtEpochMillis: Long,
) {
    /** Treat as expired 30s before actual expiry to avoid edge-case 401s. */
    fun isExpired(nowEpochMillis: Long = System.currentTimeMillis()): Boolean =
        nowEpochMillis >= (expiresAtEpochMillis - SAFETY_MARGIN_MILLIS)

    companion object {
        private const val SAFETY_MARGIN_MILLIS = 30_000L
    }
}

package com.findmydevice.domain.model

data class AuthSession(
    val tokens: AuthTokens,
    val userId: String,
    val userEmail: String,
    val userDisplayName: String?,
)

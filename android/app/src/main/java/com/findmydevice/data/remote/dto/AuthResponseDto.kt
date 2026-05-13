package com.findmydevice.data.remote.dto

import kotlinx.serialization.Serializable

@Serializable
data class AuthResponseDto(
    val accessToken: String? = null,
    val refreshToken: String? = null,
    val expiresInSeconds: Int = 0,
    val tokenType: String? = null,
    val user: UserDto? = null,
)

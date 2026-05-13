package com.findmydevice.data.remote.dto

import kotlinx.serialization.Serializable

@Serializable
data class RefreshRequestDto(
    val refreshToken: String,
)

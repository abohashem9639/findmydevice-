package com.findmydevice.data.remote.dto

import kotlinx.serialization.Serializable

@Serializable
data class UpdateFcmTokenRequestDto(
    val fcmToken: String,
)

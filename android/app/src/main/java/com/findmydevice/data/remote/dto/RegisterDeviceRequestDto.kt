package com.findmydevice.data.remote.dto

import kotlinx.serialization.Serializable

@Serializable
data class RegisterDeviceRequestDto(
    val name: String,
    val manufacturer: String? = null,
    val model: String? = null,
    val platform: String? = null,
    val osVersion: String? = null,
    val fcmToken: String? = null,
)

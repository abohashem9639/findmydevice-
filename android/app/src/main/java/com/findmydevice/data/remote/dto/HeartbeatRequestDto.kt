package com.findmydevice.data.remote.dto

import kotlinx.serialization.Serializable

@Serializable
data class HeartbeatRequestDto(
    val batteryLevel: Int? = null,
    val isCharging: Boolean? = null,
    val networkType: String? = null,
    val isLocationEnabled: Boolean,
)

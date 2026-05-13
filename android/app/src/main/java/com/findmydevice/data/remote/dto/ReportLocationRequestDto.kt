package com.findmydevice.data.remote.dto

import kotlinx.serialization.Serializable

@Serializable
data class ReportLocationRequestDto(
    val latitude: Double,
    val longitude: Double,
    val accuracy: Float,
    val altitude: Double? = null,
    val speed: Float? = null,
    val bearing: Float? = null,
    /** ISO 8601 UTC, e.g. "2026-05-11T12:34:56Z". */
    val recordedAtUtc: String,
    val batteryLevel: Int? = null,
    val isCharging: Boolean? = null,
    val networkType: String? = null,
)

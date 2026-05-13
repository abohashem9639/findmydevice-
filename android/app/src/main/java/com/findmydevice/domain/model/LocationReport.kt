package com.findmydevice.domain.model

data class LocationReport(
    val latitude: Double,
    val longitude: Double,
    val accuracy: Float,
    val altitude: Double?,
    val speed: Float?,
    val bearing: Float?,
    /** Wall-clock time (epoch millis) when the phone produced this fix. */
    val recordedAtEpochMillis: Long,
    val batteryLevel: Int?,
    val isCharging: Boolean?,
    /** "wifi" | "cellular" | "none" | "unknown" */
    val networkType: String?,
)

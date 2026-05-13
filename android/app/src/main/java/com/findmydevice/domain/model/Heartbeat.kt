package com.findmydevice.domain.model

/**
 * Lightweight "I am alive" telemetry sent on every tick — even when location
 * services are off — so the dashboard can show the phone as Online with fresh
 * battery / network info, and detect when the user has disabled Location.
 */
data class Heartbeat(
    val batteryLevel: Int?,
    val isCharging: Boolean?,
    val networkType: String?,
    val isLocationEnabled: Boolean,
)

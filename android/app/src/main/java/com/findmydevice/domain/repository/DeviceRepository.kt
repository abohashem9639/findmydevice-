package com.findmydevice.domain.repository

import com.findmydevice.domain.model.Device
import com.findmydevice.domain.model.DeviceResult
import com.findmydevice.domain.model.Heartbeat

interface DeviceRepository {

    suspend fun registerDevice(
        name: String,
        manufacturer: String?,
        model: String?,
        platform: String,
        osVersion: String?,
        fcmToken: String?,
    ): DeviceResult<Device>

    suspend fun getMyDevices(): DeviceResult<List<Device>>

    suspend fun getDevice(id: String): DeviceResult<Device>

    suspend fun updateFcmToken(id: String, fcmToken: String): DeviceResult<Device>

    suspend fun deleteDevice(id: String): DeviceResult<Unit>

    /**
     * Sends an "I am alive" telemetry report. Used on every tracking tick — even
     * when location is off — to keep the device shown as Online in the dashboard.
     */
    suspend fun sendHeartbeat(id: String, heartbeat: Heartbeat): Result<Unit>
}

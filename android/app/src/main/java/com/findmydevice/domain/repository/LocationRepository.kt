package com.findmydevice.domain.repository

import com.findmydevice.domain.model.LocationReport

interface LocationRepository {
    /**
     * Sends a location report to the backend. Returns `success` for any 2xx
     * response (including 4xx validation errors are converted to failure).
     * Designed for fire-and-forget use from background work; callers should
     * just log failures and let the next tick retry.
     */
    suspend fun reportLocation(deviceId: String, report: LocationReport): Result<Unit>
}

package com.findmydevice.data.repository

import com.findmydevice.data.remote.api.LocationApi
import com.findmydevice.data.remote.dto.ReportLocationRequestDto
import com.findmydevice.domain.model.LocationReport
import com.findmydevice.domain.repository.LocationRepository
import kotlinx.coroutines.CancellationException
import java.time.Instant
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class LocationRepositoryImpl @Inject constructor(
    private val locationApi: LocationApi,
) : LocationRepository {

    override suspend fun reportLocation(
        deviceId: String,
        report: LocationReport,
    ): Result<Unit> = try {
        val response = locationApi.report(deviceId, report.toDto())
        if (response.isSuccessful) {
            Result.success(Unit)
        } else {
            Result.failure(IllegalStateException("HTTP ${response.code()}"))
        }
    } catch (e: CancellationException) {
        throw e
    } catch (e: Exception) {
        Result.failure(e)
    }

    private fun LocationReport.toDto(): ReportLocationRequestDto = ReportLocationRequestDto(
        latitude = latitude,
        longitude = longitude,
        accuracy = accuracy,
        altitude = altitude,
        speed = speed,
        bearing = bearing,
        recordedAtUtc = Instant.ofEpochMilli(recordedAtEpochMillis).toString(),
        batteryLevel = batteryLevel,
        isCharging = isCharging,
        networkType = networkType,
    )
}

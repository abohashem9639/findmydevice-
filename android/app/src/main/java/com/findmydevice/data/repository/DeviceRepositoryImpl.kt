package com.findmydevice.data.repository

import com.findmydevice.data.remote.api.DeviceApi
import com.findmydevice.data.remote.dto.DeviceDto
import com.findmydevice.data.remote.dto.HeartbeatRequestDto
import com.findmydevice.data.remote.dto.RegisterDeviceRequestDto
import com.findmydevice.data.remote.dto.UpdateFcmTokenRequestDto
import com.findmydevice.domain.model.Device
import com.findmydevice.domain.model.DeviceError
import com.findmydevice.domain.model.DeviceResult
import com.findmydevice.domain.model.Heartbeat
import com.findmydevice.domain.repository.DeviceRepository
import kotlinx.coroutines.CancellationException
import okio.IOException
import retrofit2.Response
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class DeviceRepositoryImpl @Inject constructor(
    private val deviceApi: DeviceApi,
) : DeviceRepository {

    override suspend fun registerDevice(
        name: String,
        manufacturer: String?,
        model: String?,
        platform: String,
        osVersion: String?,
        fcmToken: String?,
    ): DeviceResult<Device> = execute {
        deviceApi.register(
            RegisterDeviceRequestDto(
                name = name,
                manufacturer = manufacturer,
                model = model,
                platform = platform,
                osVersion = osVersion,
                fcmToken = fcmToken,
            ),
        ).mapBody { it.toDomain() }
    }

    override suspend fun getMyDevices(): DeviceResult<List<Device>> = execute {
        deviceApi.getMine().mapBody { list -> list.map { it.toDomain() } }
    }

    override suspend fun getDevice(id: String): DeviceResult<Device> = execute {
        deviceApi.getById(id).mapBody { it.toDomain() }
    }

    override suspend fun updateFcmToken(id: String, fcmToken: String): DeviceResult<Device> = execute {
        deviceApi.updateFcmToken(id, UpdateFcmTokenRequestDto(fcmToken))
            .mapBody { it.toDomain() }
    }

    override suspend fun deleteDevice(id: String): DeviceResult<Unit> = execute {
        val response = deviceApi.delete(id)
        if (response.isSuccessful) DeviceResult.Success(Unit)
        else DeviceResult.Failure(mapHttpError(response.code()))
    }

    override suspend fun sendHeartbeat(id: String, heartbeat: Heartbeat): Result<Unit> = try {
        val response = deviceApi.heartbeat(
            id,
            HeartbeatRequestDto(
                batteryLevel = heartbeat.batteryLevel,
                isCharging = heartbeat.isCharging,
                networkType = heartbeat.networkType,
                isLocationEnabled = heartbeat.isLocationEnabled,
            ),
        )
        if (response.isSuccessful) Result.success(Unit)
        else Result.failure(IllegalStateException("HTTP ${response.code()}"))
    } catch (e: CancellationException) {
        throw e
    } catch (e: Exception) {
        Result.failure(e)
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private suspend fun <T> execute(
        block: suspend () -> DeviceResult<T>,
    ): DeviceResult<T> = try {
        block()
    } catch (e: CancellationException) {
        throw e
    } catch (e: IOException) {
        DeviceResult.Failure(DeviceError.Network)
    } catch (e: Exception) {
        DeviceResult.Failure(DeviceError.Unknown(e.message))
    }

    private fun <T, R> Response<T>.mapBody(transform: (T) -> R): DeviceResult<R> {
        if (isSuccessful) {
            val body = body()
                ?: return DeviceResult.Failure(DeviceError.Unknown("Empty response body."))
            return DeviceResult.Success(transform(body))
        }
        return DeviceResult.Failure(mapHttpError(code()))
    }

    private fun mapHttpError(code: Int): DeviceError = when (code) {
        400 -> DeviceError.ValidationFailed
        401 -> DeviceError.Unauthorized
        404 -> DeviceError.NotFound
        else -> DeviceError.Unknown("HTTP $code")
    }

    private fun DeviceDto.toDomain(): Device = Device(
        id = id,
        name = name,
        manufacturer = manufacturer,
        model = model,
        platform = platform,
        osVersion = osVersion,
        hasFcmToken = hasFcmToken,
        isOnline = isOnline,
        createdAtUtc = createdAtUtc,
        lastSeenAtUtc = lastSeenAtUtc,
    )
}

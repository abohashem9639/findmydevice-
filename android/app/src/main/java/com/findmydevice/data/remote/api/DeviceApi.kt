package com.findmydevice.data.remote.api

import com.findmydevice.data.remote.dto.DeviceDto
import com.findmydevice.data.remote.dto.HeartbeatRequestDto
import com.findmydevice.data.remote.dto.RegisterDeviceRequestDto
import com.findmydevice.data.remote.dto.UpdateFcmTokenRequestDto
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.DELETE
import retrofit2.http.GET
import retrofit2.http.PATCH
import retrofit2.http.POST
import retrofit2.http.Path

interface DeviceApi {

    @POST("api/v1/Devices")
    suspend fun register(@Body request: RegisterDeviceRequestDto): Response<DeviceDto>

    @GET("api/v1/Devices")
    suspend fun getMine(): Response<List<DeviceDto>>

    @GET("api/v1/Devices/{id}")
    suspend fun getById(@Path("id") id: String): Response<DeviceDto>

    @PATCH("api/v1/Devices/{id}/fcm-token")
    suspend fun updateFcmToken(
        @Path("id") id: String,
        @Body request: UpdateFcmTokenRequestDto,
    ): Response<DeviceDto>

    @DELETE("api/v1/Devices/{id}")
    suspend fun delete(@Path("id") id: String): Response<Unit>

    @POST("api/v1/Devices/{id}/heartbeat")
    suspend fun heartbeat(
        @Path("id") id: String,
        @Body request: HeartbeatRequestDto,
    ): Response<DeviceDto>
}

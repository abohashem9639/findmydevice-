package com.findmydevice.data.remote.api

import com.findmydevice.data.remote.dto.ReportLocationRequestDto
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.POST
import retrofit2.http.Path

interface LocationApi {

    @POST("api/v1/Devices/{deviceId}/Locations")
    suspend fun report(
        @Path("deviceId") deviceId: String,
        @Body request: ReportLocationRequestDto,
    ): Response<Unit>
}

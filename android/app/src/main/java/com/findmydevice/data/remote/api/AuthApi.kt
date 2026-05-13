package com.findmydevice.data.remote.api

import com.findmydevice.data.remote.dto.AuthResponseDto
import com.findmydevice.data.remote.dto.RefreshRequestDto
import com.findmydevice.data.remote.dto.SigninRequestDto
import com.findmydevice.data.remote.dto.SignupRequestDto
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.POST

interface AuthApi {

    @POST("api/v1/Auth/signup")
    suspend fun signup(@Body request: SignupRequestDto): Response<AuthResponseDto>

    @POST("api/v1/Auth/signin")
    suspend fun signin(@Body request: SigninRequestDto): Response<AuthResponseDto>

    @POST("api/v1/Auth/refresh")
    suspend fun refresh(@Body request: RefreshRequestDto): Response<AuthResponseDto>
}

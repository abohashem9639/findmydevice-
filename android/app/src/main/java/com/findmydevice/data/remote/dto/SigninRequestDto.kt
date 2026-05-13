package com.findmydevice.data.remote.dto

import kotlinx.serialization.Serializable

@Serializable
data class SigninRequestDto(
    val email: String,
    val password: String,
)

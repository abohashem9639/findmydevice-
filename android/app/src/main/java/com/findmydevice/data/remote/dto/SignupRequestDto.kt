package com.findmydevice.data.remote.dto

import kotlinx.serialization.Serializable

@Serializable
data class SignupRequestDto(
    val email: String,
    val password: String,
    val confirmPassword: String,
    val displayName: String? = null,
    val language: String? = null,
)

package com.findmydevice.data.remote.dto

import kotlinx.serialization.Serializable

@Serializable
data class UserDto(
    val id: String,
    val email: String,
    val displayName: String? = null,
    val profileImage: String? = null,
)

package com.findmydevice.data.remote.dto

import kotlinx.serialization.Serializable

@Serializable
data class DeviceDto(
    val id: String,
    val name: String,
    val manufacturer: String? = null,
    val model: String? = null,
    val platform: String,
    val osVersion: String? = null,
    val hasFcmToken: Boolean = false,
    val createdAtUtc: String,
    val lastSeenAtUtc: String? = null,
    val isOnline: Boolean = false,
)

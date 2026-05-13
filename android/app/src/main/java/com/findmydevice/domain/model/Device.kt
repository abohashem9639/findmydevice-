package com.findmydevice.domain.model

data class Device(
    val id: String,
    val name: String,
    val manufacturer: String?,
    val model: String?,
    val platform: String,
    val osVersion: String?,
    val hasFcmToken: Boolean,
    val isOnline: Boolean,
    val createdAtUtc: String,
    val lastSeenAtUtc: String?,
)

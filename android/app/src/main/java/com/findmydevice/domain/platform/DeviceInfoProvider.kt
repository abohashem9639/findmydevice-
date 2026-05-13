package com.findmydevice.domain.platform

data class DeviceInfo(
    val manufacturer: String,
    val model: String,
    val osVersion: String,
    val suggestedName: String,
)

interface DeviceInfoProvider {
    fun getInfo(): DeviceInfo
}

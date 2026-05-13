package com.findmydevice.domain.model

sealed interface DeviceResult<out T> {
    data class Success<T>(val data: T) : DeviceResult<T>
    data class Failure(val error: DeviceError) : DeviceResult<Nothing>
}

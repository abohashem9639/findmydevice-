package com.findmydevice.domain.model

sealed class DeviceError {
    data object Network : DeviceError()
    data object Unauthorized : DeviceError()
    data object NotFound : DeviceError()
    data object ValidationFailed : DeviceError()
    data class Unknown(val detail: String?) : DeviceError()
}

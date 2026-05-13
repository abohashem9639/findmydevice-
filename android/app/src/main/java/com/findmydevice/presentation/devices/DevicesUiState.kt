package com.findmydevice.presentation.devices

import com.findmydevice.domain.model.Device
import com.findmydevice.domain.platform.DeviceInfo

data class DevicesUiState(
    val isLoading: Boolean = false,
    val devices: List<Device> = emptyList(),
    val error: String? = null,

    // Register-dialog state
    val isRegisterDialogVisible: Boolean = false,
    val registerName: String = "",
    val isRegistering: Boolean = false,
    val registerError: String? = null,

    // Current phone's info — used to pre-fill the dialog and badge "This device"
    val currentDeviceInfo: DeviceInfo? = null,

    // One-shot navigation flag set when user logs out (or session is invalid)
    val isLoggedOut: Boolean = false,
)

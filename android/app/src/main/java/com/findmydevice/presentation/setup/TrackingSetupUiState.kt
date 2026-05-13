package com.findmydevice.presentation.setup

import com.findmydevice.domain.platform.PermissionStatus

data class TrackingSetupUiState(
    val isMiui: Boolean = false,

    val notificationsStatus: PermissionStatus = PermissionStatus.NotRequired,
    val locationForegroundStatus: PermissionStatus = PermissionStatus.NotRequired,
    val locationBackgroundStatus: PermissionStatus = PermissionStatus.NotRequired,

    // User confirmations for steps the OS cannot report on:
    val miuiAutostartAcknowledged: Boolean = false,
    val miuiBatterySettingsAcknowledged: Boolean = false,

    // Tracking state
    val isTrackingActive: Boolean = false,
    val trackingError: String? = null,
) {
    /** True once every required step has been satisfied. */
    val isReady: Boolean
        get() {
            val notifsOk = notificationsStatus != PermissionStatus.NotGranted
            val foregroundOk = locationForegroundStatus == PermissionStatus.Granted
            val backgroundOk = locationBackgroundStatus != PermissionStatus.NotGranted
            val miuiOk = !isMiui || (miuiAutostartAcknowledged && miuiBatterySettingsAcknowledged)
            return notifsOk && foregroundOk && backgroundOk && miuiOk
        }
}

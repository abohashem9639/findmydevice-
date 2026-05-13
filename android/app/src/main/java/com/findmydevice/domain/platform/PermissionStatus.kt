package com.findmydevice.domain.platform

enum class PermissionStatus {
    /** OS version does not require this permission — treat as granted. */
    NotRequired,
    Granted,
    NotGranted,
}

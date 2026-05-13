package com.findmydevice.domain.platform

interface PermissionsManager {

    /** Snapshot of the current grant state for a given permission. */
    fun getStatus(type: PermissionType): PermissionStatus

    /**
     * The runtime [android.Manifest.permission] string for the given logical permission,
     * or `null` if the current OS version does not require it.
     */
    fun getAndroidPermissionName(type: PermissionType): String?

    /** True when running on Xiaomi MIUI (needs extra autostart + battery setup). */
    fun isMiui(): Boolean
}

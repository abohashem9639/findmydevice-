package com.findmydevice.domain.platform

/**
 * Logical permissions our app cares about. Each maps to one or more OS-level
 * `android.permission.*` strings, with SDK guards (Notifications and Background
 * Location are gated by API level).
 */
enum class PermissionType {
    Notifications,
    LocationForeground,
    LocationBackground,
}

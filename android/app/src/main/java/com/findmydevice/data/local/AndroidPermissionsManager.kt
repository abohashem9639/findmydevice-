package com.findmydevice.data.local

import android.Manifest
import android.content.Context
import android.content.pm.PackageManager
import android.os.Build
import androidx.core.content.ContextCompat
import com.findmydevice.domain.platform.PermissionStatus
import com.findmydevice.domain.platform.PermissionType
import com.findmydevice.domain.platform.PermissionsManager
import dagger.hilt.android.qualifiers.ApplicationContext
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class AndroidPermissionsManager @Inject constructor(
    @ApplicationContext private val context: Context,
) : PermissionsManager {

    override fun getStatus(type: PermissionType): PermissionStatus {
        val permission = getAndroidPermissionName(type)
            ?: return PermissionStatus.NotRequired

        val granted = ContextCompat.checkSelfPermission(context, permission) ==
            PackageManager.PERMISSION_GRANTED

        return if (granted) PermissionStatus.Granted else PermissionStatus.NotGranted
    }

    override fun getAndroidPermissionName(type: PermissionType): String? = when (type) {
        PermissionType.Notifications -> if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.TIRAMISU) {
            Manifest.permission.POST_NOTIFICATIONS
        } else {
            null
        }

        PermissionType.LocationForeground -> Manifest.permission.ACCESS_FINE_LOCATION

        PermissionType.LocationBackground -> if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
            Manifest.permission.ACCESS_BACKGROUND_LOCATION
        } else {
            // On Android 9 and below, background location is implicit with fine location.
            null
        }
    }

    override fun isMiui(): Boolean = readSystemProperty("ro.miui.ui.version.name").isNotBlank() ||
        Build.MANUFACTURER.equals("Xiaomi", ignoreCase = true) ||
        Build.MANUFACTURER.equals("Redmi", ignoreCase = true) ||
        Build.MANUFACTURER.equals("Poco", ignoreCase = true)

    /**
     * Reads a system property via reflection. Used to detect MIUI without bringing
     * in a private Android API.
     */
    private fun readSystemProperty(key: String): String = try {
        val systemProperties = Class.forName("android.os.SystemProperties")
        val getMethod = systemProperties.getMethod("get", String::class.java)
        (getMethod.invoke(null, key) as? String).orEmpty()
    } catch (_: Throwable) {
        ""
    }
}

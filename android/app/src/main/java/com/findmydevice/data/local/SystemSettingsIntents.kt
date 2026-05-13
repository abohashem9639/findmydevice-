package com.findmydevice.data.local

import android.content.ComponentName
import android.content.Context
import android.content.Intent
import android.content.pm.PackageManager
import android.net.Uri
import android.provider.Settings

/**
 * Builds Intents that take the user straight to the relevant system / OEM page.
 *
 * MIUI deep links are best-effort: Xiaomi changes activity names between versions,
 * so we try several known component paths and fall back to the generic app-info
 * page when none resolves.
 */
object SystemSettingsIntents {

    /** App info page — works on every OEM, last-resort fallback. */
    fun appDetails(context: Context): Intent =
        Intent(Settings.ACTION_APPLICATION_DETAILS_SETTINGS).apply {
            data = Uri.fromParts("package", context.packageName, null)
            addFlags(Intent.FLAG_ACTIVITY_NEW_TASK)
        }

    /** System-wide location services on/off. */
    fun locationSettings(): Intent =
        Intent(Settings.ACTION_LOCATION_SOURCE_SETTINGS).apply {
            addFlags(Intent.FLAG_ACTIVITY_NEW_TASK)
        }

    /** Battery optimisation list (lets user mark our app as "Not optimized"). */
    fun batteryOptimization(): Intent =
        Intent(Settings.ACTION_IGNORE_BATTERY_OPTIMIZATION_SETTINGS).apply {
            addFlags(Intent.FLAG_ACTIVITY_NEW_TASK)
        }

    /** Notification settings for this app (Android 8+ channel page). */
    fun appNotificationSettings(context: Context): Intent =
        Intent(Settings.ACTION_APP_NOTIFICATION_SETTINGS).apply {
            putExtra(Settings.EXTRA_APP_PACKAGE, context.packageName)
            addFlags(Intent.FLAG_ACTIVITY_NEW_TASK)
        }

    /**
     * MIUI Security app → "Autostart". Critical on Xiaomi: without it, MIUI kills
     * our foreground service after a few hours regardless of OS permissions.
     */
    fun miuiAutostart(context: Context): Intent? {
        val candidates = listOf(
            ComponentName(
                "com.miui.securitycenter",
                "com.miui.permcenter.autostart.AutoStartManagementActivity",
            ),
            ComponentName(
                "com.miui.securitycenter",
                "com.miui.permcenter.MainActivity",
            ),
        )
        return resolveFirst(context, candidates)
    }

    /**
     * MIUI per-app battery saver. Default Xiaomi setting throttles background apps;
     * user must change it to "No restrictions".
     */
    fun miuiAppBatterySaver(context: Context): Intent? {
        val candidates = listOf(
            ComponentName(
                "com.miui.powerkeeper",
                "com.miui.powerkeeper.ui.HiddenAppsConfigActivity",
            ),
            ComponentName(
                "com.miui.powerkeeper",
                "com.miui.powerkeeper.ui.HiddenAppsContainerManagementActivity",
            ),
        )
        return resolveFirst(context, candidates)
    }

    private fun resolveFirst(context: Context, components: List<ComponentName>): Intent? {
        val pm = context.packageManager
        for (component in components) {
            val intent = Intent().apply {
                this.component = component
                addFlags(Intent.FLAG_ACTIVITY_NEW_TASK)
            }
            @Suppress("DEPRECATION")
            val resolved = pm.resolveActivity(intent, PackageManager.MATCH_DEFAULT_ONLY) != null
            if (resolved) return intent
        }
        return null
    }
}

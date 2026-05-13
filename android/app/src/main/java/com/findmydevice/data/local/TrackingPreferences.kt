package com.findmydevice.data.local

import android.content.Context
import androidx.datastore.preferences.core.booleanPreferencesKey
import androidx.datastore.preferences.core.edit
import androidx.datastore.preferences.core.stringPreferencesKey
import androidx.datastore.preferences.preferencesDataStore
import dagger.hilt.android.qualifiers.ApplicationContext
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.flow.map
import javax.inject.Inject
import javax.inject.Singleton

/**
 * Persistent tracking configuration:
 *   - `trackedDeviceId`: the server-side Device.Id we are currently reporting locations for.
 *   - `isTrackingEnabled`: whether the user has switched tracking on.
 *
 * Backed by DataStore (no encryption — these are not secrets).
 */
@Singleton
class TrackingPreferences @Inject constructor(
    @ApplicationContext private val context: Context,
) {
    private val Context.dataStore by preferencesDataStore(name = PREFS_NAME)

    val trackedDeviceIdFlow: Flow<String?> = context.dataStore.data
        .map { it[KEY_DEVICE_ID] }

    val isTrackingEnabledFlow: Flow<Boolean> = context.dataStore.data
        .map { it[KEY_TRACKING_ENABLED] ?: false }

    suspend fun getTrackedDeviceId(): String? =
        context.dataStore.data.first()[KEY_DEVICE_ID]

    suspend fun isTrackingEnabled(): Boolean =
        context.dataStore.data.first()[KEY_TRACKING_ENABLED] ?: false

    suspend fun setTracking(deviceId: String, enabled: Boolean) {
        context.dataStore.edit { prefs ->
            prefs[KEY_DEVICE_ID] = deviceId
            prefs[KEY_TRACKING_ENABLED] = enabled
        }
    }

    suspend fun setTrackingEnabled(enabled: Boolean) {
        context.dataStore.edit { prefs ->
            prefs[KEY_TRACKING_ENABLED] = enabled
        }
    }

    // ── MIUI acknowledgements (manual user confirmations the OS can't expose) ──

    suspend fun isMiuiAutostartAcknowledged(): Boolean =
        context.dataStore.data.first()[KEY_MIUI_AUTOSTART_ACK] ?: false

    suspend fun isMiuiBatteryAcknowledged(): Boolean =
        context.dataStore.data.first()[KEY_MIUI_BATTERY_ACK] ?: false

    suspend fun setMiuiAutostartAcknowledged(value: Boolean) {
        context.dataStore.edit { prefs ->
            prefs[KEY_MIUI_AUTOSTART_ACK] = value
        }
    }

    suspend fun setMiuiBatteryAcknowledged(value: Boolean) {
        context.dataStore.edit { prefs ->
            prefs[KEY_MIUI_BATTERY_ACK] = value
        }
    }

    suspend fun clear() {
        context.dataStore.edit { it.clear() }
    }

    private companion object {
        const val PREFS_NAME = "tracking_prefs"
        val KEY_DEVICE_ID = stringPreferencesKey("tracked_device_id")
        val KEY_TRACKING_ENABLED = booleanPreferencesKey("tracking_enabled")
        val KEY_MIUI_AUTOSTART_ACK = booleanPreferencesKey("miui_autostart_ack")
        val KEY_MIUI_BATTERY_ACK = booleanPreferencesKey("miui_battery_ack")
    }
}

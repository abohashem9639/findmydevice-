package com.findmydevice.data.system

import android.content.Context
import android.content.Intent
import android.content.IntentFilter
import android.os.BatteryManager
import dagger.hilt.android.qualifiers.ApplicationContext
import javax.inject.Inject
import javax.inject.Singleton

data class BatteryInfo(
    val levelPercent: Int?,
    val isCharging: Boolean?,
)

@Singleton
class BatteryInfoProvider @Inject constructor(
    @ApplicationContext private val context: Context,
) {
    /** Reads the sticky battery broadcast — no need to register a receiver. */
    fun getInfo(): BatteryInfo {
        val filter = IntentFilter(Intent.ACTION_BATTERY_CHANGED)
        @Suppress("UnspecifiedRegisterReceiverFlag")
        val intent = context.registerReceiver(null, filter)

        val level = intent?.getIntExtra(BatteryManager.EXTRA_LEVEL, -1) ?: -1
        val scale = intent?.getIntExtra(BatteryManager.EXTRA_SCALE, -1) ?: -1
        val percent = if (level >= 0 && scale > 0) (level * 100) / scale else null

        val status = intent?.getIntExtra(BatteryManager.EXTRA_STATUS, -1) ?: -1
        val charging = when (status) {
            BatteryManager.BATTERY_STATUS_CHARGING,
            BatteryManager.BATTERY_STATUS_FULL,
            -> true
            BatteryManager.BATTERY_STATUS_DISCHARGING,
            BatteryManager.BATTERY_STATUS_NOT_CHARGING,
            -> false
            else -> null
        }

        return BatteryInfo(levelPercent = percent, isCharging = charging)
    }
}

package com.findmydevice.service

import android.app.NotificationChannel
import android.app.NotificationManager
import android.content.Context
import androidx.core.content.getSystemService

object NotificationChannels {

    const val TRACKING_CHANNEL_ID = "tracking"
    const val TRACKING_NOTIFICATION_ID = 1001

    /**
     * Creates all required notification channels. Idempotent — Android merges
     * duplicate calls. Safe to call from Application.onCreate.
     *
     * Channels are required on Android 8+. Below that the call is a no-op.
     */
    fun ensureCreated(context: Context) {
        val manager = context.getSystemService<NotificationManager>() ?: return

        val tracking = NotificationChannel(
            TRACKING_CHANNEL_ID,
            "Location tracking",
            NotificationManager.IMPORTANCE_LOW, // no sound — it's an ongoing status
        ).apply {
            description = "Persistent notification while the app is tracking this device."
            setShowBadge(false)
        }

        manager.createNotificationChannel(tracking)
    }
}

package com.findmydevice.service

import android.app.Notification
import android.app.PendingIntent
import android.content.Context
import android.content.Intent
import android.content.pm.ServiceInfo
import android.os.Build
import android.os.IBinder
import android.util.Log
import androidx.core.app.NotificationCompat
import androidx.core.content.ContextCompat
import androidx.lifecycle.LifecycleService
import androidx.lifecycle.lifecycleScope
import com.findmydevice.MainActivity
import com.findmydevice.R
import com.findmydevice.data.local.TrackingPreferences
import com.findmydevice.data.location.LocationCollector
import com.findmydevice.data.system.BatteryInfoProvider
import com.findmydevice.data.system.LocationStateProvider
import com.findmydevice.data.system.NetworkInfoProvider
import com.findmydevice.domain.model.Heartbeat
import com.findmydevice.domain.model.LocationReport
import com.findmydevice.domain.repository.DeviceRepository
import com.findmydevice.domain.repository.LocationRepository
import dagger.hilt.android.AndroidEntryPoint
import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.Job
import kotlinx.coroutines.delay
import kotlinx.coroutines.isActive
import kotlinx.coroutines.launch
import javax.inject.Inject

/**
 * Foreground service that emits telemetry every [LOCATION_INTERVAL_MILLIS]:
 *   - Tries to get a fresh GPS fix (only possible when system Location is on).
 *   - If a fix is available → POST /Locations (full report).
 *   - Otherwise → POST /heartbeat (battery + network + isLocationEnabled=false).
 *
 * The latter keeps the device shown as Online in the dashboard even when the
 * user has turned Location off — and exposes that state as an anti-theft signal.
 */
@AndroidEntryPoint
class LocationTrackingService : LifecycleService() {

    @Inject lateinit var trackingPreferences: TrackingPreferences
    @Inject lateinit var locationCollector: LocationCollector
    @Inject lateinit var locationStateProvider: LocationStateProvider
    @Inject lateinit var locationRepository: LocationRepository
    @Inject lateinit var deviceRepository: DeviceRepository
    @Inject lateinit var batteryInfoProvider: BatteryInfoProvider
    @Inject lateinit var networkInfoProvider: NetworkInfoProvider

    private var collectionJob: Job? = null

    override fun onBind(intent: Intent): IBinder? {
        super.onBind(intent)
        return null
    }

    override fun onStartCommand(intent: Intent?, flags: Int, startId: Int): Int {
        super.onStartCommand(intent, flags, startId)

        if (intent?.action == ACTION_STOP) {
            Log.d(TAG, "Stop action received.")
            stopTracking()
            return START_NOT_STICKY
        }

        startTracking()
        return START_STICKY
    }

    private fun startTracking() {
        startInForeground(buildNotification())
        if (collectionJob?.isActive == true) return

        collectionJob = lifecycleScope.launch {
            val deviceId = trackingPreferences.getTrackedDeviceId()
            if (deviceId.isNullOrBlank()) {
                Log.w(TAG, "No tracked deviceId in prefs; stopping.")
                stopSelf()
                return@launch
            }

            Log.d(TAG, "Tracking loop starting for device $deviceId")

            while (isActive) {
                try {
                    tick(deviceId)
                } catch (e: CancellationException) {
                    throw e
                } catch (e: Exception) {
                    Log.w(TAG, "Tick failed: ${e.message}")
                }
                delay(LOCATION_INTERVAL_MILLIS)
            }
        }
    }

    private suspend fun tick(deviceId: String) {
        val battery = batteryInfoProvider.getInfo()
        val networkType = networkInfoProvider.currentType()
        val isLocationEnabled = locationStateProvider.isLocationEnabled()

        // Only ask FusedLocationProvider for a fix when location is actually on —
        // otherwise we waste 10s waiting for the timeout.
        val location = if (isLocationEnabled) {
            locationCollector.getCurrentLocation(timeoutMillis = 8_000)
        } else {
            null
        }

        if (location != null) {
            val report = LocationReport(
                latitude = location.latitude,
                longitude = location.longitude,
                accuracy = location.accuracy,
                altitude = if (location.hasAltitude()) location.altitude else null,
                speed = if (location.hasSpeed()) location.speed else null,
                bearing = if (location.hasBearing()) location.bearing else null,
                recordedAtEpochMillis = location.time,
                batteryLevel = battery.levelPercent,
                isCharging = battery.isCharging,
                networkType = networkType,
            )
            locationRepository.reportLocation(deviceId, report)
                .onSuccess { Log.d(TAG, "Location reported (${location.latitude}, ${location.longitude})") }
                .onFailure { Log.w(TAG, "Location report failed: ${it.message}") }
        } else {
            val heartbeat = Heartbeat(
                batteryLevel = battery.levelPercent,
                isCharging = battery.isCharging,
                networkType = networkType,
                isLocationEnabled = isLocationEnabled,
            )
            deviceRepository.sendHeartbeat(deviceId, heartbeat)
                .onSuccess { Log.d(TAG, "Heartbeat sent (locationEnabled=$isLocationEnabled)") }
                .onFailure { Log.w(TAG, "Heartbeat failed: ${it.message}") }
        }
    }

    private fun stopTracking() {
        collectionJob?.cancel()
        collectionJob = null
        stopForeground(STOP_FOREGROUND_REMOVE)
        stopSelf()
    }

    private fun startInForeground(notification: Notification) {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.UPSIDE_DOWN_CAKE) {
            startForeground(
                NotificationChannels.TRACKING_NOTIFICATION_ID,
                notification,
                ServiceInfo.FOREGROUND_SERVICE_TYPE_LOCATION,
            )
        } else {
            startForeground(NotificationChannels.TRACKING_NOTIFICATION_ID, notification)
        }
    }

    private fun buildNotification(): Notification {
        val openAppIntent = PendingIntent.getActivity(
            this,
            0,
            Intent(this, MainActivity::class.java).apply {
                flags = Intent.FLAG_ACTIVITY_SINGLE_TOP
            },
            PendingIntent.FLAG_UPDATE_CURRENT or PendingIntent.FLAG_IMMUTABLE,
        )

        val stopIntent = PendingIntent.getService(
            this,
            1,
            Intent(this, LocationTrackingService::class.java).apply { action = ACTION_STOP },
            PendingIntent.FLAG_UPDATE_CURRENT or PendingIntent.FLAG_IMMUTABLE,
        )

        return NotificationCompat.Builder(this, NotificationChannels.TRACKING_CHANNEL_ID)
            .setContentTitle("FindMyDevice")
            .setContentText("Tracking this device's location")
            .setSmallIcon(R.mipmap.ic_launcher)
            .setOngoing(true)
            .setSilent(true)
            .setPriority(NotificationCompat.PRIORITY_LOW)
            .setContentIntent(openAppIntent)
            .addAction(0, "Stop", stopIntent)
            .build()
    }

    companion object {
        private const val TAG = "LocationTrackingSvc"
        private const val ACTION_STOP = "com.findmydevice.action.STOP_TRACKING"

        /** Every 15 seconds. Dual-mode: location when possible, heartbeat otherwise. */
        private const val LOCATION_INTERVAL_MILLIS = 15_000L

        fun start(context: Context) {
            val intent = Intent(context, LocationTrackingService::class.java)
            ContextCompat.startForegroundService(context, intent)
        }

        fun stop(context: Context) {
            val intent = Intent(context, LocationTrackingService::class.java).apply {
                action = ACTION_STOP
            }
            context.startService(intent)
        }
    }
}

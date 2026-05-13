package com.findmydevice.data.location

import android.annotation.SuppressLint
import android.content.Context
import android.location.Location
import android.os.Looper
import com.google.android.gms.location.LocationCallback
import com.google.android.gms.location.LocationRequest
import com.google.android.gms.location.LocationResult
import com.google.android.gms.location.LocationServices
import com.google.android.gms.location.Priority
import com.google.android.gms.tasks.CancellationTokenSource
import dagger.hilt.android.qualifiers.ApplicationContext
import kotlinx.coroutines.channels.awaitClose
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.callbackFlow
import kotlinx.coroutines.tasks.await
import kotlinx.coroutines.withTimeoutOrNull
import javax.inject.Inject
import javax.inject.Singleton

/**
 * Wraps FusedLocationProviderClient with a coroutine Flow.
 *
 * The caller MUST hold ACCESS_FINE_LOCATION (and ACCESS_BACKGROUND_LOCATION when
 * collecting from a Service). Permission is enforced by the system; here we just
 * suppress the lint because we can't statically prove the caller has it.
 */
@Singleton
class LocationCollector @Inject constructor(
    @ApplicationContext private val context: Context,
) {
    private val fusedClient by lazy {
        LocationServices.getFusedLocationProviderClient(context)
    }

    @SuppressLint("MissingPermission")
    fun locationUpdates(intervalMillis: Long): Flow<Location> = callbackFlow {
        val request = LocationRequest.Builder(intervalMillis)
            .setPriority(Priority.PRIORITY_BALANCED_POWER_ACCURACY)
            .setMinUpdateIntervalMillis(intervalMillis / 2)
            .setWaitForAccurateLocation(false)
            .build()

        val callback = object : LocationCallback() {
            override fun onLocationResult(result: LocationResult) {
                result.locations.forEach { location -> trySend(location) }
            }
        }

        fusedClient.requestLocationUpdates(request, callback, Looper.getMainLooper())

        awaitClose {
            fusedClient.removeLocationUpdates(callback)
        }
    }

    /**
     * One-shot location request. Returns `null` if no fix is available within
     * [timeoutMillis] or if location services are disabled.
     */
    @SuppressLint("MissingPermission")
    suspend fun getCurrentLocation(timeoutMillis: Long = 10_000): Location? {
        return withTimeoutOrNull(timeoutMillis) {
            val cts = CancellationTokenSource()
            try {
                fusedClient
                    .getCurrentLocation(Priority.PRIORITY_BALANCED_POWER_ACCURACY, cts.token)
                    .await()
            } catch (_: Exception) {
                null
            } finally {
                cts.cancel()
            }
        }
    }
}

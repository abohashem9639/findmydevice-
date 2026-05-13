package com.findmydevice.data.system

import android.content.Context
import android.location.LocationManager
import android.os.Build
import androidx.core.content.getSystemService
import dagger.hilt.android.qualifiers.ApplicationContext
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class LocationStateProvider @Inject constructor(
    @ApplicationContext private val context: Context,
) {
    /**
     * Whether the phone's system-wide Location services are enabled.
     *
     * Important: this is independent of our app's runtime permission. Even with
     * `Always allow`, when this returns false NO location provider works.
     */
    fun isLocationEnabled(): Boolean {
        val lm = context.getSystemService<LocationManager>() ?: return false
        return if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.P) {
            lm.isLocationEnabled
        } else {
            @Suppress("DEPRECATION")
            lm.isProviderEnabled(LocationManager.GPS_PROVIDER) ||
                lm.isProviderEnabled(LocationManager.NETWORK_PROVIDER)
        }
    }
}

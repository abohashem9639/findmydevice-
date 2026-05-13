package com.findmydevice.data.system

import android.content.Context
import android.net.ConnectivityManager
import android.net.NetworkCapabilities
import androidx.core.content.getSystemService
import dagger.hilt.android.qualifiers.ApplicationContext
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class NetworkInfoProvider @Inject constructor(
    @ApplicationContext private val context: Context,
) {
    /**
     * Returns the active network's transport as one of:
     *   "wifi"     — connected over Wi-Fi
     *   "cellular" — connected over mobile data
     *   "none"     — no active network
     *   "unknown"  — connected but on an unrecognised transport (e.g. Ethernet)
     *
     * These match the backend's allowed NetworkType values.
     */
    fun currentType(): String {
        val cm = context.getSystemService<ConnectivityManager>() ?: return "unknown"
        val active = cm.activeNetwork ?: return "none"
        val capabilities = cm.getNetworkCapabilities(active) ?: return "none"

        return when {
            capabilities.hasTransport(NetworkCapabilities.TRANSPORT_WIFI) -> "wifi"
            capabilities.hasTransport(NetworkCapabilities.TRANSPORT_CELLULAR) -> "cellular"
            else -> "unknown"
        }
    }
}

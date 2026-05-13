package com.findmydevice

import android.app.Application
import com.findmydevice.service.NotificationChannels
import dagger.hilt.android.HiltAndroidApp

@HiltAndroidApp
class FindMyDeviceApp : Application() {

    override fun onCreate() {
        super.onCreate()
        NotificationChannels.ensureCreated(this)
    }
}

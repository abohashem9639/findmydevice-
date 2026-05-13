package com.findmydevice.data.local

import android.os.Build
import com.findmydevice.domain.platform.DeviceInfo
import com.findmydevice.domain.platform.DeviceInfoProvider
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class AndroidDeviceInfoProvider @Inject constructor() : DeviceInfoProvider {

    override fun getInfo(): DeviceInfo {
        val manufacturer = Build.MANUFACTURER.replaceFirstChar { it.uppercase() }
        val model = Build.MODEL
        val osVersion = Build.VERSION.RELEASE

        // "Xiaomi Redmi Note 13" — friendlier default than just model when model is cryptic.
        val suggestedName = if (model.startsWith(manufacturer, ignoreCase = true)) {
            model
        } else {
            "$manufacturer $model"
        }

        return DeviceInfo(
            manufacturer = manufacturer,
            model = model,
            osVersion = osVersion,
            suggestedName = suggestedName,
        )
    }
}

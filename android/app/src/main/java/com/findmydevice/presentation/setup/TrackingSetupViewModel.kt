package com.findmydevice.presentation.setup

import android.content.Context
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.findmydevice.data.local.TrackingPreferences
import com.findmydevice.domain.model.DeviceResult
import com.findmydevice.domain.platform.DeviceInfoProvider
import com.findmydevice.domain.platform.PermissionType
import com.findmydevice.domain.platform.PermissionsManager
import com.findmydevice.domain.repository.DeviceRepository
import com.findmydevice.service.LocationTrackingService
import dagger.hilt.android.lifecycle.HiltViewModel
import dagger.hilt.android.qualifiers.ApplicationContext
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import javax.inject.Inject

@HiltViewModel
class TrackingSetupViewModel @Inject constructor(
    @ApplicationContext private val context: Context,
    private val permissionsManager: PermissionsManager,
    private val deviceRepository: DeviceRepository,
    private val deviceInfoProvider: DeviceInfoProvider,
    private val trackingPreferences: TrackingPreferences,
) : ViewModel() {

    private val _uiState = MutableStateFlow(TrackingSetupUiState())
    val uiState: StateFlow<TrackingSetupUiState> = _uiState.asStateFlow()

    init {
        _uiState.update { it.copy(isMiui = permissionsManager.isMiui()) }
        refresh()
        viewModelScope.launch {
            _uiState.update {
                it.copy(
                    isTrackingActive = trackingPreferences.isTrackingEnabled(),
                    miuiAutostartAcknowledged = trackingPreferences.isMiuiAutostartAcknowledged(),
                    miuiBatterySettingsAcknowledged = trackingPreferences.isMiuiBatteryAcknowledged(),
                )
            }
        }
    }

    fun refresh() {
        _uiState.update { current ->
            current.copy(
                notificationsStatus = permissionsManager.getStatus(PermissionType.Notifications),
                locationForegroundStatus = permissionsManager.getStatus(PermissionType.LocationForeground),
                locationBackgroundStatus = permissionsManager.getStatus(PermissionType.LocationBackground),
            )
        }
    }

    fun onMarkAutostartDone() {
        _uiState.update { it.copy(miuiAutostartAcknowledged = true) }
        viewModelScope.launch {
            trackingPreferences.setMiuiAutostartAcknowledged(true)
        }
    }

    fun onMarkBatterySettingsDone() {
        _uiState.update { it.copy(miuiBatterySettingsAcknowledged = true) }
        viewModelScope.launch {
            trackingPreferences.setMiuiBatteryAcknowledged(true)
        }
    }

    fun onStartTrackingClicked() {
        viewModelScope.launch {
            _uiState.update { it.copy(trackingError = null) }

            // Try to match the current phone to a registered server-side device.
            val info = deviceInfoProvider.getInfo()
            val deviceId = when (val result = deviceRepository.getMyDevices()) {
                is DeviceResult.Success -> result.data.firstOrNull { device ->
                    device.manufacturer.equals(info.manufacturer, ignoreCase = true) &&
                        device.model.equals(info.model, ignoreCase = true)
                }?.id
                is DeviceResult.Failure -> null
            }

            if (deviceId == null) {
                _uiState.update {
                    it.copy(trackingError = "Register this device first from the Devices screen.")
                }
                return@launch
            }

            trackingPreferences.setTracking(deviceId = deviceId, enabled = true)
            LocationTrackingService.start(context)

            _uiState.update { it.copy(isTrackingActive = true) }
        }
    }

    fun onStopTrackingClicked() {
        viewModelScope.launch {
            trackingPreferences.setTrackingEnabled(false)
            LocationTrackingService.stop(context)
            _uiState.update { it.copy(isTrackingActive = false) }
        }
    }
}

package com.findmydevice.presentation.devices

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.findmydevice.domain.model.DeviceError
import com.findmydevice.domain.model.DeviceResult
import com.findmydevice.domain.platform.DeviceInfoProvider
import com.findmydevice.domain.repository.AuthRepository
import com.findmydevice.domain.repository.DeviceRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import javax.inject.Inject

@HiltViewModel
class DevicesViewModel @Inject constructor(
    private val deviceRepository: DeviceRepository,
    private val authRepository: AuthRepository,
    private val deviceInfoProvider: DeviceInfoProvider,
) : ViewModel() {

    private val _uiState = MutableStateFlow(DevicesUiState())
    val uiState: StateFlow<DevicesUiState> = _uiState.asStateFlow()

    init {
        _uiState.update { it.copy(currentDeviceInfo = deviceInfoProvider.getInfo()) }
        loadDevices()
    }

    fun loadDevices() {
        if (_uiState.value.isLoading) return

        viewModelScope.launch {
            _uiState.update { it.copy(isLoading = true, error = null) }

            when (val result = deviceRepository.getMyDevices()) {
                is DeviceResult.Success -> _uiState.update {
                    it.copy(isLoading = false, devices = result.data)
                }
                is DeviceResult.Failure -> handleListError(result.error)
            }
        }
    }

    fun onShowRegisterDialog() {
        val suggestedName = _uiState.value.currentDeviceInfo?.suggestedName.orEmpty()
        _uiState.update {
            it.copy(
                isRegisterDialogVisible = true,
                registerName = suggestedName,
                registerError = null,
            )
        }
    }

    fun onHideRegisterDialog() {
        if (_uiState.value.isRegistering) return
        _uiState.update {
            it.copy(
                isRegisterDialogVisible = false,
                registerName = "",
                registerError = null,
            )
        }
    }

    fun onRegisterNameChange(value: String) {
        _uiState.update { it.copy(registerName = value, registerError = null) }
    }

    fun onRegisterClicked() {
        val state = _uiState.value
        val info = state.currentDeviceInfo
        val trimmedName = state.registerName.trim()

        if (trimmedName.isEmpty()) {
            _uiState.update { it.copy(registerError = "Name is required.") }
            return
        }
        if (info == null || state.isRegistering) return

        viewModelScope.launch {
            _uiState.update { it.copy(isRegistering = true, registerError = null) }

            val result = deviceRepository.registerDevice(
                name = trimmedName,
                manufacturer = info.manufacturer,
                model = info.model,
                platform = "Android",
                osVersion = info.osVersion,
                fcmToken = null, // populated in a later phase (FCM integration)
            )

            when (result) {
                is DeviceResult.Success -> {
                    _uiState.update {
                        it.copy(
                            isRegistering = false,
                            isRegisterDialogVisible = false,
                            registerName = "",
                            devices = listOf(result.data) + it.devices,
                        )
                    }
                }
                is DeviceResult.Failure -> {
                    if (result.error is DeviceError.Unauthorized) {
                        forceLogout()
                    } else {
                        _uiState.update {
                            it.copy(
                                isRegistering = false,
                                registerError = mapError(result.error),
                            )
                        }
                    }
                }
            }
        }
    }

    fun onDeleteDevice(deviceId: String) {
        viewModelScope.launch {
            when (val result = deviceRepository.deleteDevice(deviceId)) {
                is DeviceResult.Success -> _uiState.update { state ->
                    state.copy(devices = state.devices.filterNot { it.id == deviceId })
                }
                is DeviceResult.Failure -> {
                    if (result.error is DeviceError.Unauthorized) {
                        forceLogout()
                    } else {
                        _uiState.update { it.copy(error = mapError(result.error)) }
                    }
                }
            }
        }
    }

    fun onLogout() {
        viewModelScope.launch {
            authRepository.logout()
            _uiState.update { it.copy(isLoggedOut = true) }
        }
    }

    fun onLogoutConsumed() {
        _uiState.update { it.copy(isLoggedOut = false) }
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private fun handleListError(error: DeviceError) {
        if (error is DeviceError.Unauthorized) {
            forceLogout()
        } else {
            _uiState.update { it.copy(isLoading = false, error = mapError(error)) }
        }
    }

    private fun forceLogout() {
        viewModelScope.launch {
            authRepository.logout()
            _uiState.update { it.copy(isLoggedOut = true, isLoading = false, isRegistering = false) }
        }
    }

    private fun mapError(error: DeviceError): String = when (error) {
        DeviceError.Network -> "No connection. Check your network and try again."
        DeviceError.Unauthorized -> "Session expired. Please sign in again."
        DeviceError.NotFound -> "Device not found."
        DeviceError.ValidationFailed -> "Please check your input."
        is DeviceError.Unknown -> error.detail ?: "Something went wrong."
    }
}

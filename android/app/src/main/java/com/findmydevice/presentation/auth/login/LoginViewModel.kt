package com.findmydevice.presentation.auth.login

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.findmydevice.domain.model.AuthError
import com.findmydevice.domain.model.AuthResult
import com.findmydevice.domain.repository.AuthRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import javax.inject.Inject

@HiltViewModel
class LoginViewModel @Inject constructor(
    private val authRepository: AuthRepository,
) : ViewModel() {

    private val _uiState = MutableStateFlow(LoginUiState())
    val uiState: StateFlow<LoginUiState> = _uiState.asStateFlow()

    fun onEmailChanged(value: String) {
        _uiState.update { it.copy(email = value, error = null) }
    }

    fun onPasswordChanged(value: String) {
        _uiState.update { it.copy(password = value, error = null) }
    }

    fun onTogglePasswordVisibility() {
        _uiState.update { it.copy(isPasswordVisible = !it.isPasswordVisible) }
    }

    fun onLoginClicked() {
        val current = _uiState.value

        if (current.email.isBlank() || current.password.isBlank()) {
            _uiState.update { it.copy(error = "Email and password are required.") }
            return
        }
        if (current.isLoading) return

        viewModelScope.launch {
            _uiState.update { it.copy(isLoading = true, error = null) }

            val result = authRepository.signin(
                email = current.email.trim(),
                password = current.password,
            )

            when (result) {
                is AuthResult.Success -> {
                    _uiState.update {
                        it.copy(isLoading = false, isLoginSuccessful = true)
                    }
                }
                is AuthResult.Failure -> {
                    _uiState.update {
                        it.copy(isLoading = false, error = mapError(result.error))
                    }
                }
            }
        }
    }

    fun onNavigationConsumed() {
        _uiState.update { it.copy(isLoginSuccessful = false) }
    }

    private fun mapError(error: AuthError): String = when (error) {
        AuthError.InvalidCredentials -> "Invalid email or password."
        AuthError.UserAlreadyExists -> "Email already in use."
        AuthError.ValidationFailed -> "Please check your input."
        AuthError.AccountLocked -> "Account is locked. Try again later."
        AuthError.Network -> "No connection. Check your network and try again."
        is AuthError.Unknown -> error.detail ?: "Something went wrong."
    }
}

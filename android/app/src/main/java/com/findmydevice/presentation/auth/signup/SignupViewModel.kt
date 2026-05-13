package com.findmydevice.presentation.auth.signup

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
class SignupViewModel @Inject constructor(
    private val authRepository: AuthRepository,
) : ViewModel() {

    private val _uiState = MutableStateFlow(SignupUiState())
    val uiState: StateFlow<SignupUiState> = _uiState.asStateFlow()

    fun onEmailChanged(value: String) {
        _uiState.update { it.copy(email = value, error = null) }
    }

    fun onPasswordChanged(value: String) {
        _uiState.update { it.copy(password = value, error = null) }
    }

    fun onConfirmPasswordChanged(value: String) {
        _uiState.update { it.copy(confirmPassword = value, error = null) }
    }

    fun onDisplayNameChanged(value: String) {
        _uiState.update { it.copy(displayName = value, error = null) }
    }

    fun onTogglePasswordVisibility() {
        _uiState.update { it.copy(isPasswordVisible = !it.isPasswordVisible) }
    }

    fun onSignupClicked() {
        val current = _uiState.value

        val validationError = validate(current)
        if (validationError != null) {
            _uiState.update { it.copy(error = validationError) }
            return
        }
        if (current.isLoading) return

        viewModelScope.launch {
            _uiState.update { it.copy(isLoading = true, error = null) }

            val result = authRepository.signup(
                email = current.email.trim(),
                password = current.password,
                confirmPassword = current.confirmPassword,
                displayName = current.displayName.trim().takeIf { it.isNotEmpty() },
            )

            when (result) {
                is AuthResult.Success -> {
                    _uiState.update {
                        it.copy(isLoading = false, isSignupSuccessful = true)
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
        _uiState.update { it.copy(isSignupSuccessful = false) }
    }

    private fun validate(state: SignupUiState): String? {
        if (state.email.isBlank()) return "Email is required."
        if (!ANDROID_EMAIL_REGEX.matches(state.email.trim())) return "Invalid email format."
        if (state.password.length < 6) return "Password must be at least 6 characters."
        if (state.password != state.confirmPassword) return "Passwords do not match."
        return null
    }

    private fun mapError(error: AuthError): String = when (error) {
        AuthError.InvalidCredentials -> "Invalid credentials."
        AuthError.UserAlreadyExists -> "An account with this email already exists."
        AuthError.ValidationFailed -> "Please check your input."
        AuthError.AccountLocked -> "Account locked."
        AuthError.Network -> "No connection. Check your network and try again."
        is AuthError.Unknown -> error.detail ?: "Something went wrong."
    }

    private companion object {
        // Conservative regex — backend does the authoritative validation.
        val ANDROID_EMAIL_REGEX = Regex("^[^\\s@]+@[^\\s@]+\\.[^\\s@]+$")
    }
}

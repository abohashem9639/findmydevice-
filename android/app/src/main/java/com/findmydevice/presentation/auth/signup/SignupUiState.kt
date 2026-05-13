package com.findmydevice.presentation.auth.signup

data class SignupUiState(
    val email: String = "",
    val password: String = "",
    val confirmPassword: String = "",
    val displayName: String = "",
    val isPasswordVisible: Boolean = false,
    val isLoading: Boolean = false,
    val error: String? = null,
    val isSignupSuccessful: Boolean = false,
)

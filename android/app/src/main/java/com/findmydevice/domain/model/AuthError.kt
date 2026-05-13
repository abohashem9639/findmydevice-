package com.findmydevice.domain.model

sealed class AuthError {
    data object InvalidCredentials : AuthError()
    data object UserAlreadyExists : AuthError()
    data object ValidationFailed : AuthError()
    data object AccountLocked : AuthError()
    data object Network : AuthError()
    data class Unknown(val detail: String?) : AuthError()
}

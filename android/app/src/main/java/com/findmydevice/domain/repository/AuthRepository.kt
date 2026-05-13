package com.findmydevice.domain.repository

import com.findmydevice.domain.model.AuthResult
import com.findmydevice.domain.model.AuthSession

interface AuthRepository {

    suspend fun signin(email: String, password: String): AuthResult<AuthSession>

    suspend fun signup(
        email: String,
        password: String,
        confirmPassword: String,
        displayName: String?,
    ): AuthResult<AuthSession>

    suspend fun refresh(): AuthResult<AuthSession>

    fun getCurrentSession(): AuthSession?

    fun isLoggedIn(): Boolean

    suspend fun logout()
}

package com.findmydevice.data.repository

import com.findmydevice.data.local.SecureTokenStorage
import com.findmydevice.data.remote.api.AuthApi
import com.findmydevice.data.remote.dto.AuthResponseDto
import com.findmydevice.data.remote.dto.ProblemDetailsDto
import com.findmydevice.data.remote.dto.RefreshRequestDto
import com.findmydevice.data.remote.dto.SigninRequestDto
import com.findmydevice.data.remote.dto.SignupRequestDto
import com.findmydevice.domain.model.AuthError
import com.findmydevice.domain.model.AuthResult
import com.findmydevice.domain.model.AuthSession
import com.findmydevice.domain.model.AuthTokens
import com.findmydevice.domain.repository.AuthRepository
import kotlinx.coroutines.CancellationException
import kotlinx.serialization.json.Json
import okio.IOException
import retrofit2.Response
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class AuthRepositoryImpl @Inject constructor(
    private val authApi: AuthApi,
    private val tokenStorage: SecureTokenStorage,
    private val json: Json,
) : AuthRepository {

    override suspend fun signin(email: String, password: String): AuthResult<AuthSession> =
        executeAuth { authApi.signin(SigninRequestDto(email = email, password = password)) }

    override suspend fun signup(
        email: String,
        password: String,
        confirmPassword: String,
        displayName: String?,
    ): AuthResult<AuthSession> = executeAuth {
        authApi.signup(
            SignupRequestDto(
                email = email,
                password = password,
                confirmPassword = confirmPassword,
                displayName = displayName,
            ),
        )
    }

    override suspend fun refresh(): AuthResult<AuthSession> {
        val current = tokenStorage.getSession()
            ?: return AuthResult.Failure(AuthError.InvalidCredentials)
        return executeAuth { authApi.refresh(RefreshRequestDto(current.tokens.refreshToken)) }
    }

    override fun getCurrentSession(): AuthSession? = tokenStorage.getSession()

    override fun isLoggedIn(): Boolean = tokenStorage.getSession() != null

    override suspend fun logout() {
        // TODO Phase 3.4+: also call POST /api/v1/Auth/revoke to invalidate refresh token server-side.
        tokenStorage.clear()
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private suspend fun executeAuth(
        block: suspend () -> Response<AuthResponseDto>,
    ): AuthResult<AuthSession> {
        return try {
            handleResponse(block())
        } catch (e: CancellationException) {
            throw e
        } catch (e: IOException) {
            AuthResult.Failure(AuthError.Network)
        } catch (e: Exception) {
            AuthResult.Failure(AuthError.Unknown(e.message))
        }
    }

    private fun handleResponse(response: Response<AuthResponseDto>): AuthResult<AuthSession> {
        if (response.isSuccessful) {
            val body = response.body()
                ?: return AuthResult.Failure(AuthError.Unknown("Empty response body."))
            val session = mapToSession(body)
                ?: return AuthResult.Failure(AuthError.Unknown("Malformed auth response."))
            tokenStorage.saveSession(session)
            return AuthResult.Success(session)
        }

        val problem = parseProblemDetails(response)
        return AuthResult.Failure(mapToAuthError(response.code(), problem))
    }

    private fun parseProblemDetails(response: Response<*>): ProblemDetailsDto? {
        val errorBodyString = response.errorBody()?.string()
        if (errorBodyString.isNullOrBlank()) return null
        return runCatching {
            json.decodeFromString(ProblemDetailsDto.serializer(), errorBodyString)
        }.getOrNull()
    }

    private fun mapToSession(dto: AuthResponseDto): AuthSession? {
        val accessToken = dto.accessToken ?: return null
        val refreshToken = dto.refreshToken ?: return null
        val user = dto.user ?: return null

        val expiresAt = System.currentTimeMillis() + (dto.expiresInSeconds * 1000L)

        return AuthSession(
            tokens = AuthTokens(accessToken, refreshToken, expiresAt),
            userId = user.id,
            userEmail = user.email,
            userDisplayName = user.displayName,
        )
    }

    private fun mapToAuthError(httpCode: Int, problem: ProblemDetailsDto?): AuthError {
        problem?.title?.let { title ->
            when (title) {
                "InvalidCredentials" -> return AuthError.InvalidCredentials
                "UserAlreadyExists", "EmailAlreadyTaken" -> return AuthError.UserAlreadyExists
                "ValidationFailed" -> return AuthError.ValidationFailed
                "AccountLocked" -> return AuthError.AccountLocked
            }
        }

        return when (httpCode) {
            400 -> AuthError.ValidationFailed
            401 -> AuthError.InvalidCredentials
            409 -> AuthError.UserAlreadyExists
            423 -> AuthError.AccountLocked
            else -> AuthError.Unknown(problem?.detail ?: "HTTP $httpCode")
        }
    }
}

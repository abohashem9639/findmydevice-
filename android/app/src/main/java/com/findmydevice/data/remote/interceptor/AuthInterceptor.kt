package com.findmydevice.data.remote.interceptor

import com.findmydevice.data.local.SecureTokenStorage
import okhttp3.Interceptor
import okhttp3.Response
import javax.inject.Inject
import javax.inject.Singleton

/**
 * Adds `Authorization: Bearer <accessToken>` to every outgoing request except the
 * public Auth endpoints (signup, signin, refresh, forgot/reset password).
 *
 * Token-refresh-on-401 is a separate concern — to be added in a later phase via
 * OkHttp's [okhttp3.Authenticator] so we don't recurse here.
 */
@Singleton
class AuthInterceptor @Inject constructor(
    private val tokenStorage: SecureTokenStorage,
) : Interceptor {

    override fun intercept(chain: Interceptor.Chain): Response {
        val request = chain.request()

        if (isPublicEndpoint(request.url.encodedPath)) {
            return chain.proceed(request)
        }

        val token = tokenStorage.getSession()?.tokens?.accessToken
        val authedRequest = if (token != null) {
            request.newBuilder()
                .header("Authorization", "Bearer $token")
                .build()
        } else {
            request
        }

        return chain.proceed(authedRequest)
    }

    private fun isPublicEndpoint(path: String): Boolean =
        PUBLIC_PATHS.any { path.endsWith(it, ignoreCase = true) }

    private companion object {
        val PUBLIC_PATHS = listOf(
            "/Auth/signup",
            "/Auth/signin",
            "/Auth/refresh",
            "/Auth/forgot-password",
            "/Auth/reset-password",
            "/Auth/signin-with-google",
        )
    }
}

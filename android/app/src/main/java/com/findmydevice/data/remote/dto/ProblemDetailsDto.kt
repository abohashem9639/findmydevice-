package com.findmydevice.data.remote.dto

import kotlinx.serialization.Serializable

/**
 * ASP.NET Core's standard error payload (RFC 7807).
 * The backend uses Problem(...) on auth failures: title carries the AuthError enum
 * name (e.g. "InvalidCredentials"), detail carries a human-readable description.
 */
@Serializable
data class ProblemDetailsDto(
    val type: String? = null,
    val title: String? = null,
    val status: Int? = null,
    val detail: String? = null,
    val instance: String? = null,
)

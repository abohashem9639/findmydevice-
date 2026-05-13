package com.findmydevice.data.local

import android.content.Context
import android.content.SharedPreferences
import androidx.security.crypto.EncryptedSharedPreferences
import androidx.security.crypto.MasterKey
import com.findmydevice.domain.model.AuthSession
import com.findmydevice.domain.model.AuthTokens
import dagger.hilt.android.qualifiers.ApplicationContext
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class EncryptedSecureTokenStorage @Inject constructor(
    @ApplicationContext private val context: Context,
) : SecureTokenStorage {

    private val prefs: SharedPreferences by lazy {
        val masterKey = MasterKey.Builder(context)
            .setKeyScheme(MasterKey.KeyScheme.AES256_GCM)
            .build()

        EncryptedSharedPreferences.create(
            context,
            PREFS_NAME,
            masterKey,
            EncryptedSharedPreferences.PrefKeyEncryptionScheme.AES256_SIV,
            EncryptedSharedPreferences.PrefValueEncryptionScheme.AES256_GCM,
        )
    }

    override fun saveSession(session: AuthSession) {
        prefs.edit()
            .putString(KEY_ACCESS_TOKEN, session.tokens.accessToken)
            .putString(KEY_REFRESH_TOKEN, session.tokens.refreshToken)
            .putLong(KEY_EXPIRES_AT, session.tokens.expiresAtEpochMillis)
            .putString(KEY_USER_ID, session.userId)
            .putString(KEY_USER_EMAIL, session.userEmail)
            .putString(KEY_USER_DISPLAY_NAME, session.userDisplayName)
            .apply()
    }

    override fun getSession(): AuthSession? {
        val accessToken = prefs.getString(KEY_ACCESS_TOKEN, null) ?: return null
        val refreshToken = prefs.getString(KEY_REFRESH_TOKEN, null) ?: return null
        val expiresAt = prefs.getLong(KEY_EXPIRES_AT, 0L)
        val userId = prefs.getString(KEY_USER_ID, null) ?: return null
        val userEmail = prefs.getString(KEY_USER_EMAIL, null) ?: return null
        val userDisplayName = prefs.getString(KEY_USER_DISPLAY_NAME, null)

        return AuthSession(
            tokens = AuthTokens(accessToken, refreshToken, expiresAt),
            userId = userId,
            userEmail = userEmail,
            userDisplayName = userDisplayName,
        )
    }

    override fun clear() {
        prefs.edit().clear().apply()
    }

    companion object {
        private const val PREFS_NAME = "findmydevice_secure_prefs"
        private const val KEY_ACCESS_TOKEN = "access_token"
        private const val KEY_REFRESH_TOKEN = "refresh_token"
        private const val KEY_EXPIRES_AT = "expires_at_epoch_millis"
        private const val KEY_USER_ID = "user_id"
        private const val KEY_USER_EMAIL = "user_email"
        private const val KEY_USER_DISPLAY_NAME = "user_display_name"
    }
}

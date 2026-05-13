package com.findmydevice.di

import com.findmydevice.data.local.AndroidDeviceInfoProvider
import com.findmydevice.data.local.AndroidPermissionsManager
import com.findmydevice.data.local.EncryptedSecureTokenStorage
import com.findmydevice.data.local.SecureTokenStorage
import com.findmydevice.domain.platform.DeviceInfoProvider
import com.findmydevice.domain.platform.PermissionsManager
import dagger.Binds
import dagger.Module
import dagger.hilt.InstallIn
import dagger.hilt.components.SingletonComponent
import javax.inject.Singleton

@Module
@InstallIn(SingletonComponent::class)
abstract class StorageModule {

    @Binds
    @Singleton
    abstract fun bindSecureTokenStorage(
        impl: EncryptedSecureTokenStorage,
    ): SecureTokenStorage

    @Binds
    @Singleton
    abstract fun bindDeviceInfoProvider(
        impl: AndroidDeviceInfoProvider,
    ): DeviceInfoProvider

    @Binds
    @Singleton
    abstract fun bindPermissionsManager(
        impl: AndroidPermissionsManager,
    ): PermissionsManager
}

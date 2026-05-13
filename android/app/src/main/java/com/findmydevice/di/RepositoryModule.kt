package com.findmydevice.di

import com.findmydevice.data.repository.AuthRepositoryImpl
import com.findmydevice.data.repository.DeviceRepositoryImpl
import com.findmydevice.data.repository.LocationRepositoryImpl
import com.findmydevice.domain.repository.AuthRepository
import com.findmydevice.domain.repository.DeviceRepository
import com.findmydevice.domain.repository.LocationRepository
import dagger.Binds
import dagger.Module
import dagger.hilt.InstallIn
import dagger.hilt.components.SingletonComponent
import javax.inject.Singleton

@Module
@InstallIn(SingletonComponent::class)
abstract class RepositoryModule {

    @Binds
    @Singleton
    abstract fun bindAuthRepository(impl: AuthRepositoryImpl): AuthRepository

    @Binds
    @Singleton
    abstract fun bindDeviceRepository(impl: DeviceRepositoryImpl): DeviceRepository

    @Binds
    @Singleton
    abstract fun bindLocationRepository(impl: LocationRepositoryImpl): LocationRepository
}

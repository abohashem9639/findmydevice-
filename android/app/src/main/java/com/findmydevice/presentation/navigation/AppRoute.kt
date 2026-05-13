package com.findmydevice.presentation.navigation

sealed class AppRoute(val route: String) {
    data object Login : AppRoute("login")
    data object Signup : AppRoute("signup")
    data object Devices : AppRoute("devices")
    data object TrackingSetup : AppRoute("tracking-setup")
}

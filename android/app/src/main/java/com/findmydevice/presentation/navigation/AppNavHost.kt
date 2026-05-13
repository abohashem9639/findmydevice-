package com.findmydevice.presentation.navigation

import androidx.compose.runtime.Composable
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController
import com.findmydevice.presentation.auth.login.LoginScreen
import com.findmydevice.presentation.auth.signup.SignupScreen
import com.findmydevice.presentation.devices.DevicesScreen
import com.findmydevice.presentation.setup.TrackingSetupScreen

@Composable
fun AppNavHost() {
    val navController = rememberNavController()

    NavHost(
        navController = navController,
        startDestination = AppRoute.Login.route,
    ) {
        composable(AppRoute.Login.route) {
            LoginScreen(
                onNavigateToSignup = {
                    navController.navigate(AppRoute.Signup.route)
                },
                onLoginSuccess = {
                    navController.navigate(AppRoute.Devices.route) {
                        popUpTo(AppRoute.Login.route) { inclusive = true }
                    }
                },
            )
        }

        composable(AppRoute.Signup.route) {
            SignupScreen(
                onNavigateBack = { navController.popBackStack() },
                onSignupSuccess = {
                    navController.navigate(AppRoute.Devices.route) {
                        popUpTo(AppRoute.Login.route) { inclusive = true }
                    }
                },
            )
        }

        composable(AppRoute.Devices.route) {
            DevicesScreen(
                onLoggedOut = {
                    navController.navigate(AppRoute.Login.route) {
                        popUpTo(AppRoute.Devices.route) { inclusive = true }
                    }
                },
                onNavigateToTrackingSetup = {
                    navController.navigate(AppRoute.TrackingSetup.route)
                },
            )
        }

        composable(AppRoute.TrackingSetup.route) {
            TrackingSetupScreen(
                onNavigateBack = { navController.popBackStack() },
            )
        }
    }
}

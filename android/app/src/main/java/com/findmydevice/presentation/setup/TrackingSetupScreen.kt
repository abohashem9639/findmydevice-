package com.findmydevice.presentation.setup

import android.Manifest
import android.content.Intent
import android.os.Build
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.filled.Check
import androidx.compose.material.icons.filled.Lock
import androidx.compose.material3.Button
import androidx.compose.material3.Card
import androidx.compose.material3.CardDefaults
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.Lifecycle
import androidx.lifecycle.compose.LifecycleEventEffect
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import com.findmydevice.data.local.SystemSettingsIntents
import com.findmydevice.domain.platform.PermissionStatus
import com.findmydevice.ui.theme.FindMyDeviceTheme

@Composable
fun TrackingSetupScreen(
    onNavigateBack: () -> Unit,
    viewModel: TrackingSetupViewModel = hiltViewModel(),
) {
    val uiState by viewModel.uiState.collectAsStateWithLifecycle()
    val context = LocalContext.current

    // Re-check permissions every time the screen comes back into focus (e.g. after
    // the user returns from a System Settings deep link).
    LifecycleEventEffect(Lifecycle.Event.ON_RESUME) {
        viewModel.refresh()
    }

    val notificationsLauncher = rememberLauncherForActivityResult(
        contract = ActivityResultContracts.RequestPermission(),
        onResult = { viewModel.refresh() },
    )

    val foregroundLocationLauncher = rememberLauncherForActivityResult(
        contract = ActivityResultContracts.RequestMultiplePermissions(),
        onResult = { viewModel.refresh() },
    )

    val openIntent: (Intent?) -> Unit = { intent ->
        if (intent != null) {
            runCatching { context.startActivity(intent) }
        }
    }

    TrackingSetupContent(
        uiState = uiState,
        onNavigateBack = onNavigateBack,
        onRequestNotifications = {
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.TIRAMISU) {
                notificationsLauncher.launch(Manifest.permission.POST_NOTIFICATIONS)
            }
        },
        onRequestForegroundLocation = {
            foregroundLocationLauncher.launch(
                arrayOf(
                    Manifest.permission.ACCESS_FINE_LOCATION,
                    Manifest.permission.ACCESS_COARSE_LOCATION,
                ),
            )
        },
        onOpenBackgroundLocationSettings = {
            openIntent(SystemSettingsIntents.appDetails(context))
        },
        onOpenMiuiAutostart = {
            val intent = SystemSettingsIntents.miuiAutostart(context)
                ?: SystemSettingsIntents.appDetails(context)
            openIntent(intent)
        },
        onMarkAutostartDone = viewModel::onMarkAutostartDone,
        onOpenMiuiBatterySettings = {
            val intent = SystemSettingsIntents.miuiAppBatterySaver(context)
                ?: SystemSettingsIntents.batteryOptimization()
            openIntent(intent)
        },
        onMarkBatterySettingsDone = viewModel::onMarkBatterySettingsDone,
        onStartTracking = viewModel::onStartTrackingClicked,
        onStopTracking = viewModel::onStopTrackingClicked,
    )
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
private fun TrackingSetupContent(
    uiState: TrackingSetupUiState,
    onNavigateBack: () -> Unit,
    onRequestNotifications: () -> Unit,
    onRequestForegroundLocation: () -> Unit,
    onOpenBackgroundLocationSettings: () -> Unit,
    onOpenMiuiAutostart: () -> Unit,
    onMarkAutostartDone: () -> Unit,
    onOpenMiuiBatterySettings: () -> Unit,
    onMarkBatterySettingsDone: () -> Unit,
    onStartTracking: () -> Unit,
    onStopTracking: () -> Unit,
) {
    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Tracking setup") },
                navigationIcon = {
                    IconButton(onClick = onNavigateBack) {
                        Icon(Icons.AutoMirrored.Filled.ArrowBack, contentDescription = "Back")
                    }
                },
            )
        },
    ) { padding ->
        LazyColumn(
            modifier = Modifier
                .fillMaxSize()
                .padding(padding),
            contentPadding = PaddingValues(16.dp),
            verticalArrangement = Arrangement.spacedBy(12.dp),
        ) {
            item {
                Text(
                    text = "Complete these steps so we can keep tracking your phone even when the app is in the background.",
                    style = MaterialTheme.typography.bodyMedium,
                    color = MaterialTheme.colorScheme.onSurfaceVariant,
                )
            }

            // Step 1: Notifications
            if (uiState.notificationsStatus != PermissionStatus.NotRequired) {
                item {
                    SetupStepCard(
                        stepNumber = 1,
                        title = "Allow notifications",
                        description = "Required so the tracking service can show its ongoing notification (Android 13+).",
                        state = uiState.notificationsStatus.toStepState(),
                        actionLabel = "Grant",
                        onActionClick = onRequestNotifications,
                    )
                }
            }

            // Step 2: Foreground location
            item {
                SetupStepCard(
                    stepNumber = 2,
                    title = "Allow location while using the app",
                    description = "We use precise location to update your device's last-known position.",
                    state = uiState.locationForegroundStatus.toStepState(),
                    actionLabel = "Grant",
                    onActionClick = onRequestForegroundLocation,
                    isLocked = false,
                )
            }

            // Step 3: Background location
            if (uiState.locationBackgroundStatus != PermissionStatus.NotRequired) {
                item {
                    SetupStepCard(
                        stepNumber = 3,
                        title = "Allow location all the time",
                        description = "Open Settings → Permissions → Location → choose \"Allow all the time\". Required so we can locate the phone when the app is closed.",
                        state = uiState.locationBackgroundStatus.toStepState(),
                        actionLabel = "Open Settings",
                        onActionClick = onOpenBackgroundLocationSettings,
                        // Locked until foreground is granted (system rule).
                        isLocked = uiState.locationForegroundStatus != PermissionStatus.Granted,
                    )
                }
            }

            // Steps 4 & 5: MIUI specifics (Xiaomi)
            if (uiState.isMiui) {
                item {
                    SetupStepCard(
                        stepNumber = 4,
                        title = "Enable Autostart (MIUI)",
                        description = "On Xiaomi, MIUI kills background services unless Autostart is enabled. Open Security app → Permissions → Autostart → enable FindMyDevice. Tap \"I did it\" after enabling.",
                        state = if (uiState.miuiAutostartAcknowledged) StepState.Done else StepState.ActionNeeded,
                        actionLabel = "Open MIUI Autostart",
                        onActionClick = onOpenMiuiAutostart,
                        secondaryActionLabel = if (!uiState.miuiAutostartAcknowledged) "I did it" else null,
                        onSecondaryActionClick = onMarkAutostartDone,
                    )
                }
                item {
                    SetupStepCard(
                        stepNumber = 5,
                        title = "Disable battery restrictions (MIUI)",
                        description = "MIUI's per-app battery saver throttles background work. Set FindMyDevice to \"No restrictions\". Tap \"I did it\" when done.",
                        state = if (uiState.miuiBatterySettingsAcknowledged) StepState.Done else StepState.ActionNeeded,
                        actionLabel = "Open battery settings",
                        onActionClick = onOpenMiuiBatterySettings,
                        secondaryActionLabel = if (!uiState.miuiBatterySettingsAcknowledged) "I did it" else null,
                        onSecondaryActionClick = onMarkBatterySettingsDone,
                        isLocked = !uiState.miuiAutostartAcknowledged,
                    )
                }
            }

            if (uiState.trackingError != null) {
                item {
                    Text(
                        text = uiState.trackingError,
                        color = MaterialTheme.colorScheme.error,
                        style = MaterialTheme.typography.bodySmall,
                    )
                }
            }

            item {
                Spacer(modifier = Modifier.height(8.dp))
                if (uiState.isTrackingActive) {
                    OutlinedButton(
                        onClick = onStopTracking,
                        modifier = Modifier.fillMaxWidth(),
                    ) {
                        Text("Stop tracking")
                    }
                } else {
                    Button(
                        onClick = onStartTracking,
                        enabled = uiState.isReady,
                        modifier = Modifier.fillMaxWidth(),
                    ) {
                        Text(if (uiState.isReady) "Start tracking" else "Complete all steps to continue")
                    }
                }
            }
        }
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// Step card
// ──────────────────────────────────────────────────────────────────────────────

private enum class StepState { Done, ActionNeeded }

private fun PermissionStatus.toStepState(): StepState = when (this) {
    PermissionStatus.Granted, PermissionStatus.NotRequired -> StepState.Done
    PermissionStatus.NotGranted -> StepState.ActionNeeded
}

@Composable
private fun SetupStepCard(
    stepNumber: Int,
    title: String,
    description: String,
    state: StepState,
    actionLabel: String,
    onActionClick: () -> Unit,
    secondaryActionLabel: String? = null,
    onSecondaryActionClick: () -> Unit = {},
    isLocked: Boolean = false,
) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        elevation = CardDefaults.cardElevation(defaultElevation = 1.dp),
    ) {
        Column(modifier = Modifier.padding(16.dp)) {
            Row(verticalAlignment = Alignment.CenterVertically) {
                StepBadge(stepNumber = stepNumber, state = state, isLocked = isLocked)
                Spacer(modifier = Modifier.size(12.dp))
                Text(
                    text = title,
                    style = MaterialTheme.typography.titleMedium,
                    fontWeight = FontWeight.SemiBold,
                    modifier = Modifier.weight(1f),
                )
            }
            Spacer(modifier = Modifier.height(8.dp))
            Text(
                text = description,
                style = MaterialTheme.typography.bodySmall,
                color = MaterialTheme.colorScheme.onSurfaceVariant,
            )

            if (state == StepState.ActionNeeded) {
                Spacer(modifier = Modifier.height(12.dp))
                Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                    Button(
                        onClick = onActionClick,
                        enabled = !isLocked,
                    ) {
                        Text(actionLabel)
                    }
                    if (secondaryActionLabel != null) {
                        OutlinedButton(
                            onClick = onSecondaryActionClick,
                            enabled = !isLocked,
                        ) {
                            Text(secondaryActionLabel)
                        }
                    }
                }
            }
        }
    }
}

@Composable
private fun StepBadge(stepNumber: Int, state: StepState, isLocked: Boolean) {
    val colors = when {
        isLocked -> MaterialTheme.colorScheme.surfaceVariant to MaterialTheme.colorScheme.onSurfaceVariant
        state == StepState.Done -> Color(0xFF4CAF50) to Color.White
        else -> MaterialTheme.colorScheme.primary to MaterialTheme.colorScheme.onPrimary
    }
    val background = colors.first
    val content = colors.second
    Box(
        modifier = Modifier
            .size(32.dp)
            .clip(CircleShape)
            .background(background),
        contentAlignment = Alignment.Center,
    ) {
        when {
            isLocked -> Icon(
                Icons.Filled.Lock,
                contentDescription = null,
                tint = content,
                modifier = Modifier.size(16.dp),
            )
            state == StepState.Done -> Icon(
                Icons.Filled.Check,
                contentDescription = "Done",
                tint = content,
                modifier = Modifier.size(18.dp),
            )
            else -> Text(
                text = stepNumber.toString(),
                color = content,
                style = MaterialTheme.typography.titleSmall,
            )
        }
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// Preview
// ──────────────────────────────────────────────────────────────────────────────

@Preview(showBackground = true, heightDp = 1200)
@Composable
private fun TrackingSetupPreview() {
    FindMyDeviceTheme {
        TrackingSetupContent(
            uiState = TrackingSetupUiState(
                isMiui = true,
                notificationsStatus = PermissionStatus.NotGranted,
                locationForegroundStatus = PermissionStatus.Granted,
                locationBackgroundStatus = PermissionStatus.NotGranted,
                miuiAutostartAcknowledged = false,
                miuiBatterySettingsAcknowledged = false,
            ),
            onNavigateBack = {},
            onRequestNotifications = {},
            onRequestForegroundLocation = {},
            onOpenBackgroundLocationSettings = {},
            onOpenMiuiAutostart = {},
            onMarkAutostartDone = {},
            onOpenMiuiBatterySettings = {},
            onMarkBatterySettingsDone = {},
            onStartTracking = {},
            onStopTracking = {},
        )
    }
}

package com.findmydevice.presentation.devices

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.Logout
import androidx.compose.material.icons.filled.Add
import androidx.compose.material.icons.filled.Delete
import androidx.compose.material.icons.filled.PhoneAndroid
import androidx.compose.material.icons.filled.Refresh
import androidx.compose.material.icons.filled.Tune
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.Button
import androidx.compose.material3.Card
import androidx.compose.material3.CardDefaults
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.ExtendedFloatingActionButton
import androidx.compose.material3.FloatingActionButton
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import com.findmydevice.domain.model.Device
import com.findmydevice.domain.platform.DeviceInfo
import com.findmydevice.ui.theme.FindMyDeviceTheme

@Composable
fun DevicesScreen(
    onLoggedOut: () -> Unit,
    onNavigateToTrackingSetup: () -> Unit,
    viewModel: DevicesViewModel = hiltViewModel(),
) {
    val uiState by viewModel.uiState.collectAsStateWithLifecycle()

    LaunchedEffect(uiState.isLoggedOut) {
        if (uiState.isLoggedOut) {
            viewModel.onLogoutConsumed()
            onLoggedOut()
        }
    }

    DevicesContent(
        uiState = uiState,
        onRefresh = viewModel::loadDevices,
        onLogout = viewModel::onLogout,
        onOpenTrackingSetup = onNavigateToTrackingSetup,
        onShowRegisterDialog = viewModel::onShowRegisterDialog,
        onHideRegisterDialog = viewModel::onHideRegisterDialog,
        onRegisterNameChange = viewModel::onRegisterNameChange,
        onRegister = viewModel::onRegisterClicked,
        onDelete = viewModel::onDeleteDevice,
    )
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
private fun DevicesContent(
    uiState: DevicesUiState,
    onRefresh: () -> Unit,
    onLogout: () -> Unit,
    onOpenTrackingSetup: () -> Unit,
    onShowRegisterDialog: () -> Unit,
    onHideRegisterDialog: () -> Unit,
    onRegisterNameChange: (String) -> Unit,
    onRegister: () -> Unit,
    onDelete: (String) -> Unit,
) {
    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("My Devices") },
                actions = {
                    IconButton(onClick = onOpenTrackingSetup) {
                        Icon(Icons.Filled.Tune, contentDescription = "Tracking setup")
                    }
                    IconButton(onClick = onRefresh, enabled = !uiState.isLoading) {
                        Icon(Icons.Filled.Refresh, contentDescription = "Refresh")
                    }
                    IconButton(onClick = onLogout) {
                        Icon(Icons.AutoMirrored.Filled.Logout, contentDescription = "Logout")
                    }
                },
            )
        },
        floatingActionButton = {
            if (uiState.devices.isNotEmpty()) {
                FloatingActionButton(onClick = onShowRegisterDialog) {
                    Icon(Icons.Filled.Add, contentDescription = "Register device")
                }
            }
        },
    ) { padding ->
        Box(
            modifier = Modifier
                .fillMaxSize()
                .padding(padding),
        ) {
            when {
                uiState.isLoading && uiState.devices.isEmpty() -> {
                    CircularProgressIndicator(modifier = Modifier.align(Alignment.Center))
                }

                uiState.error != null && uiState.devices.isEmpty() -> {
                    ErrorState(
                        message = uiState.error,
                        onRetry = onRefresh,
                        modifier = Modifier.align(Alignment.Center),
                    )
                }

                uiState.devices.isEmpty() -> {
                    EmptyState(
                        currentDeviceInfo = uiState.currentDeviceInfo,
                        onRegister = onShowRegisterDialog,
                        modifier = Modifier.align(Alignment.Center),
                    )
                }

                else -> DeviceList(
                    devices = uiState.devices,
                    currentDeviceInfo = uiState.currentDeviceInfo,
                    onDelete = onDelete,
                )
            }
        }
    }

    if (uiState.isRegisterDialogVisible) {
        RegisterDeviceDialog(
            uiState = uiState,
            onNameChange = onRegisterNameChange,
            onConfirm = onRegister,
            onDismiss = onHideRegisterDialog,
        )
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// States
// ──────────────────────────────────────────────────────────────────────────────

@Composable
private fun EmptyState(
    currentDeviceInfo: DeviceInfo?,
    onRegister: () -> Unit,
    modifier: Modifier = Modifier,
) {
    Column(
        modifier = modifier.padding(24.dp),
        horizontalAlignment = Alignment.CenterHorizontally,
    ) {
        Icon(
            imageVector = Icons.Filled.PhoneAndroid,
            contentDescription = null,
            modifier = Modifier.size(64.dp),
            tint = MaterialTheme.colorScheme.primary,
        )
        Spacer(modifier = Modifier.height(16.dp))
        Text(
            text = "No devices yet",
            style = MaterialTheme.typography.headlineSmall,
        )
        Spacer(modifier = Modifier.height(8.dp))
        Text(
            text = currentDeviceInfo?.let { "Link ${it.suggestedName} to your account to start tracking it." }
                ?: "Register your first device to start tracking.",
            style = MaterialTheme.typography.bodyMedium,
            color = MaterialTheme.colorScheme.onSurfaceVariant,
        )
        Spacer(modifier = Modifier.height(24.dp))
        ExtendedFloatingActionButton(
            onClick = onRegister,
            icon = { Icon(Icons.Filled.Add, contentDescription = null) },
            text = { Text("Register this device") },
        )
    }
}

@Composable
private fun ErrorState(
    message: String,
    onRetry: () -> Unit,
    modifier: Modifier = Modifier,
) {
    Column(
        modifier = modifier.padding(24.dp),
        horizontalAlignment = Alignment.CenterHorizontally,
    ) {
        Text(
            text = message,
            color = MaterialTheme.colorScheme.error,
            style = MaterialTheme.typography.bodyLarge,
        )
        Spacer(modifier = Modifier.height(16.dp))
        Button(onClick = onRetry) { Text("Retry") }
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// List + Card
// ──────────────────────────────────────────────────────────────────────────────

@Composable
private fun DeviceList(
    devices: List<Device>,
    currentDeviceInfo: DeviceInfo?,
    onDelete: (String) -> Unit,
) {
    LazyColumn(
        modifier = Modifier.fillMaxSize(),
        contentPadding = androidx.compose.foundation.layout.PaddingValues(16.dp),
        verticalArrangement = Arrangement.spacedBy(8.dp),
    ) {
        items(devices, key = { it.id }) { device ->
            DeviceCard(
                device = device,
                isCurrentDevice = isCurrentDevice(device, currentDeviceInfo),
                onDelete = { onDelete(device.id) },
            )
        }
    }
}

private fun isCurrentDevice(device: Device, info: DeviceInfo?): Boolean {
    if (info == null) return false
    return device.manufacturer.equals(info.manufacturer, ignoreCase = true) &&
        device.model.equals(info.model, ignoreCase = true)
}

@Composable
private fun DeviceCard(
    device: Device,
    isCurrentDevice: Boolean,
    onDelete: () -> Unit,
) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        elevation = CardDefaults.cardElevation(defaultElevation = 1.dp),
    ) {
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(16.dp),
            verticalAlignment = Alignment.CenterVertically,
        ) {
            // Online indicator dot
            Box(
                modifier = Modifier
                    .size(10.dp)
                    .clip(CircleShape)
                    .background(if (device.isOnline) Color(0xFF4CAF50) else Color(0xFF9E9E9E)),
            )
            Spacer(modifier = Modifier.size(12.dp))

            Column(modifier = Modifier.weight(1f)) {
                Row(verticalAlignment = Alignment.CenterVertically) {
                    Text(
                        text = device.name,
                        style = MaterialTheme.typography.titleMedium,
                    )
                    if (isCurrentDevice) {
                        Spacer(modifier = Modifier.size(8.dp))
                        Surface(
                            color = MaterialTheme.colorScheme.primaryContainer,
                            shape = MaterialTheme.shapes.small,
                        ) {
                            Text(
                                text = "This device",
                                style = MaterialTheme.typography.labelSmall,
                                color = MaterialTheme.colorScheme.onPrimaryContainer,
                                modifier = Modifier.padding(horizontal = 6.dp, vertical = 2.dp),
                            )
                        }
                    }
                }

                val subtitle = buildString {
                    listOfNotNull(device.manufacturer, device.model).joinToString(" ")
                        .takeIf { it.isNotBlank() }
                        ?.let { append(it) }
                    if (!device.osVersion.isNullOrBlank()) {
                        if (isNotEmpty()) append(" · ")
                        append("Android ${device.osVersion}")
                    }
                }
                if (subtitle.isNotBlank()) {
                    Text(
                        text = subtitle,
                        style = MaterialTheme.typography.bodySmall,
                        color = MaterialTheme.colorScheme.onSurfaceVariant,
                    )
                }

                Text(
                    text = if (device.isOnline) "Online" else "Offline",
                    style = MaterialTheme.typography.labelSmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant,
                )
            }

            IconButton(onClick = onDelete) {
                Icon(
                    imageVector = Icons.Filled.Delete,
                    contentDescription = "Delete device",
                    tint = MaterialTheme.colorScheme.onSurfaceVariant,
                )
            }
        }
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// Register dialog
// ──────────────────────────────────────────────────────────────────────────────

@Composable
private fun RegisterDeviceDialog(
    uiState: DevicesUiState,
    onNameChange: (String) -> Unit,
    onConfirm: () -> Unit,
    onDismiss: () -> Unit,
) {
    AlertDialog(
        onDismissRequest = onDismiss,
        title = { Text("Register this device") },
        text = {
            Column {
                OutlinedTextField(
                    value = uiState.registerName,
                    onValueChange = onNameChange,
                    label = { Text("Display name") },
                    singleLine = true,
                    enabled = !uiState.isRegistering,
                    modifier = Modifier.fillMaxWidth(),
                )

                uiState.currentDeviceInfo?.let { info ->
                    Spacer(modifier = Modifier.height(12.dp))
                    Text(
                        text = "Manufacturer: ${info.manufacturer}",
                        style = MaterialTheme.typography.bodySmall,
                    )
                    Text(
                        text = "Model: ${info.model}",
                        style = MaterialTheme.typography.bodySmall,
                    )
                    Text(
                        text = "Android version: ${info.osVersion}",
                        style = MaterialTheme.typography.bodySmall,
                    )
                }

                if (uiState.registerError != null) {
                    Spacer(modifier = Modifier.height(8.dp))
                    Text(
                        text = uiState.registerError,
                        color = MaterialTheme.colorScheme.error,
                        style = MaterialTheme.typography.bodySmall,
                    )
                }
            }
        },
        confirmButton = {
            Button(
                onClick = onConfirm,
                enabled = !uiState.isRegistering,
            ) {
                if (uiState.isRegistering) {
                    CircularProgressIndicator(
                        modifier = Modifier.size(16.dp),
                        strokeWidth = 2.dp,
                        color = MaterialTheme.colorScheme.onPrimary,
                    )
                } else {
                    Text("Register")
                }
            }
        },
        dismissButton = {
            TextButton(onClick = onDismiss, enabled = !uiState.isRegistering) {
                Text("Cancel")
            }
        },
    )
}

// ──────────────────────────────────────────────────────────────────────────────
// Preview
// ──────────────────────────────────────────────────────────────────────────────

@Preview(showBackground = true)
@Composable
private fun DevicesScreenEmptyPreview() {
    FindMyDeviceTheme {
        DevicesContent(
            uiState = DevicesUiState(
                currentDeviceInfo = DeviceInfo(
                    manufacturer = "Xiaomi",
                    model = "Redmi Note 13",
                    osVersion = "14",
                    suggestedName = "Xiaomi Redmi Note 13",
                ),
            ),
            onRefresh = {},
            onLogout = {},
            onOpenTrackingSetup = {},
            onShowRegisterDialog = {},
            onHideRegisterDialog = {},
            onRegisterNameChange = {},
            onRegister = {},
            onDelete = {},
        )
    }
}

@Preview(showBackground = true)
@Composable
private fun DevicesScreenWithListPreview() {
    FindMyDeviceTheme {
        DevicesContent(
            uiState = DevicesUiState(
                devices = listOf(
                    Device(
                        id = "1",
                        name = "My Xiaomi",
                        manufacturer = "Xiaomi",
                        model = "Redmi Note 13",
                        platform = "Android",
                        osVersion = "14",
                        hasFcmToken = true,
                        isOnline = true,
                        createdAtUtc = "2026-05-11T00:00:00Z",
                        lastSeenAtUtc = "2026-05-11T01:00:00Z",
                    ),
                    Device(
                        id = "2",
                        name = "Old Phone",
                        manufacturer = "Samsung",
                        model = "Galaxy S10",
                        platform = "Android",
                        osVersion = "12",
                        hasFcmToken = false,
                        isOnline = false,
                        createdAtUtc = "2026-04-01T00:00:00Z",
                        lastSeenAtUtc = null,
                    ),
                ),
                currentDeviceInfo = DeviceInfo(
                    manufacturer = "Xiaomi",
                    model = "Redmi Note 13",
                    osVersion = "14",
                    suggestedName = "Xiaomi Redmi Note 13",
                ),
            ),
            onRefresh = {},
            onLogout = {},
            onOpenTrackingSetup = {},
            onShowRegisterDialog = {},
            onHideRegisterDialog = {},
            onRegisterNameChange = {},
            onRegister = {},
            onDelete = {},
        )
    }
}



package com.example.transactions.ui.newTransaction

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Check
import androidx.compose.material.icons.outlined.ArrowCircleRight
import androidx.compose.material.icons.outlined.AttachMoney
import androidx.compose.material.icons.outlined.ExpandMore
import androidx.compose.material.icons.outlined.Redeem
import androidx.compose.material.icons.outlined.Savings
import androidx.compose.material.icons.outlined.ShoppingCartCheckout
import androidx.compose.material3.Card
import androidx.compose.material3.DropdownMenu
import androidx.compose.material3.DropdownMenuItem
import androidx.compose.material3.ElevatedButton
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.FloatingActionButton
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.SegmentedButton
import androidx.compose.material3.SegmentedButtonDefaults
import androidx.compose.material3.SingleChoiceSegmentedButtonRow
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.saveable.rememberSaveable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.focus.FocusRequester
import androidx.compose.ui.focus.focusRequester
import androidx.compose.ui.focus.onFocusChanged
import androidx.compose.ui.text.input.ImeAction
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.style.TextDecoration
import androidx.compose.ui.unit.dp
import com.example.transactions.MoneyTransformation
import com.example.transactions.Utility
import java.time.LocalDateTime
import java.time.ZoneOffset


@Composable
fun NewTransaction(
    viewModel: NewTransactionViewModel,
    navigateHome: () -> Unit
) {
    val uiState = viewModel.uiState.collectAsState().value

    TransactionEditScreen(
        uiState.existingTransaction == null,
        viewModel,
        uiState,
        navigateHome
    )
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun TransactionEditScreen(
    isNew: Boolean,
    viewModel: NewTransactionViewModel,
    uiState: NewTransactionUiState,
    navigateBack: () -> Unit
) {
    val typeOptions = listOf(
        Icons.Outlined.Redeem,
        Icons.Outlined.AttachMoney,
        Icons.Outlined.Savings,
        Icons.Outlined.ShoppingCartCheckout
    )

    // These 4 are for turning the text fields red if opened then closed without inputting valid value
    val purchaseTouched = rememberSaveable { mutableStateOf(!isNew) }
    val purchaseOpened = rememberSaveable { mutableStateOf(!isNew) }
    val amountTouched = rememberSaveable { mutableStateOf(!isNew) }
    val amountOpened = rememberSaveable { mutableStateOf(!isNew) }

    // These 2 are for forcing capture on invalid TextFields
    val purchaseFocus = remember { FocusRequester() }
    val amountFocus = remember { FocusRequester() }

    // These 5 are for tracking the 5 inputs when using this entire NewTransaction menu


    // This tracks the cursor in the amount field to smoothly type currency values
//    var amountCursor = rememberSaveable { mutableStateOf(0) }
//    var amountField = rememberSaveable { mutableStateOf(TextFieldValue("")) }

    val purchaseModifier = Modifier
        .onFocusChanged {
            if (it.isFocused)
                purchaseOpened.value = true
            else if (purchaseOpened.value)
                purchaseTouched.value = true
        }
        .focusRequester(purchaseFocus)

    val amountModifier = Modifier
        .onFocusChanged {
            if (it.isFocused)
                amountOpened.value = true
            else if (amountOpened.value)
                amountTouched.value = true
        }
        .focusRequester(amountFocus)

    Card(
        modifier = Modifier
            .fillMaxSize()
            .padding(15.dp)
    ) {
        Box(
            modifier = Modifier
                .fillMaxSize()
        ) {
            Column(
                modifier = Modifier
                    .fillMaxSize()
                    .padding(0.dp, 0.dp, 0.dp, 20.dp),
                horizontalAlignment = Alignment.CenterHorizontally,
                verticalArrangement = Arrangement.SpaceEvenly
            ) {
                // Title text
                Text(
                    text = if (isNew) "New Transaction" else "Edit Transaction",
                    style = MaterialTheme.typography.titleLarge,
                    textDecoration = TextDecoration.Underline
                )

                Column {
                    if (uiState.modTimestamp != LocalDateTime.MIN) {
                        Text(
                            uiState.modTimestamp.toString() + "\n" + uiState.modTimestamp.toEpochSecond(ZoneOffset.UTC)
                        )
                    }

                    // Edit timestamp dialogue opener
                    TextButton(
                        onClick = {
                            viewModel.openTimestampDialogue()
                        }
                    ) {
                        Text(
                            text = "Edit time",
                            style = MaterialTheme.typography.bodyLarge
                        )
                        Icon(
                            Icons.Outlined.ArrowCircleRight,
                            "Edit time button"
                        )
                    }
                }

                // Segmented button to pick between 4 transaction types
                Column(
                    horizontalAlignment = Alignment.CenterHorizontally
                ) {
                    SingleChoiceSegmentedButtonRow {
                        typeOptions.forEachIndexed { index, icon ->
                            SegmentedButton(
                                shape = SegmentedButtonDefaults.itemShape(index = index, count = typeOptions.size),
                                onClick = { viewModel.changeTranType(index) },
                                /*icon = {
                                    SegmentedButtonDefaults.Icon(active = index == tranType) {
                                        Icon(
                                            imageVector = icon,
                                            contentDescription = "",
                                            tint = Color.Cyan,
                                            modifier = Modifier.size(SegmentedButtonDefaults.IconSize)
                                        )
                                    }
                                },*/
                                selected = index == uiState.tranType
                            ) {
                                Icon(
                                    imageVector = icon,
                                    contentDescription = "",
                                    //tint = Color.Cyan,
                                    modifier = Modifier.size(SegmentedButtonDefaults.IconSize)
                                )
                            }
                        }
                    }
                    Text(
                        modifier = Modifier
                            .padding(5.dp),
                        textAlign = TextAlign.Center,
                        text = when (uiState.tranType) {
                            0 -> "Add balance"
                            1 -> "Spend balance"
                            2 -> "Move to savings"
                            else -> "Spend savings"
                        }
                    )
                }
                Row(
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Text(
                        modifier = Modifier
                            .padding(10.dp),
                        text = "Category"
                    )
                    DropdownMenu(
                        expanded = uiState.categoryExpanded,
                        onDismissRequest = { viewModel.changeCategoryExpanded(false) }
                    ) {
                        Utility.TRANSACTION_CATEGORIES.forEachIndexed { index, category ->
                            DropdownMenuItem(
                                text = { Text(category) },
                                onClick = { viewModel.changeCategory(index) }
                            )
                        }
                    }
                    ElevatedButton(
                        onClick = { viewModel.changeCategoryExpanded(true) }
                    ) {
                        Icon(Icons.Outlined.ExpandMore, "dropdown")
                        Text(Utility.TRANSACTION_CATEGORIES[uiState.category])
                    }
                }
                /*Row(
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Text(
                        modifier = Modifier
                            .padding(5.dp),
                        text = "Move to savings"
                    )
                    Switch(
                        checked = toSavings,
                        onCheckedChange = {
                            toSavings = it
                        },
                        thumbContent = if (toSavings) {
                            {
                                Icon(
                                    imageVector = Icons.Filled.Check,
                                    contentDescription = null,
                                    modifier = Modifier.size(SwitchDefaults.IconSize),
                                )
                            }
                        } else {
                            null
                        }
                    )
                }*/
                // Purchase text input field
                OutlinedTextField(
                    modifier = purchaseModifier,
                    value = uiState.purchaseText,
                    label = { Text("Purchase") },
                    placeholder = { Text("Purchase") },
                    isError = purchaseTouched.value && !uiState.validPurchase,
                    onValueChange = { viewModel.changePurchaseText(it) },
                    keyboardOptions = KeyboardOptions(
                        imeAction = ImeAction.Next,
                    )
                )
                // Amount text input field
                OutlinedTextField(
                    modifier = amountModifier,
                    value = uiState.amountText,
                    label = { Text("Amount") },
                    placeholder = { Text("Amount") },
                    isError = amountTouched.value && !uiState.validAmount,
                    onValueChange = { viewModel.changeAmountText(it) },
                    visualTransformation = MoneyTransformation(),
                    keyboardOptions = KeyboardOptions(
                        imeAction = ImeAction.Done,
                        keyboardType = KeyboardType.Number
                    ),
                )
            }
            FloatingActionButton(
                modifier = Modifier
                    .align(Alignment.BottomEnd)
                    .padding(25.dp),
                onClick = {
                    if (!uiState.validPurchase) {
                        purchaseTouched.value = true
                        purchaseFocus.requestFocus()
                    }
                    else if (!uiState.validAmount) {
                        amountTouched.value = true
                        amountFocus.requestFocus()
                    }
                    else {
                        viewModel.saveTransaction()
                        navigateBack()
                    }
                }
            ) {
                Icon(
                    Icons.Filled.Check, ""
                )
            }
        }
    }
}
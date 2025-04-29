/**
 * Scripts for the investment transaction form
 */

document.addEventListener('DOMContentLoaded', function () {
    initializeTransactionForm();
});

/**
 * Initialize the transaction form with event listeners
 */
function initializeTransactionForm() {
    const typeSelect = document.getElementById('InvestmentType');
    const quantityInput = document.getElementById('Quantity');
    const priceInput = document.getElementById('Price');
    const taxesInput = document.getElementById('Taxes');
    const totalValueInput = document.getElementById('totalTransactionValue');

    if (typeSelect && quantityInput && priceInput && totalValueInput) {
        // Show/hide appropriate fields based on transaction type
        typeSelect.addEventListener('change', function () {
            updateFieldsVisibility(this.value);
        });

        // Calculate total value
        const calculateTotal = () => {
            const type = parseInt(typeSelect.value) || 0;
            const quantity = parseFloat(quantityInput.value) || 0;
            const price = parseFloat(priceInput.value) || 0;
            const taxes = parseFloat(taxesInput.value) || 0;

            let total = 0;

            // Calculate based on transaction type
            switch (type) {
                case 1: // Buy
                    total = (quantity * price) + taxes;
                    break;
                case 2: // Sell
                    total = (quantity * price) - taxes;
                    break;
                case 3: // Dividend
                case 6: // JCP
                case 7: // Yield
                    total = price; // Total amount received
                    break;
                default:
                    total = quantity * price;
            }

            totalValueInput.value = total.toLocaleString('pt-BR', {
                style: 'currency',
                currency: 'BRL'
            });
        };

        // Initialize fields visibility
        updateFieldsVisibility(typeSelect.value);

        // Initialize calculation
        calculateTotal();

        // Add event listeners
        typeSelect.addEventListener('change', calculateTotal);
        quantityInput.addEventListener('input', calculateTotal);
        priceInput.addEventListener('input', calculateTotal);
        taxesInput.addEventListener('input', calculateTotal);
    }

    // Form validation
    const form = document.querySelector('form');
    if (form) {
        form.addEventListener('submit', function (e) {
            if (!validateTransactionForm()) {
                e.preventDefault();
            }
        });
    }
}

/**
 * Update fields visibility based on transaction type
 */
function updateFieldsVisibility(type) {
    const quantityGroup = document.getElementById('Quantity').closest('.mb-3');
    const priceGroup = document.getElementById('Price').closest('.mb-3');
    const taxesGroup = document.getElementById('Taxes').closest('.mb-3');

    // Reset all fields to visible
    quantityGroup.style.display = '';
    priceGroup.style.display = '';
    taxesGroup.style.display = '';

    type = parseInt(type) || 0;

    // Adjust fields based on type
    switch (type) {
        case 3: // Dividend
        case 6: // JCP
        case 7: // Yield
            // For income types, we don't need quantity
            quantityGroup.style.display = 'none';
            document.getElementById('Price').placeholder = 'Valor total recebido';
            break;
        case 4: // Split
        case 5: // Bonus
            // For these types, we only need quantity
            taxesGroup.style.display = 'none';
            priceGroup.style.display = 'none';
            break;
        default:
            // For buy/sell, show all fields
            document.getElementById('Price').placeholder = 'Preço unitário';
    }
}

/**
 * Validate the transaction form
 */
function validateTransactionForm() {
    let isValid = true;

    // Validate transaction type
    const typeSelect = document.getElementById('InvestmentType');
    if (typeSelect && typeSelect.value === '') {
        showError(typeSelect, 'O tipo de transação é obrigatório');
        isValid = false;
    }

    // Transaction type specific validations
    const type = parseInt(typeSelect.value) || 0;

    // Validate date
    const dateInput = document.getElementById('Date');
    if (dateInput && dateInput.value === '') {
        showError(dateInput, 'A data é obrigatória');
        isValid = false;
    }

    // Validate quantity for relevant transaction types
    if (type !== 3 && type !== 6 && type !== 7) {
        const quantityInput = document.getElementById('Quantity');
        if (quantityInput) {
            const quantity = parseFloat(quantityInput.value);
            if (isNaN(quantity) || quantity <= 0) {
                showError(quantityInput, 'A quantidade deve ser maior que zero');
                isValid = false;
            }
        }
    }

    // Validate price for relevant transaction types
    if (type !== 4 && type !== 5) {
        const priceInput = document.getElementById('Price');
        if (priceInput) {
            const price = parseFloat(priceInput.value);
            if (isNaN(price) || price <= 0) {
                showError(priceInput, 'O preço deve ser maior que zero');
                isValid = false;
            }
        }
    }

    return isValid;
}

/**
 * Show error message for a field
 */
function showError(input, message) {
    // Find or create error message element
    let errorElement = input.nextElementSibling;
    if (!errorElement || !errorElement.classList.contains('text-danger')) {
        errorElement = document.createElement('div');
        errorElement.className = 'text-danger';
        input.parentNode.insertBefore(errorElement, input.nextSibling);
    }

    // Set error message
    errorElement.textContent = message;

    // Add error class to input
    input.classList.add('is-invalid');
}
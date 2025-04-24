/**
 * Scripts for the investment creation form
 */

document.addEventListener('DOMContentLoaded', function () {
    initializeInvestmentForm();
});

/**
 * Initialize the investment form with event listeners
 */
function initializeInvestmentForm() {
    const quantityInput = document.getElementById('InitialQuantity');
    const priceInput = document.getElementById('InitialPrice');
    const totalValueInput = document.getElementById('totalValue');

    if (quantityInput && priceInput && totalValueInput) {
        // Calculate total on input change
        const calculateTotal = () => {
            const quantity = parseFloat(quantityInput.value) || 0;
            const price = parseFloat(priceInput.value) || 0;
            const total = quantity * price;

            totalValueInput.value = total.toLocaleString('pt-BR', {
                style: 'currency',
                currency: 'BRL'
            });
        };

        // Initialize calculation
        calculateTotal();

        // Add event listeners
        quantityInput.addEventListener('input', calculateTotal);
        priceInput.addEventListener('input', calculateTotal);
    }

    // Form validation
    const form = document.querySelector('form');
    if (form) {
        form.addEventListener('submit', function (e) {
            if (!validateInvestmentForm()) {
                e.preventDefault();
            }
        });
    }
}

/**
 * Validate the investment form
 */
function validateInvestmentForm() {
    let isValid = true;

    // Validate symbol
    const symbolInput = document.getElementById('Symbol');
    if (symbolInput && symbolInput.value.trim() === '') {
        showError(symbolInput, 'O símbolo é obrigatório');
        isValid = false;
    }

    // Validate name
    const nameInput = document.getElementById('Name');
    if (nameInput && nameInput.value.trim() === '') {
        showError(nameInput, 'O nome é obrigatório');
        isValid = false;
    }

    // Validate type
    const typeInput = document.getElementById('InvestmentType');
    if (typeInput && typeInput.value === '') {
        showError(typeInput, 'O tipo de investimento é obrigatório');
        isValid = false;
    }

    // Validate quantity
    const quantityInput = document.getElementById('InitialQuantity');
    if (quantityInput) {
        const quantity = parseFloat(quantityInput.value);
        if (isNaN(quantity) || quantity <= 0) {
            showError(quantityInput, 'A quantidade deve ser maior que zero');
            isValid = false;
        }
    }

    // Validate price
    const priceInput = document.getElementById('InitialPrice');
    if (priceInput) {
        const price = parseFloat(priceInput.value);
        if (isNaN(price) || price <= 0) {
            showError(priceInput, 'O preço deve ser maior que zero');
            isValid = false;
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
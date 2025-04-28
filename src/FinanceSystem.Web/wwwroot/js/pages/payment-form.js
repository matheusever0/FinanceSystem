/**
 * Scripts para o formulário de pagamentos
 */

document.addEventListener('DOMContentLoaded', function () {
    initializePaymentForm();
});

/**
 * Inicializa formulário de pagamento
 */
function initializePaymentForm() {
    const form = document.querySelector('form[asp-action="Create"], form[asp-action="Edit"]');
    if (!form) return;

    // Inicializa campos comuns
    initializeMoneyMask('.money-input');
    initializeRecurringToggle(form);

    // Inicializa tipos de pagamento
    initializePaymentTypeSelect();

    // Inicializa métodos de pagamento
    initializePaymentMethodSelect();

    // Configura validação
    setupFormValidation(form, validatePaymentForm);
}

/**
 * Inicializa seletor de tipo de pagamento
 */
function initializePaymentTypeSelect() {
    const paymentTypeSelect = document.getElementById('PaymentTypeId');
    if (!paymentTypeSelect) return;

    paymentTypeSelect.addEventListener('change', function () {
        toggleFinancingSection(this.value);
    });

    // Inicializa com o valor atual
    if (paymentTypeSelect.value) {
        toggleFinancingSection(paymentTypeSelect.value);
    }
}

/**
 * Alterna a visibilidade da seção de financiamento
 * @param {string} typeId - ID do tipo de pagamento
 */
function toggleFinancingSection(typeId) {
    const financingSection = document.getElementById('financingSection');
    if (!financingSection) return;

    // Verifica se o tipo de pagamento é relacionado a financiamento
    const typeOption = document.querySelector(`#PaymentTypeId option[value="${typeId}"]`);
    const isFinancingType = typeOption && typeOption.getAttribute('data-is-financing-type') === 'true';

    // Exibe/oculta seção de financiamento
    if (isFinancingType) {
        financingSection.style.display = 'block';

        // Torna o campo obrigatório se for tipo de financiamento
        const financingSelect = document.getElementById('FinancingId');
        if (financingSelect) {
            financingSelect.required = true;
        }
    } else {
        financingSection.style.display = 'none';

        // Remove a obrigatoriedade se não for tipo de financiamento
        const financingSelect = document.getElementById('FinancingId');
        if (financingSelect) {
            financingSelect.required = false;
            financingSelect.value = '';
        }
    }
}

/**
 * Inicializa seletor de método de pagamento
 */
function initializePaymentMethodSelect() {
    const paymentMethodSelect = document.getElementById('PaymentMethodId');
    if (!paymentMethodSelect) return;

    paymentMethodSelect.addEventListener('change', function () {
        togglePaymentMethodFields(
            this.value,
            this.options[this.selectedIndex].getAttribute('data-type')
        );
    });

    // Inicializa com o valor atual
    if (paymentMethodSelect.value) {
        togglePaymentMethodFields(
            paymentMethodSelect.value,
            paymentMethodSelect.options[paymentMethodSelect.selectedIndex].getAttribute('data-type')
        );
    }
}

/**
 * Alterna a visibilidade dos campos conforme o método de pagamento
 * @param {string} methodId - ID do método de pagamento
 * @param {string} methodType - Tipo do método de pagamento
 */
function togglePaymentMethodFields(methodId, methodType) {
    const creditCardSection = document.getElementById('creditCardSection');
    const bankAccountSection = document.getElementById('bankAccountSection');

    // Esconde todos os campos específicos
    if (creditCardSection) creditCardSection.style.display = 'none';
    if (bankAccountSection) bankAccountSection.style.display = 'none';

    // Mostra campos específicos conforme o tipo
    if (methodType === '2' && creditCardSection) { // Cartão de crédito
        creditCardSection.style.display = 'block';

        // Torna o campo obrigatório
        const creditCardSelect = document.getElementById('CreditCardId');
        if (creditCardSelect) creditCardSelect.required = true;
    } else if (methodType === '4' && bankAccountSection) { // Transferência bancária
        bankAccountSection.style.display = 'block';
    }

    // Se não for cartão de crédito, limpa e remove obrigatoriedade
    if (methodType !== '2' && creditCardSection) {
        const creditCardSelect = document.getElementById('CreditCardId');
        if (creditCardSelect) {
            creditCardSelect.required = false;
            creditCardSelect.value = '';
        }
    }
}

/**
 * Validação do formulário de pagamento
 * @param {Event} event - Evento de submissão
 * @returns {boolean} - Resultado da validação
 */
function validatePaymentForm(event) {
    let isValid = true;
    const form = event.target;

    // Valida descrição
    const description = form.querySelector('#Description');
    if (description && description.value.trim() === '') {
        isValid = false;
        showFieldError(description, 'A descrição é obrigatória');
    }

    // Valida valor
    const amount = form.querySelector('#Amount');
    if (amount) {
        let rawValue = amount.value;
        if (typeof rawValue === 'string') {
            rawValue = rawValue.replace(/\./g, '').replace(',', '.');
        }

        const parsed = parseFloat(rawValue);

        if (isNaN(parsed) || parsed <= 0) {
            isValid = false;
            showFieldError(amount, 'Informe um valor válido');
        } else {
            // Atualiza o valor no campo antes de enviar
            amount.value = parsed.toString();
        }
    }

    // Valida data de vencimento
    const dueDate = form.querySelector('#DueDate');
    if (dueDate && dueDate.value === '') {
        isValid = false;
        showFieldError(dueDate, 'A data de vencimento é obrigatória');
    }

    // Valida tipo de pagamento
    const paymentTypeId = form.querySelector('#PaymentTypeId');
    if (paymentTypeId && paymentTypeId.value === '') {
        isValid = false;
        showFieldError(paymentTypeId, 'O tipo de pagamento é obrigatório');
    }

    // Valida método de pagamento
    const paymentMethodId = form.querySelector('#PaymentMethodId');
    if (paymentMethodId && paymentMethodId.value === '') {
        isValid = false;
        showFieldError(paymentMethodId, 'O método de pagamento é obrigatório');
    }

    // Valida cartão de crédito se necessário
    const paymentMethodSelect = form.querySelector('#PaymentMethodId');
    const creditCardSelect = form.querySelector('#CreditCardId');

    if (paymentMethodSelect && creditCardSelect) {
        const methodType = paymentMethodSelect.options[paymentMethodSelect.selectedIndex].getAttribute('data-type');

        if (methodType === '2' && creditCardSelect.value === '') {
            isValid = false;
            showFieldError(creditCardSelect, 'Selecione um cartão de crédito');
        }
    }

    // Valida financiamento se necessário
    const paymentTypeSelect = form.querySelector('#PaymentTypeId');
    const financingSelect = form.querySelector('#FinancingId');

    if (paymentTypeSelect && financingSelect) {
        const typeOption = document.querySelector(`#PaymentTypeId option[value="${paymentTypeSelect.value}"]`);
        const isFinancingType = typeOption && typeOption.getAttribute('data-is-financing-type') === 'true';

        if (isFinancingType && financingSelect.value === '') {
            isValid = false;
            showFieldError(financingSelect, 'Selecione um financiamento');
        }
    }

    return isValid;
}

/**
 * Mostra mensagem de erro para um campo
 * @param {HTMLElement} input - Campo com erro
 * @param {string} message - Mensagem de erro
 */
function showFieldError(input, message) {
    let errorElement = input.parentElement.querySelector('.text-danger');
    if (!errorElement) {
        errorElement = document.createElement('span');
        errorElement.classList.add('text-danger');
        input.parentElement.appendChild(errorElement);
    }
    errorElement.innerText = message;
    input.classList.add('is-invalid');
}
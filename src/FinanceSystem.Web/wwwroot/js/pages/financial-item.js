/**
 * Scripts para as páginas de pagamentos e receitas
 */

document.addEventListener('DOMContentLoaded', function () {
    // Determina o tipo de página atual
    const isPaymentPage = document.querySelector('[data-page="payment"]');
    const isIncomePage = document.querySelector('[data-page="income"]');

    // Inicializa formulários financeiros
    if (document.querySelector('form[asp-action="Create"], form[asp-action="Edit"]')) {
        initializeFinancialForm();
    }

    // Inicializa listas financeiras
    if (document.querySelector('.table-payments')) {
        initializePaymentList();
    }

    if (document.querySelector('.table-incomes')) {
        initializeIncomeList();
    }

    // Inicializa filtros
    const filterButtons = document.querySelectorAll('.payment-filter, .income-filter');
    if (filterButtons.length > 0) {
        initializeFinancialFilters();
    }

    // Inicializa ações em detalhes
    if (document.querySelector('.mark-received-installment') ||
        document.querySelector('.mark-installment-paid')) {
        initializeInstallmentActions();
    }
});

/**
 * Inicializa formulário financeiro (pagamento ou receita)
 */
function initializeFinancialForm() {
    const form = document.querySelector('form[asp-action="Create"], form[asp-action="Edit"]');
    if (!form) return;

    // Inicializa campos comuns
    initializeMoneyMask('.money');
    initializeRecurringToggle(form);

    // Configuração específica para pagamentos
    if (form.querySelector('#paymentMethodSelect')) {
        initializePaymentMethodSelect();
    }

    // Configura validação
    setupFormValidation(form, validateFinancialForm);
}

/**
 * Inicializa seletor de método de pagamento
 */
function initializePaymentMethodSelect() {
    const paymentMethodSelect = document.getElementById('paymentMethodSelect');
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
}

/**
 * Validação comum de formulários financeiros
 * @param {Event} event - Evento de submissão
 * @returns {boolean} - Resultado da validação
 */
function validateFinancialForm(event) {
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

    // Validações específicas para pagamentos
    if (form.querySelector('#paymentMethodSelect')) {
        isValid = validatePaymentSpecificFields(form) && isValid;
    }

    // Validações específicas para receitas
    if (form.querySelector('#IncomeTypeId')) {
        isValid = validateIncomeSpecificFields(form) && isValid;
    }

    return isValid;
}

/**
 * Validação de campos específicos de pagamentos
 * @param {HTMLElement} form - Formulário
 * @returns {boolean} - Resultado da validação
 */
function validatePaymentSpecificFields(form) {
    let isValid = true;

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
    const paymentMethodSelect = form.querySelector('#paymentMethodSelect');
    const creditCardSelect = form.querySelector('#CreditCardId');

    if (paymentMethodSelect && creditCardSelect) {
        const methodType = paymentMethodSelect.options[paymentMethodSelect.selectedIndex].getAttribute('data-type');

        if (methodType === '2' && creditCardSelect.value === '') {
            isValid = false;
            showFieldError(creditCardSelect, 'Selecione um cartão de crédito');
        }
    }

    return isValid;
}

/**
 * Validação de campos específicos de receitas
 * @param {HTMLElement} form - Formulário
 * @returns {boolean} - Resultado da validação
 */
function validateIncomeSpecificFields(form) {
    let isValid = true;

    // Valida tipo de receita
    const incomeTypeId = form.querySelector('#IncomeTypeId');
    if (incomeTypeId && incomeTypeId.value === '') {
        isValid = false;
        showFieldError(incomeTypeId, 'O tipo de receita é obrigatório');
    }

    return isValid;
}

/**
 * Inicializa lista de pagamentos
 */
function initializePaymentList() {
    const paymentTable = document.querySelector('.table-payments');
    if (!paymentTable) return;

    // Adiciona classes para estilização
    const rows = paymentTable.querySelectorAll('tbody tr');

    rows.forEach(row => {
        const statusCell = row.querySelector('.payment-status');
        if (!statusCell) return;

        const status = statusCell.textContent.trim().toLowerCase();

        if (status.includes('pago')) {
            row.classList.add('table-success');
        } else if (status.includes('vencido')) {
            row.classList.add('table-danger');
        } else if (status.includes('pendente')) {
            row.classList.add('table-warning');
        } else if (status.includes('cancelado')) {
            row.classList.add('table-secondary');
        }
    });

    // Inicializa botões de ação
    const deleteButtons = paymentTable.querySelectorAll('.btn-delete-payment');
    deleteButtons.forEach(button => {
        button.addEventListener('click', confirmDelete);
    });
}

/**
 * Inicializa lista de receitas
 */
function initializeIncomeList() {
    const incomeTable = document.querySelector('.table-incomes');
    if (!incomeTable) return;

    // Adiciona classes para estilização
    const rows = incomeTable.querySelectorAll('tbody tr');

    rows.forEach(row => {
        const statusCell = row.querySelector('.badge');
        if (!statusCell) return;

        const status = statusCell.textContent.trim().toLowerCase();

        if (status.includes('recebido')) {
            row.classList.add('table-success');
        } else if (status.includes('pendente')) {
            row.classList.add('table-warning');
        } else if (status.includes('cancelado')) {
            row.classList.add('table-secondary');
        }
    });

    // Inicializa botões de ação
    const deleteButtons = incomeTable.querySelectorAll('.btn-danger');
    deleteButtons.forEach(button => {
        button.addEventListener('click', confirmDelete);
    });
}

/**
 * Confirma antes de excluir
 * @param {Event} e - Evento de clique
 */
function confirmDelete(e) {
    if (!confirm('Tem certeza que deseja excluir este item?')) {
        e.preventDefault();
    }
}

/**
 * Inicializa filtros financeiros
 */
function initializeFinancialFilters() {
    const filterButtons = document.querySelectorAll('.payment-filter, .income-filter');

    filterButtons.forEach(button => {
        button.addEventListener('click', function () {
            // Remove classe ativa de todos os botões
            filterButtons.forEach(btn => btn.classList.remove('active'));

            // Adiciona classe ativa ao botão clicado
            this.classList.add('active');

            // Filtra a tabela
            const filterValue = this.getAttribute('data-filter');
            filterFinancialTable(filterValue);
        });
    });

    // Filtro de pesquisa
    const searchInput = document.getElementById('financial-search');
    if (searchInput) {
        searchInput.addEventListener('input', function () {
            filterTableByText(this.value);
        });
    }
}

/**
 * Filtra tabela financeira por status
 * @param {string} status - Status para filtro
 */
function filterFinancialTable(status) {
    const rows = document.querySelectorAll('.table-payments tbody tr, .table-incomes tbody tr');

    rows.forEach(row => {
        const statusCell = row.querySelector('.payment-status, .badge');
        if (!statusCell) return;

        const rowStatus = statusCell.textContent.trim().toLowerCase();

        if (status === 'all' || rowStatus.includes(status)) {
            row.style.display = '';
        } else {
            row.style.display = 'none';
        }
    });
}

/**
 * Filtra tabela por texto
 * @param {string} text - Texto para filtro
 */
function filterTableByText(text) {
    const rows = document.querySelectorAll('.table-payments tbody tr, .table-incomes tbody tr');
    text = text.toLowerCase();

    rows.forEach(row => {
        const content = row.textContent.toLowerCase();

        if (content.includes(text)) {
            row.style.display = '';
        } else {
            row.style.display = 'none';
        }
    });
}

/**
 * Inicializa ações em parcelas
 */
function initializeInstallmentActions() {
    // Para pagamentos
    const markPaidButtons = document.querySelectorAll('.mark-installment-paid');
    markPaidButtons.forEach(button => {
        button.addEventListener('click', function () {
            const installmentId = this.getAttribute('data-installment-id');
            const form = document.getElementById('markInstallmentPaidForm');
            if (form && installmentId) {
                form.action = `/PaymentInstallments/MarkAsPaid/${installmentId}`;
            }
        });
    });

    // Para receitas
    const markReceivedButtons = document.querySelectorAll('.mark-received-installment');
    markReceivedButtons.forEach(button => {
        button.addEventListener('click', function () {
            const installmentId = this.getAttribute('data-installment-id');
            document.getElementById('installmentId').value = installmentId;
        });
    });
}
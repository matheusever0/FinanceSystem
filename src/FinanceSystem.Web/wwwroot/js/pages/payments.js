/**
 * Finance System - Payments Page
 * Scripts específicos para a página de pagamentos
 */

// Namespace global para o sistema
var FinanceSystem = FinanceSystem || {};
FinanceSystem.Pages = FinanceSystem.Pages || {};

// Módulo Payments
FinanceSystem.Pages.Payments = (function () {
    /**
     * Inicializa a página de pagamentos
     */
    function initialize() {
        // Determina qual view está ativa
        const isFormView = document.querySelector('form[asp-action="Create"], form[asp-action="Edit"]');
        const isListView = document.querySelector('.table-payments');
        const isDetailsView = document.querySelector('.payment-details-container');

        if (isFormView) {
            initializePaymentForm();
        }

        if (isListView) {
            initializePaymentList();
        }

        if (isDetailsView) {
            initializePaymentDetails();
        }

        // Inicializa filtros se existirem
        const filterButtons = document.querySelectorAll('.payment-filter');
        if (filterButtons.length > 0) {
            initializePaymentFilters();
        }
    }

    /**
     * Inicializa formulário de pagamento
     */
    function initializePaymentForm() {
        const form = document.querySelector('form[asp-action="Create"], form[asp-action="Edit"]');
        if (!form) return;

        // Inicializa campos comuns usando módulos existentes
        if (FinanceSystem.Modules && FinanceSystem.Modules.Financial) {
            FinanceSystem.Modules.Financial.initializeMoneyMask('#Amount');
            FinanceSystem.Modules.Financial.initializeRecurringToggle(form);
        } else {
            // Fallback para inicialização direta
            initializeMoneyMaskFallback('#Amount');
            initializeRecurringToggleFallback(form);
        }

        // Inicializa seletor de tipo de pagamento
        initializePaymentTypeSelect();

        // Inicializa seletor de método de pagamento
        initializePaymentMethodSelect();

        // Configura validação do formulário
        setupFormValidation(form);
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
            const selectedOption = this.options[this.selectedIndex];
            const methodType = selectedOption ? selectedOption.getAttribute('data-type') : null;

            togglePaymentMethodFields(this.value, methodType);
        });

        // Inicializa com o valor atual
        if (paymentMethodSelect.value) {
            const selectedOption = paymentMethodSelect.options[paymentMethodSelect.selectedIndex];
            const methodType = selectedOption ? selectedOption.getAttribute('data-type') : null;

            togglePaymentMethodFields(paymentMethodSelect.value, methodType);
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

        // Esconde todas as seções específicas por padrão
        if (creditCardSection) creditCardSection.style.display = 'none';
        if (bankAccountSection) bankAccountSection.style.display = 'none';

        // Mostra seções específicas conforme o tipo do método
        if (methodType === '2' && creditCardSection) { // Cartão de crédito
            creditCardSection.style.display = 'block';

            // Torna o campo obrigatório
            const creditCardSelect = document.getElementById('CreditCardId');
            if (creditCardSelect) {
                creditCardSelect.required = true;
            }
        } else if (methodType === '4' && bankAccountSection) { // Transferência bancária
            bankAccountSection.style.display = 'block';
        }

        // Remove a obrigatoriedade se não for cartão de crédito
        if (methodType !== '2') {
            const creditCardSelect = document.getElementById('CreditCardId');
            if (creditCardSelect) {
                creditCardSelect.required = false;
                creditCardSelect.value = '';
            }
        }
    }

    /**
     * Inicializa máscara de moeda para um campo (fallback)
     * @param {string} selector - Seletor do campo
     */
    function initializeMoneyMaskFallback(selector) {
        const moneyInput = document.querySelector(selector);
        if (!moneyInput) return;

        if (typeof $.fn.mask !== 'undefined') {
            $(moneyInput).mask('#.##0,00', { reverse: true });
        } else {
            // Implementação manual se mask não estiver disponível
            moneyInput.addEventListener('input', function () {
                formatCurrencyInput(this);
            });
        }
    }

    /**
     * Formata campo de entrada monetária
     * @param {HTMLElement} input - Campo a ser formatado
     */
    function formatCurrencyInput(input) {
        // Preserva a posição do cursor
        const cursorPosition = input.selectionStart;
        const inputLength = input.value.length;

        // Remove caracteres não numéricos, exceto vírgula e ponto
        let value = input.value.replace(/[^\d.,]/g, '');

        // Converte para o formato brasileiro (vírgula como separador decimal)
        value = value.replace(/\D/g, '');
        if (value === '') {
            input.value = '';
            return;
        }

        value = (parseFloat(value) / 100).toFixed(2);
        input.value = value.replace('.', ',');

        // Ajusta a posição do cursor se necessário
        const newLength = input.value.length;
        const newPosition = cursorPosition + (newLength - inputLength);
        if (newPosition >= 0) {
            input.setSelectionRange(newPosition, newPosition);
        }
    }

    /**
     * Inicializa o toggle de recorrente (fallback)
     * @param {HTMLElement} form - Formulário
     */
    function initializeRecurringToggleFallback(form) {
        const isRecurringSwitch = form.querySelector('#isRecurringSwitch');
        const isRecurringLabel = form.querySelector('#isRecurringLabel');
        const installmentsInput = form.querySelector('#NumberOfInstallments');

        if (isRecurringSwitch && isRecurringLabel && installmentsInput) {
            isRecurringSwitch.addEventListener('change', function () {
                isRecurringLabel.textContent = this.checked ? 'Sim' : 'Não';
                if (this.checked) {
                    installmentsInput.value = '1';
                    installmentsInput.disabled = true;
                } else {
                    installmentsInput.disabled = false;
                }
            });

            // Inicializa com o estado atual
            if (isRecurringSwitch.checked) {
                isRecurringLabel.textContent = 'Sim';
                installmentsInput.value = '1';
                installmentsInput.disabled = true;
            }
        }
    }

    /**
     * Configura validação personalizada para o formulário
     * @param {HTMLFormElement} form - Formulário
     */
    function setupFormValidation(form) {
        if (!form) return;

        // Usa o módulo de validação se disponível
        if (FinanceSystem.Validation && FinanceSystem.Validation.setupFormValidation) {
            FinanceSystem.Validation.setupFormValidation(form, validatePaymentForm);
        } else {
            // Fallback para validação manual
            form.addEventListener('submit', function (event) {
                if (!validatePaymentForm(event)) {
                    event.preventDefault();
                    event.stopPropagation();
                }
            });
        }
    }

    /**
     * Valida o formulário de pagamento
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
            const selectedOption = paymentMethodSelect.options[paymentMethodSelect.selectedIndex];
            const methodType = selectedOption ? selectedOption.getAttribute('data-type') : null;

            if (methodType === '2' && creditCardSelect.value === '') {
                isValid = false;
                showFieldError(creditCardSelect, 'Selecione um cartão de crédito');
            }
        }

        // Valida financiamento se necessário
        const paymentTypeSelect = form.querySelector('#PaymentTypeId');
        const financingSelect = form.querySelector('#FinancingId');

        if (paymentTypeSelect && financingSelect) {
            const selectedOption = paymentTypeSelect.options[paymentTypeSelect.selectedIndex];
            const isFinancingType = selectedOption && selectedOption.getAttribute('data-is-financing-type') === 'true';

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
        // Usa o módulo de validação se disponível
        if (FinanceSystem.Validation && FinanceSystem.Validation.showFieldError) {
            FinanceSystem.Validation.showFieldError(input, message);
            return;
        }

        // Fallback para exibição manual de erro
        let errorElement = input.parentElement.querySelector('.text-danger');
        if (!errorElement) {
            errorElement = document.createElement('span');
            errorElement.classList.add('text-danger');
            input.parentElement.appendChild(errorElement);
        }
        errorElement.innerText = message;
        input.classList.add('is-invalid');
    }

    /**
     * Inicializa a lista de pagamentos
     */
    function initializePaymentList() {
        const paymentTable = document.querySelector('.table-payments');
        if (!paymentTable) return;

        // Adiciona classes para estilização baseada no status
        stylePaymentRows();

        // Inicializa DataTables se disponível
        initializePaymentDataTable();

        // Inicializa botões de ação
        initializePaymentActionButtons();
    }

    /**
     * Estiliza linhas de pagamento de acordo com status
     */
    function stylePaymentRows() {
        const rows = document.querySelectorAll('.table-payments tbody tr');

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
    }

    /**
     * Inicializa DataTables para a tabela de pagamentos
     */
    function initializePaymentDataTable() {
        // Verifica se DataTables está disponível
        if (typeof $.fn.DataTable !== 'undefined') {
            $('.table-payments').DataTable({
                language: {
                    url: '//cdn.datatables.net/plug-ins/1.10.25/i18n/Portuguese-Brasil.json'
                },
                responsive: true,
                pageLength: 10,
                lengthMenu: [[10, 25, 50, -1], [10, 25, 50, "Todos"]],
                order: [[1, 'desc']], // Ordena por data de vencimento decrescente
                columnDefs: [
                    { orderable: false, targets: -1 } // Desabilita ordenação na coluna de ações
                ]
            });
        } else if (FinanceSystem.Modules && FinanceSystem.Modules.Tables) {
            // Usa o módulo Tables se DataTables não estiver disponível
            //FinanceSystem.Modules.Tables.initializeTableSort();
        }
    }

    /**
     * Inicializa botões de ação para pagamentos
     */
    function initializePaymentActionButtons() {
        // Botões de exclusão
        const deleteButtons = document.querySelectorAll('.btn-delete-payment');
        deleteButtons.forEach(button => {
            button.addEventListener('click', function (e) {
                if (!confirm('Tem certeza que deseja excluir este pagamento? Esta ação não pode ser desfeita.')) {
                    e.preventDefault();
                }
            });
        });

        // Botões de pagamento
        const payButtons = document.querySelectorAll('.btn-pay-payment');
        payButtons.forEach(button => {
            button.addEventListener('click', function () {
                const paymentId = this.getAttribute('data-payment-id');
                openPaymentModal(paymentId);
            });
        });
    }

    /**
     * Abre modal para confirmação de pagamento
     * @param {string} paymentId - ID do pagamento
     */
    function openPaymentModal(paymentId) {
        const modal = document.getElementById('paymentModal');
        const paymentIdInput = document.getElementById('PaymentId');

        if (modal && paymentIdInput) {
            paymentIdInput.value = paymentId;

            // Usa o módulo UI se disponível
            if (FinanceSystem.UI && FinanceSystem.UI.showModal) {
                FinanceSystem.UI.showModal('paymentModal');
            } else if (typeof bootstrap !== 'undefined') {
                // Fallback para Bootstrap
                const modalInstance = new bootstrap.Modal(modal);
                modalInstance.show();
            } else {
                // Fallback básico
                modal.style.display = 'block';
            }
        }
    }

    /**
     * Inicializa detalhes de pagamento
     */
    function initializePaymentDetails() {
        // Inicializa ações para instalações
        initializeInstallmentActions();

        // Inicializa ações para anexos se existirem
        const attachmentsSection = document.querySelector('.payment-attachments');
        if (attachmentsSection) {
            initializeAttachments();
        }
    }

    /**
     * Inicializa ações em parcelas
     */
    function initializeInstallmentActions() {
        // Botões para marcar parcela como paga
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

        // Botões para cancelar parcela
        const cancelButtons = document.querySelectorAll('.cancel-installment');
        cancelButtons.forEach(button => {
            button.addEventListener('click', function (e) {
                if (!confirm('Tem certeza que deseja cancelar esta parcela?')) {
                    e.preventDefault();
                }
            });
        });
    }

    /**
     * Inicializa gerenciamento de anexos
     */
    function initializeAttachments() {
        const uploadButton = document.getElementById('upload-attachment');
        const fileInput = document.getElementById('attachment-file');

        if (uploadButton && fileInput) {
            uploadButton.addEventListener('click', function () {
                fileInput.click();
            });

            fileInput.addEventListener('change', function () {
                if (this.files.length > 0) {
                    // Encontra o formulário de upload
                    const uploadForm = document.getElementById('attachment-form');
                    if (uploadForm) {
                        uploadForm.submit();
                    }
                }
            });
        }

        // Botões para excluir anexos
        const deleteAttachmentButtons = document.querySelectorAll('.delete-attachment');
        deleteAttachmentButtons.forEach(button => {
            button.addEventListener('click', function (e) {
                if (!confirm('Tem certeza que deseja excluir este anexo?')) {
                    e.preventDefault();
                }
            });
        });
    }

    /**
     * Inicializa filtros de pagamento
     */
    function initializePaymentFilters() {
        // Usa o módulo Financial se disponível
        if (FinanceSystem.Modules && FinanceSystem.Modules.Financial) {
            FinanceSystem.Modules.Financial.initializeFinancialFilters();
            return;
        }

        // Fallback para implementação direta
        const filterButtons = document.querySelectorAll('.payment-filter');

        filterButtons.forEach(button => {
            button.addEventListener('click', function () {
                // Remove classe ativa de todos os botões
                filterButtons.forEach(btn => btn.classList.remove('active'));

                // Adiciona classe ativa ao botão clicado
                this.classList.add('active');

                // Filtra a tabela
                const filterValue = this.getAttribute('data-filter');
                filterPaymentTable(filterValue);
            });
        });

        // Filtro de pesquisa
        const searchInput = document.getElementById('payment-search');
        if (searchInput) {
            searchInput.addEventListener('input', function () {
                filterTableByText(this.value);
            });
        }
    }

    /**
     * Filtra tabela de pagamentos por status
     * @param {string} status - Status para filtro
     */
    function filterPaymentTable(status) {
        const rows = document.querySelectorAll('.table-payments tbody tr');

        rows.forEach(row => {
            const statusCell = row.querySelector('.payment-status');
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
        const rows = document.querySelectorAll('.table-payments tbody tr');
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

    function initializeCollapseFilter(){
        const collapseFilters = document.getElementById('collapseFilters');
        const filterBtn = document.querySelector('[data-bs-toggle="collapse"]');

        filterBtn.addEventListener('click', function () {
            const icon = this.querySelector('i');
            if (collapseFilters.classList.contains('show')) {
                icon.classList.remove('fa-chevron-down');
                icon.classList.add('fa-chevron-right');
            } else {
                icon.classList.remove('fa-chevron-right');
                icon.classList.add('fa-chevron-down');
            }
        });
    }

    // API pública do módulo
    return {
        initialize: initialize,
        initializePaymentForm: initializePaymentForm,
        initializePaymentList: initializePaymentList,
        initializePaymentDetails: initializePaymentDetails,
        initializePaymentFilters: initializePaymentFilters,
        initializeCollapseFilter: initializeCollapseFilter
    };
})();
/**
 * Finance System - Payments Page
 * Scripts específicos para a página de pagamentos
 */

var FinanceSystem = FinanceSystem || {};
FinanceSystem.Pages = FinanceSystem.Pages || {};

FinanceSystem.Pages.Payments = (function () {
    function initialize() {
        const isFormView = document.querySelector('form[data-page="payment"]');
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

        const filterButtons = document.querySelectorAll('.payment-filter');
        if (filterButtons.length > 0) {
            initializePaymentFilters();
        }
    }

    function initializePaymentForm() {
        const form = document.querySelector('form[data-page="payment"]');
        if (!form) return;

        if (FinanceSystem.Modules && FinanceSystem.Modules.Financial) {
            FinanceSystem.Modules.Financial.initializeMoneyMask('#Amount');
            FinanceSystem.Modules.Financial.initializeRecurringToggle(form);
        } else {
            initializeMoneyMaskFallback('#Amount');
            initializeRecurringToggleFallback(form);
        }

        initializePaymentTypeSelect();

        initializePaymentMethodSelect();

        initializeFinancingSelect();

        setupFormValidation(form);

        const financingSelect = document.getElementById('FinancingId');
        if (financingSelect && financingSelect.value) {
            const financingSection = document.getElementById('financingSection');
            if (financingSection) {
                financingSection.style.display = 'block';
            }

            const installmentSection = document.getElementById('financingInstallmentSection');
            if (installmentSection) {
                installmentSection.style.display = 'block';
            }
        }
    }

    function initializePaymentTypeSelect() {
        const paymentTypeSelect = document.getElementById('PaymentTypeId');
        if (!paymentTypeSelect) return;

        paymentTypeSelect.addEventListener('change', function () {
            toggleFinancingSection(this.value);
        });

        if (paymentTypeSelect.value) {
            toggleFinancingSection(paymentTypeSelect.value);
        }
    }

    function toggleFinancingSection(typeId) {
        const financingSection = document.getElementById('financingSection');
        const installmentSection = document.getElementById('financingInstallmentSection');

        if (!financingSection) return;

        const typeOption = document.querySelector(`#PaymentTypeId option[value="${typeId}"]`);
        const isFinancingType = typeOption && typeOption.getAttribute('data-is-financing-type') === 'true';

        if (isFinancingType) {
            financingSection.style.display = 'block';

            const financingSelect = document.getElementById('FinancingId');
            if (financingSelect) {
                financingSelect.required = true;

                if (financingSelect.value && installmentSection) {
                    installmentSection.style.display = 'block';
                } else if (installmentSection) {
                    installmentSection.style.display = 'none';
                }
            }
        } else {
            financingSection.style.display = 'none';
            if (installmentSection) {
                installmentSection.style.display = 'none';
            }

            const financingSelect = document.getElementById('FinancingId');
            if (financingSelect) {
                financingSelect.required = false;
                financingSelect.value = '';
            }

            const installmentSelect = document.getElementById('FinancingInstallmentId');
            if (installmentSelect) {
                installmentSelect.value = '';
            }
        }
    }

    function initializePaymentMethodSelect() {
        const paymentMethodSelect = document.getElementById('PaymentMethodId') ||
            document.getElementById('paymentMethodSelect');

        if (!paymentMethodSelect) {
            return;
        }

        paymentMethodSelect.addEventListener('change', function () {
            const selectedOption = this.options[this.selectedIndex];
            const methodType = selectedOption ? selectedOption.getAttribute('data-type') : null;

            togglePaymentMethodFields(this.value, methodType);
        });

        if (paymentMethodSelect.value) {
            const selectedOption = paymentMethodSelect.options[paymentMethodSelect.selectedIndex];
            const methodType = selectedOption ? selectedOption.getAttribute('data-type') : null;

            togglePaymentMethodFields(paymentMethodSelect.value, methodType);
        } else {
        }
    }

    function togglePaymentMethodFields(methodId, methodType) {

        const creditCardSection = document.getElementById('creditCardSection');
        const bankAccountSection = document.getElementById('bankAccountSection');

        if (creditCardSection) creditCardSection.style.display = 'none';
        if (bankAccountSection) bankAccountSection.style.display = 'none';

        const methodTypeStr = String(methodType);

        if (methodTypeStr === '2' && creditCardSection) { // Credit card
            creditCardSection.style.display = 'block';

            const creditCardSelect = document.getElementById('CreditCardId');
            if (creditCardSelect) {
                creditCardSelect.required = true;
            }
        } else if (methodTypeStr === '4' && bankAccountSection) { // Bank transfer
            bankAccountSection.style.display = 'block';
        }

        if (methodTypeStr !== '2') {
            const creditCardSelect = document.getElementById('CreditCardId');
            if (creditCardSelect) {
                creditCardSelect.required = false;
                creditCardSelect.value = '';
            }
        }
    }

    function initializeMoneyMaskFallback(selector) {
        const moneyInput = document.querySelector(selector);
        if (!moneyInput) return;

        if (typeof $.fn.mask !== 'undefined') {
            $(moneyInput).mask('#.##0,00', { reverse: true });
        } else {
            moneyInput.addEventListener('input', function () {
                formatCurrencyInput(this);
            });
        }
    }

    function formatCurrencyInput(input) {
        const cursorPosition = input.selectionStart;
        const inputLength = input.value.length;

        let value = input.value.replace(/[^\d.,]/g, '');

        value = value.replace(/\D/g, '');
        if (value === '') {
            input.value = '';
            return;
        }

        value = (parseFloat(value) / 100).toFixed(2);
        input.value = value.replace('.', ',');

        const newLength = input.value.length;
        const newPosition = cursorPosition + (newLength - inputLength);
        if (newPosition >= 0) {
            input.setSelectionRange(newPosition, newPosition);
        }
    }

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

            if (isRecurringSwitch.checked) {
                isRecurringLabel.textContent = 'Sim';
                installmentsInput.value = '1';
                installmentsInput.disabled = true;
            }
        }
    }

    function setupFormValidation(form) {
        if (!form) return;

        if (FinanceSystem.Validation && FinanceSystem.Validation.setupFormValidation) {
            FinanceSystem.Validation.setupFormValidation(form, validatePaymentForm);
        } else {
            form.addEventListener('submit', function (event) {
                if (!validatePaymentForm(event)) {
                    event.preventDefault();
                    event.stopPropagation();
                }
            });
        }
    }

    function validatePaymentForm(event) {
        let isValid = true;
        const form = event.target;

        const description = form.querySelector('#Description');
        if (description && description.value.trim() === '') {
            isValid = false;
            showFieldError(description, 'A descrição é obrigatória');
        }

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

        const dueDate = form.querySelector('#DueDate');
        if (dueDate && dueDate.value === '') {
            isValid = false;
            showFieldError(dueDate, 'A data de vencimento é obrigatória');
        }

        const paymentTypeId = form.querySelector('#PaymentTypeId');
        if (paymentTypeId && paymentTypeId.value === '') {
            isValid = false;
            showFieldError(paymentTypeId, 'O tipo de pagamento é obrigatório');
        }

        const paymentMethodId = form.querySelector('#PaymentMethodId');
        if (paymentMethodId && paymentMethodId.value === '') {
            isValid = false;
            showFieldError(paymentMethodId, 'O método de pagamento é obrigatório');
        }

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

    function showFieldError(input, message) {
        if (FinanceSystem.Validation && FinanceSystem.Validation.showFieldError) {
            FinanceSystem.Validation.showFieldError(input, message);
            return;
        }

        let errorElement = input.parentElement.querySelector('.text-danger');
        if (!errorElement) {
            errorElement = document.createElement('span');
            errorElement.classList.add('text-danger');
            input.parentElement.appendChild(errorElement);
        }
        errorElement.innerText = message;
        input.classList.add('is-invalid');
    }

    function initializePaymentList() {
        const paymentTable = document.querySelector('.table-payments');
        if (!paymentTable) return;

        stylePaymentRows();

        initializePaymentDataTable();

        initializePaymentActionButtons();
    }

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

    function initializePaymentDataTable() {
        if (typeof $.fn.DataTable !== 'undefined') {
            $('#table-payments').DataTable({
                language: {
                    url: '//cdn.datatables.net/plug-ins/1.10.25/i18n/Portuguese-Brasil.json'
                },
                responsive: true,
                pageLength: 10,
                lengthMenu: [[10, 25, 50, -1], [10, 25, 50, "Todos"]],
                order: [[1, 'desc']],
                columnDefs: [
                    { orderable: false, targets: -1 } 
                ]
            });
        } else if (FinanceSystem.Modules && FinanceSystem.Modules.Tables) {
            FinanceSystem.Modules.Tables.initializeTableSort();
        } else {
            basicTableSort();
        }
    }

    function basicTableSort() {
        const table = document.getElementById('investments-table');
        if (!table) return;

        const headers = table.querySelectorAll('th');

        headers.forEach((header, index) => {
            if (index !== headers.length - 1) { 
                header.style.cursor = 'pointer';
                header.addEventListener('click', () => {
                    sortTable(table, index);
                });
            }
        });
    }

    function initializePaymentActionButtons() {
        const deleteButtons = document.querySelectorAll('.btn-delete-payment');
        deleteButtons.forEach(button => {
            button.addEventListener('click', function (e) {
                if (!confirm('Tem certeza que deseja excluir este pagamento? Esta ação não pode ser desfeita.')) {
                    e.preventDefault();
                }
            });
        });

        const payButtons = document.querySelectorAll('.btn-pay-payment');
        payButtons.forEach(button => {
            button.addEventListener('click', function () {
                const paymentId = this.getAttribute('data-payment-id');
                openPaymentModal(paymentId);
            });
        });
    }

    function openPaymentModal(paymentId) {
        const modal = document.getElementById('paymentModal');
        const paymentIdInput = document.getElementById('PaymentId');

        if (modal && paymentIdInput) {
            paymentIdInput.value = paymentId;

            if (FinanceSystem.UI && FinanceSystem.UI.showModal) {
                FinanceSystem.UI.showModal('paymentModal');
            } else if (typeof bootstrap !== 'undefined') {
                const modalInstance = new bootstrap.Modal(modal);
                modalInstance.show();
            } else {
                modal.style.display = 'block';
            }
        }
    }

    function initializePaymentDetails() {
        initializeInstallmentActions();

        const attachmentsSection = document.querySelector('.payment-attachments');
        if (attachmentsSection) {
            initializeAttachments();
        }
    }

    function initializeInstallmentActions() {
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

        const cancelButtons = document.querySelectorAll('.cancel-installment');
        cancelButtons.forEach(button => {
            button.addEventListener('click', function (e) {
                if (!confirm('Tem certeza que deseja cancelar esta parcela?')) {
                    e.preventDefault();
                }
            });
        });
    }

    function initializeAttachments() {
        const uploadButton = document.getElementById('upload-attachment');
        const fileInput = document.getElementById('attachment-file');

        if (uploadButton && fileInput) {
            uploadButton.addEventListener('click', function () {
                fileInput.click();
            });

            fileInput.addEventListener('change', function () {
                if (this.files.length > 0) {
                    const uploadForm = document.getElementById('attachment-form');
                    if (uploadForm) {
                        uploadForm.submit();
                    }
                }
            });
        }

        const deleteAttachmentButtons = document.querySelectorAll('.delete-attachment');
        deleteAttachmentButtons.forEach(button => {
            button.addEventListener('click', function (e) {
                if (!confirm('Tem certeza que deseja excluir este anexo?')) {
                    e.preventDefault();
                }
            });
        });
    }

    function initializePaymentFilters() {
        if (FinanceSystem.Modules && FinanceSystem.Modules.Financial) {
            FinanceSystem.Modules.Financial.initializeFinancialFilters();
            return;
        }

        const filterButtons = document.querySelectorAll('.payment-filter');

        filterButtons.forEach(button => {
            button.addEventListener('click', function () {
                filterButtons.forEach(btn => btn.classList.remove('active'));

                this.classList.add('active');

                const filterValue = this.getAttribute('data-filter');
                filterPaymentTable(filterValue);
            });
        });

        const searchInput = document.getElementById('payment-search');
        if (searchInput) {
            searchInput.addEventListener('input', function () {
                filterTableByText(this.value);
            });
        }
    }

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

    function initializeFinancingSelect() {
        const financingSelect = document.getElementById('FinancingId');
        if (!financingSelect) return;

        financingSelect.addEventListener('change', function () {
            const financingId = this.value;
            if (financingId) {
                const installmentSection = document.getElementById('financingInstallmentSection');
                if (installmentSection) {
                    installmentSection.style.display = 'block';
                    loadFinancingInstallments(financingId);
                }
            } else {
                const installmentSection = document.getElementById('financingInstallmentSection');
                if (installmentSection) {
                    installmentSection.style.display = 'none';
                    const installmentSelect = document.getElementById('FinancingInstallmentId');
                    if (installmentSelect) {
                        installmentSelect.innerHTML = '<option value="">Selecione a parcela</option>';
                    }
                }
            }
        });
    }

    function initializeFinancingSelect() {
        const financingSelect = document.getElementById('FinancingId');
        if (!financingSelect) return;

        financingSelect.addEventListener('change', function () {
            const financingId = this.value;
            const installmentSection = document.getElementById('financingInstallmentSection');
            const installmentSelect = document.getElementById('FinancingInstallmentId');

            if (!installmentSection || !installmentSelect) return;

            installmentSelect.innerHTML = '<option value="">Selecione a parcela</option>';

            if (financingId) {
                installmentSection.style.display = 'block';

                $.ajax({
                    url: '/Financings/GetInstallmentsByPending',
                    type: 'GET',
                    data: { Id: financingId },
                    beforeSend: function () {
                        const loadingOption = document.createElement('option');
                        loadingOption.textContent = "Carregando parcelas...";
                        loadingOption.disabled = true;
                        loadingOption.id = "loading-option";
                        installmentSelect.appendChild(loadingOption);
                    },
                    success: function (data) {
                        const loadingOption = document.getElementById('loading-option');
                        if (loadingOption) {
                            installmentSelect.removeChild(loadingOption);
                        }

                        if (data && data.length > 0) {
                            data.forEach(function (installment) {
                                const option = document.createElement('option');
                                option.value = installment.id;
                                option.textContent = `Parcela ${installment.installmentNumber} - Venc: ${installment.dueDate} - ${formatCurrency(installment.totalAmount)}`;
                                option.setAttribute('data-due-date', installment.dueDate);
                                option.setAttribute('data-payment-date', installment.dueDate);
                                option.setAttribute('data-installment-number', installment.installmentNumber);
                                installmentSelect.appendChild(option);
                            });
                        } else {
                            const emptyOption = document.createElement('option');
                            emptyOption.textContent = "Nenhuma parcela pendente disponível";
                            emptyOption.disabled = true;
                            installmentSelect.appendChild(emptyOption);
                        }
                    },
                    error: function () {
                        const loadingOption = document.getElementById('loading-option');
                        if (loadingOption) {
                            installmentSelect.removeChild(loadingOption);
                        }

                        const errorOption = document.createElement('option');
                        errorOption.textContent = "Erro ao carregar parcelas";
                        errorOption.disabled = true;
                        installmentSelect.appendChild(errorOption);
                    }
                });
            } else {
                installmentSection.style.display = 'none';
            }

            installmentSelect.addEventListener('change', function () {
                const selectedOption = this.options[this.selectedIndex];

                if (!selectedOption || !selectedOption.value) return;

                const dueDateRaw = selectedOption.getAttribute('data-due-date'); 
                const installmentNumberRaw = selectedOption.getAttribute('data-installment-number'); 
                const dueDate = dueDateRaw ? dueDateRaw.split('T')[0] : '';
                const installmentNumber = installmentNumberRaw ? installmentNumberRaw.split('T')[0] : '';

                document.getElementById('DueDate').value = dueDate;
                document.getElementById('PaymentDate').value = dueDate;
                document.getElementById('Notes').value = 'Pagamento de Parcela: ' + installmentNumber;
            });
        });
    }

    function formatCurrency(value) {
        return new Intl.NumberFormat('pt-BR', {
            style: 'currency',
            currency: 'BRL'
        }).format(value);
    }

    return {
        initialize: initialize,
        initializePaymentForm: initializePaymentForm,
        initializePaymentList: initializePaymentList,
        initializePaymentDetails: initializePaymentDetails,
        initializePaymentFilters: initializePaymentFilters,
        initializeCollapseFilter: initializeCollapseFilter
    };
})();
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

        FinanceSystem.Modules.Financial.initializeMoneyMask('#Amount');
        FinanceSystem.Modules.Financial.initializeRecurringToggle(form);

        initializePaymentTypeSelect();
        initializePaymentMethodSelect();
        initializeFinancingSelect();

        FinanceSystem.Validation.setupFormValidation(form, validatePaymentForm);

        const financingSelect = document.getElementById('FinancingId');
        const installmentSelect = document.getElementById('FinancingInstallmentId');

        if (installmentSelect) {
            installmentSelect.disabled = true;
        }

        if (financingSelect && financingSelect.value) {
            const financingSection = document.getElementById('financingSection');
            if (financingSection) {
                financingSection.style.display = 'block';
            }

            const installmentSection = document.getElementById('financingInstallmentSection');
            if (installmentSection) {
                installmentSection.style.display = 'block';
            }

            const event = new Event('change', { bubbles: true });
            financingSelect.dispatchEvent(event);
        }
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
            installmentSelect.disabled = true;

            if (financingId) {
                installmentSection.style.display = 'block';

                $.ajax({
                    url: '/Financings/GetInstallmentsByPending',
                    type: 'GET',
                    data: { id: financingId },
                    beforeSend: function () {
                        const loadingOption = document.createElement('option');
                        loadingOption.textContent = "Carregando parcelas...";
                        loadingOption.disabled = true;
                        loadingOption.id = "loading-option";
                        installmentSelect.appendChild(loadingOption);
                        installmentSelect.disabled = true;
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

                            installmentSelect.disabled = false;
                            console.log('Parcelas carregadas, campo habilitado');
                        } else {
                            const emptyOption = document.createElement('option');
                            emptyOption.textContent = "Nenhuma parcela pendente disponível";
                            emptyOption.disabled = true;
                            installmentSelect.appendChild(emptyOption);

                            installmentSelect.disabled = true;
                            console.log('Nenhuma parcela pendente encontrada');
                        }
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        console.error('Erro ao carregar parcelas:', textStatus, errorThrown);

                        const loadingOption = document.getElementById('loading-option');
                        if (loadingOption) {
                            installmentSelect.removeChild(loadingOption);
                        }

                        const errorOption = document.createElement('option');
                        errorOption.textContent = "Erro ao carregar parcelas";
                        errorOption.disabled = true;
                        installmentSelect.appendChild(errorOption);

                        installmentSelect.disabled = true;
                    }
                });
            } else {
                installmentSection.style.display = 'none';
                installmentSelect.disabled = true;
            }
        });

        const installmentSelect = document.getElementById('FinancingInstallmentId');
        if (installmentSelect) {
            installmentSelect.addEventListener('change', function () {
                const selectedOption = this.options[this.selectedIndex];

                if (!selectedOption || !selectedOption.value) return;

                const dueDateRaw = selectedOption.getAttribute('data-due-date');
                const installmentNumberRaw = selectedOption.getAttribute('data-installment-number');

                let dueDate = dueDateRaw;
                if (dueDateRaw && dueDateRaw.includes('/')) {
                    const dateParts = dueDateRaw.split('/');
                    if (dateParts.length === 3) {
                        dueDate = `${dateParts[2]}-${dateParts[1]}-${dateParts[0]}`;
                    }
                } else if (dueDateRaw && dueDateRaw.includes('T')) {
                    dueDate = dueDateRaw.split('T')[0];
                }

                const installmentNumber = installmentNumberRaw || '';

                const dueDateField = document.getElementById('DueDate');
                const paymentDateField = document.getElementById('PaymentDate');
                const notesField = document.getElementById('Notes');

                if (dueDateField) dueDateField.value = dueDate;
                if (paymentDateField) paymentDateField.value = dueDate;
                if (notesField) notesField.value = 'Pagamento de Parcela: ' + installmentNumber;
            });
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

        FinanceSystem.UI.toggleVisibility(financingSection, isFinancingType);

        const financingSelect = document.getElementById('FinancingId');
        if (financingSelect) {
            financingSelect.required = isFinancingType;

            if (isFinancingType && financingSelect.value && installmentSection) {
                FinanceSystem.UI.toggleVisibility(installmentSection, true);
            } else if (installmentSection) {
                FinanceSystem.UI.toggleVisibility(installmentSection, false);
            }

            if (!isFinancingType) {
                financingSelect.value = '';

                const installmentSelect = document.getElementById('FinancingInstallmentId');
                if (installmentSelect) {
                    installmentSelect.value = '';
                }
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
        }
    }

    function togglePaymentMethodFields(methodId, methodType) {
        const creditCardSection = document.getElementById('creditCardSection');
        const bankAccountSection = document.getElementById('bankAccountSection');

        FinanceSystem.UI.toggleVisibility(creditCardSection, false);
        FinanceSystem.UI.toggleVisibility(bankAccountSection, false);

        const methodTypeStr = String(methodType);

        if (methodTypeStr === '2' && creditCardSection) {
            FinanceSystem.UI.toggleVisibility(creditCardSection, true);

            const creditCardSelect = document.getElementById('CreditCardId');
            if (creditCardSelect) {
                creditCardSelect.required = true;
            }
        } else if (methodTypeStr === '4' && bankAccountSection) {
            FinanceSystem.UI.toggleVisibility(bankAccountSection, true);
        }

        if (methodTypeStr !== '2') {
            const creditCardSelect = document.getElementById('CreditCardId');
            if (creditCardSelect) {
                creditCardSelect.required = false;
                creditCardSelect.value = '';
            }
        }
    }

    function validatePaymentForm(event) {
        let isValid = true;
        const form = event.target;

        const description = form.querySelector('#Description');
        if (description && description.value.trim() === '') {
            isValid = false;
            FinanceSystem.Validation.showFieldError(description, 'A descrição é obrigatória');
        }

        const amount = form.querySelector('#Amount');
        if (amount) {
            const parsed = FinanceSystem.Core.parseCurrency(amount.value);

            if (isNaN(parsed) || parsed <= 0) {
                isValid = false;
                FinanceSystem.Validation.showFieldError(amount, 'Informe um valor válido');
            }
        }

        const dueDate = form.querySelector('#DueDate');
        if (dueDate && dueDate.value === '') {
            isValid = false;
            FinanceSystem.Validation.showFieldError(dueDate, 'A data de vencimento é obrigatória');
        }

        const paymentTypeId = form.querySelector('#PaymentTypeId');
        if (paymentTypeId && paymentTypeId.value === '') {
            isValid = false;
            FinanceSystem.Validation.showFieldError(paymentTypeId, 'O tipo de pagamento é obrigatório');
        }

        const paymentMethodId = form.querySelector('#PaymentMethodId');
        if (paymentMethodId && paymentMethodId.value === '') {
            isValid = false;
            FinanceSystem.Validation.showFieldError(paymentMethodId, 'O método de pagamento é obrigatório');
        }

        const paymentMethodSelect = form.querySelector('#PaymentMethodId');
        const creditCardSelect = form.querySelector('#CreditCardId');

        if (paymentMethodSelect && creditCardSelect) {
            const selectedOption = paymentMethodSelect.options[paymentMethodSelect.selectedIndex];
            const methodType = selectedOption ? selectedOption.getAttribute('data-type') : null;

            if (methodType === '2' && creditCardSelect.value === '') {
                isValid = false;
                FinanceSystem.Validation.showFieldError(creditCardSelect, 'Selecione um cartão de crédito');
            }
        }

        const paymentTypeSelect = form.querySelector('#PaymentTypeId');
        const financingSelect = form.querySelector('#FinancingId');

        if (paymentTypeSelect && financingSelect) {
            const selectedOption = paymentTypeSelect.options[paymentTypeSelect.selectedIndex];
            const isFinancingType = selectedOption && selectedOption.getAttribute('data-is-financing-type') === 'true';

            if (isFinancingType && financingSelect.value === '') {
                isValid = false;
                FinanceSystem.Validation.showFieldError(financingSelect, 'Selecione um financiamento');
            }
        }

        return isValid;
    }

    function initializePaymentList() {
        FinanceSystem.Modules.Tables.highlightTableRows('.table-payments', {
            'pago': 'table-success',
            'vencido': 'table-danger',
            'pendente': 'table-warning',
            'cancelado': 'table-secondary'
        });

        FinanceSystem.Modules.Tables.initializeTable('#table-payments', {
            order: [[1, 'desc']]
        });

        initializePaymentActionButtons();
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
        const paymentIdInput = document.getElementById('PaymentId');
        if (paymentIdInput) {
            paymentIdInput.value = paymentId;
            FinanceSystem.UI.showModal('paymentModal');
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
        FinanceSystem.Modules.Financial.initializeFinancialFilters();
    }

    function initializeCollapseFilter() {
        const collapseFilters = document.getElementById('collapseFilters');
        const filterBtn = document.querySelector('[data-bs-toggle="collapse"]');

        if (!filterBtn || !collapseFilters) return;

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
            const installmentSection = document.getElementById('financingInstallmentSection');
            const installmentSelect = document.getElementById('FinancingInstallmentId');

            if (!installmentSection || !installmentSelect) return;

            // Limpa as opções anteriores e desabilita o select
            installmentSelect.innerHTML = '<option value="">Selecione a parcela</option>';
            installmentSelect.disabled = true; // Desabilita durante o carregamento

            if (financingId) {
                // Mostra a seção de parcelas
                installmentSection.style.display = 'block';

                // Fazer requisição AJAX para buscar as parcelas pendentes
                $.ajax({
                    url: '/Financings/GetInstallmentsByPending',
                    type: 'GET',
                    data: { id: financingId }, // Corrigido: usando 'id' minúsculo
                    beforeSend: function () {
                        const loadingOption = document.createElement('option');
                        loadingOption.textContent = "Carregando parcelas...";
                        loadingOption.disabled = true;
                        loadingOption.id = "loading-option";
                        installmentSelect.appendChild(loadingOption);
                        installmentSelect.disabled = true; // Mantém desabilitado durante carregamento
                    },
                    success: function (data) {
                        // Remove a opção de carregamento
                        const loadingOption = document.getElementById('loading-option');
                        if (loadingOption) {
                            installmentSelect.removeChild(loadingOption);
                        }

                        if (data && data.length > 0) {
                            // Adiciona as parcelas encontradas
                            data.forEach(function (installment) {
                                const option = document.createElement('option');
                                option.value = installment.id;
                                option.textContent = `Parcela ${installment.installmentNumber} - Venc: ${installment.dueDate} - ${formatCurrency(installment.totalAmount)}`;
                                option.setAttribute('data-due-date', installment.dueDate);
                                option.setAttribute('data-payment-date', installment.dueDate);
                                option.setAttribute('data-installment-number', installment.installmentNumber);
                                installmentSelect.appendChild(option);
                            });

                            // HABILITA o select após carregar as parcelas
                            installmentSelect.disabled = false;
                        } else {
                            // Nenhuma parcela pendente
                            const emptyOption = document.createElement('option');
                            emptyOption.textContent = "Nenhuma parcela pendente disponível";
                            emptyOption.disabled = true;
                            installmentSelect.appendChild(emptyOption);

                            // Mantém desabilitado se não há parcelas
                            installmentSelect.disabled = true;
                        }
                    },
                    error: function () {
                        // Remove a opção de carregamento em caso de erro
                        const loadingOption = document.getElementById('loading-option');
                        if (loadingOption) {
                            installmentSelect.removeChild(loadingOption);
                        }

                        const errorOption = document.createElement('option');
                        errorOption.textContent = "Erro ao carregar parcelas";
                        errorOption.disabled = true;
                        installmentSelect.appendChild(errorOption);

                        // Mantém desabilitado em caso de erro
                        installmentSelect.disabled = true;
                    }
                });
            } else {
                // Esconde a seção de parcelas se nenhum financiamento for selecionado
                installmentSection.style.display = 'none';
                installmentSelect.disabled = true;
            }
        });

        // Adiciona listener para quando uma parcela é selecionada
        const installmentSelect = document.getElementById('FinancingInstallmentId');
        if (installmentSelect) {
            installmentSelect.addEventListener('change', function () {
                const selectedOption = this.options[this.selectedIndex];

                if (!selectedOption || !selectedOption.value) return;

                // Pega os dados da parcela selecionada
                const dueDateRaw = selectedOption.getAttribute('data-due-date');
                const installmentNumberRaw = selectedOption.getAttribute('data-installment-number');
                const dueDate = dueDateRaw ? dueDateRaw.split('T')[0] : '';
                const installmentNumber = installmentNumberRaw || '';

                // Preenche automaticamente os campos de data e observações
                const dueDateField = document.getElementById('DueDate');
                const paymentDateField = document.getElementById('PaymentDate');
                const notesField = document.getElementById('Notes');

                if (dueDateField) dueDateField.value = dueDate;
                if (paymentDateField) paymentDateField.value = dueDate;
                if (notesField) notesField.value = 'Pagamento de Parcela: ' + installmentNumber;
            });
        }
    }

    function formatCurrency(value) {
        return FinanceSystem.Core.formatCurrency(value);
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
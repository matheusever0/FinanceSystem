var FinanceSystem = FinanceSystem || {};
FinanceSystem.Invoice = (function () {

    function initialize() {
        initializeInvoiceActions();
        initializePaymentModal();
        initializeInvoiceRefresh();
        initializeQuickFilters();
    }

    function initializeInvoiceActions() {
        // Auto-refresh de dados da fatura a cada 30 segundos
        const currentInvoicePage = document.querySelector('[data-page="current-invoice"]');
        if (currentInvoicePage) {
            const creditCardId = currentInvoicePage.getAttribute('data-card-id');
            if (creditCardId) {
                setInterval(() => refreshInvoiceData(creditCardId), 30000);
            }
        }

        // Validação de valores de pagamento
        const amountInputs = document.querySelectorAll('input[name="Amount"]');
        amountInputs.forEach(input => {
            input.addEventListener('change', validatePaymentAmount);
        });
    }

    function initializePaymentModal() {
        const paymentModals = document.querySelectorAll('.payment-modal');
        paymentModals.forEach(modal => {
            modal.addEventListener('show.bs.modal', function (event) {
                const button = event.relatedTarget;
                const amount = button.getAttribute('data-amount');
                const cardName = button.getAttribute('data-card-name');

                if (amount) {
                    const amountInput = modal.querySelector('input[name="Amount"]');
                    if (amountInput) amountInput.value = amount;
                }

                if (cardName) {
                    const cardNameDisplay = modal.querySelector('.card-name-display');
                    if (cardNameDisplay) cardNameDisplay.textContent = cardName;
                }
            });
        });

        // Checkbox para pagamento total
        const payFullCheckboxes = document.querySelectorAll('input[name="PayFullAmount"]');
        payFullCheckboxes.forEach(checkbox => {
            checkbox.addEventListener('change', function () {
                const amountInput = this.closest('form').querySelector('input[name="Amount"]');
                const fullAmount = parseFloat(amountInput.getAttribute('data-full-amount') || amountInput.value);

                if (this.checked) {
                    amountInput.value = fullAmount;
                    amountInput.setAttribute('readonly', true);
                } else {
                    amountInput.removeAttribute('readonly');
                }
            });
        });
    }

    function initializeInvoiceRefresh() {
        const refreshButtons = document.querySelectorAll('.refresh-invoice-btn');
        refreshButtons.forEach(button => {
            button.addEventListener('click', function () {
                const creditCardId = this.getAttribute('data-card-id');
                refreshInvoiceData(creditCardId);
            });
        });
    }

    function initializeQuickFilters() {
        const filterButtons = document.querySelectorAll('.quick-filter-btn');
        filterButtons.forEach(button => {
            button.addEventListener('click', function () {
                const filterType = this.getAttribute('data-filter');
                applyQuickFilter(filterType);
            });
        });
    }

    function refreshInvoiceData(creditCardId) {
        if (!creditCardId) return;

        const refreshButton = document.querySelector('.refresh-invoice-btn');
        if (refreshButton) {
            refreshButton.disabled = true;
            refreshButton.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Atualizando...';
        }

        fetch(`/CreditCards/GetInvoiceData?id=${creditCardId}`)
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    updateInvoiceDisplay(data.data);
                    showToast('Dados atualizados com sucesso!', 'success');
                } else {
                    showToast('Erro ao atualizar dados', 'error');
                }
            })
            .catch(error => {
                console.error('Erro ao buscar dados da fatura:', error);
                showToast('Erro ao conectar com o servidor', 'error');
            })
            .finally(() => {
                if (refreshButton) {
                    refreshButton.disabled = false;
                    refreshButton.innerHTML = '<i class="fas fa-sync-alt"></i> Atualizar';
                }
            });
    }

    function updateInvoiceDisplay(invoiceData) {
        // Atualizar valores exibidos
        const totalAmountElement = document.querySelector('.total-amount-display');
        if (totalAmountElement) {
            totalAmountElement.textContent = formatCurrency(invoiceData.totalAmount);
        }

        const remainingAmountElement = document.querySelector('.remaining-amount-display');
        if (remainingAmountElement) {
            remainingAmountElement.textContent = formatCurrency(invoiceData.remainingAmount);
        }

        // Atualizar barra de progresso do uso
        const usageProgressBar = document.querySelector('.usage-progress-bar');
        if (usageProgressBar) {
            const percentage = invoiceData.usagePercentage;
            usageProgressBar.style.width = percentage + '%';
            usageProgressBar.className = `progress-bar ${getUsageColorClass(percentage)}`;
        }

        // Atualizar status
        updateInvoiceStatus(invoiceData);
    }

    function updateInvoiceStatus(invoiceData) {
        const statusAlert = document.querySelector('.invoice-status-alert');
        if (!statusAlert) return;

        statusAlert.className = 'alert invoice-status-alert';

        if (invoiceData.isPaid) {
            statusAlert.classList.add('alert-success');
            statusAlert.innerHTML = '<i class="fas fa-check-circle me-2"></i><strong>Fatura Paga!</strong> Quitada integralmente.';
        } else if (invoiceData.isOverdue) {
            statusAlert.classList.add('alert-danger');
            statusAlert.innerHTML = `<i class="fas fa-exclamation-triangle me-2"></i><strong>Fatura Vencida!</strong> Vencimento em ${invoiceData.dueDate}`;
        } else {
            statusAlert.classList.add('alert-warning');
            statusAlert.innerHTML = `<i class="fas fa-clock me-2"></i><strong>Fatura Pendente.</strong> Vence em ${invoiceData.dueDate}`;
        }
    }

    function validatePaymentAmount(event) {
        const input = event.target;
        const value = parseFloat(input.value);
        const minValue = parseFloat(input.getAttribute('min') || 0);
        const maxValue = parseFloat(input.getAttribute('max') || Number.MAX_VALUE);

        if (value < minValue) {
            showValidationError(input, `Valor mínimo é ${formatCurrency(minValue)}`);
            return false;
        }

        if (value > maxValue) {
            showValidationError(input, `Valor máximo é ${formatCurrency(maxValue)}`);
            return false;
        }

        clearValidationError(input);
        return true;
    }

    function showValidationError(input, message) {
        clearValidationError(input);

        const errorDiv = document.createElement('div');
        errorDiv.className = 'invalid-feedback d-block';
        errorDiv.textContent = message;

        input.classList.add('is-invalid');
        input.parentNode.appendChild(errorDiv);
    }

    function clearValidationError(input) {
        input.classList.remove('is-invalid');
        const errorDiv = input.parentNode.querySelector('.invalid-feedback');
        if (errorDiv) {
            errorDiv.remove();
        }
    }

    function applyQuickFilter(filterType) {
        const currentUrl = new URL(window.location);

        switch (filterType) {
            case 'current':
                window.location.href = '/CreditCards/CurrentInvoice/' + getCurrentCardId();
                break;
            case 'history':
                window.location.href = '/CreditCards/InvoiceHistory/' + getCurrentCardId();
                break;
            case 'overdue':
                currentUrl.searchParams.set('status', 'overdue');
                window.location.href = currentUrl.toString();
                break;
            case 'paid':
                currentUrl.searchParams.set('status', 'paid');
                window.location.href = currentUrl.toString();
                break;
        }
    }

    function getCurrentCardId() {
        const cardElement = document.querySelector('[data-card-id]');
        return cardElement ? cardElement.getAttribute('data-card-id') : '';
    }

    function getUsageColorClass(percentage) {
        if (percentage > 80) return 'bg-danger';
        if (percentage > 60) return 'bg-warning';
        return 'bg-success';
    }

    function formatCurrency(value) {
        return new Intl.NumberFormat('pt-BR', {
            style: 'currency',
            currency: 'BRL'
        }).format(value);
    }

    function showToast(message, type = 'info') {
        // Implementação de toast usando Bootstrap ou sistema existente
        if (typeof FinanceSystem.UI.showAlert === 'function') {
            FinanceSystem.UI.showAlert(message, type);
        } else {
            console.log(`[${type.toUpperCase()}] ${message}`);
        }
    }

    // Função pública para ser chamada por outros módulos
    function payInvoice(creditCardId, amount, fullAmount = true) {
        const formData = {
            Amount: amount,
            PaymentDate: new Date().toISOString().split('T')[0],
            PayFullAmount: fullAmount,
            Notes: ''
        };

        return fetch(`/CreditCards/PayInvoice/${creditCardId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: new URLSearchParams(formData)
        });
    }

    return {
        initialize: initialize,
        refreshInvoiceData: refreshInvoiceData,
        payInvoice: payInvoice,
        validatePaymentAmount: validatePaymentAmount
    };
})();

// Auto-inicialização quando o DOM estiver pronto
document.addEventListener('DOMContentLoaded', function () {
    if (typeof FinanceSystem.Invoice !== 'undefined') {
        FinanceSystem.Invoice.initialize();
    }
});

// Integração com o módulo principal
if (typeof FinanceSystem.Pages !== 'undefined' && FinanceSystem.Pages.CreditCards) {
    const originalInitialize = FinanceSystem.Pages.CreditCards.initialize;
    FinanceSystem.Pages.CreditCards.initialize = function () {
        originalInitialize.call(this);
        FinanceSystem.Invoice.initialize();
    };
}
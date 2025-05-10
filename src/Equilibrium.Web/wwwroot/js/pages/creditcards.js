/**
 * Finance System - Credit Cards Page
 * Scripts específicos para a página de cartões de crédito
 */

var FinanceSystem = FinanceSystem || {};
FinanceSystem.Pages = FinanceSystem.Pages || {};

FinanceSystem.Pages.CreditCards = (function () {

    function initialize() {
        initializeCreditCardForm();
        initializeCreditCardsList();
        initializeCreditCardDetails();
    }

    function initializeCreditCardForm() {
        const cardForm = document.getElementById('credit-card-form');
        if (!cardForm) return;

        FinanceSystem.Modules.Forms.initializeForm(cardForm, {
            moneyInputs: ['#Limit', '#AvailableLimit'],
            validations: [
                {
                    selector: '#ClosingDay',
                    type: 'range',
                    min: 1,
                    max: 31,
                    name: 'dia de fechamento'
                },
                {
                    selector: '#DueDay',
                    type: 'range',
                    min: 1,
                    max: 31,
                    name: 'dia de vencimento'
                },
                {
                    selector: '#LastFourDigits',
                    type: 'custom',
                    validator: function (field) {
                        field.value = field.value.replace(/\D/g, '').substring(0, 4);
                        if (field.value.length > 0 && field.value.length < 4) {
                            FinanceSystem.Validation.showFieldError(field, 'O campo deve conter 4 dígitos');
                            return false;
                        }
                        return true;
                    }
                }
            ],
            validate: validateCreditCardForm
        });

        const limitField = document.getElementById('Limit');
        const availableLimitField = document.getElementById('AvailableLimit');

        if (limitField && availableLimitField) {
            limitField.addEventListener('change', function () {
                const limit = FinanceSystem.Core.parseCurrency(this.value);
                const availableLimit = FinanceSystem.Core.parseCurrency(availableLimitField.value);

                if (!isNaN(limit) && !isNaN(availableLimit) && availableLimit > limit) {
                    availableLimitField.value = this.value;
                }
            });

            availableLimitField.addEventListener('change', function () {
                const limit = FinanceSystem.Core.parseCurrency(limitField.value);
                const availableLimit = FinanceSystem.Core.parseCurrency(this.value);

                if (!isNaN(limit) && !isNaN(availableLimit) && availableLimit > limit) {
                    this.value = limitField.value;
                    showLimitError();
                }
            });
        }
    }

    function showLimitError() {
        alert('O limite disponível não pode ser maior que o limite total');
    }

    function validateCreditCardForm(event) {
        let isValid = true;
        const form = event.target;

        const nameField = form.querySelector('#Name');
        if (nameField && nameField.value.trim() === '') {
            isValid = false;
            FinanceSystem.Validation.showFieldError(nameField, 'O nome do cartão é obrigatório');
        }

        const brandField = form.querySelector('#Brand');
        if (brandField && brandField.value === '') {
            isValid = false;
            FinanceSystem.Validation.showFieldError(brandField, 'A bandeira do cartão é obrigatória');
        }

        const limitField = form.querySelector('#Limit');
        if (limitField) {
            const value = limitField.value.replace(/[^\d.,]/g, '').replace(',', '.');
            const limit = parseFloat(value);
            if (isNaN(limit) || limit <= 0) {
                isValid = false;
                FinanceSystem.Validation.showFieldError(limitField, 'Por favor, insira um limite válido');
            }
        }

        const availableLimitField = form.querySelector('#AvailableLimit');
        const limitField2 = form.querySelector('#Limit');
        if (availableLimitField && limitField2) {
            const limit = FinanceSystem.Core.parseCurrency(limitField2.value);
            const available = FinanceSystem.Core.parseCurrency(availableLimitField.value);

            if (isNaN(available)) {
                isValid = false;
                FinanceSystem.Validation.showFieldError(availableLimitField, 'Por favor, insira um limite disponível válido');
            } else if (available > limit) {
                isValid = false;
                FinanceSystem.Validation.showFieldError(availableLimitField, 'O limite disponível não pode ser maior que o limite total');
            }
        }

        return isValid;
    }

    function initializeCreditCardsList() {
        FinanceSystem.Modules.Financial.initializeCreditCardComponents();

        const deleteButtons = document.querySelectorAll('.delete-card-btn');
        deleteButtons.forEach(button => {
            button.addEventListener('click', function (e) {
                if (!confirm('Tem certeza que deseja excluir este cartão? Esta ação não pode ser desfeita.')) {
                    e.preventDefault();
                }
            });
        });
    }

    function initializeCreditCardDetails() {
        FinanceSystem.Modules.Financial.calculateNextDates();

        initializeCharts();

        const transactionsTable = document.querySelector('.transactions-table');
        if (transactionsTable) {
            FinanceSystem.Modules.Tables.initializeTable('.transactions-table', {
                order: [[0, 'desc']]
            });

            const deleteButtons = document.querySelectorAll('.delete-transaction');
            deleteButtons.forEach(button => {
                button.addEventListener('click', function (e) {
                    if (!confirm('Tem certeza que deseja excluir esta transação?')) {
                        e.preventDefault();
                    }
                });
            });
        }
    }

    function initializeCharts() {
        const usageChartCanvas = document.getElementById('usageChart');
        if (usageChartCanvas) {
            const usedLimit = parseFloat(usageChartCanvas.getAttribute('data-used') || 0);
            const totalLimit = parseFloat(usageChartCanvas.getAttribute('data-limit') || 1);
            const availableLimit = totalLimit - usedLimit;

            FinanceSystem.Modules.Charts.createPieChart('usageChart',
                ['Utilizado', 'Disponível'],
                [usedLimit, availableLimit],
                {
                    cutout: '70%',
                    colors: ['rgba(231, 74, 59, 0.8)', 'rgba(28, 200, 138, 0.8)']
                }
            );
        }

        const categoryChartCanvas = document.getElementById('categoryChart');
        if (categoryChartCanvas) {
            const labels = JSON.parse(categoryChartCanvas.getAttribute('data-labels') || '[]');
            const values = JSON.parse(categoryChartCanvas.getAttribute('data-values') || '[]');

            FinanceSystem.Modules.Charts.createPieChart('categoryChart', labels, values);
        }
    }

    return {
        initialize: initialize,
        initializeCreditCardForm: initializeCreditCardForm,
        initializeCreditCardsList: initializeCreditCardsList,
        initializeCreditCardDetails: initializeCreditCardDetails
    };
})();
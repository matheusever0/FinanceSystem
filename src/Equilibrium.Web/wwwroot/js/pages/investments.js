/**
 * Finance System - Investments Page
 * Scripts específicos para a página de investimentos
 * Com suporte a múltiplas moedas
 */

var FinanceSystem = FinanceSystem || {};
FinanceSystem.Pages = FinanceSystem.Pages || {};

FinanceSystem.Pages.Investments = (function () {
    function initialize() {
        initializeInvestmentForm();
        initializeInvestmentsList();
        initializeInvestmentDetails();
        initializeTransactionForm();
    }

    function initializeInvestmentForm() {
        const form = document.querySelector('form[asp-action="Create"], form[asp-action="Edit"]');
        if (!form) return;

        FinanceSystem.Modules.Financial.initializeMoneyMask('#InitialPrice');
        FinanceSystem.Modules.Financial.initializeMoneyMask('#CurrentPrice');

        initializeTotalCalculation();

        FinanceSystem.Validation.setupFormValidation(form, validateInvestmentForm);

        initializeCurrencyField();
    }

    function initializeCurrencyField() {
        const typeSelect = document.getElementById('Type');
        const currencySelect = document.getElementById('Currency');

        if (typeSelect && currencySelect) {
            typeSelect.addEventListener('change', function () {
                if (this.value == '4') {
                    currencySelect.value = 'USD';
                } else {
                    currencySelect.value = 'BRL';
                }
            });

            if (typeSelect.value) {
                typeSelect.dispatchEvent(new Event('change'));
            }
        }
    }

    function initializeTotalCalculation() {
        const quantityInput = document.getElementById('InitialQuantity');
        const priceInput = document.getElementById('InitialPrice');
        const totalValueInput = document.getElementById('totalValue');
        const currencySelect = document.getElementById('Currency');

        if (quantityInput && priceInput && totalValueInput) {
            const calculateTotal = () => {
                const quantity = parseFloat(quantityInput.value) || 0;
                const currency = currencySelect ? currencySelect.value : 'BRL';
                const price = FinanceSystem.Core.parseCurrency(priceInput.value, currency);
                const total = quantity * price;

                totalValueInput.value = FinanceSystem.Core.formatCurrency(total, currency);
            };

            calculateTotal();

            quantityInput.addEventListener('input', calculateTotal);
            priceInput.addEventListener('input', calculateTotal);

            if (currencySelect) {
                currencySelect.addEventListener('change', calculateTotal);
            }
        }
    }

    function validateInvestmentForm(event) {
        let isValid = true;
        const form = event.target;

        const symbolInput = document.getElementById('Symbol');
        if (symbolInput && symbolInput.value.trim() === '') {
            isValid = false;
            FinanceSystem.Validation.showFieldError(symbolInput, 'O símbolo é obrigatório');
        }

        const nameInput = document.getElementById('Name');
        if (nameInput && nameInput.value.trim() === '') {
            isValid = false;
            FinanceSystem.Validation.showFieldError(nameInput, 'O nome é obrigatório');
        }

        const typeInput = document.getElementById('InvestmentType');
        if (typeInput && typeInput.value === '') {
            isValid = false;
            FinanceSystem.Validation.showFieldError(typeInput, 'O tipo de investimento é obrigatório');
        }

        const quantityInput = document.getElementById('InitialQuantity');
        if (quantityInput) {
            const quantity = parseFloat(quantityInput.value);
            if (isNaN(quantity) || quantity <= 0) {
                isValid = false;
                FinanceSystem.Validation.showFieldError(quantityInput, 'A quantidade deve ser maior que zero');
            }
        }

        const priceInput = document.getElementById('InitialPrice');
        if (priceInput) {
            const currency = document.getElementById('Currency')?.value || 'BRL';
            const price = FinanceSystem.Core.parseCurrency(priceInput.value, currency);
            if (isNaN(price) || price <= 0) {
                isValid = false;
                FinanceSystem.Validation.showFieldError(priceInput, 'O preço deve ser maior que zero');
            }
        }

        return isValid;
    }

    function initializeInvestmentsList() {
        FinanceSystem.Modules.Tables.initializeTable('#investments-table', {
            order: [[8, 'desc']] 
        });

        highlightPerformance();

        initializeActionButtons();
    }

    function highlightPerformance() {
        const performanceCells = document.querySelectorAll('.performance-value');

        performanceCells.forEach(cell => {
            const text = cell.textContent.trim();
            const value = parseFloat(text.replace('%', '').replace(',', '.'));

            if (value > 0) {
                cell.classList.add('text-success');
            } else if (value < 0) {
                cell.classList.add('text-danger');
            }
        });
    }

    function initializeActionButtons() {
        const deleteButtons = document.querySelectorAll('.btn-delete-investment');
        deleteButtons.forEach(button => {
            button.addEventListener('click', function (e) {
                if (!confirm('Tem certeza que deseja excluir este investimento? Esta ação não pode ser desfeita.')) {
                    e.preventDefault();
                }
            });
        });
    }

    function initializeInvestmentDetails() {
        initializePerformanceChart();
        FinanceSystem.Modules.Tables.initializeTable('.transactions-table', {
            order: [[0, 'desc']]
        });
        initializeDetailActionButtons();
    }

    function initializePerformanceChart() {
        const chartCanvas = document.getElementById('performanceChart');
        if (!chartCanvas) return;

        const investmentId = chartCanvas.getAttribute('data-investment-id');

        fetchPerformanceData(investmentId)
            .then(data => {
                FinanceSystem.Modules.Charts.createLineChart('performanceChart', data.labels, data.values);
            })
            .catch(error => {
                console.error('Erro ao carregar dados de desempenho:', error);
            });
    }

    function fetchPerformanceData(investmentId) {
        return new Promise((resolve) => {
            setTimeout(() => {
                const labels = [
                    'Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun',
                    'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'
                ];

                const values = generateMockData();

                resolve({ labels, values });
            }, 200);
        });
    }

    function generateMockData() {
        const data = [];
        let value = 5000 + Math.random() * 2000;

        for (let i = 0; i < 12; i++) {
            value = value * (1 + (Math.random() * 0.1 - 0.03));
            data.push(value.toFixed(2));
        }

        return data;
    }

    function initializeDetailActionButtons() {
        const addTransactionBtn = document.getElementById('add-transaction-btn');
        if (addTransactionBtn) {
            addTransactionBtn.addEventListener('click', function () {
                FinanceSystem.UI.showModal('transactionModal');
            });
        }
    }

    function initializeTransactionForm() {
        const form = document.querySelector('form[asp-action="AddTransaction"]');
        if (!form) return;

        FinanceSystem.Modules.Financial.initializeMoneyMask('#Price');

        FinanceSystem.Validation.setupFormValidation(form, validateTransactionForm);

        initializeTransactionTypeHandler();
    }

    function initializeTransactionTypeHandler() {
        const typeSelect = document.getElementById('InvestmentType');
        const quantityInput = document.getElementById('Quantity');
        const priceInput = document.getElementById('Price');
        const taxesInput = document.getElementById('Taxes');
        const totalValueInput = document.getElementById('totalTransactionValue');

        if (typeSelect && quantityInput && priceInput && totalValueInput) {
            typeSelect.addEventListener('change', function () {
                updateFieldsVisibility(this.value);
            });

            const calculateTotal = () => {
                const type = parseInt(typeSelect.value) || 0;
                const quantity = parseFloat(quantityInput.value) || 0;
                const price = FinanceSystem.Core.parseCurrency(priceInput.value);
                const taxes = FinanceSystem.Core.parseCurrency(taxesInput.value);

                let total = 0;

                switch (type) {
                    case 1: // Compra
                        total = (quantity * price) + taxes;
                        break;
                    case 2: // Venda
                        total = (quantity * price) - taxes;
                        break;
                    case 3: // Dividendo
                    case 6: // JCP
                    case 7: // Rendimento
                        total = price; // Valor total recebido
                        break;
                    default:
                        total = quantity * price;
                }

                totalValueInput.value = FinanceSystem.Core.formatCurrency(total);
            };

            updateFieldsVisibility(typeSelect.value);

            calculateTotal();

            typeSelect.addEventListener('change', calculateTotal);
            quantityInput.addEventListener('input', calculateTotal);
            priceInput.addEventListener('input', calculateTotal);
            taxesInput.addEventListener('input', calculateTotal);
        }
    }

    function updateFieldsVisibility(type) {
        const quantityGroup = document.getElementById('Quantity').closest('.mb-3');
        const priceGroup = document.getElementById('Price').closest('.mb-3');
        const taxesGroup = document.getElementById('Taxes').closest('.mb-3');

        // Default visibility
        FinanceSystem.UI.toggleVisibility(quantityGroup, true);
        FinanceSystem.UI.toggleVisibility(priceGroup, true);
        FinanceSystem.UI.toggleVisibility(taxesGroup, true);

        type = parseInt(type) || 0;

        switch (type) {
            case 3: // Dividendo
            case 6: // JCP
            case 7: // Rendimento
                FinanceSystem.UI.toggleVisibility(quantityGroup, false);
                document.getElementById('Price').placeholder = 'Valor total recebido';
                break;
            case 4: // Split
            case 5: // Bonificação
                FinanceSystem.UI.toggleVisibility(taxesGroup, false);
                FinanceSystem.UI.toggleVisibility(priceGroup, false);
                break;
            default:
                document.getElementById('Price').placeholder = 'Preço unitário';
        }
    }

    function validateTransactionForm(event) {
        let isValid = true;
        const form = event.target;

        const type = form.querySelector('#InvestmentType');
        if (!type || type.value === '') {
            isValid = false;
            FinanceSystem.Validation.showFieldError(type, 'O tipo de transação é obrigatório');
        }

        const transactionDate = form.querySelector('#TransactionDate');
        if (transactionDate && transactionDate.value === '') {
            isValid = false;
            FinanceSystem.Validation.showFieldError(transactionDate, 'A data da transação é obrigatória');
        }

        // Conditional validation based on transaction type
        const typeValue = parseInt(type.value) || 0;

        if (typeValue === 1 || typeValue === 2) { // Buy or Sell
            const quantity = form.querySelector('#Quantity');
            if (quantity && (quantity.value === '' || parseFloat(quantity.value) <= 0)) {
                isValid = false;
                FinanceSystem.Validation.showFieldError(quantity, 'A quantidade deve ser maior que zero');
            }

            const price = form.querySelector('#Price');
            if (price && (price.value === '' || FinanceSystem.Core.parseCurrency(price.value) <= 0)) {
                isValid = false;
                FinanceSystem.Validation.showFieldError(price, 'O preço deve ser maior que zero');
            }
        } else if (typeValue === 3 || typeValue === 6 || typeValue === 7) { // Dividend, JCP, Yield
            const price = form.querySelector('#Price');
            if (price && (price.value === '' || FinanceSystem.Core.parseCurrency(price.value) <= 0)) {
                isValid = false;
                FinanceSystem.Validation.showFieldError(price, 'O valor recebido deve ser maior que zero');
            }
        } else if (typeValue === 4 || typeValue === 5) { // Split or Bonus
            const quantity = form.querySelector('#Quantity');
            if (quantity && (quantity.value === '' || parseFloat(quantity.value) <= 0)) {
                isValid = false;
                FinanceSystem.Validation.showFieldError(quantity, 'A quantidade deve ser maior que zero');
            }
        }

        return isValid;
    }

    return {
        initialize: initialize,
        initializeInvestmentForm: initializeInvestmentForm,
        initializeInvestmentsList: initializeInvestmentsList,
        initializeInvestmentDetails: initializeInvestmentDetails,
        initializeTransactionForm: initializeTransactionForm
    };
})();
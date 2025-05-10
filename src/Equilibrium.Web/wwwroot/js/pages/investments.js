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
        initializeMoneyInputs();

        initializeTotalCalculation();

        setupFormValidation(form);

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

    function initializeMoneyInputs() {
        if (FinanceSystem.Modules && FinanceSystem.Modules.Financial) {
            FinanceSystem.Modules.Financial.initializeMoneyMask('#InitialPrice');
            FinanceSystem.Modules.Financial.initializeMoneyMask('#CurrentPrice');
            return;
        }

        if (typeof $.fn.mask !== 'undefined') {
            $('#InitialPrice, #CurrentPrice').mask('#.##0,00', { reverse: true });
        } else {
            const moneyInputs = document.querySelectorAll('#InitialPrice, #CurrentPrice');
            moneyInputs.forEach(input => {
                input.addEventListener('input', function () {
                    const currency = getCurrencyFromForm();
                    formatCurrencyInput(this, currency);
                });

                if (input.value) {
                    const currency = getCurrencyFromForm();
                    formatCurrencyInput(input, currency);
                }
            });
        }
    }

    function getCurrencyFromForm() {
        const currencySelect = document.getElementById('Currency');
        return currencySelect ? currencySelect.value : 'BRL';
    }

    function formatCurrencyInput(input, currency = 'BRL') {
        const cursorPosition = input.selectionStart;
        const inputLength = input.value.length;

        let value = input.value.replace(/[^\d.,]/g, '');

        if (currency === 'BRL') {
            value = value.replace(/\D/g, '');
            if (value === '') {
                input.value = '';
                return;
            }

            value = (parseFloat(value) / 100).toFixed(2);
            input.value = value.replace('.', ',');
        } else {
            value = value.replace(/,/g, '');
            if (value === '') {
                input.value = '';
                return;
            }

            let parts = value.split('.');
            if (parts.length > 2) {
                value = parts[0] + '.' + parts.slice(1).join('');
            }

            if (!value.includes('.')) {
                value = value.replace(/\D/g, '');
                value = (parseFloat(value) / 100).toFixed(2);
            } else if (value.endsWith('.')) {
                value = parseFloat(value.replace(/\.$/, '')).toFixed(0) + '.';
            } else {
                let [whole, decimal] = value.split('.');
                whole = whole.replace(/\D/g, '') || '0';
                decimal = decimal.replace(/\D/g, '');
                value = whole + '.' + decimal;
                if (decimal.length > 2) {
                    value = parseFloat(value).toFixed(2);
                }
            }

            input.value = value;
        }

        const newLength = input.value.length;
        const newPosition = cursorPosition + (newLength - inputLength);
        if (newPosition >= 0) {
            input.setSelectionRange(newPosition, newPosition);
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
                const price = parseCurrency(priceInput.value, currency);
                const total = quantity * price;

                totalValueInput.value = formatCurrency(total, currency);
            };

            calculateTotal();

            quantityInput.addEventListener('input', calculateTotal);
            priceInput.addEventListener('input', calculateTotal);

            if (currencySelect) {
                currencySelect.addEventListener('change', calculateTotal);
            }
        }
    }

    function setupFormValidation(form) {
        if (!form) return;

        if (FinanceSystem.Validation && FinanceSystem.Validation.setupFormValidation) {
            FinanceSystem.Validation.setupFormValidation(form, validateInvestmentForm);
        } else {
            form.addEventListener('submit', function (event) {
                if (!validateInvestmentForm(event)) {
                    event.preventDefault();
                    event.stopPropagation();
                }
            });
        }
    }

    function validateInvestmentForm(event) {
        let isValid = true;
        const form = event.target;

        const symbolInput = document.getElementById('Symbol');
        if (symbolInput && symbolInput.value.trim() === '') {
            isValid = false;
            showFieldError(symbolInput, 'O símbolo é obrigatório');
        }

        const nameInput = document.getElementById('Name');
        if (nameInput && nameInput.value.trim() === '') {
            isValid = false;
            showFieldError(nameInput, 'O nome é obrigatório');
        }

        const typeInput = document.getElementById('InvestmentType');
        if (typeInput && typeInput.value === '') {
            isValid = false;
            showFieldError(typeInput, 'O tipo de investimento é obrigatório');
        }

        const quantityInput = document.getElementById('InitialQuantity');
        if (quantityInput) {
            const quantity = parseFloat(quantityInput.value);
            if (isNaN(quantity) || quantity <= 0) {
                isValid = false;
                showFieldError(quantityInput, 'A quantidade deve ser maior que zero');
            }
        }

        const priceInput = document.getElementById('InitialPrice');
        if (priceInput) {
            const price = parseCurrency(priceInput.value);
            if (isNaN(price) || price <= 0) {
                isValid = false;
                showFieldError(priceInput, 'O preço deve ser maior que zero');
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

    function initializeInvestmentsList() {
        initializeInvestmentsTable();

        highlightPerformance();

        initializeActionButtons();
    }

    function initializeInvestmentsTable() {
        if (typeof $.fn.DataTable !== 'undefined') {
            $('#investments-table').DataTable({
                language: {
                    url: '//cdn.datatables.net/plug-ins/1.10.25/i18n/Portuguese-Brasil.json'
                },
                responsive: true,
                pageLength: 10,
                order: [[8, 'desc']], // Ordena por ganho/perda 
                columnDefs: [
                    { orderable: false, targets: -1 } // Desabilita ordenação na coluna de ações
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
            if (index !== headers.length - 1) { // Skip actions column
                header.style.cursor = 'pointer';
                header.addEventListener('click', () => {
                    sortTable(table, index);
                });
            }
        });
    }

    function sortTable(table, columnIndex) {
        const thead = table.querySelector('thead');
        const tbody = table.querySelector('tbody');
        const rows = Array.from(tbody.querySelectorAll('tr'));

        const header = thead.querySelectorAll('th')[columnIndex];
        const isAscending = header.classList.contains('sorting_asc');

        thead.querySelectorAll('th').forEach(th => {
            th.classList.remove('sorting_asc', 'sorting_desc');
        });

        header.classList.add(isAscending ? 'sorting_desc' : 'sorting_asc');

        rows.sort((a, b) => {
            const aValue = a.cells[columnIndex].textContent.trim();
            const bValue = b.cells[columnIndex].textContent.trim();

            if (aValue.includes('R$') && bValue.includes('R$')) {
                const aNum = parseCurrency(aValue);
                const bNum = parseCurrency(bValue);
                return isAscending ? aNum - bNum : bNum - aNum;
            }

            if (aValue.includes('%') && bValue.includes('%')) {
                const aNum = parseFloat(aValue.replace('%', '').replace(',', '.'));
                const bNum = parseFloat(bValue.replace('%', '').replace(',', '.'));
                return isAscending ? aNum - bNum : bNum - aNum;
            }

            if (aValue < bValue) return isAscending ? -1 : 1;
            if (aValue > bValue) return isAscending ? 1 : -1;
            return 0;
        });

        rows.forEach(row => tbody.appendChild(row));
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

        initializeTransactionsTable();

        initializeDetailActionButtons();
    }

    function initializePerformanceChart() {
        const chartCanvas = document.getElementById('performanceChart');
        if (!chartCanvas) return;

        const investmentId = chartCanvas.getAttribute('data-investment-id');

        fetchPerformanceData(investmentId)
            .then(data => {
                if (FinanceSystem.Modules && FinanceSystem.Modules.Charts) {
                    FinanceSystem.Modules.Charts.createLineChart('performanceChart', data.labels, data.values);
                } else if (typeof Chart !== 'undefined') {
                    createPerformanceChart(chartCanvas, data.labels, data.values);
                }
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

    function createPerformanceChart(canvas, labels, values) {
        new Chart(canvas, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Valor Total (R$)',
                    backgroundColor: 'rgba(78, 115, 223, 0.05)',
                    borderColor: 'rgba(78, 115, 223, 1)',
                    pointBackgroundColor: 'rgba(78, 115, 223, 1)',
                    pointBorderColor: '#fff',
                    pointHoverRadius: 5,
                    pointHoverBackgroundColor: '#fff',
                    pointHoverBorderColor: 'rgba(78, 115, 223, 1)',
                    data: values,
                    fill: true,
                    tension: 0.3
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    x: {
                        grid: {
                            display: false
                        }
                    },
                    y: {
                        beginAtZero: false,
                        ticks: {
                            callback: function (value) {
                                return 'R$ ' + value.toLocaleString('pt-BR');
                            }
                        }
                    }
                },
                plugins: {
                    tooltip: {
                        callbacks: {
                            label: function (context) {
                                return 'R$ ' + context.parsed.y.toLocaleString('pt-BR', { minimumFractionDigits: 2 });
                            }
                        }
                    }
                }
            }
        });
    }

    function initializeTransactionsTable() {
        if (typeof $.fn.DataTable !== 'undefined') {
            $('.transactions-table').DataTable({
                language: {
                    url: '//cdn.datatables.net/plug-ins/1.10.25/i18n/Portuguese-Brasil.json'
                },
                responsive: true,
                pageLength: 10,
                order: [[0, 'desc']] 
            });
        } else if (FinanceSystem.Modules && FinanceSystem.Modules.Tables) {
            FinanceSystem.Modules.Tables.initializeTableSort();
        }
    }

    function initializeDetailActionButtons() {
        const addTransactionBtn = document.getElementById('add-transaction-btn');
        if (addTransactionBtn) {
            addTransactionBtn.addEventListener('click', function () {
                const modal = document.getElementById('transactionModal');
                if (modal) {
                    if (FinanceSystem.UI && FinanceSystem.UI.showModal) {
                        FinanceSystem.UI.showModal('transactionModal');
                    } else if (typeof bootstrap !== 'undefined') {
                        const modalInstance = new bootstrap.Modal(modal);
                        modalInstance.show();
                    } else {
                        modal.style.display = 'block';
                    }
                }
            });
        }
    }

    function initializeTransactionForm() {
        const form = document.querySelector('form[asp-action="AddTransaction"]');
        if (!form) return;

        if (FinanceSystem.Modules && FinanceSystem.Modules.Financial) {
            FinanceSystem.Modules.Financial.initializeMoneyMask('#Price');
        } else {
            const priceInput = document.getElementById('Price');
            if (priceInput) {
                priceInput.addEventListener('input', function () {
                    formatCurrencyInput(this);
                });
            }
        }

        setupFormValidation(form);

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
                const price = parseCurrency(priceInput.value);
                const taxes = parseCurrency(taxesInput.value);

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

                totalValueInput.value = formatCurrency(total);
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

        quantityGroup.style.display = '';
        priceGroup.style.display = '';
        taxesGroup.style.display = '';

        type = parseInt(type) || 0;

        switch (type) {
            case 3: // Dividendo
            case 6: // JCP
            case 7: // Rendimento
                quantityGroup.style.display = 'none';
                document.getElementById('Price').placeholder = 'Valor total recebido';
                break;
            case 4: // Split
            case 5: // Bonificação
                taxesGroup.style.display = 'none';
                priceGroup.style.display = 'none';
                break;
            default:
                document.getElementById('Price').placeholder = 'Preço unitário';
        }
    }

    function parseCurrency(value, currency = 'BRL') {
        if (typeof value === 'number') return value;

        if (!value) return 0;

        if (currency === 'BRL') {
            value = value.toString().replace(/[R$\s.]/g, '').replace(',', '.');
        } else {
            value = value.toString().replace(/[$\s,]/g, '');
        }

        return parseFloat(value) || 0;
    }

    function formatCurrency(value, currency = 'BRL') {
        if (FinanceSystem.Core && FinanceSystem.Core.formatCurrency) {
            return FinanceSystem.Core.formatCurrency(value, currency);
        }

        const locales = {
            'BRL': 'pt-BR',
            'USD': 'en-US',
            'EUR': 'de-DE',
            'GBP': 'en-GB'
        };

        const locale = locales[currency] || 'en-US';

        return new Intl.NumberFormat(locale, {
            style: 'currency',
            currency: currency
        }).format(value);
    }


    return {
        initialize: initialize,
        initializeInvestmentForm: initializeInvestmentForm,
        initializeInvestmentsList: initializeInvestmentsList,
        initializeInvestmentDetails: initializeInvestmentDetails,
        initializeTransactionForm: initializeTransactionForm,
        formatCurrency: formatCurrency,
        parseCurrency: parseCurrency
    };
})();
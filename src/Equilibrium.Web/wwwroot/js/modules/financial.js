/**
 * Finance System - Financial Module
 * Funções específicas para cálculos e componentes financeiros
 */

var FinanceSystem = FinanceSystem || {};
FinanceSystem.Modules = FinanceSystem.Modules || {};

FinanceSystem.Modules.Financial = (function () {
    function initialize() {
    }

    function initializeMoneyMask(selector) {
        const moneyInput = typeof selector === 'string' ?
            document.querySelector(selector) : selector;

        if (!moneyInput) return;

        if (typeof $.fn.mask !== 'undefined') {
            $(moneyInput).mask('#.##0,00', { reverse: true });

            $(moneyInput).on('blur', function () {
                let value = $(this).val();
                value = value.replace(/\./g, '').replace(',', '.');
                const parsedValue = parseFloat(value);

                if (!isNaN(parsedValue) && parsedValue > 0) {
                    $(this).val(parsedValue.toFixed(2).replace('.', ','));
                    $(this).removeClass('input-validation-error');
                    $(this).next('.text-danger').text('');
                } else {
                    $(this).addClass('input-validation-error');
                    $(this).next('.text-danger').text('O campo Valor deve ser um número válido.');
                }
            });
        } else {
            moneyInput.addEventListener('input', function () {
                formatCurrencyInput(this);
            });

            moneyInput.addEventListener('blur', function () {
                validateCurrencyInput(this);
            });
        }
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

    function formatPercentInput(input) {
        let value = input.value.replace(/[^\d.,]/g, '');

        if (value.includes(',')) {
            const parts = value.split(',');
            if (parts[1].length > 2) {
                parts[1] = parts[1].substring(0, 2);
                value = parts.join(',');
            }
        }

        if (!input.value.includes('%') && value) {
            value += '%';
        }

        input.value = value;
    }

    function validateCurrencyInput(input) {
        let value = input.value;
        value = value.replace(/\./g, '').replace(',', '.');
        const parsedValue = parseFloat(value);

        if (!isNaN(parsedValue) && parsedValue >= 0) {
            input.value = parsedValue.toFixed(2).replace('.', ',');
            input.classList.remove('is-invalid');
            const errorElement = input.parentElement.querySelector('.text-danger');
            if (errorElement) {
                errorElement.textContent = '';
            }
            return true;
        } else {
            input.classList.add('is-invalid');
            let errorElement = input.parentElement.querySelector('.text-danger');
            if (!errorElement) {
                errorElement = document.createElement('div');
                errorElement.classList.add('text-danger');
                input.parentElement.appendChild(errorElement);
            }
            errorElement.textContent = 'O campo Valor deve ser um número válido.';
            return false;
        }
    }

    function initializeRecurringToggle(form) {
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

    function initializeCreditCardComponents() {
        const progressBars = document.querySelectorAll('.credit-card-progress');

        progressBars.forEach(progressBar => {
            const percentage = progressBar.getAttribute('data-percentage') || 0;

            if (percentage > 90) {
                progressBar.classList.add('bg-danger');
            } else if (percentage > 70) {
                progressBar.classList.add('bg-warning');
            } else if (percentage > 50) {
                progressBar.classList.add('bg-info');
            } else {
                progressBar.classList.add('bg-success');
            }

            progressBar.style.width = `${percentage}%`;
        });

        updateLimitsDisplay();
    }

    function updateLimitsDisplay() {
        const limitDisplays = document.querySelectorAll('.limit-display');

        limitDisplays.forEach(display => {
            const total = parseFloat(display.getAttribute('data-total') || 0);
            const used = parseFloat(display.getAttribute('data-used') || 0);
            const available = total - used;
            const percentage = (used / total * 100) || 0;

            const totalElement = display.querySelector('.total-limit');
            const availableElement = display.querySelector('.available-limit');
            const usedElement = display.querySelector('.used-limit');
            const percentageElement = display.querySelector('.used-percentage');

            if (totalElement) totalElement.textContent = formatCurrency(total);
            if (availableElement) availableElement.textContent = formatCurrency(available);
            if (usedElement) usedElement.textContent = formatCurrency(used);
            if (percentageElement) percentageElement.textContent = `${percentage.toFixed(0)}%`;

            const progressBar = display.querySelector('.progress-bar');
            if (progressBar) {
                progressBar.style.width = `${percentage}%`;

                progressBar.classList.remove('bg-success', 'bg-info', 'bg-warning', 'bg-danger');

                if (percentage > 90) {
                    progressBar.classList.add('bg-danger');
                } else if (percentage > 70) {
                    progressBar.classList.add('bg-warning');
                } else if (percentage > 50) {
                    progressBar.classList.add('bg-info');
                } else {
                    progressBar.classList.add('bg-success');
                }
            }
        });
    }

    function calculateNextDates() {
        const cardElements = document.querySelectorAll('.credit-card-dates');

        cardElements.forEach(element => {
            const closingDay = parseInt(element.getAttribute('data-closing-day'), 10);
            const dueDay = parseInt(element.getAttribute('data-due-day'), 10);

            if (isNaN(closingDay) || isNaN(dueDay)) return;

            const today = new Date();
            let nextClosingDate = new Date(today.getFullYear(), today.getMonth(), closingDay);
            let nextDueDate = new Date(today.getFullYear(), today.getMonth(), dueDay);

            if (today.getDate() > closingDay) {
                nextClosingDate.setMonth(nextClosingDate.getMonth() + 1);
            }

            if (today.getDate() > dueDay) {
                nextDueDate.setMonth(nextDueDate.getMonth() + 1);
            }

            const closingDateElement = element.querySelector('.next-closing-date');
            const dueDateElement = element.querySelector('.next-due-date');
            const daysToClosingElement = element.querySelector('.days-to-closing');
            const daysToDueElement = element.querySelector('.days-to-due');

            if (closingDateElement) {
                closingDateElement.textContent = formatDate(nextClosingDate);
            }

            if (dueDateElement) {
                dueDateElement.textContent = formatDate(nextDueDate);
            }

            if (daysToClosingElement) {
                const daysToClosing = Math.ceil((nextClosingDate - today) / (1000 * 60 * 60 * 24));
                daysToClosingElement.textContent = daysToClosing;
            }

            if (daysToDueElement) {
                const daysToDue = Math.ceil((nextDueDate - today) / (1000 * 60 * 60 * 24));
                daysToDueElement.textContent = daysToDue;
            }
        });
    }

    function formatDate(date) {
        return date.toLocaleDateString('pt-BR');
    }

    function initializeFinancialFilters() {
        const filterButtons = document.querySelectorAll('.payment-filter, .income-filter');

        filterButtons.forEach(button => {
            button.addEventListener('click', function () {
                filterButtons.forEach(btn => btn.classList.remove('active'));

                this.classList.add('active');

                const filterValue = this.getAttribute('data-filter');
                filterFinancialTable(filterValue);
            });
        });

        const searchInput = document.getElementById('financial-search');
        if (searchInput) {
            searchInput.addEventListener('input', function () {
                filterTableByText(this.value);
            });
        }
    }

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

    function calculateInstallments(totalAmount, interestRate, termMonths, system = 'PRICE') {
        if (!totalAmount || !termMonths) {
            return [];
        }

        const monthlyRate = interestRate > 0 ? (interestRate / 100) / 12 : 0;
        const installments = [];

        let remainingBalance = totalAmount;
        let totalPaid = 0;
        let totalInterest = 0;

        if (system === 'PRICE') {
            let installmentValue;

            if (monthlyRate === 0) {
                installmentValue = totalAmount / termMonths;
            } else {
                installmentValue = totalAmount * (monthlyRate * Math.pow(1 + monthlyRate, termMonths)) / (Math.pow(1 + monthlyRate, termMonths) - 1);
            }

            for (let i = 1; i <= termMonths; i++) {
                const interestAmount = remainingBalance * monthlyRate;
                const principalAmount = installmentValue - interestAmount;
                remainingBalance -= principalAmount;

                let actualInstallment = installmentValue;
                if (i === termMonths) {
                    actualInstallment = principalAmount + interestAmount;
                    remainingBalance = 0;
                }

                totalPaid += actualInstallment;
                totalInterest += interestAmount;

                installments.push({
                    number: i,
                    value: actualInstallment,
                    principal: principalAmount,
                    interest: interestAmount,
                    balance: Math.max(0, remainingBalance),
                    totalPaid: totalPaid,
                    totalInterest: totalInterest
                });
            }
        } else if (system === 'SAC') {
            const amortization = totalAmount / termMonths;

            for (let i = 1; i <= termMonths; i++) {
                const interestAmount = remainingBalance * monthlyRate;
                const installmentValue = amortization + interestAmount;
                remainingBalance -= amortization;

                totalPaid += installmentValue;
                totalInterest += interestAmount;

                installments.push({
                    number: i,
                    value: installmentValue,
                    principal: amortization,
                    interest: interestAmount,
                    balance: Math.max(0, remainingBalance),
                    totalPaid: totalPaid,
                    totalInterest: totalInterest
                });
            }
        }

        return installments;
    }

    function calculateFutureValue(presentValue, rate, periods, periodicContribution = 0, contributionAtBeginning = false) {
        rate = rate / 100; // Converte percentual para decimal

        const futureValuePV = presentValue * Math.pow(1 + rate, periods);

        if (periodicContribution === 0) {
            return futureValuePV;
        }

        let futureValuePMT;
        if (contributionAtBeginning) {
            futureValuePMT = periodicContribution * ((Math.pow(1 + rate, periods) - 1) / rate) * (1 + rate);
        } else {
            futureValuePMT = periodicContribution * ((Math.pow(1 + rate, periods) - 1) / rate);
        }

        return futureValuePV + futureValuePMT;
    }

    function parseCurrency(value, currency = 'BRL') {
        if (typeof value === 'number') {
            return value;
        }

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
            'GBP': 'en-GB',
            'JPY': 'ja-JP'
        };

        const locale = locales[currency] || 'en-US';

        return new Intl.NumberFormat(locale, {
            style: 'currency',
            currency: currency
        }).format(value);
    }

    function formatPercent(value, decimals = 2) {
        return value.toFixed(decimals) + '%';
    }

    function formatCurrencyValues() {
        const currencyElements = document.querySelectorAll('.currency-value');
        currencyElements.forEach(element => {
            const value = parseFloat(element.getAttribute('data-value'));
            const currency = element.getAttribute('data-currency') || 'BRL';
            if (!isNaN(value)) {
                element.textContent = formatCurrency(value, currency);
            }
        });
    }

    return {
        initialize: initialize,
        initializeMoneyMask: initializeMoneyMask,
        formatCurrencyInput: formatCurrencyInput,
        formatPercentInput: formatPercentInput,
        validateCurrencyInput: validateCurrencyInput,
        initializeRecurringToggle: initializeRecurringToggle,
        initializeCreditCardComponents: initializeCreditCardComponents,
        updateLimitsDisplay: updateLimitsDisplay,
        calculateNextDates: calculateNextDates,
        initializeFinancialFilters: initializeFinancialFilters,
        filterFinancialTable: filterFinancialTable,
        filterTableByText: filterTableByText,
        calculateInstallments: calculateInstallments,
        calculateFutureValue: calculateFutureValue,
        parseCurrency: parseCurrency,
        formatCurrency: formatCurrency,
        formatPercent: formatPercent,
        formatCurrencyValues: formatCurrencyValues
    };
})();
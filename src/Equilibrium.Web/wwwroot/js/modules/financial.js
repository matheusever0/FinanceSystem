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

        const moneyInput = document.querySelector(selector);
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

    function initializeStatusToggle(switchSelector, labelSelector) {
        const statusSwitch = document.querySelector(switchSelector);
        const statusLabel = document.querySelector(labelSelector);

        if (!statusSwitch || !statusLabel) return;

        function updateStatusLabel() {
            if (statusSwitch.checked) {
                statusLabel.classList.remove('bg-danger');
                statusLabel.classList.add('bg-success');
                statusLabel.textContent = 'Ativo';
            } else {
                statusLabel.classList.remove('bg-success');
                statusLabel.classList.add('bg-danger');
                statusLabel.textContent = 'Inativo';
            }
        }

        updateStatusLabel();

        statusSwitch.addEventListener('change', updateStatusLabel);
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

    function calculateIRR(cashflows, guess = 0.1) {
        if (!cashflows || cashflows.length < 2) {
            return null;
        }

        const maxIterations = 1000;
        const tolerance = 0.0000001;

        let rate = guess;

        for (let i = 0; i < maxIterations; i++) {
            const npv = calculateNPV(cashflows, rate);

            if (Math.abs(npv) < tolerance) {
                return rate;
            }

            const derivative = calculateNPVDerivative(cashflows, rate);

            if (derivative === 0) {
                break;
            }

            const newRate = rate - npv / derivative;

            if (Math.abs(newRate - rate) < tolerance) {
                return newRate;
            }

            rate = newRate;
        }

        return null; // Não convergiu
    }

    function calculateNPV(cashflows, rate) {
        let npv = 0;

        for (let i = 0; i < cashflows.length; i++) {
            npv += cashflows[i] / Math.pow(1 + rate, i);
        }

        return npv;
    }

    function calculateNPVDerivative(cashflows, rate) {
        let derivative = 0;

        for (let i = 1; i < cashflows.length; i++) {
            derivative -= i * cashflows[i] / Math.pow(1 + rate, i + 1);
        }

        return derivative;
    }

    function calculatePresentValue(payment, rate, periods) {
        rate = rate / 100; // Converte percentual para decimal

        if (rate === 0) {
            return payment * periods;
        }

        return payment * ((1 - Math.pow(1 + rate, -periods)) / rate);
    }

    function calculatePayment(principal, rate, periods) {
        rate = rate / 100; // Converte percentual para decimal

        if (rate === 0) {
            return principal / periods;
        }

        return principal * (rate * Math.pow(1 + rate, periods)) / (Math.pow(1 + rate, periods) - 1);
    }

    function calculatePeriods(principal, payment, rate) {
        rate = rate / 100; // Converte percentual para decimal

        if (rate === 0) {
            return principal / payment;
        }

        return Math.log(payment / (payment - principal * rate)) / Math.log(1 + rate);
    }

    function calculateRate(principal, payment, periods) {
        if (payment * periods <= principal) {
            return 0; // Não é possível amortizar o empréstimo com esses parâmetros
        }

        const tolerance = 0.0000001;
        const maxIterations = 100;

        let rate = 0.1; // Estimativa inicial

        for (let i = 0; i < maxIterations; i++) {
            const currentPayment = calculatePayment(principal, rate * 100, periods);
            const diff = currentPayment - payment;

            if (Math.abs(diff) < tolerance) {
                return rate * 100;
            }

            rate = rate * (1 + diff / payment * 0.1);
        }

        return rate * 100; // Converte decimal para percentual
    }

    function calculateROI(initialInvestment, finalValue) {
        if (initialInvestment === 0) {
            return null; // Evita divisão por zero
        }

        return ((finalValue - initialInvestment) / initialInvestment) * 100;
    }

    function parseCurrency(value, currency = 'BRL') {
        if (typeof value === 'number') {
            return value;
        }

        value = value.replace(/[^\d,.-]/g, '');

        if (currency === 'BRL') {
            if (value.indexOf(',') > -1 && value.indexOf('.') > -1) {
                value = value.replace(/\./g, '').replace(',', '.');
            } else if (value.indexOf(',') > -1) {
                value = value.replace(',', '.');
            }
        } else {
            if (value.indexOf(',') > -1) {
                value = value.replace(/,/g, '');
            }
        }

        return parseFloat(value);
    }

    function formatCurrency(value, currency = 'BRL') {
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

    function clickButtonReceived() {
        const markReceivedButtons = document.querySelectorAll('.mark-received-installment');
        markReceivedButtons.forEach(button => {
            button.addEventListener('click', function () {
                const installmentId = this.getAttribute('data-installment-id');
                document.getElementById('installmentId').value = installmentId;
            });
        });
    }

    return {
        initialize: initialize,
        initializeMoneyMask: initializeMoneyMask,
        initializeRecurringToggle: initializeRecurringToggle,
        initializeStatusToggle: initializeStatusToggle,
        initializeCreditCardComponents: initializeCreditCardComponents,
        initializeFinancialFilters: initializeFinancialFilters,
        calculateInstallments: calculateInstallments,
        calculateFutureValue: calculateFutureValue,
        calculateIRR: calculateIRR,
        calculateNPV: calculateNPV,
        calculatePresentValue: calculatePresentValue,
        calculatePayment: calculatePayment,
        calculatePeriods: calculatePeriods,
        calculateRate: calculateRate,
        calculateROI: calculateROI,
        parseCurrency: parseCurrency,
        formatCurrency: formatCurrency,
        formatPercent: formatPercent,
        updateLimitsDisplay: updateLimitsDisplay,
        calculateNextDates: calculateNextDates,
        filterFinancialTable: filterFinancialTable,
        filterTableByText: filterTableByText,
        clickButtonReceived: clickButtonReceived
    };
})();
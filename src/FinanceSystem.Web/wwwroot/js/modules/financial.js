/**
 * Finance System - Financial Module
 * Funções específicas para cálculos e componentes financeiros
 */

// Namespace global para o sistema
var FinanceSystem = FinanceSystem || {};
FinanceSystem.Modules = FinanceSystem.Modules || {};

// Módulo Financial
FinanceSystem.Modules.Financial = (function () {
    /**
     * Inicializa o módulo financeiro
     */
    function initialize() {
        // Nada a inicializar automaticamente
    }

    /**
     * Inicializa uma máscara de moeda em um campo
     * @param {string} selector - Seletor do campo
     */
    function initializeMoneyMask(selector) {

        console.log('mask aqui')

        const moneyInput = document.querySelector(selector);

        console.log(moneyInput)
        if (!moneyInput) return;

        if (typeof $.fn.mask !== 'undefined') {
            $(moneyInput).mask('#.##0,00', { reverse: true });

            // Evento blur para validação
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
            // Implementação manual se mask não estiver disponível
            moneyInput.addEventListener('input', function () {
                formatCurrencyInput(this);
            });

            moneyInput.addEventListener('blur', function () {
                validateCurrencyInput(this);
            });
        }
    }

    /**
     * Formata um campo de entrada como moeda
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
     * Valida um campo de entrada de moeda
     * @param {HTMLElement} input - Campo a ser validado
     * @returns {boolean} - Resultado da validação
     */
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

    /**
     * Inicializa o toggle de recorrente
     * @param {HTMLElement} form - Formulário
     */
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

            // Inicializa com o estado atual
            if (isRecurringSwitch.checked) {
                isRecurringLabel.textContent = 'Sim';
                installmentsInput.value = '1';
                installmentsInput.disabled = true;
            }
        }
    }

    /**
     * Inicializa o toggle de status
     * @param {string} switchSelector - Seletor do switch
     * @param {string} labelSelector - Seletor do label
     */
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

        // Inicializa com o estado atual
        updateStatusLabel();

        // Atualiza quando o switch mudar
        statusSwitch.addEventListener('change', updateStatusLabel);
    }

    /**
     * Calcula parcelas para um financiamento
     * @param {number} totalAmount - Valor total do financiamento
     * @param {number} interestRate - Taxa de juros (anual)
     * @param {number} termMonths - Prazo em meses
     * @param {string} system - Sistema de amortização (PRICE, SAC)
     * @returns {Array} - Array de objetos com informações das parcelas
     */
    function calculateInstallments(totalAmount, interestRate, termMonths, system = 'PRICE') {
        if (!totalAmount || !termMonths) {
            return [];
        }

        // Converte taxa anual para mensal
        const monthlyRate = interestRate > 0 ? (interestRate / 100) / 12 : 0;
        const installments = [];

        let remainingBalance = totalAmount;
        let totalPaid = 0;
        let totalInterest = 0;

        if (system === 'PRICE') {
            // Sistema Price (parcelas fixas)
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

                // Correção para último mês (devido a arredondamentos)
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
            // Sistema SAC (amortizações fixas)
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

    /**
     * Calcula o valor futuro de um investimento
     * @param {number} presentValue - Valor presente (investimento inicial)
     * @param {number} rate - Taxa de juros (por período)
     * @param {number} periods - Número de períodos
     * @param {number} periodicContribution - Contribuição periódica (opcional)
     * @param {boolean} contributionAtBeginning - Indica se a contribuição é feita no início do período
     * @returns {number} - Valor futuro
     */
    function calculateFutureValue(presentValue, rate, periods, periodicContribution = 0, contributionAtBeginning = false) {
        rate = rate / 100; // Converte percentual para decimal

        // Cálculo do valor futuro do investimento inicial
        const futureValuePV = presentValue * Math.pow(1 + rate, periods);

        // Se não houver contribuições periódicas, retorna apenas o valor futuro do investimento inicial
        if (periodicContribution === 0) {
            return futureValuePV;
        }

        // Cálculo do valor futuro das contribuições periódicas
        let futureValuePMT;
        if (contributionAtBeginning) {
            futureValuePMT = periodicContribution * ((Math.pow(1 + rate, periods) - 1) / rate) * (1 + rate);
        } else {
            futureValuePMT = periodicContribution * ((Math.pow(1 + rate, periods) - 1) / rate);
        }

        // Retorna a soma dos dois valores futuros
        return futureValuePV + futureValuePMT;
    }

    /**
     * Calcula a taxa interna de retorno
     * @param {Array} cashflows - Array de fluxos de caixa (o primeiro é o investimento inicial, negativo)
     * @param {number} guess - Estimativa inicial (opcional)
     * @returns {number} - Taxa interna de retorno
     */
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

    /**
     * Calcula o valor presente líquido
     * @param {Array} cashflows - Array de fluxos de caixa (o primeiro é o investimento inicial, negativo)
     * @param {number} rate - Taxa de desconto
     * @returns {number} - Valor presente líquido
     */
    function calculateNPV(cashflows, rate) {
        let npv = 0;

        for (let i = 0; i < cashflows.length; i++) {
            npv += cashflows[i] / Math.pow(1 + rate, i);
        }

        return npv;
    }

    /**
     * Calcula a derivada do valor presente líquido (para cálculo da TIR)
     * @param {Array} cashflows - Array de fluxos de caixa
     * @param {number} rate - Taxa de desconto
     * @returns {number} - Derivada do NPV
     */
    function calculateNPVDerivative(cashflows, rate) {
        let derivative = 0;

        for (let i = 1; i < cashflows.length; i++) {
            derivative -= i * cashflows[i] / Math.pow(1 + rate, i + 1);
        }

        return derivative;
    }

    /**
     * Calcula o valor presente de uma série de pagamentos futuros
     * @param {number} payment - Valor do pagamento periódico
     * @param {number} rate - Taxa de juros por período
     * @param {number} periods - Número de períodos
     * @returns {number} - Valor presente
     */
    function calculatePresentValue(payment, rate, periods) {
        rate = rate / 100; // Converte percentual para decimal

        if (rate === 0) {
            return payment * periods;
        }

        return payment * ((1 - Math.pow(1 + rate, -periods)) / rate);
    }

    /**
     * Calcula o pagamento periódico para amortizar um empréstimo
     * @param {number} principal - Valor principal (valor do empréstimo)
     * @param {number} rate - Taxa de juros por período
     * @param {number} periods - Número de períodos
     * @returns {number} - Valor do pagamento periódico
     */
    function calculatePayment(principal, rate, periods) {
        rate = rate / 100; // Converte percentual para decimal

        if (rate === 0) {
            return principal / periods;
        }

        return principal * (rate * Math.pow(1 + rate, periods)) / (Math.pow(1 + rate, periods) - 1);
    }

    /**
     * Calcula o número de períodos necessários para amortizar um empréstimo
     * @param {number} principal - Valor principal (valor do empréstimo)
     * @param {number} payment - Valor do pagamento periódico
     * @param {number} rate - Taxa de juros por período
     * @returns {number} - Número de períodos
     */
    function calculatePeriods(principal, payment, rate) {
        rate = rate / 100; // Converte percentual para decimal

        if (rate === 0) {
            return principal / payment;
        }

        return Math.log(payment / (payment - principal * rate)) / Math.log(1 + rate);
    }

    /**
     * Calcula a taxa de juros necessária para amortizar um empréstimo
     * @param {number} principal - Valor principal (valor do empréstimo)
     * @param {number} payment - Valor do pagamento periódico
     * @param {number} periods - Número de períodos
     * @returns {number} - Taxa de juros por período
     */
    function calculateRate(principal, payment, periods) {
        // Esta função usa aproximação iterativa, pois não há fórmula direta
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

            // Ajusta a taxa com base na diferença
            rate = rate * (1 + diff / payment * 0.1);
        }

        return rate * 100; // Converte decimal para percentual
    }

    /**
     * Calcula o retorno sobre investimento (ROI)
     * @param {number} initialInvestment - Investimento inicial
     * @param {number} finalValue - Valor final
     * @returns {number} - ROI em percentual
     */
    function calculateROI(initialInvestment, finalValue) {
        if (initialInvestment === 0) {
            return null; // Evita divisão por zero
        }

        return ((finalValue - initialInvestment) / initialInvestment) * 100;
    }

    /**
     * Converte um valor monetário para número
     * @param {string} value - Valor em formato monetário (ex: "R$ 1.234,56")
     * @returns {number} - Valor numérico
     */
    function parseCurrency(value) {
        if (typeof value === 'number') {
            return value;
        }

        // Remove símbolos de moeda e espaços
        value = value.replace(/[^\d,.-]/g, '');

        // Trata formato brasileiro (1.234,56)
        if (value.indexOf(',') > -1 && value.indexOf('.') > -1) {
            value = value.replace(/\./g, '').replace(',', '.');
        } else if (value.indexOf(',') > -1) {
            value = value.replace(',', '.');
        }

        return parseFloat(value);
    }

    /**
     * Formata um valor como moeda brasileira
     * @param {number} value - Valor a ser formatado
     * @returns {string} - Valor formatado
     */
    function formatCurrency(value) {
        return new Intl.NumberFormat('pt-BR', {
            style: 'currency',
            currency: 'BRL'
        }).format(value);
    }

    /**
     * Formata uma porcentagem
     * @param {number} value - Valor a ser formatado
     * @param {number} decimals - Número de casas decimais
     * @returns {string} - Valor formatado
     */
    function formatPercent(value, decimals = 2) {
        return value.toFixed(decimals) + '%';
    }

    /**
     * Inicializa componentes para cartões de crédito
     */
    function initializeCreditCardComponents() {
        const progressBars = document.querySelectorAll('.credit-card-progress');

        progressBars.forEach(progressBar => {
            const percentage = progressBar.getAttribute('data-percentage') || 0;

            // Define a cor da barra com base na porcentagem
            if (percentage > 90) {
                progressBar.classList.add('bg-danger');
            } else if (percentage > 70) {
                progressBar.classList.add('bg-warning');
            } else if (percentage > 50) {
                progressBar.classList.add('bg-info');
            } else {
                progressBar.classList.add('bg-success');
            }

            // Define a largura da barra
            progressBar.style.width = `${percentage}%`;
        });

        // Atualiza exibição de limites
        updateLimitsDisplay();
    }

    /**
     * Atualiza exibição de limites de cartões de crédito
     */
    function updateLimitsDisplay() {
        const limitDisplays = document.querySelectorAll('.limit-display');

        limitDisplays.forEach(display => {
            const total = parseFloat(display.getAttribute('data-total') || 0);
            const used = parseFloat(display.getAttribute('data-used') || 0);
            const available = total - used;
            const percentage = (used / total * 100) || 0;

            // Atualiza elementos na tela
            const totalElement = display.querySelector('.total-limit');
            const availableElement = display.querySelector('.available-limit');
            const usedElement = display.querySelector('.used-limit');
            const percentageElement = display.querySelector('.used-percentage');

            if (totalElement) totalElement.textContent = formatCurrency(total);
            if (availableElement) availableElement.textContent = formatCurrency(available);
            if (usedElement) usedElement.textContent = formatCurrency(used);
            if (percentageElement) percentageElement.textContent = `${percentage.toFixed(0)}%`;

            // Atualiza barra de progresso
            const progressBar = display.querySelector('.progress-bar');
            if (progressBar) {
                progressBar.style.width = `${percentage}%`;

                // Atualiza classe baseado no uso
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

    /**
     * Calcula próximas datas de fechamento e vencimento de cartões de crédito
     */
    function calculateNextDates() {
        const cardElements = document.querySelectorAll('.credit-card-dates');

        cardElements.forEach(element => {
            const closingDay = parseInt(element.getAttribute('data-closing-day'), 10);
            const dueDay = parseInt(element.getAttribute('data-due-day'), 10);

            if (isNaN(closingDay) || isNaN(dueDay)) return;

            const today = new Date();
            let nextClosingDate = new Date(today.getFullYear(), today.getMonth(), closingDay);
            let nextDueDate = new Date(today.getFullYear(), today.getMonth(), dueDay);

            // Se a data de fechamento já passou este mês, avança para o próximo
            if (today.getDate() > closingDay) {
                nextClosingDate.setMonth(nextClosingDate.getMonth() + 1);
            }

            // Se a data de vencimento já passou este mês, avança para o próximo
            if (today.getDate() > dueDay) {
                nextDueDate.setMonth(nextDueDate.getMonth() + 1);
            }

            // Atualiza elementos na página
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

    /**
     * Formata uma data no padrão brasileiro
     * @param {Date} date - Data a ser formatada
     * @returns {string} - Data formatada
     */
    function formatDate(date) {
        return date.toLocaleDateString('pt-BR');
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

    function clickButtonReceived() {
        const markReceivedButtons = document.querySelectorAll('.mark-received-installment');
        markReceivedButtons.forEach(button => {
            button.addEventListener('click', function () {
                const installmentId = this.getAttribute('data-installment-id');
                document.getElementById('installmentId').value = installmentId;
            });
        });
    }

    // API pública do módulo
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
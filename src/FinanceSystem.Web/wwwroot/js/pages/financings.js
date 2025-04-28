/**
 * Scripts específicos para as páginas de financiamentos
 */

document.addEventListener('DOMContentLoaded', function () {
    initializeFinancingForm();
    initializeFinancingDetails();
    initializeFinancingCalculations();
});

/**
 * Inicializa formulário de financiamento
 */
function initializeFinancingForm() {
    const form = document.querySelector('form[asp-action="Create"], form[asp-action="Edit"], form[asp-action="Simulate"]');
    if (!form) return;

    // Inicializa máscaras para campos monetários
    initializeMoneyMasks();

    // Inicializa cálculos para simulação
    initializeCalculations();

    // Configura validação
    setupFormValidation(form, validateFinancingForm);
}

/**
 * Inicializa máscaras para valores monetários
 */
function initializeMoneyMasks() {
    const moneyInputs = document.querySelectorAll('.money-input, #TotalAmount, #InterestRate');

    moneyInputs.forEach(input => {
        // Remove eventos anteriores para evitar duplicação
        input.removeEventListener('input', handleMoneyInput);
        input.addEventListener('input', handleMoneyInput);

        // Formata valores existentes
        if (input.value) {
            formatMoneyInput(input);
        }
    });
}

/**
 * Manipula entrada de valores monetários
 */
function handleMoneyInput(e) {
    formatMoneyInput(e.target);
}

/**
 * Formata campo de entrada monetária
 */
function formatMoneyInput(input) {
    // Preserva a posição do cursor
    const cursorPosition = input.selectionStart;
    const inputLength = input.value.length;

    // Remove caracteres não numéricos, exceto vírgula e ponto
    let value = input.value.replace(/[^\d.,]/g, '');

    // Converte para o formato brasileiro (vírgula como separador decimal)
    if (input.id === 'InterestRate') {
        // Para taxa de juros, permite decimais com até 2 casas
        value = value.replace(/[^\d,]/g, '').replace('.', ',');
        if (value.indexOf(',') >= 0) {
            const parts = value.split(',');
            if (parts[1].length > 2) {
                parts[1] = parts[1].substring(0, 2);
                value = parts.join(',');
            }
        }
        input.value = value;
    } else {
        // Para valores monetários
        value = value.replace(/\D/g, '');
        if (value === '') {
            input.value = '';
            return;
        }

        value = (parseFloat(value) / 100).toFixed(2);
        input.value = value.replace('.', ',');
    }

    // Ajusta a posição do cursor se necessário
    const newLength = input.value.length;
    const newPosition = cursorPosition + (newLength - inputLength);
    if (newPosition >= 0) {
        input.setSelectionRange(newPosition, newPosition);
    }
}

/**
 * Inicializa cálculos para simulação
 */
function initializeCalculations() {
    const totalAmountInput = document.getElementById('TotalAmount');
    const interestRateInput = document.getElementById('InterestRate');
    const termMonthsInput = document.getElementById('TermMonths');
    const calculationButton = document.getElementById('calculateButton');

    if (calculationButton) {
        calculationButton.addEventListener('click', function (e) {
            e.preventDefault();
            calculateInstallment();
        });
    }

    // Recalcula automaticamente quando os valores mudam
    if (totalAmountInput && interestRateInput && termMonthsInput) {
        totalAmountInput.addEventListener('change', calculateInstallment);
        interestRateInput.addEventListener('change', calculateInstallment);
        termMonthsInput.addEventListener('change', calculateInstallment);
    }
}

/**
 * Calcula valor de parcela
 */
function calculateInstallment() {
    const totalAmountInput = document.getElementById('TotalAmount');
    const interestRateInput = document.getElementById('InterestRate');
    const termMonthsInput = document.getElementById('TermMonths');
    const resultElement = document.getElementById('calculationResult');
    const typeSelect = document.getElementById('Type');

    if (!totalAmountInput || !interestRateInput || !termMonthsInput || !resultElement) return;

    // Obter valores dos campos
    let totalAmount = parseFloat(totalAmountInput.value.replace(/\./g, '').replace(',', '.'));
    let interestRate = parseFloat(interestRateInput.value.replace(',', '.'));
    let termMonths = parseInt(termMonthsInput.value);
    let type = typeSelect ? typeSelect.value : 'PRICE';

    if (isNaN(totalAmount) || isNaN(interestRate) || isNaN(termMonths)) {
        resultElement.textContent = 'Preencha todos os campos corretamente';
        return;
    }

    // Converter taxa de juros anual para mensal (se necessário)
    interestRate = interestRate / 100; // Converter de percentual para decimal
    const monthlyRate = interestRate / 12;

    let installmentValue = 0;

    if (type === 'PRICE') {
        // Cálculo pelo sistema Price (parcelas fixas)
        if (interestRate === 0) {
            installmentValue = totalAmount / termMonths;
        } else {
            installmentValue = totalAmount * (monthlyRate * Math.pow(1 + monthlyRate, termMonths)) / (Math.pow(1 + monthlyRate, termMonths) - 1);
        }
    } else if (type === 'SAC') {
        // Cálculo pelo sistema SAC (amortizações fixas)
        const amortization = totalAmount / termMonths;
        const interest = totalAmount * monthlyRate;
        installmentValue = amortization + interest;

        // No SAC a primeira parcela é a maior
        resultElement.innerHTML = `
            Primeira parcela: R$ ${installmentValue.toFixed(2).replace('.', ',')}<br>
            Última parcela: R$ ${(amortization + (totalAmount / termMonths) * monthlyRate).toFixed(2).replace('.', ',')}<br>
            Amortização mensal: R$ ${amortization.toFixed(2).replace('.', ',')}
        `;
        return;
    }

    resultElement.textContent = `Valor da parcela: R$ ${installmentValue.toFixed(2).replace('.', ',')}`;
}

/**
 * Validação de formulário de financiamento
 */
function validateFinancingForm(event) {
    let isValid = true;
    const form = event.target;

    // Validar campos obrigatórios
    const requiredFields = form.querySelectorAll('[required]');
    requiredFields.forEach(field => {
        if (!field.value.trim()) {
            isValid = false;
            showFieldError(field, 'Este campo é obrigatório');
        }
    });

    // Validar campo de valor total
    const totalAmountField = form.querySelector('#TotalAmount');
    if (totalAmountField) {
        let value = totalAmountField.value.replace(/\./g, '').replace(',', '.');
        const amount = parseFloat(value);

        if (isNaN(amount) || amount <= 0) {
            isValid = false;
            showFieldError(totalAmountField, 'Informe um valor válido maior que zero');
        }
    }

    // Validar taxa de juros
    const interestRateField = form.querySelector('#InterestRate');
    if (interestRateField) {
        let value = interestRateField.value.replace(',', '.');
        const rate = parseFloat(value);

        if (isNaN(rate) || rate < 0 || rate > 100) {
            isValid = false;
            showFieldError(interestRateField, 'A taxa de juros deve estar entre 0 e 100%');
        }
    }

    // Validar prazo
    const termMonthsField = form.querySelector('#TermMonths');
    if (termMonthsField) {
        const months = parseInt(termMonthsField.value);

        if (isNaN(months) || months <= 0 || months > 600) {
            isValid = false;
            showFieldError(termMonthsField, 'O prazo deve estar entre 1 e 600 meses');
        }
    }

    return isValid;
}

/**
 * Inicializa visualização da página de detalhes
 */
function initializeFinancingDetails() {
    const detailsPage = document.querySelector('.financing-details-container');
    if (!detailsPage) return;

    // Inicializa gráficos se houver
    initializeFinancingCharts();

    // Configura ações para botões de parcelas
    const installmentButtons = document.querySelectorAll('.installment-action');
    installmentButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            const action = this.getAttribute('data-action');
            const installmentId = this.getAttribute('data-installment-id');

            if (action === 'pay') {
                openPayInstallmentModal(installmentId);
            }
        });
    });
}

/**
 * Inicializa gráficos na página de detalhes
 */
function initializeFinancingCharts() {
    const progressChart = document.getElementById('progressChart');
    if (progressChart) {
        const percentage = parseFloat(progressChart.getAttribute('data-progress') || '0');

        new Chart(progressChart, {
            type: 'doughnut',
            data: {
                datasets: [{
                    data: [percentage, 100 - percentage],
                    backgroundColor: ['rgba(54, 162, 235, 0.8)', 'rgba(220, 220, 220, 0.8)']
                }]
            },
            options: {
                cutout: '70%',
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        enabled: false
                    }
                }
            }
        });

        // Mostrar percentual no centro
        const progressLabel = document.getElementById('progressLabel');
        if (progressLabel) {
            progressLabel.textContent = `${percentage.toFixed(0)}%`;
        }
    }
}

/**
 * Abre modal para pagar parcela
 */
function openPayInstallmentModal(installmentId) {
    const modal = document.getElementById('payInstallmentModal');
    const installmentIdInput = document.getElementById('InstallmentId');

    if (modal && installmentIdInput) {
        installmentIdInput.value = installmentId;

        // Usa Bootstrap para abrir o modal
        const modalInstance = new bootstrap.Modal(modal);
        modalInstance.show();
    }
}

/**
 * Inicializa cálculos adicionais
 */
function initializeFinancingCalculations() {
    // Para a página de simulação
    const simulationForm = document.querySelector('form[asp-action="Simulate"]');
    if (simulationForm) {
        const simulateButton = document.getElementById('simulateButton');
        if (simulateButton) {
            simulateButton.addEventListener('click', function (e) {
                // Validar antes de enviar
                const totalAmountField = document.getElementById('TotalAmount');
                const interestRateField = document.getElementById('InterestRate');
                const termMonthsField = document.getElementById('TermMonths');

                if (!totalAmountField.value || !interestRateField.value || !termMonthsField.value) {
                    e.preventDefault();
                    alert('Preencha todos os campos obrigatórios');
                }
            });
        }
    }

    // Para a página de correção monetária
    const correctionForm = document.querySelector('form[asp-action="ApplyCorrection"]');
    if (correctionForm) {
        const indexValueField = document.getElementById('IndexValue');
        if (indexValueField) {
            // Formatar campo de valor do índice
            indexValueField.addEventListener('input', function () {
                let value = this.value.replace(/[^\d,]/g, '');
                if (value.indexOf(',') >= 0) {
                    const parts = value.split(',');
                    if (parts[1].length > 4) {
                        parts[1] = parts[1].substring(0, 4);
                        value = parts.join(',');
                    }
                }
                this.value = value;
            });
        }
    }
}

/**
 * Exibe mensagem de erro para um campo
 */
function showFieldError(field, message) {
    // Procura por um elemento de feedback existente
    let feedbackElement = field.nextElementSibling;
    if (!feedbackElement || !feedbackElement.classList.contains('text-danger')) {
        feedbackElement = document.createElement('div');
        feedbackElement.className = 'text-danger';
        field.parentNode.insertBefore(feedbackElement, field.nextSibling);
    }

    // Define a mensagem de erro
    feedbackElement.textContent = message;

    // Adiciona classe de inválido ao campo
    field.classList.add('is-invalid');

    // Mostra mensagem por 5 segundos e então remove
    setTimeout(() => {
        feedbackElement.textContent = '';
        field.classList.remove('is-invalid');
    }, 5000);
}
/**
 * Finance System - Financings Page
 * Scripts específicos para a página de financiamentos
 */

var FinanceSystem = FinanceSystem || {};
FinanceSystem.Pages = FinanceSystem.Pages || {};

FinanceSystem.Pages.Financings = (function () {
    function initialize() {
        const isFormView = document.querySelector('form[asp-action="Create"], form[asp-action="Edit"]');
        const isListView = document.querySelector('.financings-list');
        const isDetailsView = document.querySelector('.financing-details-container');
        const isSimulationView = document.querySelector('form[asp-action="Simulate"]');

        if (isFormView) {
            initializeFinancingForm();
        }

        if (isListView) {
            initializeFinancingsList();
        }

        if (isDetailsView) {
            initializeFinancingDetails();
        }

        if (isSimulationView) {
            initializeFinancingSimulation();
        }
    }

    function initializeFinancingForm() {
        const form = document.querySelector('form[asp-action="Create"], form[asp-action="Edit"]');
        if (!form) return;

        FinanceSystem.Modules.Forms.initializeForm(form, {
            moneyInputs: ['#TotalAmount', '#InstallmentValue'],
            validations: [
                {
                    selector: '#ClosingDay',
                    type: 'range',
                    min: 1,
                    max: 31,
                    name: 'dia de fechamento'
                },
                {
                    selector: '#TermMonths',
                    type: 'range',
                    min: 1,
                    max: 600,
                    name: 'prazo'
                }
            ],
            validate: validateFinancingForm
        });

        initializeInstallmentCalculator();
    }

    function initializeInstallmentCalculator() {
        const totalAmountInput = document.getElementById('TotalAmount');
        const interestRateInput = document.getElementById('InterestRate');
        const termMonthsInput = document.getElementById('TermMonths');
        const installmentValueInput = document.getElementById('InstallmentValue');
        const calculationButton = document.getElementById('calculateButton');
        const amortizationSystemSelect = document.getElementById('Type');

        if (calculationButton && totalAmountInput && interestRateInput && termMonthsInput && installmentValueInput) {
            calculationButton.addEventListener('click', function (e) {
                e.preventDefault();
                calculateInstallment();
            });
        }

        if (totalAmountInput && interestRateInput && termMonthsInput && installmentValueInput) {
            totalAmountInput.addEventListener('change', calculateInstallment);
            interestRateInput.addEventListener('change', calculateInstallment);
            termMonthsInput.addEventListener('change', calculateInstallment);

            if (amortizationSystemSelect) {
                amortizationSystemSelect.addEventListener('change', calculateInstallment);
            }
        }
    }

    function calculateInstallment() {
        const totalAmountInput = document.getElementById('TotalAmount');
        const interestRateInput = document.getElementById('InterestRate');
        const termMonthsInput = document.getElementById('TermMonths');
        const installmentValueInput = document.getElementById('InstallmentValue');
        const resultElement = document.getElementById('calculationResult');
        const typeSelect = document.getElementById('Type');

        if (!totalAmountInput || !interestRateInput || !termMonthsInput) return;

        let totalAmount = FinanceSystem.Core.parseCurrency(totalAmountInput.value);
        let interestRate = parsePercent(interestRateInput.value);
        let termMonths = parseInt(termMonthsInput.value);
        let type = typeSelect ? typeSelect.value : 'PRICE';

        if (isNaN(totalAmount) || isNaN(interestRate) || isNaN(termMonths) || termMonths <= 0) {
            if (resultElement) {
                resultElement.textContent = 'Preencha todos os campos corretamente';
            }
            return;
        }

        const installments = FinanceSystem.Modules.Financial.calculateInstallments(
            totalAmount, interestRate * 100, termMonths, type
        );

        if (installments && installments.length > 0) {
            installmentValueInput.value = formatNumber(installments[0].value);

            if (resultElement) {
                if (type === 'SAC') {
                    resultElement.innerHTML = `
                        Primeira parcela: R$ ${formatNumber(installments[0].value)}<br>
                        Última parcela: R$ ${formatNumber(installments[installments.length - 1].value)}<br>
                        Amortização mensal: R$ ${formatNumber(installments[0].principal)}
                    `;
                } else {
                    resultElement.textContent = `Valor da parcela: R$ ${formatNumber(installments[0].value)}`;
                }
            }
        }
    }

    function parsePercent(value) {
        return FinanceSystem.Core.parseCurrency(value) / 100;
    }

    function formatNumber(value) {
        return value.toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    function validateFinancingForm(event) {
        let isValid = true;
        const form = event.target;

        const nameField = form.querySelector('#Name');
        if (nameField && nameField.value.trim() === '') {
            isValid = false;
            FinanceSystem.Validation.showFieldError(nameField, 'O nome do financiamento é obrigatório');
        }

        const totalAmountField = form.querySelector('#TotalAmount');
        if (totalAmountField) {
            const totalAmount = FinanceSystem.Core.parseCurrency(totalAmountField.value);
            if (isNaN(totalAmount) || totalAmount <= 0) {
                isValid = false;
                FinanceSystem.Validation.showFieldError(totalAmountField, 'Informe um valor total válido');
            }
        }

        const interestRateField = form.querySelector('#InterestRate');
        if (interestRateField) {
            const interestRate = parsePercent(interestRateField.value);
            if (isNaN(interestRate) || interestRate < 0 || interestRate > 100) {
                isValid = false;
                FinanceSystem.Validation.showFieldError(interestRateField, 'A taxa de juros deve estar entre 0 e 100%');
            }
        }

        const termMonthsField = form.querySelector('#TermMonths');
        if (termMonthsField) {
            const termMonths = parseInt(termMonthsField.value);
            if (isNaN(termMonths) || termMonths <= 0 || termMonths > 600) {
                isValid = false;
                FinanceSystem.Validation.showFieldError(termMonthsField, 'O prazo deve estar entre 1 e 600 meses');
            }
        }

        const typeField = form.querySelector('#Type');
        if (typeField && typeField.value === '') {
            isValid = false;
            FinanceSystem.Validation.showFieldError(typeField, 'Selecione o tipo de financiamento');
        }

        return isValid;
    }

    function initializeFinancingsList() {
        // Initialize the financings table using the Tables module
        FinanceSystem.Modules.Tables.initializeTable('.financings-table', {
            order: [[3, 'desc']] // Ordena por data de aquisição decrescente
        });

        // Initialize delete confirmation
        const deleteButtons = document.querySelectorAll('.btn-delete-financing');
        deleteButtons.forEach(button => {
            button.addEventListener('click', function (e) {
                if (!confirm('Tem certeza que deseja excluir este financiamento? Esta ação não pode ser desfeita.')) {
                    e.preventDefault();
                }
            });
        });

        // Initialize correction modal buttons
        const correctionButtons = document.querySelectorAll('.btn-apply-correction');
        correctionButtons.forEach(button => {
            button.addEventListener('click', function () {
                const financingId = this.getAttribute('data-financing-id');
                openCorrectionModal(financingId);
            });
        });
    }

    function openCorrectionModal(financingId) {
        const financingIdInput = document.getElementById('FinancingId');
        if (financingIdInput) {
            financingIdInput.value = financingId;
            FinanceSystem.UI.showModal('correctionModal');
        }
    }

    function initializeFinancingDetails() {
        initializeCharts();

        // Initialize installments table
        FinanceSystem.Modules.Tables.initializeTable('.installments-table', {
            order: [[0, 'asc']]
        });

        initializeInstallmentActions();
    }

    function initializeCharts() {
        const progressChart = document.getElementById('progressChart');
        if (progressChart) {
            const percentage = parseFloat(progressChart.getAttribute('data-progress') || '0');

            FinanceSystem.Modules.Charts.createDoughnutChart('progressChart',
                ['Pago', 'Restante'],
                [percentage, 100 - percentage],
                {
                    cutout: '70%',
                    colors: ['rgba(28, 200, 138, 0.8)', 'rgba(220, 220, 220, 0.8)']
                }
            );

            const progressLabel = document.getElementById('progressLabel');
            if (progressLabel) {
                progressLabel.textContent = `${percentage.toFixed(0)}%`;
            }
        }

        const distributionChart = document.getElementById('distributionChart');
        if (distributionChart) {
            const principal = parseFloat(distributionChart.getAttribute('data-principal') || '0');
            const interest = parseFloat(distributionChart.getAttribute('data-interest') || '0');

            FinanceSystem.Modules.Charts.createPieChart('distributionChart',
                ['Principal', 'Juros'],
                [principal, interest],
                {
                    colors: ['rgba(78, 115, 223, 0.8)', 'rgba(231, 74, 59, 0.8)']
                }
            );
        }
    }

    function initializeInstallmentActions() {
        const payButtons = document.querySelectorAll('.pay-installment');
        payButtons.forEach(button => {
            button.addEventListener('click', function () {
                const installmentId = this.getAttribute('data-installment-id');
                openPayInstallmentModal(installmentId);
            });
        });
    }

    function openPayInstallmentModal(installmentId) {
        const installmentIdInput = document.getElementById('InstallmentId');
        if (installmentIdInput) {
            installmentIdInput.value = installmentId;
            FinanceSystem.UI.showModal('payInstallmentModal');
        }
    }

    function initializeFinancingSimulation() {
        FinanceSystem.Modules.Financial.initializeMoneyMask('#TotalAmount');

        initializeInstallmentCalculator();

        const simulateButton = document.getElementById('simulateButton');
        if (simulateButton) {
            simulateButton.addEventListener('click', function (e) {
                const totalAmountField = document.getElementById('TotalAmount');
                const interestRateField = document.getElementById('InterestRate');
                const termMonthsField = document.getElementById('TermMonths');

                if (!totalAmountField.value || !interestRateField.value || !termMonthsField.value) {
                    e.preventDefault();
                    alert('Preencha todos os campos obrigatórios');
                }
            });
        }

        initializeSimulationCharts();
    }

    function initializeSimulationCharts() {
        const amortizationChart = document.getElementById('amortizationChart');
        if (amortizationChart) {
            const labelsRaw = amortizationChart.getAttribute('data-labels');
            const principalRaw = amortizationChart.getAttribute('data-principal');
            const interestRaw = amortizationChart.getAttribute('data-interest');

            if (labelsRaw && principalRaw && interestRaw) {
                const labels = JSON.parse(labelsRaw);
                const principal = JSON.parse(principalRaw);
                const interest = JSON.parse(interestRaw);

                FinanceSystem.Modules.Charts.createGroupedBarChart('amortizationChart', labels, [
                    {
                        label: 'Principal',
                        data: principal,
                        color: 'rgba(78, 115, 223, 0.8)'
                    },
                    {
                        label: 'Juros',
                        data: interest,
                        color: 'rgba(231, 74, 59, 0.8)'
                    }
                ]);
            }
        }

        const balanceChart = document.getElementById('balanceChart');
        if (balanceChart) {
            const labelsRaw = balanceChart.getAttribute('data-labels');
            const valuesRaw = balanceChart.getAttribute('data-values');

            if (labelsRaw && valuesRaw) {
                const labels = JSON.parse(labelsRaw);
                const values = JSON.parse(valuesRaw);

                FinanceSystem.Modules.Charts.createLineChart('balanceChart', labels, values);
            }
        }

        FinanceSystem.Modules.Tables.initializeTable('.simulation-results-table', {
            pageLength: 12,
            lengthMenu: [[12, 24, 36, -1], [12, 24, 36, "Todos"]],
            dom: 'Bfrtip',
            buttons: [
                {
                    extend: 'print',
                    text: 'Imprimir',
                    className: 'btn btn-sm btn-primary'
                },
                {
                    extend: 'excel',
                    text: 'Excel',
                    className: 'btn btn-sm btn-success'
                },
                {
                    extend: 'pdf',
                    text: 'PDF',
                    className: 'btn btn-sm btn-danger'
                }
            ]
        });
    }

    return {
        initialize: initialize,
        initializeFinancingForm: initializeFinancingForm,
        initializeFinancingsList: initializeFinancingsList,
        initializeFinancingDetails: initializeFinancingDetails,
        initializeFinancingSimulation: initializeFinancingSimulation,
        calculateInstallment: calculateInstallment
    };
})();
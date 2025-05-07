/**
 * Finance System - Credit Cards Page
 * Scripts específicos para a página de cartões de crédito
 */

// Namespace global para o sistema
var FinanceSystem = FinanceSystem || {};
FinanceSystem.Pages = FinanceSystem.Pages || {};

// Módulo CreditCards
FinanceSystem.Pages.CreditCards = (function () {
    /**
     * Inicializa a página de cartões de crédito
     */
    function initialize() {
            initializeCreditCardForm();
            initializeCreditCardsList();
            initializeCreditCardDetails();
    }

    /**
     * Inicializa formulário de cartão de crédito
     */
    function initializeCreditCardForm() {
        const cardForm = document.getElementById('credit-card-form');

        // Inicializa máscaras para campos monetários
        initializeMoneyMasks();

        // Validação de campos específicos
        initializeFieldValidations();
    }

    /**
     * Inicializa máscaras para valores monetários
     */
    function initializeMoneyMasks() {
        // Usa o módulo Financial se disponível
        if (FinanceSystem.Modules && FinanceSystem.Modules.Financial) {
            FinanceSystem.Modules.Financial.initializeMoneyMask('#Limit');
            FinanceSystem.Modules.Financial.initializeMoneyMask('#AvailableLimit');
            return;
        }

        // Fallback para jQuery Mask se disponível
        if (typeof $.fn.mask !== 'undefined') {
            $('#Limit, #AvailableLimit').mask('#.##0,00', { reverse: true });
        } else {
            // Implementação manual se as bibliotecas não estiverem disponíveis
            const moneyInputs = document.querySelectorAll('#Limit, #AvailableLimit');
            moneyInputs.forEach(input => {
                input.addEventListener('input', function (e) {
                    formatCurrencyInput(this);
                });

                // Formata valor inicial se existir
                if (input.value) {
                    formatCurrencyInput(input);
                }
            });
        }
    }

    /**
     * Formata campo de entrada monetária
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
     * Inicializa validações de campos específicos
     */
    function initializeFieldValidations() {
        const closingDayField = document.getElementById('ClosingDay');
        const dueDayField = document.getElementById('DueDay');

        if (closingDayField) {
            closingDayField.addEventListener('change', function () {
                validateDayField(this, 'dia de fechamento');
            });
        }

        if (dueDayField) {
            dueDayField.addEventListener('change', function () {
                validateDayField(this, 'dia de vencimento');
            });
        }

        // Validação de limite
        const limitField = document.getElementById('Limit');
        const availableLimitField = document.getElementById('AvailableLimit');

        if (limitField && availableLimitField) {
            limitField.addEventListener('change', function () {
                // Se o limite disponível for maior que o limite total, ajusta
                const limit = parseFloat(this.value.replace(/[^\d.,]/g, '').replace(',', '.'));
                const availableLimit = parseFloat(availableLimitField.value.replace(/[^\d.,]/g, '').replace(',', '.'));

                if (!isNaN(limit) && !isNaN(availableLimit) && availableLimit > limit) {
                    availableLimitField.value = this.value;
                }
            });

            availableLimitField.addEventListener('change', function () {
                // Garante que o limite disponível não seja maior que o limite total
                const limit = parseFloat(limitField.value.replace(/[^\d.,]/g, '').replace(',', '.'));
                const availableLimit = parseFloat(this.value.replace(/[^\d.,]/g, '').replace(',', '.'));

                if (!isNaN(limit) && !isNaN(availableLimit) && availableLimit > limit) {
                    this.value = limitField.value;
                    showLimitError();
                }
            });
        }

        // Formatação de campo de dígitos do cartão
        const lastFourDigitsField = document.getElementById('LastFourDigits');
        if (lastFourDigitsField) {
            lastFourDigitsField.addEventListener('input', function () {
                // Limita a 4 dígitos numéricos
                this.value = this.value.replace(/\D/g, '').substring(0, 4);
            });
        }

        // Configura validação do formulário
        setupFormValidation();
    }

    /**
     * Valida um campo de dia do mês
     * @param {HTMLElement} field - Campo a ser validado
     * @param {string} fieldName - Nome do campo para mensagem de erro
     */
    function validateDayField(field, fieldName) {
        const value = parseInt(field.value, 10);

        if (isNaN(value) || value < 1 || value > 31) {
            if (FinanceSystem.Validation && FinanceSystem.Validation.showFieldError) {
                FinanceSystem.Validation.showFieldError(field, `Por favor, insira um ${fieldName} válido (1-31)`);
            } else {
                field.setCustomValidity(`Por favor, insira um ${fieldName} válido (1-31)`);
            }
        } else {
            field.setCustomValidity('');
        }
    }

    /**
     * Exibe mensagem de erro quando o limite disponível é maior que o limite total
     */
    function showLimitError() {
        alert('O limite disponível não pode ser maior que o limite total');
    }

    /**
     * Configura validação do formulário
     */
    function setupFormValidation() {
        const form = document.querySelector('form[asp-action="Create"], form[asp-action="Edit"]');

        // Usa o módulo de validação se disponível
        if (FinanceSystem.Validation && FinanceSystem.Validation.setupFormValidation) {
            FinanceSystem.Validation.setupFormValidation(form, validateCreditCardForm);
        } else {
            form.addEventListener('submit', function (event) {
                if (!validateCreditCardForm(event)) {
                    event.preventDefault();
                    event.stopPropagation();
                }
            });
        }
    }

    /**
     * Valida o formulário de cartão de crédito
     * @param {Event} event - Evento de submissão
     * @returns {boolean} - Resultado da validação
     */
    function validateCreditCardForm(event) {
        let isValid = true;
        const form = event.target;

        // Valida nome
        const nameField = form.querySelector('#Name');
        if (nameField && nameField.value.trim() === '') {
            isValid = false;
            showFieldError(nameField, 'O nome do cartão é obrigatório');
        }

        // Valida bandeira
        const brandField = form.querySelector('#Brand');
        if (brandField && brandField.value === '') {
            isValid = false;
            showFieldError(brandField, 'A bandeira do cartão é obrigatória');
        }

        // Valida dias de fechamento e vencimento
        const closingDayField = form.querySelector('#ClosingDay');
        if (closingDayField) {
            const value = parseInt(closingDayField.value, 10);
            if (isNaN(value) || value < 1 || value > 31) {
                isValid = false;
                showFieldError(closingDayField, 'Por favor, insira um dia de fechamento válido (1-31)');
            }
        }

        const dueDayField = form.querySelector('#DueDay');
        if (dueDayField) {
            const value = parseInt(dueDayField.value, 10);
            if (isNaN(value) || value < 1 || value > 31) {
                isValid = false;
                showFieldError(dueDayField, 'Por favor, insira um dia de vencimento válido (1-31)');
            }
        }

        // Valida limite
        const limitField = form.querySelector('#Limit');
        if (limitField) {
            const value = limitField.value.replace(/[^\d.,]/g, '').replace(',', '.');
            const limit = parseFloat(value);
            if (isNaN(limit) || limit <= 0) {
                isValid = false;
                showFieldError(limitField, 'Por favor, insira um limite válido');
            }
        }

        // Valida limite disponível
        const availableLimitField = form.querySelector('#AvailableLimit');
        const limitField2 = form.querySelector('#Limit');
        if (availableLimitField && limitField2) {
            const limit = parseFloat(limitField2.value.replace(/[^\d.,]/g, '').replace(',', '.'));
            const available = parseFloat(availableLimitField.value.replace(/[^\d.,]/g, '').replace(',', '.'));

            if (isNaN(available)) {
                isValid = false;
                showFieldError(availableLimitField, 'Por favor, insira um limite disponível válido');
            } else if (available > limit) {
                isValid = false;
                showFieldError(availableLimitField, 'O limite disponível não pode ser maior que o limite total');
            }
        }

        return isValid;
    }

    /**
     * Mostra mensagem de erro para um campo
     * @param {HTMLElement} input - Campo com erro
     * @param {string} message - Mensagem de erro
     */
    function showFieldError(input, message) {
        // Usa o módulo de validação se disponível
        if (FinanceSystem.Validation && FinanceSystem.Validation.showFieldError) {
            FinanceSystem.Validation.showFieldError(input, message);
            return;
        }

        // Fallback para exibição manual de erro
        let errorElement = input.parentElement.querySelector('.text-danger');
        if (!errorElement) {
            errorElement = document.createElement('span');
            errorElement.classList.add('text-danger');
            input.parentElement.appendChild(errorElement);
        }
        errorElement.innerText = message;
        input.classList.add('is-invalid');
    }

    /**
     * Inicializa a lista de cartões de crédito
     */
    function initializeCreditCardsList() {
        // Inicializa componentes de cartão de crédito usando o módulo Financial
        if (FinanceSystem.Modules && FinanceSystem.Modules.Financial) {
            FinanceSystem.Modules.Financial.initializeCreditCardComponents();
        } else {
            // Fallback para implementação direta
            initializeProgressBars();
            updateLimitsDisplay();
        }

        // Botões de ação
        initializeActionButtons();
    }

    /**
     * Inicializa barras de progresso para cartões
     */
    function initializeProgressBars() {
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
    }

    /**
     * Atualiza exibição de limites
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
     * Inicializa botões de ação para cartões
     */
    function initializeActionButtons() {
        // Botões de exclusão
        const deleteButtons = document.querySelectorAll('.delete-card-btn');

        deleteButtons.forEach(button => {
            button.addEventListener('click', function (e) {
                if (!confirm('Tem certeza que deseja excluir este cartão? Esta ação não pode ser desfeita.')) {
                    e.preventDefault();
                }
            });
        });
    }

    /**
     * Formata um valor como moeda brasileira
     * @param {number} value - Valor a ser formatado
     * @returns {string} - Valor formatado
     */
    function formatCurrency(value) {
        // Usa o módulo Core se disponível
        if (FinanceSystem.Core && FinanceSystem.Core.formatCurrency) {
            return FinanceSystem.Core.formatCurrency(value);
        }

        // Fallback
        return new Intl.NumberFormat('pt-BR', {
            style: 'currency',
            currency: 'BRL'
        }).format(value);
    }

    /**
     * Inicializa a página de detalhes do cartão de crédito
     */
    function initializeCreditCardDetails() {
        // Inicializa datas de fechamento e vencimento
        calculateNextDates();

        // Inicializa gráficos se existirem
        initializeCharts();

        // Inicializa lista de transações se existir
        const transactionsTable = document.querySelector('.transactions-table');
        if (transactionsTable) {
            initializeTransactionsTable();
        }
    }

    /**
     * Calcula próximas datas de fechamento e vencimento
     */
    function calculateNextDates() {
        // Usa o módulo Financial se disponível
        if (FinanceSystem.Modules && FinanceSystem.Modules.Financial) {
            FinanceSystem.Modules.Financial.calculateNextDates();
            return;
        }

        // Implementação direta como fallback
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
        // Usa o módulo Core se disponível
        if (FinanceSystem.Core && FinanceSystem.Core.formatDate) {
            return FinanceSystem.Core.formatDate(date);
        }

        // Fallback
        return date.toLocaleDateString('pt-BR');
    }

    /**
     * Inicializa gráficos na página de detalhes
     */
    function initializeCharts() {
        // Gráfico de uso do cartão
        const usageChartCanvas = document.getElementById('usageChart');
        if (usageChartCanvas) {
            const usedLimit = parseFloat(usageChartCanvas.getAttribute('data-used') || 0);
            const totalLimit = parseFloat(usageChartCanvas.getAttribute('data-limit') || 1);
            const availableLimit = totalLimit - usedLimit;

            // Usa o módulo Charts se disponível
            if (FinanceSystem.Modules && FinanceSystem.Modules.Charts) {
                FinanceSystem.Modules.Charts.createPieChart('usageChart',
                    ['Utilizado', 'Disponível'],
                    [usedLimit, availableLimit],
                    {
                        cutout: '70%',
                        colors: ['rgba(231, 74, 59, 0.8)', 'rgba(28, 200, 138, 0.8)']
                    }
                );
            } else if (typeof Chart !== 'undefined') {
                // Fallback para Chart.js diretamente
                createUsagePieChart(usageChartCanvas, usedLimit, availableLimit);
            }
        }

        // Gráfico de gastos por categoria
        const categoryChartCanvas = document.getElementById('categoryChart');
        if (categoryChartCanvas) {
            const labels = JSON.parse(categoryChartCanvas.getAttribute('data-labels') || '[]');
            const values = JSON.parse(categoryChartCanvas.getAttribute('data-values') || '[]');

            // Usa o módulo Charts se disponível
            if (FinanceSystem.Modules && FinanceSystem.Modules.Charts) {
                FinanceSystem.Modules.Charts.createPieChart('categoryChart', labels, values);
            } else if (typeof Chart !== 'undefined') {
                // Fallback para Chart.js diretamente
                createCategoryPieChart(categoryChartCanvas, labels, values);
            }
        }
    }

    /**
     * Cria gráfico de pizza para uso do limite (fallback)
     * @param {HTMLCanvasElement} canvas - Elemento canvas
     * @param {number} usedLimit - Valor utilizado
     * @param {number} availableLimit - Valor disponível
     */
    function createUsagePieChart(canvas, usedLimit, availableLimit) {
        new Chart(canvas, {
            type: 'doughnut',
            data: {
                labels: ['Utilizado', 'Disponível'],
                datasets: [{
                    data: [usedLimit, availableLimit],
                    backgroundColor: [
                        'rgba(231, 74, 59, 0.8)',
                        'rgba(28, 200, 138, 0.8)'
                    ],
                    borderColor: [
                        'rgba(231, 74, 59, 1)',
                        'rgba(28, 200, 138, 1)'
                    ],
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '70%',
                plugins: {
                    legend: {
                        position: 'bottom'
                    },
                    tooltip: {
                        callbacks: {
                            label: function (context) {
                                const value = context.raw;
                                const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                const percentage = ((value / total) * 100).toFixed(1);
                                return `${context.label}: R$ ${value.toLocaleString('pt-BR')} (${percentage}%)`;
                            }
                        }
                    }
                }
            }
        });
    }

    /**
     * Cria gráfico de pizza para categorias (fallback)
     * @param {HTMLCanvasElement} canvas - Elemento canvas
     * @param {Array} labels - Rótulos das categorias
     * @param {Array} values - Valores por categoria
     */
    function createCategoryPieChart(canvas, labels, values) {
        new Chart(canvas, {
            type: 'doughnut',
            data: {
                labels: labels,
                datasets: [{
                    data: values,
                    backgroundColor: [
                        'rgba(78, 115, 223, 0.8)',
                        'rgba(28, 200, 138, 0.8)',
                        'rgba(246, 194, 62, 0.8)',
                        'rgba(231, 74, 59, 0.8)',
                        'rgba(54, 185, 204, 0.8)',
                        'rgba(133, 135, 150, 0.8)'
                    ],
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom'
                    },
                    tooltip: {
                        callbacks: {
                            label: function (context) {
                                const value = context.raw;
                                const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                const percentage = ((value / total) * 100).toFixed(1);
                                return `${context.label}: R$ ${value.toLocaleString('pt-BR')} (${percentage}%)`;
                            }
                        }
                    }
                }
            }
        });
    }

    /**
     * Inicializa tabela de transações do cartão
     */
    function initializeTransactionsTable() {
        // Usa DataTables se disponível
        if (typeof $.fn.DataTable !== 'undefined') {
            $('.transactions-table').DataTable({
                language: {
                    url: '//cdn.datatables.net/plug-ins/1.10.25/i18n/Portuguese-Brasil.json'
                },
                responsive: true,
                pageLength: 10,
                order: [[0, 'desc']], // Ordena por data decrescente
                columnDefs: [
                    { orderable: false, targets: -1 } // Desabilita ordenação na coluna de ações
                ]
            });
        } else if (FinanceSystem.Modules && FinanceSystem.Modules.Tables) {
            // Usa o módulo Tables se DataTables não estiver disponível
            FinanceSystem.Modules.Tables.initializeTableSort();
        }

        // Eventos para botões de ação
        const deleteButtons = document.querySelectorAll('.delete-transaction');
        deleteButtons.forEach(button => {
            button.addEventListener('click', function (e) {
                if (!confirm('Tem certeza que deseja excluir esta transação?')) {
                    e.preventDefault();
                }
            });
        });
    }

    // API pública do módulo
    return {
        initialize: initialize,
        initializeCreditCardForm: initializeCreditCardForm,
        initializeCreditCardsList: initializeCreditCardsList,
        initializeCreditCardDetails: initializeCreditCardDetails,
        updateLimitsDisplay: updateLimitsDisplay,
        calculateNextDates: calculateNextDates
    };
})();
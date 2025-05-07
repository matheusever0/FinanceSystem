/**
 * Finance System - Investments Page
 * Scripts específicos para a página de investimentos
 * Com suporte a múltiplas moedas
 */

// Namespace global para o sistema
var FinanceSystem = FinanceSystem || {};
FinanceSystem.Pages = FinanceSystem.Pages || {};

// Módulo Investments
FinanceSystem.Pages.Investments = (function () {
    /**
     * Inicializa a página de investimentos
     */
    function initialize() {

        initializeInvestmentForm();
        initializeInvestmentsList();
        initializeInvestmentDetails();
        initializeTransactionForm();

    }

    /**
     * Inicializa formulário de investimento
     */
    function initializeInvestmentForm() {
        const form = document.querySelector('form[asp-action="Create"], form[asp-action="Edit"]');
        // Inicializa campos monetários
        initializeMoneyInputs();

        // Inicializa cálculo total
        initializeTotalCalculation();

        // Configura validação do formulário
        setupFormValidation(form);

        // Inicializa o campo de seleção de moeda
        initializeCurrencyField();
    }

    /**
 * Inicializa o campo de seleção de moeda
 */
    function initializeCurrencyField() {
        const typeSelect = document.getElementById('Type');
        const currencySelect = document.getElementById('Currency');

        if (typeSelect && currencySelect) {
            // Atualiza moeda com base no tipo de investimento selecionado
            typeSelect.addEventListener('change', function () {
                // Se for ações estrangeiras (tipo 4), definir moeda como USD
                if (this.value == '4') {
                    currencySelect.value = 'USD';
                } else {
                    // Caso contrário, definir como BRL (padrão para investimentos brasileiros)
                    currencySelect.value = 'BRL';
                }
            });

            // Trigger inicial para definir a moeda correta
            if (typeSelect.value) {
                typeSelect.dispatchEvent(new Event('change'));
            }
        }
    }

    /**
     * Inicializa campos monetários
     */
    function initializeMoneyInputs() {
        // Usa o módulo Financial se disponível
        if (FinanceSystem.Modules && FinanceSystem.Modules.Financial) {
            FinanceSystem.Modules.Financial.initializeMoneyMask('#InitialPrice');
            FinanceSystem.Modules.Financial.initializeMoneyMask('#CurrentPrice');
            return;
        }

        // Fallback para jQuery Mask se disponível
        if (typeof $.fn.mask !== 'undefined') {
            $('#InitialPrice, #CurrentPrice').mask('#.##0,00', { reverse: true });
        } else {
            // Implementação manual se as bibliotecas não estiverem disponíveis
            const moneyInputs = document.querySelectorAll('#InitialPrice, #CurrentPrice');
            moneyInputs.forEach(input => {
                input.addEventListener('input', function () {
                    const currency = getCurrencyFromForm();
                    formatCurrencyInput(this, currency);
                });

                // Formata valor inicial se existir
                if (input.value) {
                    const currency = getCurrencyFromForm();
                    formatCurrencyInput(input, currency);
                }
            });
        }
    }

    /**
 * Obtém a moeda selecionada no formulário
 * @returns {string} - Código da moeda (BRL, USD, etc.)
 */
    function getCurrencyFromForm() {
        const currencySelect = document.getElementById('Currency');
        return currencySelect ? currencySelect.value : 'BRL';
    }

    /**
     * Formata campo de entrada monetária
     * @param {HTMLElement} input - Campo a ser formatado
     * @param {string} currency - Código da moeda (BRL, USD, etc.)
     */
    function formatCurrencyInput(input, currency = 'BRL') {
        // Preserva a posição do cursor
        const cursorPosition = input.selectionStart;
        const inputLength = input.value.length;

        // Remove caracteres não numéricos, exceto vírgula e ponto
        let value = input.value.replace(/[^\d.,]/g, '');

        if (currency === 'BRL') {
            // Formato brasileiro (vírgula como separador decimal)
            value = value.replace(/\D/g, '');
            if (value === '') {
                input.value = '';
                return;
            }

            value = (parseFloat(value) / 100).toFixed(2);
            input.value = value.replace('.', ',');
        } else {
            // Formato americano/internacional (ponto como separador decimal)
            value = value.replace(/,/g, '');
            if (value === '') {
                input.value = '';
                return;
            }

            // Remove todos os pontos exceto o último (separador decimal)
            let parts = value.split('.');
            if (parts.length > 2) {
                value = parts[0] + '.' + parts.slice(1).join('');
            }

            if (!value.includes('.')) {
                value = value.replace(/\D/g, '');
                value = (parseFloat(value) / 100).toFixed(2);
            } else if (value.endsWith('.')) {
                // Mantém o ponto decimal se for o último caractere
                value = parseFloat(value.replace(/\.$/, '')).toFixed(0) + '.';
            } else {
                // Mantém as casas decimais conforme digitado
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

        // Ajusta a posição do cursor se necessário
        const newLength = input.value.length;
        const newPosition = cursorPosition + (newLength - inputLength);
        if (newPosition >= 0) {
            input.setSelectionRange(newPosition, newPosition);
        }
    }

    /**
     * Inicializa cálculo de valor total
     */
    function initializeTotalCalculation() {
        const quantityInput = document.getElementById('InitialQuantity');
        const priceInput = document.getElementById('InitialPrice');
        const totalValueInput = document.getElementById('totalValue');
        const currencySelect = document.getElementById('Currency');

        if (quantityInput && priceInput && totalValueInput) {
            // Função para calcular e exibir o total
            const calculateTotal = () => {
                const quantity = parseFloat(quantityInput.value) || 0;
                const currency = currencySelect ? currencySelect.value : 'BRL';
                const price = parseCurrency(priceInput.value, currency);
                const total = quantity * price;

                totalValueInput.value = formatCurrency(total, currency);
            };

            // Inicializa o cálculo
            calculateTotal();

            // Adiciona event listeners
            quantityInput.addEventListener('input', calculateTotal);
            priceInput.addEventListener('input', calculateTotal);

            // Observa mudanças no seletor de moeda, se estiver presente
            if (currencySelect) {
                currencySelect.addEventListener('change', calculateTotal);
            }
        }
    }

    /**
     * Configura validação do formulário
     * @param {HTMLFormElement} form - Formulário
     */
    function setupFormValidation(form) {
        if (!form) return;

        // Usa o módulo de validação se disponível
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

    /**
     * Valida o formulário de investimento
     * @param {Event} event - Evento de submissão
     * @returns {boolean} - Resultado da validação
     */
    function validateInvestmentForm(event) {
        let isValid = true;
        const form = event.target;

        // Valida símbolo
        const symbolInput = document.getElementById('Symbol');
        if (symbolInput && symbolInput.value.trim() === '') {
            isValid = false;
            showFieldError(symbolInput, 'O símbolo é obrigatório');
        }

        // Valida nome
        const nameInput = document.getElementById('Name');
        if (nameInput && nameInput.value.trim() === '') {
            isValid = false;
            showFieldError(nameInput, 'O nome é obrigatório');
        }

        // Valida tipo
        const typeInput = document.getElementById('InvestmentType');
        if (typeInput && typeInput.value === '') {
            isValid = false;
            showFieldError(typeInput, 'O tipo de investimento é obrigatório');
        }

        // Valida quantidade
        const quantityInput = document.getElementById('InitialQuantity');
        if (quantityInput) {
            const quantity = parseFloat(quantityInput.value);
            if (isNaN(quantity) || quantity <= 0) {
                isValid = false;
                showFieldError(quantityInput, 'A quantidade deve ser maior que zero');
            }
        }

        // Valida preço
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
     * Inicializa a lista de investimentos
     */
    function initializeInvestmentsList() {
        // Inicializa DataTables se disponível
        initializeInvestmentsTable();

        // Inicializa destaque para ganhos/perdas
        highlightPerformance();

        // Inicializa botões de ação
        initializeActionButtons();
    }

    /**
     * Inicializa DataTables para a tabela de investimentos
     */
    function initializeInvestmentsTable() {
        // Verifica se DataTables está disponível
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
            // Usa o módulo Tables se DataTables não estiver disponível
            FinanceSystem.Modules.Tables.initializeTableSort();
        } else {
            // Implementação básica de ordenação
            basicTableSort();
        }
    }

    /**
     * Implementa ordenação básica para tabelas
     */
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

    /**
     * Ordena tabela por coluna
     * @param {HTMLTableElement} table - Tabela 
     * @param {number} columnIndex - Índice da coluna
     */
    function sortTable(table, columnIndex) {
        const thead = table.querySelector('thead');
        const tbody = table.querySelector('tbody');
        const rows = Array.from(tbody.querySelectorAll('tr'));

        const header = thead.querySelectorAll('th')[columnIndex];
        const isAscending = header.classList.contains('sorting_asc');

        // Remove ordenação de todas as colunas
        thead.querySelectorAll('th').forEach(th => {
            th.classList.remove('sorting_asc', 'sorting_desc');
        });

        // Define a nova ordenação
        header.classList.add(isAscending ? 'sorting_desc' : 'sorting_asc');

        // Ordena as linhas
        rows.sort((a, b) => {
            const aValue = a.cells[columnIndex].textContent.trim();
            const bValue = b.cells[columnIndex].textContent.trim();

            // Verifica se é valor monetário
            if (aValue.includes('R$') && bValue.includes('R$')) {
                const aNum = parseCurrency(aValue);
                const bNum = parseCurrency(bValue);
                return isAscending ? aNum - bNum : bNum - aNum;
            }

            // Verifica se é valor percentual
            if (aValue.includes('%') && bValue.includes('%')) {
                const aNum = parseFloat(aValue.replace('%', '').replace(',', '.'));
                const bNum = parseFloat(bValue.replace('%', '').replace(',', '.'));
                return isAscending ? aNum - bNum : bNum - aNum;
            }

            // Ordenação padrão (texto)
            if (aValue < bValue) return isAscending ? -1 : 1;
            if (aValue > bValue) return isAscending ? 1 : -1;
            return 0;
        });

        // Reposiciona as linhas
        rows.forEach(row => tbody.appendChild(row));
    }

    /**
     * Destaca ganhos e perdas na tabela
     */
    function highlightPerformance() {
        const performanceCells = document.querySelectorAll('.performance-value');

        performanceCells.forEach(cell => {
            // Obtém o valor numérico da célula (percentual)
            const text = cell.textContent.trim();
            const value = parseFloat(text.replace('%', '').replace(',', '.'));

            if (value > 0) {
                cell.classList.add('text-success');
            } else if (value < 0) {
                cell.classList.add('text-danger');
            }
        });
    }

    /**
     * Inicializa botões de ação
     */
    function initializeActionButtons() {
        // Botões de exclusão
        const deleteButtons = document.querySelectorAll('.btn-delete-investment');
        deleteButtons.forEach(button => {
            button.addEventListener('click', function (e) {
                if (!confirm('Tem certeza que deseja excluir este investimento? Esta ação não pode ser desfeita.')) {
                    e.preventDefault();
                }
            });
        });
    }

    /**
     * Inicializa detalhes do investimento
     */
    function initializeInvestmentDetails() {
        // Inicializa gráficos
        initializePerformanceChart();

        // Inicializa tabela de transações
        initializeTransactionsTable();

        // Inicializa botões de ação
        initializeDetailActionButtons();
    }

    /**
     * Inicializa gráfico de desempenho
     */
    function initializePerformanceChart() {
        const chartCanvas = document.getElementById('performanceChart');
        if (!chartCanvas) return;

        // Obter dados do gráfico
        const investmentId = chartCanvas.getAttribute('data-investment-id');

        // Em uma implementação real, carregaria os dados do servidor
        // Aqui usamos dados de exemplo
        fetchPerformanceData(investmentId)
            .then(data => {
                // Usa o módulo Charts se disponível
                if (FinanceSystem.Modules && FinanceSystem.Modules.Charts) {
                    FinanceSystem.Modules.Charts.createLineChart('performanceChart', data.labels, data.values);
                } else if (typeof Chart !== 'undefined') {
                    // Fallback para Chart.js diretamente
                    createPerformanceChart(chartCanvas, data.labels, data.values);
                }
            })
            .catch(error => {
                console.error('Erro ao carregar dados de desempenho:', error);
            });
    }

    /**
     * Busca dados de desempenho do investimento
     * @param {string} investmentId - ID do investimento
     * @returns {Promise} - Promise com os dados
     */
    function fetchPerformanceData(investmentId) {
        // Em uma implementação real, faria uma requisição AJAX
        // Aqui geramos dados de exemplo
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

    /**
     * Gera dados de exemplo para o gráfico
     * @returns {Array} - Array de valores
     */
    function generateMockData() {
        const data = [];
        let value = 5000 + Math.random() * 2000;

        for (let i = 0; i < 12; i++) {
            // Adiciona variação para simular movimentos do mercado
            value = value * (1 + (Math.random() * 0.1 - 0.03));
            data.push(value.toFixed(2));
        }

        return data;
    }

    /**
     * Cria gráfico de desempenho (fallback)
     * @param {HTMLCanvasElement} canvas - Elemento canvas
     * @param {Array} labels - Rótulos dos meses
     * @param {Array} values - Valores do investimento
     */
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

    /**
     * Inicializa tabela de transações
     */
    function initializeTransactionsTable() {
        // Verifica se DataTables está disponível
        if (typeof $.fn.DataTable !== 'undefined') {
            $('.transactions-table').DataTable({
                language: {
                    url: '//cdn.datatables.net/plug-ins/1.10.25/i18n/Portuguese-Brasil.json'
                },
                responsive: true,
                pageLength: 10,
                order: [[0, 'desc']] // Ordena por data decrescente
            });
        } else if (FinanceSystem.Modules && FinanceSystem.Modules.Tables) {
            // Usa o módulo Tables se DataTables não estiver disponível
            FinanceSystem.Modules.Tables.initializeTableSort();
        }
    }

    /**
     * Inicializa botões de ação da página de detalhes
     */
    function initializeDetailActionButtons() {
        // Botão para adicionar transação
        const addTransactionBtn = document.getElementById('add-transaction-btn');
        if (addTransactionBtn) {
            addTransactionBtn.addEventListener('click', function () {
                // Busca o modal de transação
                const modal = document.getElementById('transactionModal');
                if (modal) {
                    // Usa o módulo UI se disponível
                    if (FinanceSystem.UI && FinanceSystem.UI.showModal) {
                        FinanceSystem.UI.showModal('transactionModal');
                    } else if (typeof bootstrap !== 'undefined') {
                        // Fallback para Bootstrap
                        const modalInstance = new bootstrap.Modal(modal);
                        modalInstance.show();
                    } else {
                        // Fallback básico
                        modal.style.display = 'block';
                    }
                }
            });
        }
    }

    /**
     * Inicializa formulário de transação
     */
    function initializeTransactionForm() {
        const form = document.querySelector('form[asp-action="AddTransaction"]');
        if (!form) return;

        // Inicializa campos monetários
        if (FinanceSystem.Modules && FinanceSystem.Modules.Financial) {
            FinanceSystem.Modules.Financial.initializeMoneyMask('#Price');
        } else {
            // Fallback
            const priceInput = document.getElementById('Price');
            if (priceInput) {
                priceInput.addEventListener('input', function () {
                    formatCurrencyInput(this);
                });
            }
        }

        // Validação de formulário
        setupFormValidation(form);

        // Inicializa comportamento por tipo de transação
        initializeTransactionTypeHandler();
    }

    /**
     * Inicializa handler para tipo de transação
     */
    function initializeTransactionTypeHandler() {
        const typeSelect = document.getElementById('InvestmentType');
        const quantityInput = document.getElementById('Quantity');
        const priceInput = document.getElementById('Price');
        const taxesInput = document.getElementById('Taxes');
        const totalValueInput = document.getElementById('totalTransactionValue');

        if (typeSelect && quantityInput && priceInput && totalValueInput) {
            // Handler para alterar campo por tipo
            typeSelect.addEventListener('change', function () {
                updateFieldsVisibility(this.value);
            });

            // Função para calcular total
            const calculateTotal = () => {
                const type = parseInt(typeSelect.value) || 0;
                const quantity = parseFloat(quantityInput.value) || 0;
                const price = parseCurrency(priceInput.value);
                const taxes = parseCurrency(taxesInput.value);

                let total = 0;

                // Calcula baseado no tipo de transação
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

            // Inicializar visibilidade dos campos
            updateFieldsVisibility(typeSelect.value);

            // Inicializar cálculo
            calculateTotal();

            // Adicionar event listeners
            typeSelect.addEventListener('change', calculateTotal);
            quantityInput.addEventListener('input', calculateTotal);
            priceInput.addEventListener('input', calculateTotal);
            taxesInput.addEventListener('input', calculateTotal);
        }
    }

    /**
     * Atualiza visibilidade dos campos baseado no tipo de transação
     * @param {string} type - Tipo de transação
     */
    function updateFieldsVisibility(type) {
        const quantityGroup = document.getElementById('Quantity').closest('.mb-3');
        const priceGroup = document.getElementById('Price').closest('.mb-3');
        const taxesGroup = document.getElementById('Taxes').closest('.mb-3');

        // Reset - torna todos visíveis
        quantityGroup.style.display = '';
        priceGroup.style.display = '';
        taxesGroup.style.display = '';

        type = parseInt(type) || 0;

        // Ajusta campos baseado no tipo
        switch (type) {
            case 3: // Dividendo
            case 6: // JCP
            case 7: // Rendimento
                // Para tipos de renda, não precisamos de quantidade
                quantityGroup.style.display = 'none';
                document.getElementById('Price').placeholder = 'Valor total recebido';
                break;
            case 4: // Split
            case 5: // Bonificação
                // Para estes tipos, apenas quantidade
                taxesGroup.style.display = 'none';
                priceGroup.style.display = 'none';
                break;
            default:
                // Para compra/venda, mostra todos os campos
                document.getElementById('Price').placeholder = 'Preço unitário';
        }
    }

    /**
     * Converte valor monetário para número
     * @param {string|number} value - Valor em formato monetário
     * @param {string} currency - Código da moeda (BRL, USD, etc.)
     * @returns {number} - Valor numérico
     */
    function parseCurrency(value, currency = 'BRL') {
        // Se já for um número, retorna diretamente
        if (typeof value === 'number') return value;

        // Se for string vazia ou undefined, retorna 0
        if (!value) return 0;

        if (currency === 'BRL') {
            // Remove símbolos de moeda, pontos e espaços
            value = value.toString().replace(/[R$\s.]/g, '').replace(',', '.');
        } else {
            // Formato americano/internacional
            value = value.toString().replace(/[$\s,]/g, '');
        }

        return parseFloat(value) || 0;
    }

    /**
     * Formata um valor como moeda
     * @param {number} value - Valor a ser formatado
     * @param {string} currency - Código da moeda (BRL, USD, etc.)
     * @returns {string} - Valor formatado
     */
    function formatCurrency(value, currency = 'BRL') {
        // Usa o módulo Core se disponível
        if (FinanceSystem.Core && FinanceSystem.Core.formatCurrency) {
            return FinanceSystem.Core.formatCurrency(value, currency);
        }

        // Fallback
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
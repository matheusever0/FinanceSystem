/**
 * Finance System - Dashboard Page
 * Scripts específicos para a página de dashboard
 */

// Namespace global para o sistema
var FinanceSystem = FinanceSystem || {};
FinanceSystem.Pages = FinanceSystem.Pages || {};

// Módulo Dashboard
FinanceSystem.Pages.Dashboard = (function () {
    /**
     * Inicializa a página de dashboard
     */
    function initialize() {
        initializeDashboardCharts();
        initializeDashboardStats();
        setupDashboardFilters();
        initializeCollapseSections();

        // Inicializar gráficos de relatórios se estiver na página de relatórios
        if (document.getElementById('monthlyComparisonChart') ||
            document.getElementById('incomeTypesPieChart') ||
            document.getElementById('incomeStatusPieChart')) {
            initializeReportCharts();
        }
    }

    /**
     * Clean up all dashboard charts
     */
    function cleanupDashboardCharts() {
        // Use our chart registry if available
        if (FinanceSystem.Modules && FinanceSystem.Modules.Charts) {
            const charts = FinanceSystem.Modules.Charts;
            if (typeof charts.destroyChartSafely === 'function') {
                charts.destroyChartSafely('monthlyExpensesChart');
                charts.destroyChartSafely('paymentTypesPieChart');
                charts.destroyChartSafely('paymentStatusPieChart');
                // Any other charts you have
            } else if (typeof charts.cleanupAllCharts === 'function') {
                // Alternative: clean up all charts at once
                charts.cleanupAllCharts();
            }
        }

        // Reset instance variables
        monthlyExpensesChart = null;
        paymentTypesPieChart = null;
        paymentStatusPieChart = null;
    }


    /**
     * Inicializa gráficos do dashboard
     */
    function initializeDashboardCharts() {
        // Gráfico de gastos mensais com comparação de receitas
        initializeMonthlyComparisonChart();

        // Outros gráficos
        initializePaymentTypesChart();
        initializePaymentStatusChart();
        initializeCreditCardChart();
    }

    /**
     * Inicializa gráficos da página de relatórios
     */
    function initializeReportCharts() {
        // Gráfico de comparação mensal (receitas x despesas)
        initializeMonthlyReportComparisonChart();

        // Gráficos de receitas
        initializeIncomeTypesPieChart();
        initializeIncomeStatusPieChart();

        // Gráficos de pagamentos já estão sendo inicializados
        initializePaymentTypesChart();
        initializePaymentStatusChart();
    }

    /**
     * Inicializa o gráfico de comparação mensal
     */
    function initializeMonthlyComparisonChart() {
        const chartCanvas = document.getElementById('monthlyExpensesChart');
        if (!chartCanvas) return;

        try {
            // Obter dados do elemento canvas ou atributos de data
            const labelsRaw = chartCanvas.getAttribute('data-labels');
            const incomeValuesRaw = chartCanvas.getAttribute('data-income-values');
            const paymentValuesRaw = chartCanvas.getAttribute('data-payment-values');

            // Parsear os dados JSON
            const labels = JSON.parse(labelsRaw);
            const incomeValues = JSON.parse(incomeValuesRaw);
            const paymentValues = JSON.parse(paymentValuesRaw);

            // Preparar conjuntos de dados para o gráfico
            const datasets = [
                {
                    label: 'Receitas',
                    data: incomeValues,
                    color: 'rgba(28, 200, 138, 0.8)' // Verde para receitas
                },
                {
                    label: 'Despesas',
                    data: paymentValues,
                    color: 'rgba(231, 74, 59, 0.8)' // Vermelho para despesas
                }
            ];

            // Criar gráfico de barras agrupadas usando o módulo Charts
            if (FinanceSystem.Modules && FinanceSystem.Modules.Charts) {
                FinanceSystem.Modules.Charts.createGroupedBarChart('monthlyExpensesChart', labels, datasets, {
                    responsive: true,
                    maintainAspectRatio: false,
                    layout: {
                        padding: {
                            left: 10,
                            right: 25,
                            top: 25,
                            bottom: 0
                        }
                    }
                });
            } else {
                // Fallback para Chart.js direto se o módulo não estiver disponível
                createMonthlyChartFallback(chartCanvas, labels, incomeValues, paymentValues);
            }
        } catch (error) {
            console.error('Erro ao inicializar gráfico de comparação mensal:', error);
        }
    }

    /**
     * Cria o gráfico de comparação mensal diretamente com Chart.js (fallback)
     * @param {HTMLCanvasElement} canvas - Elemento canvas
     * @param {Array} labels - Rótulos dos meses
     * @param {Array} incomeValues - Valores de receitas
     * @param {Array} paymentValues - Valores de despesas
     */
    function createMonthlyChartFallback(canvas, labels, incomeValues, paymentValues) {
        if (typeof Chart === 'undefined') return;

        new Chart(canvas, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: 'Receitas',
                        data: incomeValues,
                        backgroundColor: 'rgba(28, 200, 138, 0.8)', // Verde para receitas
                        borderColor: 'rgba(28, 200, 138, 1)',
                        borderWidth: 1
                    },
                    {
                        label: 'Despesas',
                        data: paymentValues,
                        backgroundColor: 'rgba(231, 74, 59, 0.8)', // Vermelho para despesas
                        borderColor: 'rgba(231, 74, 59, 1)',
                        borderWidth: 1
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                layout: {
                    padding: {
                        left: 10,
                        right: 25,
                        top: 25,
                        bottom: 0
                    }
                },
                scales: {
                    x: {
                        grid: {
                            display: false,
                            drawBorder: false
                        }
                    },
                    y: {
                        ticks: {
                            callback: function (value) {
                                return 'R$ ' + value.toLocaleString('pt-BR');
                            }
                        },
                        grid: {
                            color: "rgb(234, 236, 244)",
                            drawBorder: false,
                            borderDash: [2],
                            zeroLineBorderDash: [2]
                        }
                    }
                },
                plugins: {
                    legend: {
                        display: true,
                        position: 'top'
                    },
                    tooltip: {
                        callbacks: {
                            label: function (context) {
                                return `${context.dataset.label}: R$ ${context.raw.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}`;
                            }
                        }
                    }
                }
            }
        });
    }

    /**
     * Inicializa o gráfico de tipos de pagamento
     */
    function initializePaymentTypesChart() {
        const chartCanvas = document.getElementById('paymentTypesPieChart');
        if (!chartCanvas) return;

        // Obter dados do elemento canvas
        const labelsRaw = chartCanvas.getAttribute('data-labels');
        const valuesRaw = chartCanvas.getAttribute('data-values');

        // Parsear os dados JSON
        const labels = JSON.parse(labelsRaw);
        const values = JSON.parse(valuesRaw);

        // Criar gráfico de pizza usando o módulo Charts
        if (FinanceSystem.Modules && FinanceSystem.Modules.Charts) {
            FinanceSystem.Modules.Charts.createPieChart('paymentTypesPieChart', labels, values, {
                cutout: '70%'
            });
        } else {
            // Fallback para Chart.js direto se o módulo não estiver disponível
            createPieChartFallback(chartCanvas, labels, values);
        }
    }

    /**
     * Inicializa o gráfico de status de pagamento
     */
    function initializePaymentStatusChart() {
        const chartCanvas = document.getElementById('paymentStatusPieChart');
        if (!chartCanvas) return;

        // Obter dados do elemento canvas
        const labels = JSON.parse(chartCanvas.getAttribute('data-labels') || '[]');
        const values = JSON.parse(chartCanvas.getAttribute('data-values') || '[]');

        // Cores para cada status
        const backgroundColors = [
            'rgba(28, 200, 138, 0.8)',   // Pago
            'rgba(246, 194, 62, 0.8)',   // Pendente
            'rgba(231, 74, 59, 0.8)',    // Vencido
            'rgba(133, 135, 150, 0.8)'   // Cancelado
        ];

        // Criar gráfico de pizza usando o módulo Charts
        if (FinanceSystem.Modules && FinanceSystem.Modules.Charts) {
            FinanceSystem.Modules.Charts.createPieChart('paymentStatusPieChart', labels, values, {
                cutout: '70%',
                colors: backgroundColors
            });
        } else {
            // Fallback para Chart.js direto se o módulo não estiver disponível
            createPieChartFallback(chartCanvas, labels, values, backgroundColors);
        }
    }

    /**
     * Cria um gráfico de pizza diretamente com Chart.js (fallback)
     * @param {HTMLCanvasElement} canvas - Elemento canvas
     * @param {Array} labels - Rótulos das fatias
     * @param {Array} values - Valores
     * @param {Array} colors - Cores (opcional)
     */
    function createPieChartFallback(canvas, labels, values, colors) {
        if (typeof Chart === 'undefined') return;

        // Se não foram fornecidas cores, gera cores
        if (!colors) {
            colors = generateColors(values.length);
        }

        new Chart(canvas, {
            type: 'doughnut',
            data: {
                labels: labels,
                datasets: [{
                    data: values,
                    backgroundColor: colors,
                    hoverBackgroundColor: colors.map(color => adjustColor(color, -20)),
                    hoverBorderColor: 'rgba(234, 236, 244, 1)',
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '70%',
                plugins: {
                    legend: {
                        position: 'bottom',
                        display: true
                    },
                    tooltip: {
                        callbacks: {
                            label: function (context) {
                                const label = context.label || '';
                                const value = context.raw;
                                const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                const percentage = ((value / total) * 100).toFixed(1);
                                return `${label}: R$ ${value.toLocaleString('pt-BR')} (${percentage}%)`;
                            }
                        }
                    }
                }
            }
        });
    }

    /**
     * Inicializa gráficos de cartão de crédito
     */
    function initializeCreditCardChart() {
        const progressBars = document.querySelectorAll('.credit-card-progress');

        progressBars.forEach(progressBar => {
            const percentage = progressBar.getAttribute('data-percentage');
            const color = getProgressBarColor(percentage);

            progressBar.style.width = `${percentage}%`;
            progressBar.classList.add(`bg-${color}`);
        });
    }

    /**
     * Determina a cor de uma barra de progresso com base na porcentagem
     * @param {number} percentage - Porcentagem
     * @returns {string} - Nome da cor
     */
    function getProgressBarColor(percentage) {
        if (percentage >= 90) return 'danger';
        if (percentage >= 75) return 'warning';
        if (percentage >= 50) return 'info';
        return 'success';
    }

    /**
     * Gera cores para conjuntos de dados
     * @param {number} count - Número de cores a serem geradas
     * @returns {Array} - Array de cores
     */
    function generateColors(count) {
        const baseColors = [
            'rgba(78, 115, 223, 0.8)',
            'rgba(28, 200, 138, 0.8)',
            'rgba(246, 194, 62, 0.8)',
            'rgba(231, 74, 59, 0.8)',
            'rgba(54, 185, 204, 0.8)',
            'rgba(133, 135, 150, 0.8)'
        ];

        const colors = [];
        for (let i = 0; i < count; i++) {
            if (i < baseColors.length) {
                colors.push(baseColors[i]);
            } else {
                // Gera cores aleatórias se precisar de mais
                const r = Math.floor(Math.random() * 200) + 55;
                const g = Math.floor(Math.random() * 200) + 55;
                const b = Math.floor(Math.random() * 200) + 55;
                colors.push(`rgba(${r}, ${g}, ${b}, 0.8)`);
            }
        }

        return colors;
    }

    /**
     * Ajusta uma cor para mais claro ou mais escuro
     * @param {string} color - Cor em formato rgba
     * @param {number} amount - Quantidade de ajuste (-255 a 255)
     * @returns {string} - Cor ajustada
     */
    function adjustColor(color, amount) {
        const rgbaMatch = color.match(/rgba\((\d+),\s*(\d+),\s*(\d+),\s*([\d.]+)\)/);
        if (!rgbaMatch) return color;

        let r = parseInt(rgbaMatch[1]);
        let g = parseInt(rgbaMatch[2]);
        let b = parseInt(rgbaMatch[3]);
        const a = parseFloat(rgbaMatch[4]);

        r = Math.max(0, Math.min(255, r + amount));
        g = Math.max(0, Math.min(255, g + amount));
        b = Math.max(0, Math.min(255, b + amount));

        return `rgba(${r}, ${g}, ${b}, ${a})`;
    }

    /**
     * Inicializa estatísticas do dashboard
     */
    function initializeDashboardStats() {
        // Atualiza contadores
        updateCounters();

        // Inicializa tendências
        initializeTrends();
    }

    /**
     * Atualiza contadores com animação
     */
    function updateCounters() {
        const counters = document.querySelectorAll('.counter');

        counters.forEach(counter => {
            const target = parseFloat(counter.getAttribute('data-target'));
            const duration = parseInt(counter.getAttribute('data-duration') || '2000');
            const isDecimal = counter.getAttribute('data-decimal') === 'true';
            const isCurrency = counter.getAttribute('data-currency') === 'true';

            // Animação de contagem
            let start = 0;
            const increment = target / (duration / 16);
            const timer = setInterval(() => {
                start += increment;

                if (start >= target) {
                    start = target;
                    clearInterval(timer);
                }

                // Formata o número
                let formattedValue = '';
                if (isCurrency) {
                    formattedValue = formatCurrency(start);
                } else if (isDecimal) {
                    formattedValue = start.toFixed(2).replace('.', ',');
                } else {
                    formattedValue = Math.floor(start).toString();
                }

                counter.textContent = formattedValue;
            }, 16);
        });
    }

    /**
     * Formata um valor como moeda
     * @param {number} value - Valor
     * @returns {string} - Valor formatado
     */
    function formatCurrency(value) {
        // Usa a função do módulo Core se disponível
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
     * Inicializa indicadores de tendência
     */
    function initializeTrends() {
        const trends = document.querySelectorAll('.trend-indicator');

        trends.forEach(trend => {
            const value = parseFloat(trend.getAttribute('data-value') || '0');

            // Define classe e ícone baseado no valor
            if (value > 0) {
                trend.classList.add('text-success');
                trend.innerHTML = `<i class="fas fa-arrow-up me-1"></i>${value.toFixed(1)}%`;
            } else if (value < 0) {
                trend.classList.add('text-danger');
                trend.innerHTML = `<i class="fas fa-arrow-down me-1"></i>${Math.abs(value).toFixed(1)}%`;
            } else {
                trend.classList.add('text-muted');
                trend.innerHTML = `<i class="fas fa-minus me-1"></i>0%`;
            }
        });
    }

    /**
     * Configura filtros do dashboard
     */
    function setupDashboardFilters() {
        const filterButtons = document.querySelectorAll('.dashboard-filter');

        filterButtons.forEach(button => {
            button.addEventListener('click', function () {
                // Remove classe ativa de todos os botões
                filterButtons.forEach(btn => btn.classList.remove('active'));

                // Adiciona classe ativa ao botão clicado
                this.classList.add('active');

                // Filtra dados conforme o período selecionado
                const period = this.getAttribute('data-period');
                filterDashboardData(period);
            });
        });
    }

    /**
     * Filtra dados do dashboard por período
     * @param {string} period - Período para filtro
     */
    function filterDashboardData(period) {
        // Exibe indicador de carregamento se existir
        const loadingIndicator = document.getElementById('dashboard-loading');
        if (loadingIndicator) {
            loadingIndicator.style.display = 'block';
        }

        // Busca os dados para o período selecionado
        fetch(`/Dashboard/GetData?period=${period}`)
            .then(response => response.json())
            .then(data => {
                // Atualiza os gráficos com os novos dados
                updateDashboardCharts(data);

                // Esconde indicador de carregamento
                if (loadingIndicator) {
                    loadingIndicator.style.display = 'none';
                }
            })
            .catch(error => {
                console.error('Erro ao carregar dados:', error);

                // Esconde indicador de carregamento
                if (loadingIndicator) {
                    loadingIndicator.style.display = 'none';
                }

                // Exibe mensagem de erro
                alert('Erro ao carregar dados do dashboard. Tente novamente mais tarde.');
            });
    }

    /**
     * Atualiza os gráficos do dashboard com novos dados
     * @param {Object} data - Dados para atualização
     */
    function updateDashboardCharts(data) {
        // Atualiza o gráfico de gastos mensais
        if (data.monthlyExpenses && window.monthlyExpensesChart) {
            window.monthlyExpensesChart.data.labels = data.monthlyExpenses.labels;
            window.monthlyExpensesChart.data.datasets[0].data = data.monthlyExpenses.incomeValues;
            window.monthlyExpensesChart.data.datasets[1].data = data.monthlyExpenses.paymentValues;
            window.monthlyExpensesChart.update();
        }

        // Atualiza o gráfico de tipos de pagamento
        if (data.paymentTypes && window.paymentTypesChart) {
            window.paymentTypesChart.data.labels = data.paymentTypes.labels;
            window.paymentTypesChart.data.datasets[0].data = data.paymentTypes.values;
            window.paymentTypesChart.update();
        }

        // Atualiza o gráfico de status de pagamentos
        if (data.paymentStatus && window.paymentStatusChart) {
            window.paymentStatusChart.data.labels = data.paymentStatus.labels;
            window.paymentStatusChart.data.datasets[0].data = data.paymentStatus.values;
            window.paymentStatusChart.update();
        }

        // Atualiza estatísticas
        if (data.stats) {
            // Atualiza contadores
            document.querySelectorAll('[data-stat]').forEach(element => {
                const statName = element.getAttribute('data-stat');
                if (data.stats[statName] !== undefined) {
                    const value = data.stats[statName];
                    element.textContent = formatCurrency(value);
                }
            });

            // Atualiza tendências
            document.querySelectorAll('[data-trend]').forEach(element => {
                const trendName = element.getAttribute('data-trend');
                if (data.stats[trendName] !== undefined) {
                    const value = data.stats[trendName];

                    if (value > 0) {
                        element.innerHTML = `<i class="fas fa-arrow-up text-success me-1"></i>${value.toFixed(1)}%`;
                    } else if (value < 0) {
                        element.innerHTML = `<i class="fas fa-arrow-down text-danger me-1"></i>${Math.abs(value).toFixed(1)}%`;
                    } else {
                        element.innerHTML = `<i class="fas fa-minus text-muted me-1"></i>0%`;
                    }
                }
            });
        }
    }

    /**
     * Inicializa seções colapsáveis do dashboard
     */
    function initializeCollapseSections() {
        document.querySelectorAll('.collapse-toggle').forEach(button => {
            button.addEventListener('click', function () {
                const id = this.getAttribute('data-id');
                const url = this.getAttribute('data-url');
                const isExpanded = this.getAttribute('data-expanded') === 'true';
                const collapseElement = document.getElementById(`collapse-${id}`);
                const container = document.getElementById(`container-${id}`);

                const icon = this.querySelector('i');
                icon.classList.toggle('fa-chevron-down');
                icon.classList.toggle('fa-chevron-up');

                this.setAttribute('data-expanded', (!isExpanded).toString());
                this.setAttribute('aria-expanded', (!isExpanded).toString());

                if (!isExpanded) {
                    const contentContainer = collapseElement.querySelector('.content-container');
                    if (contentContainer.children.length === 0) {
                        loadSectionContent(id, url);
                    }

                    // Usa Bootstrap para mostrar o collapse
                    if (typeof bootstrap !== 'undefined' && bootstrap.Collapse) {
                        new bootstrap.Collapse(collapseElement, {
                            show: true
                        });
                    } else {
                        collapseElement.classList.add('show');
                    }
                } else {
                    // Usa Bootstrap para esconder o collapse
                    if (typeof bootstrap !== 'undefined' && bootstrap.Collapse) {
                        new bootstrap.Collapse(collapseElement, {
                            hide: true
                        });
                    } else {
                        collapseElement.classList.remove('show');
                    }
                }
            });
        });

        document.querySelectorAll('.reload-btn').forEach(button => {
            button.addEventListener('click', function () {
                const container = this.closest('.collapse-container');
                const id = container.id.replace('container-', '');
                const toggleButton = container.querySelector('.collapse-toggle');
                const url = toggleButton.getAttribute('data-url');

                loadSectionContent(id, url);
            });
        });

        // Inicializa seções que começam expandidas
        document.querySelectorAll('.collapse-toggle[data-expanded="true"]').forEach(button => {
            const id = button.getAttribute('data-id');
            const url = button.getAttribute('data-url');
            const collapseElement = document.getElementById(`collapse-${id}`);
            const contentContainer = collapseElement.querySelector('.content-container');

            if (contentContainer.children.length === 0) {
                loadSectionContent(id, url);
            }
        });
    }

    /**
     * Carrega conteúdo dinamicamente para uma seção
     * @param {string} id - ID da seção
     * @param {string} url - URL para carregar o conteúdo
     */
    function loadSectionContent(id, url) {
        const collapseElement = document.getElementById(`collapse-${id}`);
        const contentContainer = collapseElement.querySelector('.content-container');
        const loadingIndicator = collapseElement.querySelector('.loading-indicator');
        const errorMessage = collapseElement.querySelector('.error-message');

        contentContainer.innerHTML = '';
        contentContainer.classList.add('d-none');
        errorMessage.classList.add('d-none');
        loadingIndicator.classList.remove('d-none');

        fetch(url)
            .then(response => {
                if (!response.ok) {
                    return response.text().then(text => {
                        throw new Error(text || 'Ocorreu um erro ao carregar os dados.');
                    });
                }
                return response.text();
            })
            .then(html => {
                contentContainer.innerHTML = html;
                contentContainer.classList.remove('d-none');
                loadingIndicator.classList.add('d-none');

                // Inicializa gráficos específicos após carregar o conteúdo
                if (id === 'monthly-chart') {
                    initializeMonthlyComparisonChart();
                }
            })
            .catch(error => {
                console.error('Erro ao carregar seção:', error);

                loadingIndicator.classList.add('d-none');
                errorMessage.classList.remove('d-none');
                errorMessage.querySelector('.error-text').textContent = error.message || 'Ocorreu um erro ao carregar os dados.';
            });
    }

    /**
 * Inicializa o gráfico de comparação mensal (receitas x despesas) para relatórios
 */
    function initializeMonthlyReportComparisonChart() {
        const chartCanvas = document.getElementById('monthlyComparisonChart');
        if (!chartCanvas) return;

        try {
            // Obter dados do elemento canvas
            const labels = JSON.parse(chartCanvas.getAttribute('data-labels') || '[]');
            const incomeValues = JSON.parse(chartCanvas.getAttribute('data-income-values') || '[]');
            const expenseValues = JSON.parse(chartCanvas.getAttribute('data-payment-values') || '[]');

            // Preparar conjuntos de dados para o gráfico
            const datasets = [
                {
                    label: 'Receitas',
                    data: incomeValues,
                    color: 'rgba(28, 200, 138, 0.8)', // Verde para receitas
                    backgroundColor: 'rgba(28, 200, 138, 0.8)'
                },
                {
                    label: 'Despesas',
                    data: expenseValues,
                    color: 'rgba(231, 74, 59, 0.8)', // Vermelho para despesas
                    backgroundColor: 'rgba(231, 74, 59, 0.8)'
                }
            ];

            // Criar gráfico de barras agrupadas usando o módulo Charts
            if (FinanceSystem.Modules && FinanceSystem.Modules.Charts) {
                FinanceSystem.Modules.Charts.createGroupedBarChart('monthlyComparisonChart', labels, datasets, {
                    responsive: true,
                    maintainAspectRatio: false,
                    layout: {
                        padding: {
                            left: 10,
                            right: 25,
                            top: 25,
                            bottom: 0
                        }
                    }
                });
            } else {
                // Fallback para Chart.js direto se o módulo não estiver disponível
                createComparisonChartFallback(chartCanvas, labels, incomeValues, expenseValues);
            }
        } catch (error) {
            console.error('Erro ao inicializar gráfico de comparação mensal:', error);
        }
    }

    /**
     * Cria o gráfico de comparação como fallback
     */
    function createComparisonChartFallback(canvas, labels, incomeValues, expenseValues) {
        if (typeof Chart === 'undefined') return;

        new Chart(canvas, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: 'Receitas',
                        data: incomeValues,
                        backgroundColor: 'rgba(28, 200, 138, 0.8)', // Verde para receitas
                        borderColor: 'rgba(28, 200, 138, 1)',
                        borderWidth: 1
                    },
                    {
                        label: 'Despesas',
                        data: expenseValues,
                        backgroundColor: 'rgba(231, 74, 59, 0.8)', // Vermelho para despesas
                        borderColor: 'rgba(231, 74, 59, 1)',
                        borderWidth: 1
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                layout: {
                    padding: {
                        left: 10,
                        right: 25,
                        top: 25,
                        bottom: 0
                    }
                },
                scales: {
                    x: {
                        grid: {
                            display: false,
                            drawBorder: false
                        }
                    },
                    y: {
                        ticks: {
                            callback: function (value) {
                                return 'R$ ' + value.toLocaleString('pt-BR');
                            }
                        },
                        grid: {
                            color: "rgb(234, 236, 244)",
                            drawBorder: false,
                            borderDash: [2],
                            zeroLineBorderDash: [2]
                        }
                    }
                },
                plugins: {
                    legend: {
                        display: true,
                        position: 'top'
                    },
                    tooltip: {
                        callbacks: {
                            label: function (context) {
                                return `${context.dataset.label}: R$ ${context.raw.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}`;
                            }
                        }
                    }
                }
            }
        });
    }

    /**
     * Inicializa o gráfico de tipos de receita
     */
    function initializeIncomeTypesPieChart() {
        const chartCanvas = document.getElementById('incomeTypesPieChart');
        if (!chartCanvas) return;

        try {
            // Obter dados do elemento canvas
            const labelsRaw = chartCanvas.getAttribute('data-labels');
            const valuesRaw = chartCanvas.getAttribute('data-values');

            if (!labelsRaw || !valuesRaw) return;

            // Parsear os dados JSON
            const labels = JSON.parse(labelsRaw);
            const values = JSON.parse(valuesRaw);

            // Criar gráfico de pizza usando o módulo Charts
            if (FinanceSystem.Modules && FinanceSystem.Modules.Charts) {
                FinanceSystem.Modules.Charts.createPieChart('incomeTypesPieChart', labels, values, {
                    cutout: '70%'
                });
            } else {
                // Fallback para Chart.js direto se o módulo não estiver disponível
                createPieChartFallback(chartCanvas, labels, values, backgroundColors);
            }
        } catch (error) {
            console.error('Erro ao inicializar gráfico de tipos de receita:', error);
        }
    }

    /**
     * Inicializa o gráfico de status das receitas
     */
    function initializeIncomeStatusPieChart() {
        const chartCanvas = document.getElementById('incomeStatusPieChart');
        if (!chartCanvas) return;

        try {
            // Obter dados do elemento canvas
            const labelsRaw = chartCanvas.getAttribute('data-labels');
            const valuesRaw = chartCanvas.getAttribute('data-values');

            if (!labelsRaw || !valuesRaw) return;

            // Parsear os dados JSON
            const labels = JSON.parse(labelsRaw);
            const values = JSON.parse(valuesRaw);

            // Cores para cada status
            const backgroundColors = [
                'rgba(28, 200, 138, 0.8)',   // Recebido - Verde
                'rgba(246, 194, 62, 0.8)',   // Pendente - Amarelo
                'rgba(231, 74, 59, 0.8)',    // Vencido - Vermelho
                'rgba(133, 135, 150, 0.8)'   // Cancelado - Cinza
            ];

            // Criar gráfico de pizza usando o módulo Charts
            if (FinanceSystem.Modules && FinanceSystem.Modules.Charts) {
                FinanceSystem.Modules.Charts.createPieChart('incomeStatusPieChart', labels, values, {
                    cutout: '70%',
                    colors: backgroundColors
                });
            } else {
                // Fallback para Chart.js direto se o módulo não estiver disponível
                createPieChartFallback(chartCanvas, labels, values, backgroundColors);
            }
        } catch (error) {
            console.error('Erro ao inicializar gráfico de status de receitas:', error);
        }
    }

    // API pública do módulo
    return {
        initialize: initialize,
        initializeDashboardCharts: initializeDashboardCharts,
        initializeDashboardStats: initializeDashboardStats,
        updateDashboardCharts: updateDashboardCharts,
        filterDashboardData: filterDashboardData,
        cleanupDashboardCharts: cleanupDashboardCharts,
        initializeReportCharts: initializeReportCharts,
        initializeIncomeTypesPieChart: initializeIncomeTypesPieChart,
        initializeIncomeStatusPieChart: initializeIncomeStatusPieChart,
        initializeMonthlyReportComparisonChart: initializeMonthlyReportComparisonChart
    };
})();
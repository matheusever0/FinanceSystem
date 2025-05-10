/**
 * Finance System - Dashboard Page
 * Scripts específicos para a página de dashboard
 */

var FinanceSystem = FinanceSystem || {};
FinanceSystem.Pages = FinanceSystem.Pages || {};

FinanceSystem.Pages.Dashboard = (function () {
    function initialize() {
        initializeDashboardCharts();
        initializeDashboardStats();
        setupDashboardFilters();
        initializeCollapseSections();
        formatCurrencyValues();

        if (document.getElementById('monthlyComparisonChart') ||
            document.getElementById('incomeTypesPieChart') ||
            document.getElementById('incomeStatusPieChart')) {
            initializeReportCharts();
        }
    }

    function cleanupDashboardCharts() {
        if (FinanceSystem.Modules && FinanceSystem.Modules.Charts) {
            const charts = FinanceSystem.Modules.Charts;
            if (typeof charts.destroyChartSafely === 'function') {
                charts.destroyChartSafely('monthlyExpensesChart');
                charts.destroyChartSafely('paymentTypesPieChart');
                charts.destroyChartSafely('paymentStatusPieChart');
            } else if (typeof charts.cleanupAllCharts === 'function') {
                charts.cleanupAllCharts();
            }
        }

        monthlyExpensesChart = null;
        paymentTypesPieChart = null;
        paymentStatusPieChart = null;
    }

    function initializeDashboardCharts() {
        initializeMonthlyComparisonChart();
        initializeMonthlyAnnualComparisonChart();

        initializePaymentTypesChart();
        initializePaymentStatusChart();
        initializeCreditCardChart();
    }

    function initializeReportCharts() {
        initializeMonthlyReportComparisonChart();

        initializeIncomeTypesPieChart();
        initializeIncomeStatusPieChart();

        initializePaymentTypesChart();
        initializePaymentStatusChart();
    }

    function initializeMonthlyComparisonChart() {
        const chartCanvas = document.getElementById('monthlyExpensesChart');
        if (!chartCanvas) return;

        try {
            const labelsRaw = chartCanvas.getAttribute('data-labels');
            const incomeValuesRaw = chartCanvas.getAttribute('data-income-values');
            const paymentValuesRaw = chartCanvas.getAttribute('data-payment-values');

            const labels = JSON.parse(labelsRaw);
            const incomeValues = JSON.parse(incomeValuesRaw);
            const paymentValues = JSON.parse(paymentValuesRaw);

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
                createMonthlyChartFallback(chartCanvas, labels, incomeValues, paymentValues);
            }
        } catch (error) {
            console.error('Erro ao inicializar gráfico de comparação mensal:', error);
        }
    }

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

    function initializeMonthlyAnnualComparisonChart() {
        const chartCanvas = document.getElementById('monthlyExpensesAnnualChart');
        if (!chartCanvas) return;

        try {
            const labels = JSON.parse(chartCanvas.getAttribute('data-labels'));
            const incomeValues = JSON.parse(chartCanvas.getAttribute('data-income-values'));
            const paymentValues = JSON.parse(chartCanvas.getAttribute('data-payment-values'));
            const balanceValues = JSON.parse(chartCanvas.getAttribute('data-balance-values'));

            const datasets = [
                {
                    label: 'Receitas',
                    data: incomeValues,
                    backgroundColor: 'rgba(28, 200, 138, 0.8)',
                    borderColor: 'rgba(28, 200, 138, 1)',
                    borderWidth: 1
                },
                {
                    label: 'Despesas',
                    data: paymentValues,
                    backgroundColor: 'rgba(231, 74, 59, 0.8)',
                    borderColor: 'rgba(231, 74, 59, 1)',
                    borderWidth: 1
                },
                {
                    label: 'Saldo',
                    data: balanceValues,
                    type: 'line',
                    borderColor: 'rgba(54, 185, 204, 1)',
                    backgroundColor: 'rgba(54, 185, 204, 0.2)',
                    tension: 0.4,
                    borderWidth: 2,
                    fill: false,
                    yAxisID: 'y'
                }
            ];

            if (FinanceSystem.Modules && FinanceSystem.Modules.Charts) {
                FinanceSystem.Modules.Charts.createGroupedBarChart('monthlyExpensesAnnualChart', labels, datasets, {
                    responsive: true,
                    maintainAspectRatio: false
                });
            } else {
                createMonthlyAnnualChartFallback(chartCanvas, labels, incomeValues, paymentValues, balanceValues);
            }
        } catch (error) {
            console.error('Erro ao inicializar gráfico de comparação mensal:', error);
        }
    }

    function createMonthlyAnnualChartFallback(canvas, labels, incomeValues, paymentValues, balanceValues) {
        if (typeof Chart === 'undefined') return;

        new Chart(canvas, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: 'Receitas',
                        data: incomeValues,
                        backgroundColor: 'rgba(28, 200, 138, 0.8)',
                        borderColor: 'rgba(28, 200, 138, 1)',
                        borderWidth: 1
                    },
                    {
                        label: 'Despesas',
                        data: paymentValues,
                        backgroundColor: 'rgba(231, 74, 59, 0.8)',
                        borderColor: 'rgba(231, 74, 59, 1)',
                        borderWidth: 1
                    },
                    {
                        label: 'Saldo',
                        data: balanceValues,
                        type: 'line',
                        borderColor: 'rgba(54, 185, 204, 1)',
                        backgroundColor: 'rgba(54, 185, 204, 0.2)',
                        tension: 0.4,
                        borderWidth: 2,
                        fill: false,
                        yAxisID: 'y'
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

    function initializePaymentTypesChart() {
        const chartCanvas = document.getElementById('paymentTypesPieChart');
        if (!chartCanvas) return;

        const labelsRaw = chartCanvas.getAttribute('data-labels');
        const valuesRaw = chartCanvas.getAttribute('data-values');

        const labels = JSON.parse(labelsRaw);
        const values = JSON.parse(valuesRaw);

        if (FinanceSystem.Modules && FinanceSystem.Modules.Charts) {
            FinanceSystem.Modules.Charts.createPieChart('paymentTypesPieChart', labels, values, {
                cutout: '70%'
            });
        } else {
            createPieChartFallback(chartCanvas, labels, values);
        }
    }

    function initializePaymentStatusChart() {
        const chartCanvas = document.getElementById('paymentStatusPieChart');
        if (!chartCanvas) return;

        const labels = JSON.parse(chartCanvas.getAttribute('data-labels') || '[]');
        const values = JSON.parse(chartCanvas.getAttribute('data-values') || '[]');

        const backgroundColors = [
            'rgba(28, 200, 138, 0.8)',   // Pago
            'rgba(246, 194, 62, 0.8)',   // Pendente
            'rgba(231, 74, 59, 0.8)',    // Vencido
            'rgba(133, 135, 150, 0.8)'   // Cancelado
        ];

        if (FinanceSystem.Modules && FinanceSystem.Modules.Charts) {
            FinanceSystem.Modules.Charts.createPieChart('paymentStatusPieChart', labels, values, {
                cutout: '70%',
                colors: backgroundColors
            });
        } else {
            createPieChartFallback(chartCanvas, labels, values, backgroundColors);
        }
    }

    function createPieChartFallback(canvas, labels, values, colors) {
        if (typeof Chart === 'undefined') return;

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

    function initializeCreditCardChart() {
        const progressBars = document.querySelectorAll('.credit-card-progress');

        progressBars.forEach(progressBar => {
            const percentage = progressBar.getAttribute('data-percentage');
            const color = getProgressBarColor(percentage);

            progressBar.style.width = `${percentage}%`;
            progressBar.classList.add(`bg-${color}`);
        });
    }

    function getProgressBarColor(percentage) {
        if (percentage >= 90) return 'danger';
        if (percentage >= 75) return 'warning';
        if (percentage >= 50) return 'info';
        return 'success';
    }

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
                const r = Math.floor(Math.random() * 200) + 55;
                const g = Math.floor(Math.random() * 200) + 55;
                const b = Math.floor(Math.random() * 200) + 55;
                colors.push(`rgba(${r}, ${g}, ${b}, 0.8)`);
            }
        }

        return colors;
    }

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

    function initializeDashboardStats() {
        updateCounters();

        initializeTrends();
    }

    function updateCounters() {
        const counters = document.querySelectorAll('.counter');

        counters.forEach(counter => {
            const target = parseFloat(counter.getAttribute('data-target'));
            const duration = parseInt(counter.getAttribute('data-duration') || '2000');
            const isDecimal = counter.getAttribute('data-decimal') === 'true';
            const isCurrency = counter.getAttribute('data-currency') === 'true';

            let start = 0;
            const increment = target / (duration / 16);
            const timer = setInterval(() => {
                start += increment;

                if (start >= target) {
                    start = target;
                    clearInterval(timer);
                }

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

    function formatCurrencyValues() {
        const currencyElements = document.querySelectorAll('.currency-value');
        currencyElements.forEach(element => {
            const value = parseFloat(element.getAttribute('data-value'));
            const currency = element.getAttribute('data-currency');
            if (!isNaN(value) && currency) {
                element.textContent = formatCurrency(value, currency);
            }
        });
    }

    function formatCurrency(value, currency = 'BRL') {
        if (FinanceSystem.Core && FinanceSystem.Core.formatCurrency) {
            return FinanceSystem.Core.formatCurrency(value, currency); // Pass both parameters
        }

        return new Intl.NumberFormat('pt-BR', {
            style: 'currency',
            currency: currency
        }).format(value);
    }

    function initializeTrends() {
        const trends = document.querySelectorAll('.trend-indicator');

        trends.forEach(trend => {
            const value = parseFloat(trend.getAttribute('data-value') || '0');

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

    function setupDashboardFilters() {
        const filterButtons = document.querySelectorAll('.dashboard-filter');

        filterButtons.forEach(button => {
            button.addEventListener('click', function () {
                filterButtons.forEach(btn => btn.classList.remove('active'));

                this.classList.add('active');

                const period = this.getAttribute('data-period');
                filterDashboardData(period);
            });
        });
    }

    function filterDashboardData(period) {
        const loadingIndicator = document.getElementById('dashboard-loading');
        if (loadingIndicator) {
            loadingIndicator.style.display = 'block';
        }

        fetch(`/Dashboard/GetData?period=${period}`)
            .then(response => response.json())
            .then(data => {
                updateDashboardCharts(data);

                if (loadingIndicator) {
                    loadingIndicator.style.display = 'none';
                }
            })
            .catch(error => {
                console.error('Erro ao carregar dados:', error);

                if (loadingIndicator) {
                    loadingIndicator.style.display = 'none';
                }

                alert('Erro ao carregar dados do dashboard. Tente novamente mais tarde.');
            });
    }

    function updateDashboardCharts(data) {
        if (data.monthlyExpenses && window.monthlyExpensesChart) {
            window.monthlyExpensesChart.data.labels = data.monthlyExpenses.labels;
            window.monthlyExpensesChart.data.datasets[0].data = data.monthlyExpenses.incomeValues;
            window.monthlyExpensesChart.data.datasets[1].data = data.monthlyExpenses.paymentValues;
            window.monthlyExpensesChart.update();
        }

        if (data.paymentTypes && window.paymentTypesChart) {
            window.paymentTypesChart.data.labels = data.paymentTypes.labels;
            window.paymentTypesChart.data.datasets[0].data = data.paymentTypes.values;
            window.paymentTypesChart.update();
        }

        if (data.paymentStatus && window.paymentStatusChart) {
            window.paymentStatusChart.data.labels = data.paymentStatus.labels;
            window.paymentStatusChart.data.datasets[0].data = data.paymentStatus.values;
            window.paymentStatusChart.update();
        }

        if (data.stats) {
            document.querySelectorAll('[data-stat]').forEach(element => {
                const statName = element.getAttribute('data-stat');
                if (data.stats[statName] !== undefined) {
                    const value = data.stats[statName];
                    element.textContent = formatCurrency(value);
                }
            });

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

                    if (typeof bootstrap !== 'undefined' && bootstrap.Collapse) {
                        new bootstrap.Collapse(collapseElement, {
                            show: true
                        });
                    } else {
                        collapseElement.classList.add('show');
                    }
                } else {
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

                if (id === 'monthly-chart') {
                    initializeMonthlyComparisonChart();
                }
                else if (id === 'investment-summary') {
                    formatCurrencyValues();
                }
            })
            .catch(error => {
                console.error('Erro ao carregar seção:', error);

                loadingIndicator.classList.add('d-none');
                errorMessage.classList.remove('d-none');
                errorMessage.querySelector('.error-text').textContent = error.message || 'Ocorreu um erro ao carregar os dados.';
            });
    }

    function initializeMonthlyReportComparisonChart() {
        const chartCanvas = document.getElementById('monthlyComparisonChart');
        if (!chartCanvas) return;

        try {
            const labels = JSON.parse(chartCanvas.getAttribute('data-labels') || '[]');
            const incomeValues = JSON.parse(chartCanvas.getAttribute('data-income-values') || '[]');
            const expenseValues = JSON.parse(chartCanvas.getAttribute('data-payment-values') || '[]');

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
                createComparisonChartFallback(chartCanvas, labels, incomeValues, expenseValues);
            }
        } catch (error) {
            console.error('Erro ao inicializar gráfico de comparação mensal:', error);
        }
    }

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

    function initializeIncomeTypesPieChart() {
        const chartCanvas = document.getElementById('incomeTypesPieChart');
        if (!chartCanvas) return;

        try {
            const labelsRaw = chartCanvas.getAttribute('data-labels');
            const valuesRaw = chartCanvas.getAttribute('data-values');

            if (!labelsRaw || !valuesRaw) return;

            const labels = JSON.parse(labelsRaw);
            const values = JSON.parse(valuesRaw);

            if (FinanceSystem.Modules && FinanceSystem.Modules.Charts) {
                FinanceSystem.Modules.Charts.createPieChart('incomeTypesPieChart', labels, values, {
                    cutout: '70%'
                });
            } else {
                createPieChartFallback(chartCanvas, labels, values, backgroundColors);
            }
        } catch (error) {
            console.error('Erro ao inicializar gráfico de tipos de receita:', error);
        }
    }

    function initializeIncomeStatusPieChart() {
        const chartCanvas = document.getElementById('incomeStatusPieChart');
        if (!chartCanvas) return;

        try {
            const labelsRaw = chartCanvas.getAttribute('data-labels');
            const valuesRaw = chartCanvas.getAttribute('data-values');

            if (!labelsRaw || !valuesRaw) return;

            const labels = JSON.parse(labelsRaw);
            const values = JSON.parse(valuesRaw);

            const backgroundColors = [
                'rgba(28, 200, 138, 0.8)',   // Recebido - Verde
                'rgba(246, 194, 62, 0.8)',   // Pendente - Amarelo
                'rgba(231, 74, 59, 0.8)',    // Vencido - Vermelho
                'rgba(133, 135, 150, 0.8)'   // Cancelado - Cinza
            ];

            if (FinanceSystem.Modules && FinanceSystem.Modules.Charts) {
                FinanceSystem.Modules.Charts.createPieChart('incomeStatusPieChart', labels, values, {
                    cutout: '70%',
                    colors: backgroundColors
                });
            } else {
                createPieChartFallback(chartCanvas, labels, values, backgroundColors);
            }
        } catch (error) {
            console.error('Erro ao inicializar gráfico de status de receitas:', error);
        }
    }

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
        initializeMonthlyReportComparisonChart: initializeMonthlyReportComparisonChart,
        initializeMonthlyAnnualComparisonChart: initializeMonthlyAnnualComparisonChart,
        formatCurrencyValues: formatCurrencyValues
    };
})();
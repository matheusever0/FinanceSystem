/**
 * Scripts específicos para o dashboard
 */

document.addEventListener('DOMContentLoaded', function () {
    initializeDashboardCharts();
    initializeDashboardStats();
    setupDashboardFilters();
});

// Inicializa gráficos do dashboard
function initializeDashboardCharts() {
    // Gráfico de gastos mensais com comparação de receitas
    initializeMonthlyComparisonChart();

    // Outros gráficos
    initializePaymentTypesChart();
    initializePaymentStatusChart();
    initializeCreditCardChart();
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

        new Chart(chartCanvas, {
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
    } catch (error) {
        console.error('Erro ao inicializar gráfico de comparação mensal:', error);
    }
}

function initializePaymentTypesChart() {
    const chartCanvas = document.getElementById('paymentTypesPieChart');
    if (!chartCanvas) return;

    const labelsRaw = chartCanvas.getAttribute('data-labels');
    const valuesRaw = chartCanvas.getAttribute('data-values');

    const labels = JSON.parse(labelsRaw);
    const values = JSON.parse(valuesRaw);

    // Gerar cores
    const backgroundColors = generateColors(labels.length);

    new Chart(chartCanvas, {
        type: 'doughnut',
        data: {
            labels: labels,
            datasets: [{
                data: values,
                backgroundColor: backgroundColors,
                hoverBackgroundColor: backgroundColors.map(color => adjustColor(color, -20)),
                hoverBorderColor: 'rgba(234, 236, 244, 1)',
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
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
            },
            cutout: '70%'
        }
    });
}

function initializePaymentStatusChart() {
    const chartCanvas = document.getElementById('paymentStatusPieChart');
    if (!chartCanvas) return;

    // Obter dados do atributo data
    const labels = JSON.parse(chartCanvas.getAttribute('data-labels') || '[]');
    const values = JSON.parse(chartCanvas.getAttribute('data-values') || '[]');

    // Cores para cada status
    const backgroundColors = [
        'rgba(28, 200, 138, 0.8)',   // Pago
        'rgba(246, 194, 62, 0.8)',   // Pendente
        'rgba(231, 74, 59, 0.8)',    // Vencido
        'rgba(133, 135, 150, 0.8)'   // Cancelado
    ];

    new Chart(chartCanvas, {
        type: 'doughnut',
        data: {
            labels: labels,
            datasets: [{
                data: values,
                backgroundColor: backgroundColors,
                hoverBackgroundColor: backgroundColors.map(color => adjustColor(color, -20)),
                hoverBorderColor: 'rgba(234, 236, 244, 1)',
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
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
            },
            cutout: '70%'
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

// Inicializa estatísticas do dashboard
function initializeDashboardStats() {
    // Atualiza contadores
    updateCounters();

    // Inicializa tendências
    initializeTrends();
}

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

// Configura filtros do dashboard
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

function filterDashboardData(period) {
    // Esta função deve ser implementada conforme os requisitos específicos do dashboard
    console.log(`Filtrar por período: ${period}`);

    // Exemplo: atualizar gráficos via AJAX
    fetch(`/Dashboard/GetData?period=${period}`)
        .then(response => response.json())
        .then(data => {
            // Atualiza os gráficos com os novos dados
            updateDashboardCharts(data);
        })
        .catch(error => console.error('Erro ao carregar dados:', error));
}

function updateDashboardCharts(data) {
    // Atualiza o gráfico de gastos mensais
    if (data.monthlyExpenses && window.monthlyExpensesChart) {
        window.monthlyExpensesChart.data.labels = data.monthlyExpenses.labels;
        window.monthlyExpensesChart.data.datasets[0].data = data.monthlyExpenses.values;
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
    }
}

// Funções auxiliares
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

function getProgressBarColor(percentage) {
    if (percentage >= 90) return 'danger';
    if (percentage >= 75) return 'warning';
    if (percentage >= 50) return 'info';
    return 'success';
}

function formatCurrency(value) {
    return new Intl.NumberFormat('pt-BR', {
        style: 'currency',
        currency: 'BRL'
    }).format(value);
}
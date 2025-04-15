/**
 * Gráficos do Dashboard Financeiro
 */
document.addEventListener('DOMContentLoaded', function () {
    initializeMonthlyExpensesChart();
    initializeCreditCardChart();
    initializePaymentTypesPieChart();
});

/**
 * Inicializa o gráfico de gastos mensais
 */
function initializeMonthlyExpensesChart() {
    const chartCanvas = document.getElementById('monthlyExpensesChart');
    if (!chartCanvas) return;

    try {
        // Dados fornecidos pelo backend
        const labels = JSON.parse(chartCanvas.getAttribute('data-labels'));
        const values = JSON.parse(chartCanvas.getAttribute('data-values'));

        new Chart(chartCanvas, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Gastos Mensais',
                    data: values,
                    backgroundColor: 'rgba(78, 115, 223, 0.05)',
                    borderColor: 'rgba(78, 115, 223, 1)',
                    pointBackgroundColor: 'rgba(78, 115, 223, 1)',
                    pointBorderColor: '#fff',
                    pointHoverBackgroundColor: '#fff',
                    pointHoverBorderColor: 'rgba(78, 115, 223, 1)',
                    pointRadius: 3,
                    pointHoverRadius: 5,
                    tension: 0.3,
                    fill: true
                }]
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
                        display: false
                    },
                    tooltip: {
                        callbacks: {
                            label: function (context) {
                                return 'R$ ' + context.raw.toLocaleString('pt-BR', { minimumFractionDigits: 2 });
                            }
                        }
                    }
                }
            }
        });
    } catch (error) {
        console.error('Erro ao inicializar gráfico de gastos mensais:', error);
    }
}

/**
 * Inicializa o gráfico de tipos de pagamento
 */
function initializePaymentTypesPieChart() {
    const chartCanvas = document.getElementById('paymentTypesPieChart');
    if (!chartCanvas) return;

    try {
        const labels = JSON.parse(chartCanvas.getAttribute('data-labels'));
        const values = JSON.parse(chartCanvas.getAttribute('data-values'));
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
    } catch (error) {
        console.error('Erro ao inicializar gráfico de tipos de pagamento:', error);
    }
}

/**
 * Inicializa o gráfico de cartões de crédito
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
 * Gera cores aleatórias para gráficos
 * @param {number} count - Quantidade de cores
 * @returns {string[]} Array de cores em formato rgba
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
 * Ajusta o brilho de uma cor
 * @param {string} color - Cor em formato rgba
 * @param {number} amount - Quantidade de ajuste
 * @returns {string} Cor ajustada
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
 * Retorna a cor adequada para a barra de progresso baseada na porcentagem
 * @param {number} percentage - Porcentagem de uso
 * @returns {string} Classe de cor para a barra de progresso
 */
function getProgressBarColor(percentage) {
    if (percentage >= 90) return 'danger';
    if (percentage >= 75) return 'warning';
    if (percentage >= 50) return 'info';
    return 'success';
}
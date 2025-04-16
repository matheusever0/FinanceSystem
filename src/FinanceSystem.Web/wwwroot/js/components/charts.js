/**
 * Gráficos para o sistema Finance System
 */

// Inicialização de gráficos
function initializeChart(chartId, config) {
    const chartElement = document.getElementById(chartId);
    if (!chartElement) return null;

    return new Chart(chartElement, config);
}

// Criar gráfico de tipo linha
function createLineChart(chartId, labels, data, options = {}) {
    const defaultOptions = {
        responsive: true,
        maintainAspectRatio: false,
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
        }
    };

    const mergedOptions = { ...defaultOptions, ...options };

    const config = {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'Valor',
                data: data,
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
        options: mergedOptions
    };

    return initializeChart(chartId, config);
}

// Criar gráfico de tipo barra
function createBarChart(chartId, labels, data, options = {}) {
    const defaultOptions = {
        responsive: true,
        maintainAspectRatio: false,
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
        }
    };

    const mergedOptions = { ...defaultOptions, ...options };

    const config = {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: 'Valor',
                data: data,
                backgroundColor: 'rgba(78, 115, 223, 0.8)',
                borderColor: 'rgba(78, 115, 223, 1)',
                borderWidth: 1
            }]
        },
        options: mergedOptions
    };

    return initializeChart(chartId, config);
}

// Criar gráfico de tipo pizza/donut
function createPieChart(chartId, labels, data, options = {}) {
    const colors = generateColors(data.length);

    const defaultOptions = {
        responsive: true,
        maintainAspectRatio: false,
        cutout: '50%',
        plugins: {
            legend: {
                position: 'bottom',
                display: true
            },
            tooltip: {
                callbacks: {
                    label: function (context) {
                        const value = context.raw;
                        const total = context.chart.data.datasets[0].data.reduce((a, b) => a + b, 0);
                        const percentage = ((value / total) * 100).toFixed(1);
                        return `${context.label}: R$ ${value.toLocaleString('pt-BR')} (${percentage}%)`;
                    }
                }
            }
        }
    };

    const mergedOptions = { ...defaultOptions, ...options };

    const config = {
        type: 'doughnut',
        data: {
            labels: labels,
            datasets: [{
                data: data,
                backgroundColor: colors,
                hoverBackgroundColor: colors.map(color => adjustColor(color, -20)),
                hoverBorderColor: 'rgba(234, 236, 244, 1)',
            }]
        },
        options: mergedOptions
    };

    return initializeChart(chartId, config);
}

// Funções auxiliares para gráficos
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
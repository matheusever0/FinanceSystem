/**
 * Finance System - Charts Module
 * Funções reutilizáveis para criação e manipulação de gráficos
 * With chart registry for better chart instance management
 */

var FinanceSystem = FinanceSystem || {};
FinanceSystem.Modules = FinanceSystem.Modules || {};

FinanceSystem.Modules.Charts = (function () {
    const chartRegistry = {};

    const defaultColors = [
        'rgba(78, 115, 223, 0.8)',   // Azul
        'rgba(28, 200, 138, 0.8)',   // Verde
        'rgba(246, 194, 62, 0.8)',   // Amarelo
        'rgba(231, 74, 59, 0.8)',    // Vermelho
        'rgba(54, 185, 204, 0.8)',   // Azul claro
        'rgba(133, 135, 150, 0.8)'   // Cinza
    ];

    const statusColors = {
        success: 'rgba(28, 200, 138, 0.8)',
        warning: 'rgba(246, 194, 62, 0.8)',
        danger: 'rgba(231, 74, 59, 0.8)',
        info: 'rgba(54, 185, 204, 0.8)',
        primary: 'rgba(78, 115, 223, 0.8)',
        secondary: 'rgba(133, 135, 150, 0.8)'
    };

    function initialize() {
        if (typeof Chart === 'undefined') {
            console.warn('Chart.js não está disponível. Os gráficos não serão renderizados.');
            return;
        }

        configureChartDefaults();
    }

    function configureChartDefaults() {
        if (typeof Chart === 'undefined') return;

        Chart.defaults.font.family = "'Nunito', 'Segoe UI', 'Arial', sans-serif";
        Chart.defaults.color = '#666';
        Chart.defaults.responsive = true;

        Chart.defaults.plugins.tooltip.backgroundColor = 'rgba(0, 0, 0, 0.7)';
        Chart.defaults.plugins.tooltip.titleColor = '#fff';
        Chart.defaults.plugins.tooltip.bodyColor = '#fff';
        Chart.defaults.plugins.tooltip.borderColor = 'rgba(255, 255, 255, 0.1)';
        Chart.defaults.plugins.tooltip.borderWidth = 1;
        Chart.defaults.plugins.tooltip.padding = 10;
        Chart.defaults.plugins.tooltip.cornerRadius = 5;
    }

    function registerChart(chartId, chartInstance) {
        chartRegistry[chartId] = chartInstance;
        return chartInstance;
    }

    function getChart(chartId) {
        return chartRegistry[chartId] || null;
    }

    function destroyChart(chartId) {
        if (chartRegistry[chartId]) {
            chartRegistry[chartId].destroy();
            delete chartRegistry[chartId];
            return true;
        }
        return false;
    }

    function destroyChartSafely(chartId) {
        if (destroyChart(chartId)) return;

        const chartElement = document.getElementById(chartId);
        if (!chartElement) return;

        if (typeof Chart.getChart === 'function') {
            const chartInstance = Chart.getChart(chartElement);
            if (chartInstance) {
                chartInstance.destroy();
            }
        }
        else if (chartElement.chart) {
            chartElement.chart.destroy();
        }
    }

    function initializeChart(chartId, config) {
        const chartElement = document.getElementById(chartId);
        if (!chartElement || typeof Chart === 'undefined') return null;

        destroyChartSafely(chartId);

        const chart = new Chart(chartElement, config);

        return registerChart(chartId, chart);
    }

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

    function createMultiLineChart(chartId, labels, datasets, options = {}) {
        if (!Array.isArray(datasets) || datasets.length === 0) return null;

        const defaultOptions = {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: true,
                    position: 'top'
                },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            const label = context.dataset.label || '';
                            return label + ': R$ ' + context.raw.toLocaleString('pt-BR', { minimumFractionDigits: 2 });
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

        const formattedDatasets = datasets.map((dataset, index) => {
            const color = dataset.color || defaultColors[index % defaultColors.length];
            return {
                label: dataset.label || `Dataset ${index + 1}`,
                data: dataset.data || [],
                backgroundColor: dataset.fill ? adjustOpacity(color, 0.05) : 'transparent',
                borderColor: color,
                pointBackgroundColor: color,
                pointBorderColor: '#fff',
                pointHoverBackgroundColor: '#fff',
                pointHoverBorderColor: color,
                pointRadius: 3,
                pointHoverRadius: 5,
                tension: 0.3,
                fill: dataset.fill || false
            };
        });

        const config = {
            type: 'line',
            data: {
                labels: labels,
                datasets: formattedDatasets
            },
            options: mergedOptions
        };

        return initializeChart(chartId, config);
    }

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

    function createGroupedBarChart(chartId, labels, datasets, options = {}) {
        if (!Array.isArray(datasets) || datasets.length === 0) return null;

        const defaultOptions = {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: true,
                    position: 'top'
                },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            const label = context.dataset.label || '';
                            return label + ': R$ ' + context.raw.toLocaleString('pt-BR', { minimumFractionDigits: 2 });
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

        const formattedDatasets = datasets.map((dataset, index) => {
            const color = dataset.color || defaultColors[index % defaultColors.length];
            return {
                label: dataset.label || `Dataset ${index + 1}`,
                data: dataset.data || [],
                backgroundColor: color,
                borderColor: adjustOpacity(color, 1),
                borderWidth: 1
            };
        });

        const config = {
            type: 'bar',
            data: {
                labels: labels,
                datasets: formattedDatasets
            },
            options: mergedOptions
        };

        return initializeChart(chartId, config);
    }

    function createPieChart(chartId, labels, data, options = {}) {
        const colors = options.colors || generateColors(data.length);

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

    function createDoughnutChart(chartId, labels, data, options = {}) {
        // Explicitly set cutout to make it a doughnut
        const doughnutOptions = {
            ...options,
            cutout: options.cutout || '70%'
        };

        return createPieChart(chartId, labels, data, doughnutOptions);
    }

    function generateColors(count) {
        const colors = [];
        for (let i = 0; i < count; i++) {
            if (i < defaultColors.length) {
                colors.push(defaultColors[i]);
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

    function adjustOpacity(color, opacity) {
        const rgbaMatch = color.match(/rgba\((\d+),\s*(\d+),\s*(\d+),\s*([\d.]+)\)/);
        if (rgbaMatch) {
            return `rgba(${rgbaMatch[1]}, ${rgbaMatch[2]}, ${rgbaMatch[3]}, ${opacity})`;
        }

        const rgbMatch = color.match(/rgb\((\d+),\s*(\d+),\s*(\d+)\)/);
        if (rgbMatch) {
            return `rgba(${rgbMatch[1]}, ${rgbMatch[2]}, ${rgbMatch[3]}, ${opacity})`;
        }

        const hexMatch = color.match(/#([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})/i);
        if (hexMatch) {
            const r = parseInt(hexMatch[1], 16);
            const g = parseInt(hexMatch[2], 16);
            const b = parseInt(hexMatch[3], 16);
            return `rgba(${r}, ${g}, ${b}, ${opacity})`;
        }

        return color;
    }

    function updateChartData(chart, labels, data, animate = true) {
        if (!chart) return;

        chart.data.labels = labels;

        if (Array.isArray(data)) {
            if (chart.data.datasets.length > 0) {
                chart.data.datasets[0].data = data;
            }
        } else if (typeof data === 'object') {
            Object.keys(data).forEach((key, index) => {
                if (chart.data.datasets.length > index) {
                    chart.data.datasets[index].data = data[key];
                }
            });
        }

        chart.update(animate ? undefined : 'none');
    }

    function addDataset(chart, dataset, update = true) {
        if (!chart) return;

        const color = dataset.color || defaultColors[chart.data.datasets.length % defaultColors.length];

        const newDataset = {
            label: dataset.label || `Dataset ${chart.data.datasets.length + 1}`,
            data: dataset.data || [],
            backgroundColor: dataset.backgroundColor || color,
            borderColor: dataset.borderColor || adjustOpacity(color, 1),
            borderWidth: dataset.borderWidth || 1
        };

        if (chart.config.type === 'line') {
            newDataset.pointBackgroundColor = dataset.pointBackgroundColor || color;
            newDataset.pointBorderColor = dataset.pointBorderColor || '#fff';
            newDataset.pointHoverBackgroundColor = dataset.pointHoverBackgroundColor || '#fff';
            newDataset.pointHoverBorderColor = dataset.pointHoverBorderColor || color;
            newDataset.pointRadius = dataset.pointRadius || 3;
            newDataset.pointHoverRadius = dataset.pointHoverRadius || 5;
            newDataset.tension = dataset.tension || 0.3;
            newDataset.fill = dataset.fill || false;
        }

        chart.data.datasets.push(newDataset);

        if (update) {
            chart.update();
        }
    }

    function removeDataset(chart, index, update = true) {
        if (!chart || index < 0 || index >= chart.data.datasets.length) return;

        chart.data.datasets.splice(index, 1);

        if (update) {
            chart.update();
        }
    }

    function cleanupAllCharts() {
        Object.keys(chartRegistry).forEach(chartId => {
            destroyChart(chartId);
        });
    }

    return {
        initialize: initialize,
        createLineChart: createLineChart,
        createMultiLineChart: createMultiLineChart,
        createBarChart: createBarChart,
        createGroupedBarChart: createGroupedBarChart,
        createPieChart: createPieChart,
        createDoughnutChart: createDoughnutChart,
        updateChartData: updateChartData,
        addDataset: addDataset,
        removeDataset: removeDataset,
        generateColors: generateColors,
        adjustColor: adjustColor,
        adjustOpacity: adjustOpacity,
        getChart: getChart,
        destroyChart: destroyChart,
        destroyChartSafely: destroyChartSafely,
        cleanupAllCharts: cleanupAllCharts
    };
})();
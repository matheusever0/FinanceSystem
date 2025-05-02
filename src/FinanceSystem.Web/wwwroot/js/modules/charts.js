/**
 * Finance System - Charts Module
 * Funções reutilizáveis para criação e manipulação de gráficos
 * With chart registry for better chart instance management
 */

// Namespace global para o sistema
var FinanceSystem = FinanceSystem || {};
FinanceSystem.Modules = FinanceSystem.Modules || {};

// Módulo Charts
FinanceSystem.Modules.Charts = (function () {
    // Registry to store chart instances
    const chartRegistry = {};

    // Cores padrão para gráficos
    const defaultColors = [
        'rgba(78, 115, 223, 0.8)',   // Azul
        'rgba(28, 200, 138, 0.8)',   // Verde
        'rgba(246, 194, 62, 0.8)',   // Amarelo
        'rgba(231, 74, 59, 0.8)',    // Vermelho
        'rgba(54, 185, 204, 0.8)',   // Azul claro
        'rgba(133, 135, 150, 0.8)'   // Cinza
    ];

    // Cores para estados específicos
    const statusColors = {
        success: 'rgba(28, 200, 138, 0.8)',
        warning: 'rgba(246, 194, 62, 0.8)',
        danger: 'rgba(231, 74, 59, 0.8)',
        info: 'rgba(54, 185, 204, 0.8)',
        primary: 'rgba(78, 115, 223, 0.8)',
        secondary: 'rgba(133, 135, 150, 0.8)'
    };

    /**
     * Inicializa o módulo de gráficos
     */
    function initialize() {
        // Verificar se Chart.js está disponível
        if (typeof Chart === 'undefined') {
            console.warn('Chart.js não está disponível. Os gráficos não serão renderizados.');
            return;
        }

        // Configurações globais para Chart.js
        configureChartDefaults();
    }

    /**
     * Configura padrões globais para o Chart.js
     */
    function configureChartDefaults() {
        if (typeof Chart === 'undefined') return;

        // Configurações padrão para todos os gráficos
        Chart.defaults.font.family = "'Nunito', 'Segoe UI', 'Arial', sans-serif";
        Chart.defaults.color = '#666';
        Chart.defaults.responsive = true;

        // Configurações padrão para tooltips
        Chart.defaults.plugins.tooltip.backgroundColor = 'rgba(0, 0, 0, 0.7)';
        Chart.defaults.plugins.tooltip.titleColor = '#fff';
        Chart.defaults.plugins.tooltip.bodyColor = '#fff';
        Chart.defaults.plugins.tooltip.borderColor = 'rgba(255, 255, 255, 0.1)';
        Chart.defaults.plugins.tooltip.borderWidth = 1;
        Chart.defaults.plugins.tooltip.padding = 10;
        Chart.defaults.plugins.tooltip.cornerRadius = 5;
    }

    /**
     * Register a chart instance in the registry
     * @param {string} chartId - ID of the chart element
     * @param {Chart} chartInstance - Chart.js instance
     * @returns {Chart} - The registered chart instance
     */
    function registerChart(chartId, chartInstance) {
        chartRegistry[chartId] = chartInstance;
        return chartInstance;
    }

    /**
     * Get a chart instance from the registry
     * @param {string} chartId - ID of the chart element
     * @returns {Chart|null} - Chart instance or null if not found
     */
    function getChart(chartId) {
        return chartRegistry[chartId] || null;
    }

    /**
     * Destroy a chart instance and remove it from the registry
     * @param {string} chartId - ID of the chart element
     * @returns {boolean} - True if chart was destroyed, false if not found
     */
    function destroyChart(chartId) {
        if (chartRegistry[chartId]) {
            chartRegistry[chartId].destroy();
            delete chartRegistry[chartId];
            return true;
        }
        return false;
    }

    /**
     * Safely destroy a chart instance using Chart.js API
     * Works with both Chart.js v2 and v3+
     * @param {string} chartId - ID of the chart element
     */
    function destroyChartSafely(chartId) {
        // First try from our registry
        if (destroyChart(chartId)) return;

        // If not in registry, try Chart.js methods
        const chartElement = document.getElementById(chartId);
        if (!chartElement) return;

        // For Chart.js v3+
        if (typeof Chart.getChart === 'function') {
            const chartInstance = Chart.getChart(chartElement);
            if (chartInstance) {
                chartInstance.destroy();
            }
        }
        // For Chart.js v2
        else if (chartElement.chart) {
            chartElement.chart.destroy();
        }
    }

    /**
     * Inicializa um gráfico com a configuração fornecida
     * @param {string} chartId - ID do elemento canvas
     * @param {Object} config - Configuração para o gráfico
     * @returns {Chart|null} - Instância do gráfico ou null
     */
    function initializeChart(chartId, config) {
        const chartElement = document.getElementById(chartId);
        if (!chartElement || typeof Chart === 'undefined') return null;

        // Destroy any existing chart on this canvas
        destroyChartSafely(chartId);

        // Create new chart instance
        const chart = new Chart(chartElement, config);

        // Register chart in our registry
        return registerChart(chartId, chart);
    }

    /**
     * Cria um gráfico de tipo linha
     * @param {string} chartId - ID do elemento canvas
     * @param {Array} labels - Rótulos para o eixo X
     * @param {Array} data - Dados para o gráfico
     * @param {Object} options - Opções adicionais
     * @returns {Chart|null} - Instância do gráfico ou null
     */
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

    /**
     * Cria um gráfico de tipo linha com múltiplos conjuntos de dados
     * @param {string} chartId - ID do elemento canvas
     * @param {Array} labels - Rótulos para o eixo X
     * @param {Array} datasets - Conjuntos de dados
     * @param {Object} options - Opções adicionais
     * @returns {Chart|null} - Instância do gráfico ou null
     */
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

        // Formata os conjuntos de dados
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

    /**
     * Cria um gráfico de tipo barra
     * @param {string} chartId - ID do elemento canvas
     * @param {Array} labels - Rótulos para o eixo X
     * @param {Array} data - Dados para o gráfico
     * @param {Object} options - Opções adicionais
     * @returns {Chart|null} - Instância do gráfico ou null
     */
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

    /**
     * Cria um gráfico de barras agrupadas
     * @param {string} chartId - ID do elemento canvas
     * @param {Array} labels - Rótulos para o eixo X
     * @param {Array} datasets - Conjuntos de dados para as barras
     * @param {Object} options - Opções adicionais
     * @returns {Chart|null} - Instância do gráfico ou null
     */
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

        // Formata os conjuntos de dados
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

    /**
     * Cria um gráfico de tipo pizza/donut
     * @param {string} chartId - ID do elemento canvas
     * @param {Array} labels - Rótulos para as fatias
     * @param {Array} data - Dados para o gráfico
     * @param {Object} options - Opções adicionais
     * @returns {Chart|null} - Instância do gráfico ou null
     */
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

    /**
     * Cria um gráfico de tipo área polar
     * @param {string} chartId - ID do elemento canvas
     * @param {Array} labels - Rótulos para o eixo X
     * @param {Array} data - Dados para o gráfico
     * @param {Object} options - Opções adicionais
     * @returns {Chart|null} - Instância do gráfico ou null
     */
    function createPolarAreaChart(chartId, labels, data, options = {}) {
        const colors = generateColors(data.length);

        const defaultOptions = {
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
                            const value = context.raw;
                            return `${context.label}: ${value.toLocaleString('pt-BR')}`;
                        }
                    }
                }
            },
            scales: {
                r: {
                    ticks: {
                        backdropColor: 'transparent'
                    }
                }
            }
        };

        const mergedOptions = { ...defaultOptions, ...options };

        const config = {
            type: 'polarArea',
            data: {
                labels: labels,
                datasets: [{
                    data: data,
                    backgroundColor: colors,
                    borderColor: colors.map(color => adjustColor(color, 20)),
                    borderWidth: 1
                }]
            },
            options: mergedOptions
        };

        return initializeChart(chartId, config);
    }

    /**
     * Cria um gráfico de radar
     * @param {string} chartId - ID do elemento canvas
     * @param {Array} labels - Rótulos para os eixos
     * @param {Array} datasets - Conjuntos de dados
     * @param {Object} options - Opções adicionais
     * @returns {Chart|null} - Instância do gráfico ou null
     */
    function createRadarChart(chartId, labels, datasets, options = {}) {
        if (!Array.isArray(datasets) || datasets.length === 0) return null;

        const defaultOptions = {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'top',
                    display: true
                }
            },
            scales: {
                r: {
                    angleLines: {
                        display: true,
                        color: 'rgba(0,0,0,0.1)'
                    },
                    grid: {
                        color: 'rgba(0,0,0,0.1)'
                    }
                }
            }
        };

        const mergedOptions = { ...defaultOptions, ...options };

        // Formata os conjuntos de dados
        const formattedDatasets = datasets.map((dataset, index) => {
            const color = dataset.color || defaultColors[index % defaultColors.length];
            return {
                label: dataset.label || `Dataset ${index + 1}`,
                data: dataset.data || [],
                backgroundColor: adjustOpacity(color, 0.2),
                borderColor: color,
                pointBackgroundColor: color,
                pointBorderColor: '#fff',
                pointHoverBackgroundColor: '#fff',
                pointHoverBorderColor: color,
                pointRadius: 3,
                pointHoverRadius: 5
            };
        });

        const config = {
            type: 'radar',
            data: {
                labels: labels,
                datasets: formattedDatasets
            },
            options: mergedOptions
        };

        return initializeChart(chartId, config);
    }

    /**
     * Cria um gráfico de gauge
     * @param {string} chartId - ID do elemento canvas
     * @param {number} value - Valor para o medidor (0-100)
     * @param {Object} options - Opções adicionais
     * @returns {Chart|null} - Instância do gráfico ou null
     */
    function createGaugeChart(chartId, value, options = {}) {
        // Limita o valor entre 0 e 100
        value = Math.max(0, Math.min(100, value));

        // Determina a cor do gauge com base no valor
        let color = statusColors.success;
        if (value > 90) {
            color = statusColors.danger;
        } else if (value > 75) {
            color = statusColors.warning;
        } else if (value > 50) {
            color = statusColors.info;
        }

        const defaultOptions = {
            responsive: true,
            maintainAspectRatio: false,
            circumference: 180,
            rotation: 270,
            cutout: '75%',
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    enabled: false
                }
            }
        };

        const mergedOptions = { ...defaultOptions, ...options };

        const config = {
            type: 'doughnut',
            data: {
                datasets: [{
                    data: [value, 100 - value],
                    backgroundColor: [
                        color,
                        'rgba(220, 220, 220, 0.5)'
                    ],
                    borderWidth: 0
                }]
            },
            options: mergedOptions
        };

        // Cria o gráfico
        const chart = initializeChart(chartId, config);

        // Adiciona texto central, se possível
        if (chart) {
            const chartElement = document.getElementById(chartId);
            if (chartElement) {
                const container = chartElement.parentNode;
                if (container) {
                    // Cria ou atualiza o indicador de valor
                    let indicator = container.querySelector('.gauge-value');
                    if (!indicator) {
                        indicator = document.createElement('div');
                        indicator.className = 'gauge-value';
                        indicator.style.position = 'absolute';
                        indicator.style.bottom = '25%';
                        indicator.style.left = '0';
                        indicator.style.right = '0';
                        indicator.style.textAlign = 'center';
                        indicator.style.fontSize = '1.5rem';
                        indicator.style.fontWeight = 'bold';
                        container.style.position = 'relative';
                        container.appendChild(indicator);
                    }
                    indicator.textContent = `${value}%`;
                }
            }
        }

        return chart;
    }

    /**
     * Gera cores para conjuntos de dados
     * @param {number} count - Número de cores a serem geradas
     * @returns {Array} - Array de cores
     */
    function generateColors(count) {
        const colors = [];
        for (let i = 0; i < count; i++) {
            if (i < defaultColors.length) {
                colors.push(defaultColors[i]);
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
     * Ajusta a opacidade de uma cor
     * @param {string} color - Cor em formato rgba
     * @param {number} opacity - Opacidade (0-1)
     * @returns {string} - Cor com opacidade ajustada
     */
    function adjustOpacity(color, opacity) {
        const rgbaMatch = color.match(/rgba\((\d+),\s*(\d+),\s*(\d+),\s*([\d.]+)\)/);
        if (rgbaMatch) {
            return `rgba(${rgbaMatch[1]}, ${rgbaMatch[2]}, ${rgbaMatch[3]}, ${opacity})`;
        }

        const rgbMatch = color.match(/rgb\((\d+),\s*(\d+),\s*(\d+)\)/);
        if (rgbMatch) {
            return `rgba(${rgbMatch[1]}, ${rgbMatch[2]}, ${rgbMatch[3]}, ${opacity})`;
        }

        // Para cores hexadecimais
        const hexMatch = color.match(/#([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})/i);
        if (hexMatch) {
            const r = parseInt(hexMatch[1], 16);
            const g = parseInt(hexMatch[2], 16);
            const b = parseInt(hexMatch[3], 16);
            return `rgba(${r}, ${g}, ${b}, ${opacity})`;
        }

        return color;
    }

    /**
     * Atualiza os dados de um gráfico existente
     * @param {Chart} chart - Instância do gráfico
     * @param {Array} labels - Novos rótulos
     * @param {Array} data - Novos dados
     * @param {boolean} animate - Indica se deve animar a atualização
     */
    function updateChartData(chart, labels, data, animate = true) {
        if (!chart) return;

        chart.data.labels = labels;

        if (Array.isArray(data)) {
            // Se data for um array simples, atualiza o primeiro dataset
            if (chart.data.datasets.length > 0) {
                chart.data.datasets[0].data = data;
            }
        } else if (typeof data === 'object') {
            // Se data for um objeto com múltiplos datasets
            Object.keys(data).forEach((key, index) => {
                if (chart.data.datasets.length > index) {
                    chart.data.datasets[index].data = data[key];
                }
            });
        }

        // Atualiza o gráfico
        chart.update(animate ? undefined : 'none');
    }

    /**
     * Adiciona um novo conjunto de dados a um gráfico existente
     * @param {Chart} chart - Instância do gráfico
     * @param {Object} dataset - Configuração do novo dataset
     * @param {boolean} update - Indica se deve atualizar o gráfico imediatamente
     */
    function addDataset(chart, dataset, update = true) {
        if (!chart) return;

        // Configura o dataset com valores padrão
        const color = dataset.color || defaultColors[chart.data.datasets.length % defaultColors.length];

        const newDataset = {
            label: dataset.label || `Dataset ${chart.data.datasets.length + 1}`,
            data: dataset.data || [],
            backgroundColor: dataset.backgroundColor || color,
            borderColor: dataset.borderColor || adjustOpacity(color, 1),
            borderWidth: dataset.borderWidth || 1
        };

        // Para gráficos de linha, adiciona configurações específicas
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

        // Adiciona o dataset ao gráfico
        chart.data.datasets.push(newDataset);

        // Atualiza o gráfico, se necessário
        if (update) {
            chart.update();
        }
    }

    /**
     * Remove um conjunto de dados de um gráfico existente
     * @param {Chart} chart - Instância do gráfico
     * @param {number} index - Índice do dataset a ser removido
     * @param {boolean} update - Indica se deve atualizar o gráfico imediatamente
     */
    function removeDataset(chart, index, update = true) {
        if (!chart || index < 0 || index >= chart.data.datasets.length) return;

        // Remove o dataset
        chart.data.datasets.splice(index, 1);

        // Atualiza o gráfico, se necessário
        if (update) {
            chart.update();
        }
    }

    /**
     * Converte dados de gráfico para uma tabela HTML
     * @param {Array} labels - Rótulos do gráfico
     * @param {Array|Object} datasets - Conjuntos de dados do gráfico
     * @returns {string} - Tabela HTML
     */
    function convertChartToTable(labels, datasets) {
        let html = '<table class="table table-striped table-sm">';
        html += '<thead><tr><th>Categoria</th>';

        // Determina se datasets é um array de objetos ou um array simples
        const isMultiDataset = Array.isArray(datasets) && typeof datasets[0] === 'object' && datasets[0].data;

        if (isMultiDataset) {
            // Adiciona cabeçalhos para múltiplos datasets
            datasets.forEach(dataset => {
                html += `<th>${dataset.label || 'Valor'}</th>`;
            });
        } else {
            // Apenas uma coluna para dataset único
            html += '<th>Valor</th>';
        }

        html += '</tr></thead><tbody>';

        // Adiciona linhas de dados
        labels.forEach((label, index) => {
            html += `<tr><td>${label}</td>`;

            if (isMultiDataset) {
                // Adiciona células para múltiplos datasets
                datasets.forEach(dataset => {
                    const value = dataset.data[index];
                    html += `<td>${typeof value === 'number' ? value.toLocaleString('pt-BR') : value}</td>`;
                });
            } else {
                // Apenas uma célula para dataset único
                const value = datasets[index];
                html += `<td>${typeof value === 'number' ? value.toLocaleString('pt-BR') : value}</td>`;
            }

            html += '</tr>';
        });

        html += '</tbody></table>';
        return html;
    }

    /**
     * Clean up all charts - useful when navigating away from a page
     */
    function cleanupAllCharts() {
        // Destroy all charts in the registry
        Object.keys(chartRegistry).forEach(chartId => {
            destroyChart(chartId);
        });
    }

    // API pública do módulo
    return {
        initialize: initialize,
        createLineChart: createLineChart,
        createMultiLineChart: createMultiLineChart,
        createBarChart: createBarChart,
        createGroupedBarChart: createGroupedBarChart,
        createPieChart: createPieChart,
        createPolarAreaChart: createPolarAreaChart,
        createRadarChart: createRadarChart,
        createGaugeChart: createGaugeChart,
        updateChartData: updateChartData,
        addDataset: addDataset,
        removeDataset: removeDataset,
        convertChartToTable: convertChartToTable,
        generateColors: generateColors,
        adjustColor: adjustColor,
        adjustOpacity: adjustOpacity,
        getChart: getChart,
        destroyChart: destroyChart,
        destroyChartSafely: destroyChartSafely,
        cleanupAllCharts: cleanupAllCharts
    };
})();
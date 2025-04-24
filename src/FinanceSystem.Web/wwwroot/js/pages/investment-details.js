/**
 * Scripts for the investment details page
 */

document.addEventListener('DOMContentLoaded', function () {
    initializePerformanceChart();
});

/**
 * Initialize the performance chart using Chart.js
 */
function initializePerformanceChart() {
    const canvas = document.getElementById('performanceChart');
    if (!canvas) return;

    // Sample data - This would ideally come from the API
    // In a real implementation, you would fetch this data from the server
    const investmentId = canvas.getAttribute('data-investment-id');

    // Mock data for demonstration
    const labels = [
        'Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun',
        'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'
    ];

    const data = {
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
            data: generateMockData(),
            fill: true,
            tension: 0.3
        }]
    };

    const config = {
        type: 'line',
        data: data,
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
    };

    new Chart(canvas, config);
}

/**
 * Generate mock performance data for demonstration
 */
function generateMockData() {
    const data = [];
    let value = 5000 + Math.random() * 2000;

    for (let i = 0; i < 12; i++) {
        // Add some variability to simulate market movements
        value = value * (1 + (Math.random() * 0.1 - 0.03));
        data.push(value.toFixed(2));
    }

    return data;
}
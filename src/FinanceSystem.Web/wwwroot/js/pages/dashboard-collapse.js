/**
 * Gerenciamento de seções colapsáveis com carregamento sob demanda
 */
document.addEventListener('DOMContentLoaded', function () {
    initializeCollapseSections();
});

function initializeCollapseSections() {
    // Manipulador para os botões de toggle
    document.querySelectorAll('.collapse-toggle').forEach(button => {
        button.addEventListener('click', function () {
            const id = this.getAttribute('data-id');
            const url = this.getAttribute('data-url');
            const isExpanded = this.getAttribute('data-expanded') === 'true';
            const collapseElement = document.getElementById(`collapse-${id}`);
            const container = document.getElementById(`container-${id}`);

            // Alterna o ícone
            const icon = this.querySelector('i');
            icon.classList.toggle('fa-chevron-down');
            icon.classList.toggle('fa-chevron-up');

            // Atualiza atributos
            this.setAttribute('data-expanded', (!isExpanded).toString());
            this.setAttribute('aria-expanded', (!isExpanded).toString());

            // Se estiver expandindo e o conteúdo não foi carregado ainda
            if (!isExpanded) {
                const contentContainer = collapseElement.querySelector('.content-container');

                // Se o conteúdo ainda não foi carregado
                if (contentContainer.children.length === 0) {
                    loadSectionContent(id, url);
                }

                // Expande o collapse usando Bootstrap
                new bootstrap.Collapse(collapseElement, {
                    show: true
                });
            } else {
                // Colapsa usando Bootstrap
                new bootstrap.Collapse(collapseElement, {
                    hide: true
                });
            }
        });
    });

    // Manipulador para botões de recarregar
    document.querySelectorAll('.reload-btn').forEach(button => {
        button.addEventListener('click', function () {
            const container = this.closest('.collapse-container');
            const id = container.id.replace('container-', '');
            const toggleButton = container.querySelector('.collapse-toggle');
            const url = toggleButton.getAttribute('data-url');

            loadSectionContent(id, url);
        });
    });
}

function loadSectionContent(id, url) {
    const collapseElement = document.getElementById(`collapse-${id}`);
    const contentContainer = collapseElement.querySelector('.content-container');
    const loadingIndicator = collapseElement.querySelector('.loading-indicator');
    const errorMessage = collapseElement.querySelector('.error-message');

    // Mostra indicador de carregamento
    contentContainer.innerHTML = '';
    contentContainer.classList.add('d-none');
    errorMessage.classList.add('d-none');
    loadingIndicator.classList.remove('d-none');

    // Faz a requisição AJAX
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
            // Carrega o conteúdo na seção
            contentContainer.innerHTML = html;
            contentContainer.classList.remove('d-none');
            loadingIndicator.classList.add('d-none');

            // Inicializa scripts específicos para cada seção
            if (id === 'monthly-chart') {
                initializeMonthlyComparisonChart();
            }
        })
        .catch(error => {
            console.error('Erro ao carregar seção:', error);

            // Mostra mensagem de erro
            loadingIndicator.classList.add('d-none');
            errorMessage.classList.remove('d-none');
            errorMessage.querySelector('.error-text').textContent = error.message || 'Ocorreu um erro ao carregar os dados.';
        });
}

function initializeMonthlyComparisonChart() {
    // Este método será chamado quando o gráfico de comparação mensal for carregado
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
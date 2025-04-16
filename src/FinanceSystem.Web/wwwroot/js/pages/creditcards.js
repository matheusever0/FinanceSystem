/**
 * Scripts específicos para as páginas de cartões de crédito
 */

document.addEventListener('DOMContentLoaded', function () {
    initializeCreditCardForm();
    initializeCreditCardsList();
    updateLimitsDisplay();
});

// Inicializa formulário de cartão de crédito
function initializeCreditCardForm() {
    const cardForm = document.getElementById('credit-card-form');
    if (!cardForm) return;

    // Validação de campos específicos
    const closingDayField = document.getElementById('ClosingDay');
    const dueDayField = document.getElementById('DueDay');

    if (closingDayField) {
        closingDayField.addEventListener('change', function () {
            validateDayField(this, 'dia de fechamento');
        });
    }

    if (dueDayField) {
        dueDayField.addEventListener('change', function () {
            validateDayField(this, 'dia de vencimento');
        });
    }

    // Validação de limite
    const limitField = document.getElementById('Limit');
    const availableLimitField = document.getElementById('AvailableLimit');

    if (limitField && availableLimitField) {
        limitField.addEventListener('change', function () {
            // Se o limite disponível for maior que o limite total, ajusta
            const limit = parseFloat(this.value.replace(/[^\d.,]/g, '').replace(',', '.'));
            const availableLimit = parseFloat(availableLimitField.value.replace(/[^\d.,]/g, '').replace(',', '.'));

            if (availableLimit > limit) {
                availableLimitField.value = this.value;
            }
        });

        availableLimitField.addEventListener('change', function () {
            // Garante que o limite disponível não seja maior que o limite total
            const limit = parseFloat(limitField.value.replace(/[^\d.,]/g, '').replace(',', '.'));
            const availableLimit = parseFloat(this.value.replace(/[^\d.,]/g, '').replace(',', '.'));

            if (availableLimit > limit) {
                this.value = limitField.value;
                alert('O limite disponível não pode ser maior que o limite total');
            }
        });
    }

    // Formatação de campo de dígitos do cartão
    const lastFourDigitsField = document.getElementById('LastFourDigits');
    if (lastFourDigitsField) {
        lastFourDigitsField.addEventListener('input', function () {
            // Limita a 4 dígitos numéricos
            this.value = this.value.replace(/\D/g, '').substring(0, 4);
        });
    }
}

function validateDayField(field, fieldName) {
    const value = parseInt(field.value, 10);

    if (isNaN(value) || value < 1 || value > 31) {
        field.setCustomValidity(`Por favor, insira um ${fieldName} válido (1-31)`);
    } else {
        field.setCustomValidity('');
    }
}

// Inicializa lista de cartões de crédito
function initializeCreditCardsList() {
    const progressBars = document.querySelectorAll('.credit-card-progress');

    progressBars.forEach(progressBar => {
        const percentage = progressBar.getAttribute('data-percentage') || 0;

        // Define a cor da barra com base na porcentagem
        if (percentage > 90) {
            progressBar.classList.add('bg-danger');
        } else if (percentage > 70) {
            progressBar.classList.add('bg-warning');
        } else if (percentage > 50) {
            progressBar.classList.add('bg-info');
        } else {
            progressBar.classList.add('bg-success');
        }

        // Define a largura da barra
        progressBar.style.width = `${percentage}%`;
    });

    // Botões de ação
    const deleteButtons = document.querySelectorAll('.delete-card-btn');

    deleteButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            if (!confirm('Tem certeza que deseja excluir este cartão? Esta ação não pode ser desfeita.')) {
                e.preventDefault();
            }
        });
    });
}

// Atualiza exibição de limites
function updateLimitsDisplay() {
    const limitDisplays = document.querySelectorAll('.limit-display');

    limitDisplays.forEach(display => {
        const total = parseFloat(display.getAttribute('data-total') || 0);
        const used = parseFloat(display.getAttribute('data-used') || 0);
        const available = total - used;
        const percentage = (used / total * 100) || 0;

        // Atualiza elementos na tela
        const totalElement = display.querySelector('.total-limit');
        const availableElement = display.querySelector('.available-limit');
        const usedElement = display.querySelector('.used-limit');
        const percentageElement = display.querySelector('.used-percentage');

        if (totalElement) totalElement.textContent = formatCurrency(total);
        if (availableElement) availableElement.textContent = formatCurrency(available);
        if (usedElement) usedElement.textContent = formatCurrency(used);
        if (percentageElement) percentageElement.textContent = `${percentage.toFixed(0)}%`;

        // Atualiza barra de progresso
        const progressBar = display.querySelector('.progress-bar');
        if (progressBar) {
            progressBar.style.width = `${percentage}%`;

            // Atualiza classe baseado no uso
            progressBar.classList.remove('bg-success', 'bg-info', 'bg-warning', 'bg-danger');

            if (percentage > 90) {
                progressBar.classList.add('bg-danger');
            } else if (percentage > 70) {
                progressBar.classList.add('bg-warning');
            } else if (percentage > 50) {
                progressBar.classList.add('bg-info');
            } else {
                progressBar.classList.add('bg-success');
            }
        }
    });
}

// Cálculo de datas de fechamento e vencimento
function calculateNextDates() {
    const cardElements = document.querySelectorAll('.credit-card-dates');

    cardElements.forEach(element => {
        const closingDay = parseInt(element.getAttribute('data-closing-day'), 10);
        const dueDay = parseInt(element.getAttribute('data-due-day'), 10);

        if (isNaN(closingDay) || isNaN(dueDay)) return;

        const today = new Date();
        let nextClosingDate = new Date(today.getFullYear(), today.getMonth(), closingDay);
        let nextDueDate = new Date(today.getFullYear(), today.getMonth(), dueDay);

        // Se a data de fechamento já passou este mês, avança para o próximo
        if (today.getDate() > closingDay) {
            nextClosingDate.setMonth(nextClosingDate.getMonth() + 1);
        }

        // Se a data de vencimento já passou este mês, avança para o próximo
        if (today.getDate() > dueDay) {
            nextDueDate.setMonth(nextDueDate.getMonth() + 1);
        }

        // Atualiza elementos na página
        const closingDateElement = element.querySelector('.next-closing-date');
        const dueDateElement = element.querySelector('.next-due-date');
        const daysToClosingElement = element.querySelector('.days-to-closing');
        const daysToDueElement = element.querySelector('.days-to-due');

        if (closingDateElement) {
            closingDateElement.textContent = formatDate(nextClosingDate);
        }

        if (dueDateElement) {
            dueDateElement.textContent = formatDate(nextDueDate);
        }

        if (daysToClosingElement) {
            const daysToClosing = Math.ceil((nextClosingDate - today) / (1000 * 60 * 60 * 24));
            daysToClosingElement.textContent = daysToClosing;
        }

        if (daysToDueElement) {
            const daysToDue = Math.ceil((nextDueDate - today) / (1000 * 60 * 60 * 24));
            daysToDueElement.textContent = daysToDue;
        }
    });
}

// Funções auxiliares
function formatCurrency(value) {
    return new Intl.NumberFormat('pt-BR', {
        style: 'currency',
        currency: 'BRL'
    }).format(value);
}

function formatDate(date) {
    return date.toLocaleDateString('pt-BR');
}
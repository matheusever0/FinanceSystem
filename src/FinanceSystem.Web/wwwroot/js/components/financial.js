/**
 * Componentes financeiros comuns para pagamentos e receitas
 */

/**
 * Inicializa uma máscara de moeda em um campo
 * @param {string} selector - Seletor do campo
 */
function initializeMoneyMask(selector) {
    const moneyInput = document.querySelector(selector);
    if (!moneyInput) return;

    if (typeof $.fn.mask !== 'undefined') {
        $(moneyInput).mask('#.##0,00', { reverse: true });

        // Evento blur para validação
        $(moneyInput).on('blur', function () {
            let value = $(this).val();
            value = value.replace(/\./g, '').replace(',', '.');
            const parsedValue = parseFloat(value);

            if (!isNaN(parsedValue) && parsedValue > 0) {
                $(this).val(parsedValue.toFixed(2));
                $(this).removeClass('input-validation-error');
                $(this).next('.text-danger').text('');
            } else {
                $(this).addClass('input-validation-error');
                $(this).next('.text-danger').text('O campo Valor deve ser um número válido.');
            }
        });
    } else {
        // Implementação manual se mask não estiver disponível
        moneyInput.addEventListener('input', function (e) {
            let value = e.target.value.replace(/\D/g, '');
            value = (parseInt(value) / 100).toFixed(2);
            e.target.value = value.replace('.', ',');
        });
    }
}

/**
 * Inicializa o toggle de recorrente
 * @param {HTMLElement} form - Formulário
 */
function initializeRecurringToggle(form) {
    const isRecurringSwitch = form.querySelector('#isRecurringSwitch');
    const isRecurringLabel = form.querySelector('#isRecurringLabel');
    const installmentsInput = form.querySelector('#NumberOfInstallments');

    if (isRecurringSwitch && isRecurringLabel && installmentsInput) {
        isRecurringSwitch.addEventListener('change', function () {
            isRecurringLabel.textContent = this.checked ? 'Sim' : 'Não';
            if (this.checked) {
                installmentsInput.value = '1';
                installmentsInput.disabled = true;
            } else {
                installmentsInput.disabled = false;
            }
        });

        // Inicializa com o estado atual
        if (isRecurringSwitch.checked) {
            isRecurringLabel.textContent = 'Sim';
            installmentsInput.value = '1';
            installmentsInput.disabled = true;
        }
    }
}

/**
 * Inicializa o toggle de status
 * @param {string} switchSelector - Seletor do switch
 * @param {string} labelSelector - Seletor do label
 */
function initializeStatusToggle(switchSelector, labelSelector) {
    const statusSwitch = document.querySelector(switchSelector);
    const statusLabel = document.querySelector(labelSelector);

    if (!statusSwitch || !statusLabel) return;

    function updateStatusLabel() {
        if (statusSwitch.checked) {
            statusLabel.classList.remove('bg-danger');
            statusLabel.classList.add('bg-success');
            statusLabel.textContent = 'Ativo';
        } else {
            statusLabel.classList.remove('bg-success');
            statusLabel.classList.add('bg-danger');
            statusLabel.textContent = 'Inativo';
        }
    }

    // Inicializa com o estado atual
    updateStatusLabel();

    // Atualiza quando o switch mudar
    statusSwitch.addEventListener('change', updateStatusLabel);
}

/**
 * Configura validação básica de formulário
 * @param {HTMLElement} form - Formulário
 * @param {Function} validateCallback - Função de validação específica
 */
function setupFormValidation(form, validateCallback) {
    if (!form) return;

    form.addEventListener('submit', function (event) {
        if (form.checkValidity() === false) {
            event.preventDefault();
            event.stopPropagation();
        }

        form.classList.add('was-validated');

        // Chama validação específica se fornecida
        if (typeof validateCallback === 'function') {
            if (validateCallback(event) === false) {
                event.preventDefault();
                event.stopPropagation();
            }
        }
    });
}

/**
 * Mostra mensagem de erro para um campo
 * @param {HTMLElement} input - Campo com erro
 * @param {string} message - Mensagem de erro
 */
function showFieldError(input, message) {
    let errorElement = input.parentElement.querySelector('.text-danger');
    if (!errorElement) {
        errorElement = document.createElement('span');
        errorElement.classList.add('text-danger');
        input.parentElement.appendChild(errorElement);
    }
    errorElement.innerText = message;
    input.classList.add('is-invalid');
}
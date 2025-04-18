document.addEventListener('DOMContentLoaded', function () {
    const createEditForm = document.querySelector('form[asp-action="Create"], form[asp-action="Edit"]');

    if (createEditForm) {
        initializeRecurringToggle(createEditForm);
        initializeMoneyMask(createEditForm);
        setupFormValidation(createEditForm);
    }
});

function initializeRecurringToggle(form) {
    const isRecurringSwitch = form.querySelector('#isRecurringSwitch');
    const isRecurringLabel = form.querySelector('#isRecurringLabel');
    const installmentsInput = form.querySelector('#NumberOfInstallments');

    if (isRecurringSwitch && isRecurringLabel && installmentsInput) {
        isRecurringSwitch.addEventListener('change', function () {
            isRecurringLabel.textContent = this.checked ? 'Sim' : 'Não';
            installmentsInput.value = this.checked ? '1' : installmentsInput.value;
            installmentsInput.disabled = this.checked;
        });
    }
}

function initializeMoneyMask(form) {
    const moneyInput = form.querySelector('.money');
    if (moneyInput && typeof $.fn.mask !== 'undefined') {
        $(moneyInput).mask('#.##0,00', { reverse: true });

        // Evento de "blur" (quando sai do campo) para validar e formatar corretamente
        $(moneyInput).on('blur', function () {
            let value = $(this).val();
            // Remove qualquer formatação anterior para garantir que o número é válido
            value = value.replace(/\./g, '').replace(',', '.');
            const parsedValue = parseFloat(value);
            if (!isNaN(parsedValue) && parsedValue > 0) {
                $(this).val(parsedValue.toFixed(2)); // Formata com 2 casas decimais
                $(this).removeClass('input-validation-error');
                $(this).next('.text-danger').text(''); // Remove mensagem de erro
            } else {
                $(this).addClass('input-validation-error');
                $(this).next('.text-danger').text('O campo Valor deve ser um número válido.');
            }
        });
    }
}

function setupFormValidation(form) {
    form.addEventListener('submit', validateIncomeForm);
}

function validateIncomeForm(event) {
    let isValid = true;
    const form = event.target;

    // Valida o campo de valor
    const amount = form.querySelector('#Amount');
    if (amount) {
        let rawValue = amount.value;

        // Converte o valor formatado para número
        if (typeof rawValue === 'string') {
            rawValue = rawValue.replace(/\./g, '').replace(',', '.');
        }

        const parsed = parseFloat(rawValue);

        if (isNaN(parsed) || parsed <= 0) {
            isValid = false;
            showError(amount, 'Informe um valor válido');
        } else {
            // Atualiza o valor no campo antes de enviar
            amount.value = parsed.toString();
        }
    }

    // Valida a descrição
    const description = form.querySelector('#Description');
    if (description && description.value.trim() === '') {
        isValid = false;
        showError(description, 'A descrição é obrigatória');
    }

    // Valida a data de vencimento
    const dueDate = form.querySelector('#DueDate');
    if (dueDate && dueDate.value === '') {
        isValid = false;
        showError(dueDate, 'A data de vencimento é obrigatória');
    }

    // Valida o tipo de receita
    const incomeTypeId = form.querySelector('#IncomeTypeId');
    if (incomeTypeId && incomeTypeId.value === '') {
        isValid = false;
        showError(incomeTypeId, 'O tipo de receita é obrigatório');
    }

    if (!isValid) {
        event.preventDefault();
    }
}

function showError(input, message) {
    let errorElement = input.parentElement.querySelector('.text-danger');
    if (!errorElement) {
        errorElement = document.createElement('span');
        errorElement.classList.add('text-danger');
        input.parentElement.appendChild(errorElement);
    }
    errorElement.innerText = message;
    input.classList.add('is-invalid');
}

/**
 * Scripts para formulários do Finance System
 */

// Inicialização de formulários
document.addEventListener('DOMContentLoaded', function () {
    initializeFormValidation();
    initializeSelectFields();
    initializeFormatedInputs();
    initializeFormDependencies();
});

// Inicializa validação de formulários
function initializeFormValidation() {
    const forms = document.querySelectorAll('form.needs-validation');

    Array.from(forms).forEach(form => {
        form.addEventListener('submit', event => {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }

            form.classList.add('was-validated');
        }, false);
    });

    // Validação de campos específicos
    initializePasswordValidation();
    initializeCustomValidation();
}

// Validação de campos de senha
function initializePasswordValidation() {
    const passwordFields = document.querySelectorAll('input[type="password"].validate-password');

    passwordFields.forEach(field => {
        field.addEventListener('input', function () {
            validatePassword(this);
        });

        if (field.value) {
            validatePassword(field);
        }
    });
}

function validatePassword(field) {
    const minLength = field.getAttribute('data-min-length') || 6;
    const value = field.value;

    // Verifica tamanho mínimo
    if (value.length < minLength) {
        field.setCustomValidity(`A senha deve ter pelo menos ${minLength} caracteres`);
        return;
    }

    // Verifica força da senha - opcional
    if (field.classList.contains('validate-strength')) {
        const hasLetter = /[a-zA-Z]/.test(value);
        const hasNumber = /[0-9]/.test(value);
        const hasSpecial = /[^a-zA-Z0-9]/.test(value);

        if (!(hasLetter && hasNumber)) {
            field.setCustomValidity('A senha deve conter letras e números');
            return;
        }

        if (field.classList.contains('validate-strong') && !hasSpecial) {
            field.setCustomValidity('A senha deve conter pelo menos um caractere especial');
            return;
        }
    }

    field.setCustomValidity('');
}

// Validações customizadas
function initializeCustomValidation() {
    // Validação de campos de email
    const emailFields = document.querySelectorAll('input[type="email"]');
    emailFields.forEach(field => {
        field.addEventListener('input', function () {
            validateEmail(this);
        });

        if (field.value) {
            validateEmail(field);
        }
    });

    // Validação de campos de CPF/CNPJ
    const documentFields = document.querySelectorAll('.validate-document');
    documentFields.forEach(field => {
        field.addEventListener('input', function () {
            validateDocument(this);
        });

        if (field.value) {
            validateDocument(field);
        }
    });
}

function validateEmail(field) {
    const value = field.value;
    const regex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;

    if (!regex.test(value)) {
        field.setCustomValidity('Digite um email válido');
    } else {
        field.setCustomValidity('');
    }
}

function validateDocument(field) {
    const value = field.value.replace(/[^\d]/g, '');

    if (value.length === 11) {
        // Validação de CPF
        if (!validateCPF(value)) {
            field.setCustomValidity('CPF inválido');
        } else {
            field.setCustomValidity('');
        }
    } else if (value.length === 14) {
        // Validação de CNPJ
        if (!validateCNPJ(value)) {
            field.setCustomValidity('CNPJ inválido');
        } else {
            field.setCustomValidity('');
        }
    } else {
        field.setCustomValidity('Documento inválido');
    }
}

// Funções de validação de CPF e CNPJ
function validateCPF(cpf) {
    // Implementação de validação de CPF
    if (cpf.length !== 11 || /^(\d)\1{10}$/.test(cpf)) return false;

    let sum = 0;
    let remainder;

    for (let i = 1; i <= 9; i++) {
        sum += parseInt(cpf.substring(i - 1, i)) * (11 - i);
    }

    remainder = (sum * 10) % 11;
    if (remainder === 10 || remainder === 11) remainder = 0;
    if (remainder !== parseInt(cpf.substring(9, 10))) return false;

    sum = 0;
    for (let i = 1; i <= 10; i++) {
        sum += parseInt(cpf.substring(i - 1, i)) * (12 - i);
    }

    remainder = (sum * 10) % 11;
    if (remainder === 10 || remainder === 11) remainder = 0;
    if (remainder !== parseInt(cpf.substring(10, 11))) return false;

    return true;
}

function validateCNPJ(cnpj) {
    // Implementação de validação de CNPJ
    if (cnpj.length !== 14 || /^(\d)\1{13}$/.test(cnpj)) return false;

    // Validação dos dígitos verificadores
    let size = cnpj.length - 2;
    let numbers = cnpj.substring(0, size);
    let digits = cnpj.substring(size);
    let sum = 0;
    let pos = size - 7;

    for (let i = size; i >= 1; i--) {
        sum += numbers.charAt(size - i) * pos--;
        if (pos < 2) pos = 9;
    }

    let result = sum % 11 < 2 ? 0 : 11 - sum % 11;
    if (result !== parseInt(digits.charAt(0))) return false;

    size += 1;
    numbers = cnpj.substring(0, size);
    sum = 0;
    pos = size - 7;

    for (let i = size; i >= 1; i--) {
        sum += numbers.charAt(size - i) * pos--;
        if (pos < 2) pos = 9;
    }

    result = sum % 11 < 2 ? 0 : 11 - sum % 11;
    if (result !== parseInt(digits.charAt(1))) return false;

    return true;
}

// Inicialização de campos select
function initializeSelectFields() {
    // Verifica se há elementos select2 na página
    if (typeof $.fn.select2 !== 'undefined') {
        $('.select2').select2({
            width: '100%',
            language: 'pt-BR'
        });
    }

    // Manipulação de eventos em campos select
    const dependentSelects = document.querySelectorAll('select[data-dependent-on]');
    dependentSelects.forEach(select => {
        const parentId = select.getAttribute('data-dependent-on');
        const parentSelect = document.getElementById(parentId);

        if (parentSelect) {
            parentSelect.addEventListener('change', function () {
                updateDependentSelect(select, this.value);
            });

            // Inicializa com o valor atual do parent
            if (parentSelect.value) {
                updateDependentSelect(select, parentSelect.value);
            }
        }
    });
}

function updateDependentSelect(select, parentValue) {
    // Obtém a URL de dados ou filtra opções existentes
    const dataUrl = select.getAttribute('data-url');
    const filterAttribute = select.getAttribute('data-filter-attribute') || 'data-parent';

    if (dataUrl) {
        // Carrega opções via AJAX
        fetch(`${dataUrl}?parentId=${parentValue}`)
            .then(response => response.json())
            .then(data => {
                populateSelect(select, data);
            })
            .catch(error => console.error('Erro ao carregar opções:', error));
    } else {
        // Filtra opções existentes
        const options = select.querySelectorAll('option');
        options.forEach(option => {
            if (option.value === '' || option.getAttribute(filterAttribute) === parentValue) {
                option.style.display = '';
            } else {
                option.style.display = 'none';
            }
        });

        // Reseta o valor se a opção atual não estiver mais disponível
        if (select.value !== '' &&
            select.querySelector(`option[value="${select.value}"][${filterAttribute}="${parentValue}"]`) === null) {
            select.value = '';
        }
    }
}

function populateSelect(select, data) {
    // Limpa opções existentes
    select.innerHTML = '';

    // Adiciona opção vazia
    const emptyOption = document.createElement('option');
    emptyOption.value = '';
    emptyOption.text = select.getAttribute('data-placeholder') || 'Selecione...';
    select.appendChild(emptyOption);

    // Adiciona novas opções
    data.forEach(item => {
        const option = document.createElement('option');
        option.value = item.value || item.id;
        option.text = item.text || item.name;

        // Adiciona atributos customizados se necessário
        if (item.attributes) {
            Object.keys(item.attributes).forEach(key => {
                option.setAttribute(key, item.attributes[key]);
            });
        }

        select.appendChild(option);
    });

    // Atualiza select2 se estiver sendo usado
    if (typeof $.fn.select2 !== 'undefined' && $(select).hasClass('select2')) {
        $(select).trigger('change');
    }
}

// Inicializa inputs formatados (máscaras)
function initializeFormatedInputs() {
    // Verifica se a biblioteca de máscaras está disponível
    if (typeof $.fn.mask !== 'undefined') {
        // Máscaras comuns
        $('.mask-date').mask('00/00/0000');
        $('.mask-time').mask('00:00');
        $('.mask-date-time').mask('00/00/0000 00:00');
        $('.mask-cep').mask('00000-000');
        $('.mask-phone').mask('(00) 0000-00009');
        $('.mask-phone').blur(function (event) {
            if ($(this).val().length === 15) {
                $(this).mask('(00) 00000-0009');
            } else {
                $(this).mask('(00) 0000-00009');
            }
        });
        $('.mask-cpf').mask('000.000.000-00');
        $('.mask-cnpj').mask('00.000.000/0000-00');
        $('.mask-money').mask('#.##0,00', { reverse: true });
        $('.mask-percent').mask('##0,00%', { reverse: true });
    }

    // Inicialização manual de máscaras se a biblioteca não estiver disponível
    if (typeof $.fn.mask === 'undefined') {
        // Campos de moeda
        const moneyInputs = document.querySelectorAll('.mask-money');
        moneyInputs.forEach(input => {
            input.addEventListener('input', function (e) {
                let value = e.target.value.replace(/\D/g, '');
                value = (parseInt(value) / 100).toFixed(2);
                e.target.value = value.replace('.', ',');
            });
        });

        // Campos de data
        const dateInputs = document.querySelectorAll('.mask-date');
        dateInputs.forEach(input => {
            input.addEventListener('input', function (e) {
                let value = e.target.value.replace(/\D/g, '');
                if (value.length > 8) value = value.substr(0, 8);
                if (value.length > 4) value = value.substr(0, 4) + '/' + value.substr(4);
                if (value.length > 2) value = value.substr(0, 2) + '/' + value.substr(2);
                e.target.value = value;
            });
        });
    }
}

// Inicializa dependências entre campos de formulário
function initializeFormDependencies() {
    const triggerElements = document.querySelectorAll('[data-toggle-target]');

    triggerElements.forEach(element => {
        const targetSelector = element.getAttribute('data-toggle-target');
        const targets = document.querySelectorAll(targetSelector);

        if (element.type === 'checkbox') {
            element.addEventListener('change', function () {
                toggleTargetVisibility(targets, this.checked);
            });

            // Inicializa com o estado atual
            toggleTargetVisibility(targets, element.checked);
        } else if (element.tagName === 'SELECT') {
            element.addEventListener('change', function () {
                const showValue = element.getAttribute('data-toggle-value');
                const isMatch = showValue === this.value;
                toggleTargetVisibility(targets, isMatch);
            });

            // Inicializa com o valor atual
            const showValue = element.getAttribute('data-toggle-value');
            toggleTargetVisibility(targets, showValue === element.value);
        }
    });
}

function toggleTargetVisibility(targets, show) {
    targets.forEach(target => {
        if (show) {
            target.style.display = '';

            // Habilita campos dentro do target
            const fields = target.querySelectorAll('input, select, textarea');
            fields.forEach(field => {
                field.disabled = false;
            });
        } else {
            target.style.display = 'none';

            // Desabilita campos dentro do target
            const fields = target.querySelectorAll('input, select, textarea');
            fields.forEach(field => {
                field.disabled = true;
            });
        }
    });
}
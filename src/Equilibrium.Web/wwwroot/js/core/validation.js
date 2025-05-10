/**
 * Finance System - Validation
 * Funções de validação de formulários e campos
 */

var FinanceSystem = FinanceSystem || {};

FinanceSystem.Validation = (function () {
    function initialize() {
        initializeFormValidation();
    }

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
    }

    function setupFormValidation(form, validateCallback) {
        if (!form) return;

        form.addEventListener('submit', function (event) {
            if (form.checkValidity() === false) {
                event.preventDefault();
                event.stopPropagation();
            }

            form.classList.add('was-validated');

            if (typeof validateCallback === 'function') {
                if (validateCallback(event) === false) {
                    event.preventDefault();
                    event.stopPropagation();
                }
            }
        });
    }

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

    function clearFieldError(input) {
        const errorElement = input.parentElement.querySelector('.text-danger');
        if (errorElement) {
            errorElement.innerText = '';
        }
        input.classList.remove('is-invalid');
    }

    function isValidEmail(email) {
        const regex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
        return regex.test(email);
    }

    function isValidCPF(cpf) {
        cpf = cpf.replace(/\D/g, '');

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

    function isValidCNPJ(cnpj) {
        cnpj = cnpj.replace(/\D/g, '');

        if (cnpj.length !== 14 || /^(\d)\1{13}$/.test(cnpj)) return false;

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

    function isValidDate(date) {
        if (!/^\d{2}\/\d{2}\/\d{4}$/.test(date)) return false;

        const parts = date.split('/');
        const day = parseInt(parts[0], 10);
        const month = parseInt(parts[1], 10);
        const year = parseInt(parts[2], 10);

        if (month < 1 || month > 12) return false;

        const daysInMonth = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];
        if (year % 400 === 0 || (year % 100 !== 0 && year % 4 === 0)) {
            daysInMonth[1] = 29;
        }

        return day > 0 && day <= daysInMonth[month - 1];
    }

    function isValidPassword(password) {
        return password.length >= 6 &&
            /[a-zA-Z]/.test(password) &&
            /\d/.test(password);
    }

    function isValidNumber(value) {
        return !isNaN(parseFloat(value)) && isFinite(value);
    }

    function isValidCurrency(value) {
        value = value.replace(/\./g, '').replace(',', '.');
        const number = parseFloat(value);
        return !isNaN(number) && isFinite(number) && number >= 0;
    }

    function isValidDocument(document) {
        document = document.replace(/\D/g, '');

        if (document.length === 11) {
            return isValidCPF(document);
        }

        if (document.length === 14) {
            return isValidCNPJ(document);
        }

        return false;
    }

    function isValidUsername(username) {
        return /^[a-zA-Z0-9._-]{3,30}$/.test(username);
    }

    function isEmpty(value) {
        return value === null || value === undefined || value.trim() === '';
    }

    function validateField(field) {
        const value = field.value;
        const type = field.type;
        const required = field.required;

        if (required && isEmpty(value)) {
            showFieldError(field, 'Este campo é obrigatório');
            return false;
        }

        if (isEmpty(value) && !required) {
            return true;
        }

        switch (type) {
            case 'email':
                if (!isValidEmail(value)) {
                    showFieldError(field, 'Digite um email válido');
                    return false;
                }
                break;
            case 'password':
                if (field.classList.contains('validate-password') && !isValidPassword(value)) {
                    showFieldError(field, 'A senha deve ter pelo menos 6 caracteres, incluindo letras e números');
                    return false;
                }
                break;
            case 'date':
                if (!isValidDate(value)) {
                    showFieldError(field, 'Digite uma data válida');
                    return false;
                }
                break;
            case 'number':
                if (!isValidNumber(value)) {
                    showFieldError(field, 'Digite um número válido');
                    return false;
                }
                break;
        }

        if (field.classList.contains('validate-cpf') && !isValidCPF(value)) {
            showFieldError(field, 'Digite um CPF válido');
            return false;
        }

        if (field.classList.contains('validate-cnpj') && !isValidCNPJ(value)) {
            showFieldError(field, 'Digite um CNPJ válido');
            return false;
        }

        if (field.classList.contains('validate-document') && !isValidDocument(value)) {
            showFieldError(field, 'Digite um documento válido (CPF ou CNPJ)');
            return false;
        }

        if (field.classList.contains('validate-currency') && !isValidCurrency(value)) {
            showFieldError(field, 'Digite um valor monetário válido');
            return false;
        }

        if (field.classList.contains('validate-username') && !isValidUsername(value)) {
            showFieldError(field, 'O nome de usuário deve conter apenas letras, números, pontos, hífens e underscores (3-30 caracteres)');
            return false;
        }

        clearFieldError(field);
        return true;
    }

    function validateForm(form) {
        if (!form) return false;

        const fields = form.querySelectorAll('input, select, textarea');
        let isValid = true;

        fields.forEach(field => {
            if (!validateField(field)) {
                isValid = false;
            }
        });

        return isValid;
    }

    return {
        initialize: initialize,
        setupFormValidation: setupFormValidation,
        showFieldError: showFieldError,
        clearFieldError: clearFieldError,
        isValidEmail: isValidEmail,
        isValidCPF: isValidCPF,
        isValidCNPJ: isValidCNPJ,
        isValidDate: isValidDate,
        isValidPassword: isValidPassword,
        isValidNumber: isValidNumber,
        isValidCurrency: isValidCurrency,
        isValidDocument: isValidDocument,
        isValidUsername: isValidUsername,
        isEmpty: isEmpty,
        validateField: validateField,
        validateForm: validateForm
    };
})();
/**
 * Finance System - Forms Module
 * Funções reutilizáveis para manipulação de formulários
 */

var FinanceSystem = FinanceSystem || {};
FinanceSystem.Modules = FinanceSystem.Modules || {};

FinanceSystem.Modules.Forms = (function () {
    function initialize() {
        initializeFormatedInputs();
        initializeSelectFields();
        initializeFormDependencies();
    }

    function initializeFormatedInputs() {
        if (typeof $.fn.mask !== 'undefined') {
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
        } else {
            initializeManualMasks();
        }
    }

    function initializeManualMasks() {
        const moneyInputs = document.querySelectorAll('.mask-money');
        moneyInputs.forEach(input => {
            input.addEventListener('input', function (e) {
                FinanceSystem.Modules.Financial.formatCurrencyInput(this);
            });

            if (input.value) {
                FinanceSystem.Modules.Financial.formatCurrencyInput(input);
            }
        });

        const percentInputs = document.querySelectorAll('.mask-percent');
        percentInputs.forEach(input => {
            input.addEventListener('input', function (e) {
                FinanceSystem.Modules.Financial.formatPercentInput(this);
            });

            if (input.value) {
                FinanceSystem.Modules.Financial.formatPercentInput(input);
            }
        });

        const dateInputs = document.querySelectorAll('.mask-date');
        dateInputs.forEach(input => {
            input.addEventListener('input', function (e) {
                formatDateInput(this);
            });

            if (input.value) {
                formatDateInput(input);
            }
        });

        const cpfInputs = document.querySelectorAll('.mask-cpf');
        cpfInputs.forEach(input => {
            input.addEventListener('input', function (e) {
                formatCPFInput(this);
            });

            if (input.value) {
                formatCPFInput(input);
            }
        });

        const cnpjInputs = document.querySelectorAll('.mask-cnpj');
        cnpjInputs.forEach(input => {
            input.addEventListener('input', function (e) {
                formatCNPJInput(this);
            });

            if (input.value) {
                formatCNPJInput(input);
            }
        });

        const phoneInputs = document.querySelectorAll('.mask-phone');
        phoneInputs.forEach(input => {
            input.addEventListener('input', function (e) {
                formatPhoneInput(this);
            });

            if (input.value) {
                formatPhoneInput(input);
            }
        });
    }

    function formatDateInput(input) {
        let value = input.value.replace(/\D/g, '');
        if (value.length > 8) value = value.substring(0, 8);

        if (value.length > 4) {
            value = value.substring(0, 2) + '/' + value.substring(2, 4) + '/' + value.substring(4);
        } else if (value.length > 2) {
            value = value.substring(0, 2) + '/' + value.substring(2);
        }

        input.value = value;
    }

    function formatCPFInput(input) {
        let value = input.value.replace(/\D/g, '');
        if (value.length > 11) value = value.substring(0, 11);

        if (value.length > 9) {
            value = value.substring(0, 3) + '.' + value.substring(3, 6) + '.' + value.substring(6, 9) + '-' + value.substring(9);
        } else if (value.length > 6) {
            value = value.substring(0, 3) + '.' + value.substring(3, 6) + '.' + value.substring(6);
        } else if (value.length > 3) {
            value = value.substring(0, 3) + '.' + value.substring(3);
        }

        input.value = value;
    }

    function formatCNPJInput(input) {
        let value = input.value.replace(/\D/g, '');
        if (value.length > 14) value = value.substring(0, 14);

        if (value.length > 12) {
            value = value.substring(0, 2) + '.' + value.substring(2, 5) + '.' + value.substring(5, 8) + '/' + value.substring(8, 12) + '-' + value.substring(12);
        } else if (value.length > 8) {
            value = value.substring(0, 2) + '.' + value.substring(2, 5) + '.' + value.substring(5, 8) + '/' + value.substring(8);
        } else if (value.length > 5) {
            value = value.substring(0, 2) + '.' + value.substring(2, 5) + '.' + value.substring(5);
        } else if (value.length > 2) {
            value = value.substring(0, 2) + '.' + value.substring(2);
        }

        input.value = value;
    }

    function formatPhoneInput(input) {
        let value = input.value.replace(/\D/g, '');
        if (value.length > 11) value = value.substring(0, 11);

        if (value.length > 10) { // Celular com 9 dígitos
            value = '(' + value.substring(0, 2) + ') ' + value.substring(2, 7) + '-' + value.substring(7);
        } else if (value.length > 6) { // Fixo ou celular sem 9
            value = '(' + value.substring(0, 2) + ') ' + value.substring(2, 6) + '-' + value.substring(6);
        } else if (value.length > 2) {
            value = '(' + value.substring(0, 2) + ') ' + value.substring(2);
        } else if (value.length > 0) {
            value = '(' + value;
        }

        input.value = value;
    }

    function initializeSelectFields() {
        if (typeof $.fn.select2 !== 'undefined') {
            $('.select2').select2({
                width: '100%',
                language: 'pt-BR'
            });
        }

        initializeSelectDependencies();
    }

    function initializeSelectDependencies() {
        const dependentSelects = document.querySelectorAll('select[data-dependent-on]');
        dependentSelects.forEach(select => {
            const parentId = select.getAttribute('data-dependent-on');
            const parentSelect = document.getElementById(parentId);

            if (parentSelect) {
                parentSelect.addEventListener('change', function () {
                    updateDependentSelect(select, this.value);
                });

                if (parentSelect.value) {
                    updateDependentSelect(select, parentSelect.value);
                }
            }
        });
    }

    function updateDependentSelect(select, parentValue) {
        const dataUrl = select.getAttribute('data-url');
        const filterAttribute = select.getAttribute('data-filter-attribute') || 'data-parent';

        if (dataUrl) {
            fetch(`${dataUrl}?parentId=${parentValue}`)
                .then(response => response.json())
                .then(data => {
                    populateSelect(select, data);
                })
                .catch(error => console.error('Erro ao carregar opções:', error));
        } else {
            const options = select.querySelectorAll('option');
            options.forEach(option => {
                if (option.value === '' || option.getAttribute(filterAttribute) === parentValue) {
                    option.style.display = '';
                } else {
                    option.style.display = 'none';
                }
            });

            if (select.value !== '' &&
                select.querySelector(`option[value="${select.value}"][${filterAttribute}="${parentValue}"]`) === null) {
                select.value = '';
            }
        }
    }

    function populateSelect(select, data) {
        select.innerHTML = '';

        const emptyOption = document.createElement('option');
        emptyOption.value = '';
        emptyOption.text = select.getAttribute('data-placeholder') || 'Selecione...';
        select.appendChild(emptyOption);

        data.forEach(item => {
            const option = document.createElement('option');
            option.value = item.value || item.id;
            option.text = item.text || item.name;

            if (item.attributes) {
                Object.keys(item.attributes).forEach(key => {
                    option.setAttribute(key, item.attributes[key]);
                });
            }

            select.appendChild(option);
        });

        if (typeof $.fn.select2 !== 'undefined' && $(select).hasClass('select2')) {
            $(select).trigger('change');
        }
    }

    function initializeFormDependencies() {
        const triggerElements = document.querySelectorAll('[data-toggle-target]');

        triggerElements.forEach(element => {
            const targetSelector = element.getAttribute('data-toggle-target');
            const targets = document.querySelectorAll(targetSelector);

            if (element.type === 'checkbox') {
                element.addEventListener('change', function () {
                    FinanceSystem.UI.toggleVisibility(targets, this.checked);
                });

                FinanceSystem.UI.toggleVisibility(targets, element.checked);
            } else if (element.tagName === 'SELECT') {
                element.addEventListener('change', function () {
                    const showValue = element.getAttribute('data-toggle-value');
                    const isMatch = showValue === this.value;
                    FinanceSystem.UI.toggleVisibility(targets, isMatch);
                });

                const showValue = element.getAttribute('data-toggle-value');
                FinanceSystem.UI.toggleVisibility(targets, showValue === element.value);
            }
        });
    }

    function clearForm(form, excludeSelectors = []) {
        if (!form) return;

        const fields = form.querySelectorAll('input, select, textarea');

        fields.forEach(field => {
            let excluded = false;
            for (let selector of excludeSelectors) {
                if (field.matches(selector)) {
                    excluded = true;
                    break;
                }
            }

            if (excluded) return;

            switch (field.type) {
                case 'checkbox':
                case 'radio':
                    field.checked = false;
                    break;
                case 'select-one':
                case 'select-multiple':
                    field.selectedIndex = 0;
                    if (typeof $.fn.select2 !== 'undefined' && $(field).hasClass('select2')) {
                        $(field).val(null).trigger('change');
                    }
                    break;
                default:
                    field.value = '';
            }

            field.classList.remove('is-invalid', 'is-valid');
        });

        const errorMessages = form.querySelectorAll('.text-danger, .invalid-feedback');
        errorMessages.forEach(message => {
            message.textContent = '';
        });

        form.classList.remove('was-validated');
    }

    function populateForm(form, data) {
        if (!form || !data) return;

        Object.keys(data).forEach(key => {
            const field = form.querySelector(`[name="${key}"], #${key}`);
            if (!field) return;

            const value = data[key];

            switch (field.type) {
                case 'checkbox':
                    field.checked = Boolean(value);
                    field.dispatchEvent(new Event('change'));
                    break;
                case 'radio':
                    const radio = form.querySelector(`[name="${key}"][value="${value}"]`);
                    if (radio) {
                        radio.checked = true;
                        radio.dispatchEvent(new Event('change'));
                    }
                    break;
                case 'select-one':
                case 'select-multiple':
                    field.value = value;
                    if (typeof $.fn.select2 !== 'undefined' && $(field).hasClass('select2')) {
                        $(field).val(value).trigger('change');
                    } else {
                        field.dispatchEvent(new Event('change'));
                    }
                    break;
                default:
                    field.value = value === null || value === undefined ? '' : value;
            }
        });
    }

    function getFormData(form) {
        if (!form) return {};

        const formData = new FormData(form);
        const data = {};

        formData.forEach((value, key) => {
            if (data[key] !== undefined) {
                if (!Array.isArray(data[key])) {
                    data[key] = [data[key]];
                }
                data[key].push(value);
            } else {
                data[key] = value;
            }
        });

        return data;
    }

    // Initialize common form fields for one specific form
    function initializeForm(form, options = {}) {
        if (!form) return;

        // Money inputs initialization
        if (options.moneyInputs) {
            options.moneyInputs.forEach(selector => {
                const input = form.querySelector(selector);
                if (input) {
                    FinanceSystem.Modules.Financial.initializeMoneyMask(input);
                }
            });
        }

        // Field validations
        if (options.validations) {
            options.validations.forEach(validation => {
                const field = form.querySelector(validation.selector);
                if (field) {
                    field.addEventListener('change', function () {
                        switch (validation.type) {
                            case 'range':
                                FinanceSystem.Validation.validateNumericRange(
                                    this,
                                    validation.min,
                                    validation.max,
                                    validation.name
                                );
                                break;
                            case 'custom':
                                if (typeof validation.validator === 'function') {
                                    validation.validator(this);
                                }
                                break;
                        }
                    });
                }
            });
        }

        // Setup form validation
        if (options.validate) {
            FinanceSystem.Validation.setupFormValidation(form, options.validate);
        }
    }

    return {
        initialize: initialize,
        initializeFormatedInputs: initializeFormatedInputs,
        formatDateInput: formatDateInput,
        formatCPFInput: formatCPFInput,
        formatCNPJInput: formatCNPJInput,
        formatPhoneInput: formatPhoneInput,
        populateSelect: populateSelect,
        clearForm: clearForm,
        populateForm: populateForm,
        getFormData: getFormData,
        initializeForm: initializeForm
    };
})();
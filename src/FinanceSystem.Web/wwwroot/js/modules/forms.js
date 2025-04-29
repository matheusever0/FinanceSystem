/**
 * Finance System - Forms Module
 * Funções reutilizáveis para manipulação de formulários
 */

// Namespace global para o sistema
var FinanceSystem = FinanceSystem || {};
FinanceSystem.Modules = FinanceSystem.Modules || {};

// Módulo Forms
FinanceSystem.Modules.Forms = (function () {
    /**
     * Inicializa o módulo de formulários
     */
    function initialize() {
        initializeFormatedInputs();
        initializeSelectFields();
        initializeFormDependencies();
    }

    /**
     * Inicializa campos formatados (máscaras)
     */
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
        } else {
            // Inicialização manual de máscaras se a biblioteca não estiver disponível
            initializeManualMasks();
        }
    }

    /**
     * Inicializa máscaras manualmente quando jQuery mask não está disponível
     */
    function initializeManualMasks() {
        // Campos de moeda
        const moneyInputs = document.querySelectorAll('.mask-money');
        moneyInputs.forEach(input => {
            input.addEventListener('input', function (e) {
                formatCurrencyInput(this);
            });

            // Formata valor inicial se existir
            if (input.value) {
                formatCurrencyInput(input);
            }
        });

        // Campos de percentual
        const percentInputs = document.querySelectorAll('.mask-percent');
        percentInputs.forEach(input => {
            input.addEventListener('input', function (e) {
                formatPercentInput(this);
            });

            // Formata valor inicial se existir
            if (input.value) {
                formatPercentInput(input);
            }
        });

        // Campos de data
        const dateInputs = document.querySelectorAll('.mask-date');
        dateInputs.forEach(input => {
            input.addEventListener('input', function (e) {
                formatDateInput(this);
            });

            // Formata valor inicial se existir
            if (input.value) {
                formatDateInput(input);
            }
        });

        // Campos de CPF
        const cpfInputs = document.querySelectorAll('.mask-cpf');
        cpfInputs.forEach(input => {
            input.addEventListener('input', function (e) {
                formatCPFInput(this);
            });

            // Formata valor inicial se existir
            if (input.value) {
                formatCPFInput(input);
            }
        });

        // Campos de CNPJ
        const cnpjInputs = document.querySelectorAll('.mask-cnpj');
        cnpjInputs.forEach(input => {
            input.addEventListener('input', function (e) {
                formatCNPJInput(this);
            });

            // Formata valor inicial se existir
            if (input.value) {
                formatCNPJInput(input);
            }
        });

        // Campos de telefone
        const phoneInputs = document.querySelectorAll('.mask-phone');
        phoneInputs.forEach(input => {
            input.addEventListener('input', function (e) {
                formatPhoneInput(this);
            });

            // Formata valor inicial se existir
            if (input.value) {
                formatPhoneInput(input);
            }
        });
    }

    /**
     * Formata campo de entrada monetária
     * @param {HTMLElement} input - Campo a ser formatado
     */
    function formatCurrencyInput(input) {
        // Preserva a posição do cursor
        const cursorPosition = input.selectionStart;
        const inputLength = input.value.length;

        // Remove caracteres não numéricos, exceto vírgula e ponto
        let value = input.value.replace(/[^\d.,]/g, '');

        // Converte para o formato brasileiro (vírgula como separador decimal)
        value = value.replace(/\D/g, '');
        if (value === '') {
            input.value = '';
            return;
        }

        value = (parseFloat(value) / 100).toFixed(2);
        input.value = value.replace('.', ',');

        // Ajusta a posição do cursor se necessário
        const newLength = input.value.length;
        const newPosition = cursorPosition + (newLength - inputLength);
        if (newPosition >= 0) {
            input.setSelectionRange(newPosition, newPosition);
        }
    }

    /**
     * Formata campo de entrada percentual
     * @param {HTMLElement} input - Campo a ser formatado
     */
    function formatPercentInput(input) {
        let value = input.value.replace(/[^\d.,]/g, '');

        // Se tem vírgula, limita a duas casas decimais
        if (value.includes(',')) {
            const parts = value.split(',');
            if (parts[1].length > 2) {
                parts[1] = parts[1].substring(0, 2);
                value = parts.join(',');
            }
        }

        // Adiciona o símbolo de percentual
        if (value && !value.includes('%')) {
            value = value + '%';
        }

        input.value = value;
    }

    /**
     * Formata campo de entrada de data
     * @param {HTMLElement} input - Campo a ser formatado
     */
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

    /**
     * Formata campo de entrada de CPF
     * @param {HTMLElement} input - Campo a ser formatado
     */
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

    /**
     * Formata campo de entrada de CNPJ
     * @param {HTMLElement} input - Campo a ser formatado
     */
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

    /**
     * Formata campo de entrada de telefone
     * @param {HTMLElement} input - Campo a ser formatado
     */
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

    /**
     * Inicializa campos select
     */
    function initializeSelectFields() {
        // Verifica se há elementos select2 na página
        if (typeof $.fn.select2 !== 'undefined') {
            $('.select2').select2({
                width: '100%',
                language: 'pt-BR'
            });
        }

        // Manipulação de eventos em campos select dependentes
        initializeSelectDependencies();
    }

    /**
     * Inicializa dependências entre selects
     */
    function initializeSelectDependencies() {
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

    /**
     * Atualiza select dependente com base no valor do select pai
     * @param {HTMLSelectElement} select - Select dependente
     * @param {string} parentValue - Valor do select pai
     */
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

    /**
     * Preenche um select com opções
     * @param {HTMLSelectElement} select - Select a ser preenchido
     * @param {Array} data - Array de dados para as opções
     */
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

    /**
     * Inicializa dependências entre campos de formulário
     */
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

    /**
     * Alterna a visibilidade dos elementos alvo
     * @param {NodeList} targets - Elementos alvo
     * @param {boolean} show - Indica se deve mostrar ou esconder
     */
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

    /**
     * Inicializa uma máscara de moeda em um campo específico
     * @param {string} selector - Seletor do campo
     */
    function initializeMoneyMask(selector) {
        const moneyInput = document.querySelector(selector);
        if (!moneyInput) return;

        if (typeof $.fn.mask !== 'undefined') {
            $(moneyInput).mask('#.##0,00', { reverse: true });
        } else {
            // Implementação manual se mask não estiver disponível
            moneyInput.addEventListener('input', function () {
                formatCurrencyInput(this);
            });

            // Formata valor inicial se existir
            if (moneyInput.value) {
                formatCurrencyInput(moneyInput);
            }
        }
    }

    /**
     * Inicializa um toggle para recorrência (usado em formulários de pagamento/receita)
     * @param {HTMLElement} form - Formulário contendo o toggle
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
     * Inicializa um toggle de status (ativo/inativo)
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
     * Limpa os campos de um formulário
     * @param {HTMLFormElement} form - Formulário a ser limpo
     * @param {Array} excludeSelectors - Array de seletores para campos que não devem ser limpos
     */
    function clearForm(form, excludeSelectors = []) {
        if (!form) return;

        // Seleciona todos os campos do formulário
        const fields = form.querySelectorAll('input, select, textarea');

        fields.forEach(field => {
            // Verifica se o campo deve ser excluído da limpeza
            let excluded = false;
            for (let selector of excludeSelectors) {
                if (field.matches(selector)) {
                    excluded = true;
                    break;
                }
            }

            if (excluded) return;

            // Limpa o campo de acordo com seu tipo
            switch (field.type) {
                case 'checkbox':
                case 'radio':
                    field.checked = false;
                    break;
                case 'select-one':
                case 'select-multiple':
                    field.selectedIndex = 0;
                    // Atualiza select2 se estiver sendo usado
                    if (typeof $.fn.select2 !== 'undefined' && $(field).hasClass('select2')) {
                        $(field).val(null).trigger('change');
                    }
                    break;
                default:
                    field.value = '';
            }

            // Remove classes de validação
            field.classList.remove('is-invalid', 'is-valid');
        });

        // Limpa mensagens de erro
        const errorMessages = form.querySelectorAll('.text-danger, .invalid-feedback');
        errorMessages.forEach(message => {
            message.textContent = '';
        });

        // Remove classe de validação do formulário
        form.classList.remove('was-validated');
    }

    /**
     * Preenche um formulário com dados
     * @param {HTMLFormElement} form - Formulário a ser preenchido
     * @param {Object} data - Objeto com os dados para preenchimento
     */
    function populateForm(form, data) {
        if (!form || !data) return;

        // Itera sobre as propriedades do objeto de dados
        Object.keys(data).forEach(key => {
            // Procura o campo no formulário
            const field = form.querySelector(`[name="${key}"], #${key}`);
            if (!field) return;

            const value = data[key];

            // Preenche o campo de acordo com seu tipo
            switch (field.type) {
                case 'checkbox':
                    field.checked = Boolean(value);
                    // Dispara evento change para atualizar dependências
                    field.dispatchEvent(new Event('change'));
                    break;
                case 'radio':
                    const radio = form.querySelector(`[name="${key}"][value="${value}"]`);
                    if (radio) {
                        radio.checked = true;
                        // Dispara evento change para atualizar dependências
                        radio.dispatchEvent(new Event('change'));
                    }
                    break;
                case 'select-one':
                case 'select-multiple':
                    field.value = value;
                    // Atualiza select2 se estiver sendo usado
                    if (typeof $.fn.select2 !== 'undefined' && $(field).hasClass('select2')) {
                        $(field).val(value).trigger('change');
                    } else {
                        // Dispara evento change para atualizar dependências
                        field.dispatchEvent(new Event('change'));
                    }
                    break;
                default:
                    field.value = value === null || value === undefined ? '' : value;
            }
        });
    }

    /**
     * Coleta os dados de um formulário em um objeto
     * @param {HTMLFormElement} form - Formulário a ser processado
     * @returns {Object} - Objeto com os dados do formulário
     */
    function getFormData(form) {
        if (!form) return {};

        const formData = new FormData(form);
        const data = {};

        formData.forEach((value, key) => {
            // Verifica se já existe uma propriedade com esta chave
            if (data[key] !== undefined) {
                // Se já existe, converte para array se ainda não for
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

    // API pública do módulo
    return {
        initialize: initialize,
        initializeFormatedInputs: initializeFormatedInputs,
        initializeMoneyMask: initializeMoneyMask,
        initializeRecurringToggle: initializeRecurringToggle,
        initializeStatusToggle: initializeStatusToggle,
        formatCurrencyInput: formatCurrencyInput,
        populateSelect: populateSelect,
        toggleTargetVisibility: toggleTargetVisibility,
        clearForm: clearForm,
        populateForm: populateForm,
        getFormData: getFormData
    };
})();
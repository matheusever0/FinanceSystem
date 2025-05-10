/**
 * Equilibrium Finance System - Forms Enhanced
 * Módulo aprimorado para gerenciamento de formulários
 */

// Namespace global para o sistema
var FinanceSystem = FinanceSystem || {};
FinanceSystem.Modules = FinanceSystem.Modules || {};

// Módulo FormsEnhanced
FinanceSystem.Modules.FormsEnhanced = (function () {
    /**
     * Inicializa o módulo de formulários aprimorado
     */
    function initialize() {
        // Verifica se o módulo principal já existe
        if (FinanceSystem.Modules.Forms && typeof FinanceSystem.Modules.Forms.initialize === 'function') {
            // Já inicializado, não faz nada
            return;
        }

        initializeFormatedInputs();
        initializeSelectFields();
        initializeFormDependencies();
    }

    /**
     * Inicializa máscaras de entrada para diferentes tipos de campos
     */
    function initializeFormatedInputs() {
        // Verifica se a biblioteca de máscaras está disponível
        if (FinanceSystem.Utils.isLibraryAvailable('jquery.mask')) {
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
                FinanceSystem.Utils.formatCurrencyInput(this);
            });

            // Formata valor inicial se existir
            if (input.value) {
                FinanceSystem.Utils.formatCurrencyInput(input);
            }
        });

        // Campos de percentual
        const percentInputs = document.querySelectorAll('.mask-percent');
        percentInputs.forEach(input => {
            input.addEventListener('input', function (e) {
                FinanceSystem.Utils.formatPercentInput(this);
            });

            // Formata valor inicial se existir
            if (input.value) {
                FinanceSystem.Utils.formatPercentInput(input);
            }
        });

        // Outros tipos de campos (CPF, CNPJ, data, etc.)
        initializeCustomMasks();
    }

    /**
     * Inicializa máscaras customizadas (versão reduzida)
     */
    function initializeCustomMasks() {
        // Data, CPF, CNPJ, etc.
        // Implementação simplificada, mantenha apenas o essencial
    }

    /**
     * Inicializa uma máscara de moeda em um campo específico
     * @param {string} selector - Seletor do campo
     */
    function initializeMoneyMask(selector) {
        const moneyInput = document.querySelector(selector);
        if (!moneyInput) return;

        if (FinanceSystem.Utils.isLibraryAvailable('jquery.mask')) {
            $(moneyInput).mask('#.##0,00', { reverse: true });
        } else {
            // Implementação manual se mask não estiver disponível
            moneyInput.addEventListener('input', function () {
                FinanceSystem.Utils.formatCurrencyInput(this);
            });

            // Formata valor inicial se existir
            if (moneyInput.value) {
                FinanceSystem.Utils.formatCurrencyInput(moneyInput);
            }
        }
    }

    /**
     * Inicializa campos select
     */
    function initializeSelectFields() {
        // Verifica se há elementos select2 na página
        if (FinanceSystem.Utils.isLibraryAvailable('select2')) {
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
        if (FinanceSystem.Utils.isLibraryAvailable('select2') && $(select).hasClass('select2')) {
            $(select).trigger('change');
        }
    }

    /**
     * Configura validação de formulário
     * @param {HTMLFormElement} form - Formulário a ser validado
     * @param {Function} validateCallback - Função de validação customizada
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
     * Função para validar um formulário
     * @param {HTMLFormElement} form - Formulário a ser validado
     * @returns {boolean} - Resultado da validação
     */
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

    /**
     * Valida um campo individual
     * @param {HTMLElement} field - Campo a ser validado
     * @returns {boolean} - Resultado da validação
     */
    function validateField(field) {
        const value = field.value;
        const type = field.type;
        const required = field.required;

        // Verifica se é um campo obrigatório vazio
        if (required && isEmpty(value)) {
            FinanceSystem.Utils.showFieldError(field, 'Este campo é obrigatório');
            return false;
        }

        // Se o campo estiver vazio e não for obrigatório, é válido
        if (isEmpty(value) && !required) {
            return true;
        }

        // Verificações por tipo de campo (email, senha, etc)
        // Implementar conforme necessário
        
        // Passou por todas as validações
        FinanceSystem.Utils.clearFieldError(field);
        return true;
    }

    /**
     * Verifica se uma string está vazia
     * @param {string} value - String a ser verificada
     * @returns {boolean} - Resultado da verificação
     */
    function isEmpty(value) {
        return value === null || value === undefined || value.trim() === '';
    }

    // API pública do módulo
    return {
        initialize: initialize,
        initializeFormatedInputs: initializeFormatedInputs,
        initializeMoneyMask: initializeMoneyMask,
        initializeRecurringToggle: initializeRecurringToggle,
        initializeStatusToggle: initializeStatusToggle,
        populateSelect: populateSelect,
        toggleTargetVisibility: toggleTargetVisibility,
        setupFormValidation: setupFormValidation,
        validateForm: validateForm,
        validateField: validateField
    };
})();

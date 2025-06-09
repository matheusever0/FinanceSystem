var FinanceSystem = FinanceSystem || {};
FinanceSystem.Core = FinanceSystem.Core || {};

FinanceSystem.Core.DeleteHelper = (function () {
    function initialize() {
        initializeDeleteButtons();
        replaceLegacyDeleteFunctions();
    }

    function replaceLegacyDeleteFunctions() {
        if (window.confirmDelete) {
            console.warn('Legacy confirmDelete function detected - will be replaced');
            window.confirmDelete = function () {
                console.error('Use _DeleteButton partial instead');
            };
        }
    }

    function handleDeleteFromConfig(button) {
        try {
            const configJson = button.getAttribute('data-delete-config');
            if (!configJson) {
                console.error('Configuração de delete não encontrada');
                return;
            }

            const config = JSON.parse(configJson);
            console.log('Config carregada:', config);

            if (!validateEntitySpecificRules(config)) {
                return;
            }

            showDeleteConfirmation(config);
        } catch (error) {
            console.error('Erro ao processar configuração de delete:', error);
            alert('Erro interno. Tente novamente.');
        }
    }

    function handleDeleteClick(event) {
        event.preventDefault();

        const button = event.currentTarget;
        const config = extractDeleteConfig(button);

        if (!validateEntitySpecificRules(config)) {
            return;
        }

        showDeleteConfirmation(config);
    }

    function validateEntitySpecificRules(config) {
        if (config.checkStatus) {
            const statusElement = document.querySelector(`[data-entity-status]`);
            if (statusElement) {
                const currentStatus = statusElement.getAttribute('data-entity-status');
                const allowedStatuses = config.allowedStatuses ? config.allowedStatuses.split(',') : [];

                if (allowedStatuses.length > 0 && !allowedStatuses.includes(currentStatus)) {
                    alert(`Não é possível excluir ${config.entityName} com status atual.`);
                    return false;
                }
            }
        }
        return true;
    }

    function extractDeleteConfig(button) {
        const config = {
            id: button.getAttribute('data-delete-id'),
            controller: button.getAttribute('data-delete-controller'),
            entityName: button.getAttribute('data-delete-entity'),
            description: button.getAttribute('data-delete-description'),
            customMessage: button.getAttribute('data-delete-message'),
            redirectUrl: button.getAttribute('data-delete-redirect') || null,
            checkStatus: button.getAttribute('data-delete-check-status') === 'true',
            statusField: button.getAttribute('data-delete-status-field') || 'Status',
            allowedStatuses: button.getAttribute('data-delete-allowed-statuses') || null,
            confirmationRequired: button.getAttribute('data-delete-confirmation') !== 'false'
        };

        console.log('Delete config extracted:', config);

        return config;
    }

    function showDeleteConfirmation(config) {
        let message = '';

        if (config.customMessage && config.customMessage.trim() !== '') {
            message = config.customMessage;
        } else {
            message = `Tem certeza que deseja excluir ${config.entityName} "${config.description}"?\n\nEsta ação não pode ser desfeita.`;

            if (config.entityName === 'pagamento') {
                message += '\n\nTodas as parcelas associadas também serão excluídas.';
            } else if (config.entityName === 'receita') {
                message += '\n\nTodas as parcelas associadas também serão excluídas.';
            } else if (config.entityName === 'cartão de crédito') {
                message += '\n\nTodas as transações associadas serão mantidas, mas não será possível criar novas.';
            }
        }

        console.log('Delete message:', message);
        console.log('Config completa:', config);

        if (config.confirmationRequired !== false && !confirm(message)) {
            return;
        }

        executeDelete(config);
    }

    function executeDelete(config) {
        const form = createDeleteForm(config);
        document.body.appendChild(form);
        form.submit();
    }

    function createDeleteForm(config) {
        const form = document.createElement('form');
        form.method = 'POST';
        form.action = `/${config.controller}/Delete/${config.id}`;

        const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
        if (tokenElement) {
            const tokenInput = document.createElement('input');
            tokenInput.type = 'hidden';
            tokenInput.name = '__RequestVerificationToken';
            tokenInput.value = tokenElement.value;
            form.appendChild(tokenInput);
        }

        if (config.redirectUrl) {
            const redirectInput = document.createElement('input');
            redirectInput.type = 'hidden';
            redirectInput.name = 'redirectUrl';
            redirectInput.value = config.redirectUrl;
            form.appendChild(redirectInput);
        }

        return form;
    }

    function initializeDeleteButtons() {
        document.querySelectorAll('[data-delete-action]').forEach(button => {
            button.addEventListener('click', handleDeleteClick);
        });

    }

    function attachDeleteHandler(selector, config) {
        const elements = document.querySelectorAll(selector);
        elements.forEach(element => {
            Object.keys(config).forEach(key => {
                element.setAttribute(`data-delete-${key}`, config[key]);
            });
            element.setAttribute('data-delete-action', 'true');
            element.addEventListener('click', handleDeleteClick);
        });
    }

    return {
        initialize: initialize,
        attachDeleteHandler: attachDeleteHandler,
        handleDeleteFromConfig: handleDeleteFromConfig
    };
})();
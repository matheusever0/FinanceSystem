/**
 * Finance System - Core
 * Funções e utilidades fundamentais do sistema
 */

var FinanceSystem = FinanceSystem || {};

FinanceSystem.Core = (function () {
    /**
     * Inicializa todas as funcionalidades principais
     */
    function initialize() {
        initializeAjaxInterceptors();
    }

    /**
     * Inicializa os interceptores de AJAX para tratamento de erros
     */
    function initializeAjaxInterceptors() {
        if (typeof $ !== 'undefined' && $.ajaxError) {
            $(document).ajaxError(function (event, jqXHR, settings, thrownError) {
                if (jqXHR.status === 401) {
                    window.location.href = '/Account/Login?expired=true';
                }
            });
        }

        const originalFetch = window.fetch;
        if (originalFetch) {
            window.fetch = function (url, options) {
                return originalFetch(url, options)
                    .then(response => {
                        if (response.status === 401) {
                            window.location.href = '/Account/Login?expired=true';
                            return Promise.reject('Unauthorized');
                        }
                        return response;
                    });
            };
        }
    }

    /**
     * Formata um valor numérico como moeda
     * @param {number} value - Valor a ser formatado
     * @param {string} currency - Código da moeda (ex: 'BRL', 'USD')
     * @returns {string} - Valor formatado
     */
    function formatCurrency(value, currency = 'BRL') {
        const locales = {
            'BRL': 'pt-BR',
            'USD': 'en-US',
            'EUR': 'de-DE',
            'GBP': 'en-GB',
            'JPY': 'ja-JP'
        };

        const locale = locales[currency] || 'en-US';

        return new Intl.NumberFormat(locale, {
            style: 'currency',
            currency: currency
        }).format(value);
    }

    /**
     * Formata uma data no padrão brasileiro
     * @param {Date|string} date - Data a ser formatada
     * @returns {string} - Data formatada
     */
    function formatDate(date) {
        if (typeof date === 'string') {
            date = new Date(date);
        }
        return date.toLocaleDateString('pt-BR');
    }

    /**
     * Formata uma data com hora no padrão brasileiro
     * @param {Date|string} date - Data a ser formatada
     * @returns {string} - Data formatada
     */
    function formatDateTime(date) {
        if (typeof date === 'string') {
            date = new Date(date);
        }
        return date.toLocaleString('pt-BR');
    }

    /**
     * Formata um número como percentual
     * @param {number} value - Valor a ser formatado
     * @param {number} decimals - Número de casas decimais
     * @returns {string} - Valor formatado
     */
    function formatPercent(value, decimals = 2) {
        return value.toFixed(decimals) + '%';
    }

    /**
     * Formata um número com separadores de milhar
     * @param {number} value - Valor a ser formatado
     * @returns {string} - Valor formatado
     */
    function formatNumber(value) {
        return new Intl.NumberFormat('pt-BR').format(value);
    }

    /**
     * Formata um CPF (000.000.000-00)
     * @param {string} cpf - CPF a ser formatado
     * @returns {string} - CPF formatado
     */
    function formatCPF(cpf) {
        cpf = cpf.replace(/\D/g, '');
        if (cpf.length !== 11) return cpf;
        return cpf.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
    }

    /**
     * Formata um CNPJ (00.000.000/0000-00)
     * @param {string} cnpj - CNPJ a ser formatado
     * @returns {string} - CNPJ formatado
     */
    function formatCNPJ(cnpj) {
        cnpj = cnpj.replace(/\D/g, '');
        if (cnpj.length !== 14) return cnpj;
        return cnpj.replace(/(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})/, '$1.$2.$3/$4-$5');
    }

    /**
     * Cria um evento customizado
     * @param {string} eventName - Nome do evento
     * @param {object} detail - Detalhes do evento
     * @returns {CustomEvent} - Evento customizado
     */
    function createCustomEvent(eventName, detail = {}) {
        return new CustomEvent(eventName, {
            bubbles: true,
            cancelable: true,
            detail: detail
        });
    }

    /**
     * Dispara um evento customizado
     * @param {string} eventName - Nome do evento
     * @param {HTMLElement} element - Elemento que disparará o evento
     * @param {object} detail - Detalhes do evento
     */
    function dispatchCustomEvent(eventName, element, detail = {}) {
        const event = createCustomEvent(eventName, detail);
        element.dispatchEvent(event);
    }

    /**
     * Função para confirmar ação de exclusão
     * @param {string} formId - ID do formulário a ser submetido
     * @param {string} message - Mensagem de confirmação opcional
     * @returns {boolean} - Resultado da confirmação
     */
    function confirmDelete(formId, message) {
        message = message || 'Tem certeza que deseja excluir este item? Esta ação não pode ser desfeita.';
        if (confirm(message)) {
            const form = document.getElementById(formId);
            if (form) form.submit();
        }
        return false;
    }

    return {
        initialize: initialize,
        formatCurrency: formatCurrency,
        formatDate: formatDate,
        formatDateTime: formatDateTime,
        formatPercent: formatPercent,
        formatNumber: formatNumber,
        formatCPF: formatCPF,
        formatCNPJ: formatCNPJ,
        createCustomEvent: createCustomEvent,
        dispatchCustomEvent: dispatchCustomEvent,
        confirmDelete: confirmDelete
    };
})();
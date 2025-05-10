/**
 * Finance System - Core
 * Funções e utilidades fundamentais do sistema
 */

var FinanceSystem = FinanceSystem || {};

FinanceSystem.Core = (function () {

    function initialize() {
        initializeAjaxInterceptors();
    }

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

    function formatDate(date) {
        if (typeof date === 'string') {
            date = new Date(date);
        }
        return date.toLocaleDateString('pt-BR');
    }

    function formatDateTime(date) {
        if (typeof date === 'string') {
            date = new Date(date);
        }
        return date.toLocaleString('pt-BR');
    }

    function formatPercent(value, decimals = 2) {
        return value.toFixed(decimals) + '%';
    }

    function formatNumber(value) {
        return new Intl.NumberFormat('pt-BR').format(value);
    }

    function formatCPF(cpf) {
        cpf = cpf.replace(/\D/g, '');
        if (cpf.length !== 11) return cpf;
        return cpf.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
    }

    function formatCNPJ(cnpj) {
        cnpj = cnpj.replace(/\D/g, '');
        if (cnpj.length !== 14) return cnpj;
        return cnpj.replace(/(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})/, '$1.$2.$3/$4-$5');
    }

    function parseCurrency(value, currency = 'BRL') {
        if (typeof value === 'number') return value;

        if (!value) return 0;

        if (currency === 'BRL') {
            value = value.toString().replace(/[R$\s.]/g, '').replace(',', '.');
        } else {
            value = value.toString().replace(/[$\s,]/g, '');
        }

        return parseFloat(value) || 0;
    }

    function createCustomEvent(eventName, detail = {}) {
        return new CustomEvent(eventName, {
            bubbles: true,
            cancelable: true,
            detail: detail
        });
    }

    function dispatchCustomEvent(eventName, element, detail = {}) {
        const event = createCustomEvent(eventName, detail);
        element.dispatchEvent(event);
    }

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
        parseCurrency: parseCurrency,
        createCustomEvent: createCustomEvent,
        dispatchCustomEvent: dispatchCustomEvent,
        confirmDelete: confirmDelete
    };
})();
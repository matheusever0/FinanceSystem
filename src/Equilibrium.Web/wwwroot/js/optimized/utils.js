/**
 * Equilibrium Finance System - Utils
 * Funções utilitárias centralizadas
 */

// Namespace global para o sistema
var FinanceSystem = FinanceSystem || {};

// Módulo Utils
FinanceSystem.Utils = (function () {
    /**
     * Inicializa as funções utilitárias
     */
    function initialize() {
        // Apenas registro do módulo
        console.log("Utils module initialized");
    }

    /**
     * Formata um valor numérico como moeda
     * @param {number} value - Valor a ser formatado
     * @param {string} currency - Código da moeda (ex: 'BRL', 'USD')
     * @returns {string} - Valor formatado
     */
    function formatCurrency(value, currency = 'BRL') {
        // Define locales e formatos com base na moeda
        const locales = {
            'BRL': 'pt-BR',
            'USD': 'en-US',
            'EUR': 'de-DE',
            'GBP': 'en-GB',
            'JPY': 'ja-JP'
        };

        // Define locale padrão para outras moedas não listadas
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
     * Converte valor monetário para número
     * @param {string} value - Valor em formato monetário (ex: "R$ 1.234,56", "$1,234.56")
     * @param {string} currency - Código da moeda (ex: 'BRL', 'USD')
     * @returns {number} - Valor numérico
     */
    function parseCurrency(value, currency = 'BRL') {
        if (typeof value === 'number') {
            return value;
        }

        // Se for string vazia ou undefined, retorna 0
        if (!value) return 0;

        if (currency === 'BRL') {
            // Remove símbolos de moeda, pontos e espaços
            value = value.toString().replace(/[R$\s.]/g, '').replace(',', '.');
        } else {
            // Formato americano/internacional
            value = value.toString().replace(/[$\s,]/g, '');
        }

        return parseFloat(value) || 0;
    }

    /**
     * Formata campo de entrada monetária
     * @param {HTMLElement} input - Campo a ser formatado
     * @param {string} currency - Código da moeda (BRL, USD, etc.)
     */
    function formatCurrencyInput(input, currency = 'BRL') {
        // Preserva a posição do cursor
        const cursorPosition = input.selectionStart;
        const inputLength = input.value.length;

        // Remove caracteres não numéricos, exceto vírgula e ponto
        let value = input.value.replace(/[^\d.,]/g, '');

        if (currency === 'BRL') {
            // Formato brasileiro (vírgula como separador decimal)
            value = value.replace(/\D/g, '');
            if (value === '') {
                input.value = '';
                return;
            }

            value = (parseFloat(value) / 100).toFixed(2);
            input.value = value.replace('.', ',');
        } else {
            // Formato americano/internacional (ponto como separador decimal)
            value = value.replace(/,/g, '');
            if (value === '') {
                input.value = '';
                return;
            }

            // Remove todos os pontos exceto o último (separador decimal)
            let parts = value.split('.');
            if (parts.length > 2) {
                value = parts[0] + '.' + parts.slice(1).join('');
            }

            if (!value.includes('.')) {
                value = value.replace(/\D/g, '');
                value = (parseFloat(value) / 100).toFixed(2);
            } else if (value.endsWith('.')) {
                // Mantém o ponto decimal se for o último caractere
                value = parseFloat(value.replace(/\.$/, '')).toFixed(0) + '.';
            } else {
                // Mantém as casas decimais conforme digitado
                let [whole, decimal] = value.split('.');
                whole = whole.replace(/\D/g, '') || '0';
                decimal = decimal.replace(/\D/g, '');
                value = whole + '.' + decimal;
                if (decimal.length > 2) {
                    value = parseFloat(value).toFixed(2);
                }
            }

            input.value = value;
        }

        // Ajusta a posição do cursor se necessário
        const newLength = input.value.length;
        const newPosition = cursorPosition + (newLength - inputLength);
        if (newPosition >= 0) {
            input.setSelectionRange(newPosition, newPosition);
        }
    }

    /**
     * Formata um percentual
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

    // Função auxiliar para verificar se uma biblioteca está disponível
    function isLibraryAvailable(libraryName) {
        switch (libraryName) {
            case 'jquery':
                return typeof $ !== 'undefined';
            case 'jquery.mask':
                return typeof $.fn !== 'undefined' && typeof $.fn.mask !== 'undefined';
            case 'chart':
                return typeof Chart !== 'undefined';
            case 'bootstrap':
                return typeof bootstrap !== 'undefined';
            case 'select2':
                return typeof $.fn !== 'undefined' && typeof $.fn.select2 !== 'undefined';
            case 'datatable':
                return typeof $.fn !== 'undefined' && typeof $.fn.DataTable !== 'undefined';
            default:
                return false;
        }
    }

    /**
     * Exibe mensagem de erro para um campo
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

    /**
     * Remove mensagem de erro de um campo
     * @param {HTMLElement} input - Campo
     */
    function clearFieldError(input) {
        const errorElement = input.parentElement.querySelector('.text-danger');
        if (errorElement) {
            errorElement.innerText = '';
        }
        input.classList.remove('is-invalid');
    }

    /**
     * Inicializa um modal
     * @param {string} modalId - ID do modal
     * @returns {object|null} - Instância do modal ou null
     */
    function initializeModal(modalId) {
        const modalElement = document.getElementById(modalId);
        if (!modalElement) return null;

        if (isLibraryAvailable('bootstrap')) {
            return new bootstrap.Modal(modalElement);
        }
        return null;
    }

    /**
     * Mostra um modal
     * @param {string} modalId - ID do modal
     */
    function showModal(modalId) {
        const modal = initializeModal(modalId);
        if (modal) {
            modal.show();
        } else {
            // Fallback básico
            const modalElement = document.getElementById(modalId);
            if (modalElement) {
                modalElement.style.display = 'block';
            }
        }
    }

    /**
     * Esconde um modal
     * @param {string} modalId - ID do modal
     */
    function hideModal(modalId) {
        const modal = initializeModal(modalId);
        if (modal) {
            modal.hide();
        } else {
            // Fallback básico
            const modalElement = document.getElementById(modalId);
            if (modalElement) {
                modalElement.style.display = 'none';
            }
        }
    }

    // API pública do módulo
    return {
        initialize: initialize,
        formatCurrency: formatCurrency,
        formatDate: formatDate,
        formatDateTime: formatDateTime,
        formatPercent: formatPercent,
        parseCurrency: parseCurrency,
        formatCurrencyInput: formatCurrencyInput,
        formatPercentInput: formatPercentInput,
        isLibraryAvailable: isLibraryAvailable,
        showFieldError: showFieldError,
        clearFieldError: clearFieldError,
        initializeModal: initializeModal,
        showModal: showModal,
        hideModal: hideModal
    };
})();

/**
 * Utilitários globais para o sistema Finance System
 */

// ===== FORMATADORES =====

/**
 * Formata um valor numérico como moeda brasileira
 * @param {number} value - Valor a ser formatado
 * @returns {string} - Valor formatado
 */
function formatCurrency(value) {
    return new Intl.NumberFormat('pt-BR', {
        style: 'currency',
        currency: 'BRL'
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

// ===== VALIDADORES =====

/**
 * Verifica se um valor é um número válido
 * @param {any} value - Valor a ser verificado
 * @returns {boolean} - Resultado da verificação
 */
function isValidNumber(value) {
    return !isNaN(parseFloat(value)) && isFinite(value);
}

/**
 * Verifica se uma string está vazia
 * @param {string} value - String a ser verificada
 * @returns {boolean} - Resultado da verificação
 */
function isEmpty(value) {
    return value === null || value === undefined || value.trim() === '';
}

/**
 * Valida um email
 * @param {string} email - Email a ser validado
 * @returns {boolean} - Resultado da validação
 */
function isValidEmail(email) {
    const regex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    return regex.test(email);
}

/**
 * Valida um CPF
 * @param {string} cpf - CPF a ser validado
 * @returns {boolean} - Resultado da validação
 */
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

/**
 * Valida uma data
 * @param {string} date - Data a ser validada (formato dd/mm/yyyy)
 * @returns {boolean} - Resultado da validação
 */
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

/**
 * Valida uma senha (mínimo 6 caracteres, pelo menos 1 letra e 1 número)
 * @param {string} password - Senha a ser validada
 * @returns {boolean} - Resultado da validação
 */
function isValidPassword(password) {
    return password.length >= 6 &&
        /[a-zA-Z]/.test(password) &&
        /\d/.test(password);
}
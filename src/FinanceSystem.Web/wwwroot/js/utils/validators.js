/**
 * Utilitários de validação para o sistema Finance System
 */

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
    cpf = cpf.replace(/[^\d]/g, '');

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
 * Valida um CNPJ
 * @param {string} cnpj - CNPJ a ser validado
 * @returns {boolean} - Resultado da validação
 */
function isValidCNPJ(cnpj) {
    cnpj = cnpj.replace(/[^\d]/g, '');

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

/**
 * Valida uma data
 * @param {string} date - Data a ser validada (formato dd/mm/yyyy)
 * @returns {boolean} - Resultado da validação
 */
function isValidDate(date) {
    // Verifica formato dd/mm/yyyy
    if (!/^\d{2}\/\d{2}\/\d{4}$/.test(date)) return false;

    const parts = date.split('/');
    const day = parseInt(parts[0], 10);
    const month = parseInt(parts[1], 10);
    const year = parseInt(parts[2], 10);

    // Verifica mês válido
    if (month < 1 || month > 12) return false;

    // Verifica dias por mês
    const daysInMonth = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];

    // Ajuste para ano bissexto
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

/**
 * Valida um CEP
 * @param {string} cep - CEP a ser validado
 * @returns {boolean} - Resultado da validação
 */
function isValidCEP(cep) {
    return /^\d{5}-\d{3}$/.test(cep) || /^\d{8}$/.test(cep);
}

/**
 * Valida um telefone
 * @param {string} phone - Telefone a ser validado
 * @returns {boolean} - Resultado da validação
 */
function isValidPhone(phone) {
    phone = phone.replace(/\D/g, '');
    return phone.length >= 10 && phone.length <= 11;
}
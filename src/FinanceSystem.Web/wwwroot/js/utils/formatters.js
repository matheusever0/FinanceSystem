/**
 * Utilitários de formatação para o sistema Finance System
 */

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

/**
 * Formata um CEP (00000-000)
 * @param {string} cep - CEP a ser formatado
 * @returns {string} - CEP formatado
 */
function formatCEP(cep) {
    cep = cep.replace(/\D/g, '');
    if (cep.length !== 8) return cep;
    return cep.replace(/(\d{5})(\d{3})/, '$1-$2');
}

/**
 * Formata um telefone ((00) 00000-0000 ou (00) 0000-0000)
 * @param {string} phone - Telefone a ser formatado
 * @returns {string} - Telefone formatado
 */
function formatPhone(phone) {
    phone = phone.replace(/\D/g, '');
    if (phone.length < 10) return phone;

    if (phone.length === 11) {
        return phone.replace(/(\d{2})(\d{5})(\d{4})/, '($1) $2-$3');
    }

    return phone.replace(/(\d{2})(\d{4})(\d{4})/, '($1) $2-$3');
}
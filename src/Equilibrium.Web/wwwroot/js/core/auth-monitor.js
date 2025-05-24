/**
 * Finance System - Authentication Monitor (Versão Simplificada)
 * Monitora a autenticação e renova automaticamente quando necessário
 */

var FinanceSystem = FinanceSystem || {};

FinanceSystem.AuthMonitor = (function () {
    let authCheckInterval;
    let warningShown = false;
    const CHECK_INTERVAL = 10 * 60 * 1000; // 10 minutos
    const WARNING_THRESHOLD = 30 * 60 * 1000; // 30 minutos antes de expirar

    function initialize() {
        // Iniciar monitoramento apenas se o usuário estiver autenticado
        if (isUserAuthenticated()) {
            console.log('Iniciando monitoramento de autenticação...');
            startAuthenticationMonitoring();
        }
    }

    function isUserAuthenticated() {
        // Verificar se existe um indicador de autenticação na página
        return document.querySelector('.topbar .dropdown-toggle') !== null ||
            document.body.classList.contains('authenticated');
    }

    function startAuthenticationMonitoring() {
        // Verificar imediatamente
        checkAuthenticationStatus();

        // Configurar verificação periódica
        authCheckInterval = setInterval(checkAuthenticationStatus, CHECK_INTERVAL);

        // Verificar quando a aba volta ao foco
        document.addEventListener('visibilitychange', function () {
            if (!document.hidden) {
                console.log('Aba voltou ao foco, verificando autenticação...');
                checkAuthenticationStatus();
            }
        });
    }

    function checkAuthenticationStatus() {
        fetch('/Account/VerifyToken', {
            method: 'GET',
            credentials: 'include',
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        })
            .then(response => {
                if (response.status === 401) {
                    console.log('Token inválido, redirecionando para login...');
                    handleAuthenticationExpired();
                    return null;
                }

                if (response.ok) {
                    return response.json();
                }

                throw new Error(`Erro HTTP: ${response.status}`);
            })
            .then(data => {
                if (data && data.expiresAt) {
                    const expirationTime = new Date(data.expiresAt);
                    const now = new Date();
                    const timeUntilExpiration = expirationTime.getTime() - now.getTime();

                    console.log(`Autenticação expira em: ${Math.round(timeUntilExpiration / (1000 * 60))} minutos`);

                    // Mostrar aviso se estiver próximo da expiração
                    if (timeUntilExpiration <= WARNING_THRESHOLD && timeUntilExpiration > 0 && !warningShown) {
                        showExpirationWarning(Math.round(timeUntilExpiration / (1000 * 60)));
                    }

                    // Redirecionar se expirou
                    if (timeUntilExpiration <= 0) {
                        console.log('Autenticação expirada');
                        handleAuthenticationExpired();
                    }
                }
            })
            .catch(error => {
                console.error('Erro ao verificar status de autenticação:', error);
                // Em caso de erro, verificar novamente em 2 minutos
                setTimeout(checkAuthenticationStatus, 2 * 60 * 1000);
            });
    }

    function showExpirationWarning(minutesRemaining) {
        if (warningShown) return;

        warningShown = true;

        const message = `Sua sessão expirará em ${minutesRemaining} minutos. Clique OK para renovar automaticamente.`;

        if (confirm(message)) {
            attemptTokenRenewal();
        } else {
            // Usuário optou por não renovar, avisar novamente em 5 minutos
            setTimeout(() => {
                warningShown = false;
            }, 5 * 60 * 1000);
        }
    }

    function attemptTokenRenewal() {
        console.log('Tentando renovar autenticação...');

        const token = getAntiForgeToken();
        const formData = new FormData();
        if (token) {
            formData.append('__RequestVerificationToken', token);
        }

        fetch('/Account/RenewToken', {
            method: 'POST',
            credentials: 'include',
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: formData
        })
            .then(response => {
                if (response.ok) {
                    return response.json();
                }

                if (response.status === 401) {
                    handleAuthenticationExpired();
                    return null;
                }

                throw new Error(`Erro HTTP: ${response.status}`);
            })
            .then(data => {
                if (data && data.success) {
                    console.log('Autenticação renovada com sucesso');
                    warningShown = false; // Reset warning flag
                    showSuccessMessage('Sessão renovada automaticamente');
                }
            })
            .catch(error => {
                console.error('Erro ao renovar autenticação:', error);
                showErrorMessage('Erro ao renovar sessão. Você será redirecionado para o login em breve.');
                // Se falhou a renovação, redirecionar em 30 segundos
                setTimeout(handleAuthenticationExpired, 30000);
            });
    }

    function showSuccessMessage(message) {
        console.log('Sucesso:', message);

        // Criar e mostrar um toast/alerta simples
        const alertDiv = document.createElement('div');
        alertDiv.className = 'alert alert-success alert-dismissible fade show position-fixed';
        alertDiv.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
        alertDiv.innerHTML = `
            <i class="fas fa-check-circle me-2"></i>${message}
            <button type="button" class="btn-close" onclick="this.parentElement.remove()"></button>
        `;

        document.body.appendChild(alertDiv);

        // Remover automaticamente após 5 segundos
        setTimeout(() => {
            if (alertDiv.parentElement) {
                alertDiv.remove();
            }
        }, 5000);
    }

    function showErrorMessage(message) {
        console.error('Erro:', message);

        // Criar e mostrar um alerta de erro simples
        const alertDiv = document.createElement('div');
        alertDiv.className = 'alert alert-warning alert-dismissible fade show position-fixed';
        alertDiv.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
        alertDiv.innerHTML = `
            <i class="fas fa-exclamation-triangle me-2"></i>${message}
            <button type="button" class="btn-close" onclick="this.parentElement.remove()"></button>
        `;

        document.body.appendChild(alertDiv);

        // Remover automaticamente após 10 segundos
        setTimeout(() => {
            if (alertDiv.parentElement) {
                alertDiv.remove();
            }
        }, 10000);
    }

    function handleAuthenticationExpired() {
        console.log('Autenticação expirada, redirecionando para login...');

        // Parar monitoramento
        if (authCheckInterval) {
            clearInterval(authCheckInterval);
            authCheckInterval = null;
        }

        // Limpar dados da sessão se possível
        try {
            if (typeof sessionStorage !== 'undefined') {
                sessionStorage.clear();
            }
        } catch (e) {
            console.warn('Não foi possível limpar sessionStorage:', e);
        }

        // Mostrar mensagem e redirecionar
        showErrorMessage('Sua sessão expirou. Redirecionando para o login...');

        setTimeout(() => {
            window.location.href = '/Account/Login?expired=true';
        }, 2000);
    }

    function getAntiForgeToken() {
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        return token ? token.value : '';
    }

    function stopMonitoring() {
        if (authCheckInterval) {
            clearInterval(authCheckInterval);
            authCheckInterval = null;
        }
        console.log('Monitoramento de autenticação parado');
    }

    // Expor métodos públicos
    return {
        initialize: initialize,
        checkAuthenticationStatus: checkAuthenticationStatus,
        stopMonitoring: stopMonitoring,
        attemptTokenRenewal: attemptTokenRenewal
    };
})();

// Inicializar quando o DOM estiver pronto
document.addEventListener('DOMContentLoaded', function () {
    if (FinanceSystem.AuthMonitor) {
        FinanceSystem.AuthMonitor.initialize();
    }
});
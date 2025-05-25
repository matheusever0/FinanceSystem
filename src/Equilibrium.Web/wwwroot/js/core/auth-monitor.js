/**
 * Finance System - Authentication Monitor (Versão Corrigida)
 * Monitora a autenticação e renova automaticamente quando necessário
 */

var FinanceSystem = FinanceSystem || {};

FinanceSystem.AuthMonitor = (function () {
    let authCheckInterval;
    let warningShown = false;
    let isCheckingAuth = false;

    const CHECK_INTERVAL = 15 * 60 * 1000; // 15 minutos
    const WARNING_THRESHOLD = 30 * 60 * 1000; // 30 minutos antes de expirar
    const SESSION_EXTEND_THRESHOLD = 2 * 60 * 60 * 1000; // 2 horas antes de expirar

    function initialize() {
        // Iniciar monitoramento apenas se o usuário estiver autenticado
        if (isUserAuthenticated()) {
            startAuthenticationMonitoring();
        }
    }

    function isUserAuthenticated() {
        // Verificar se existe um indicador de autenticação na página
        return document.querySelector('.topbar .dropdown-toggle') !== null ||
            document.body.classList.contains('authenticated');
    }

    function startAuthenticationMonitoring() {
        checkAuthenticationStatus();
        authCheckInterval = setInterval(checkAuthenticationStatus, CHECK_INTERVAL);
        document.addEventListener('visibilitychange', handleVisibilityChange);
        ['click', 'keypress', 'scroll', 'mousemove'].forEach(event => {
            document.addEventListener(event, throttle(checkAuthenticationStatus, 5 * 60 * 1000), { passive: true });
        });
    }

    function handleVisibilityChange() {
        if (!document.hidden && isUserAuthenticated()) {
            setTimeout(checkAuthenticationStatus, 1000);
        }
    }

    function checkAuthenticationStatus() {
        if (isCheckingAuth) {
            return;
        }

        isCheckingAuth = true;

        // Usar XMLHttpRequest para evitar conflitos com o interceptor do fetch
        const xhr = new XMLHttpRequest();

        xhr.open('GET', '/Account/VerifyToken', true);
        xhr.setRequestHeader('X-Requested-With', 'XMLHttpRequest');
        xhr.setRequestHeader('Cache-Control', 'no-cache');

        xhr.onreadystatechange = function () {
            if (xhr.readyState === 4) {
                isCheckingAuth = false;

                if (xhr.status === 401) {
                    handleAuthenticationExpired();
                    return;
                }

                if (xhr.status === 200) {
                    try {
                        const data = JSON.parse(xhr.responseText);
                        handleAuthenticationValid(data);
                    } catch (e) {
                        console.error('❌ Erro ao processar resposta:', e);
                        scheduleNextCheck();
                    }
                } else {
                    console.warn('⚠️ Erro na verificação de autenticação:', xhr.status);
                    scheduleNextCheck();
                }
            }
        };

        xhr.onerror = function () {
            isCheckingAuth = false;
            console.error('❌ Erro de rede na verificação de autenticação');
            scheduleNextCheck();
        };

        xhr.send();
    }

    function handleAuthenticationValid(data) {
        if (data && data.expiresAt) {
            const expirationTime = new Date(data.expiresAt);
            const now = new Date();
            const timeUntilExpiration = expirationTime.getTime() - now.getTime();

            if (timeUntilExpiration <= SESSION_EXTEND_THRESHOLD && timeUntilExpiration > WARNING_THRESHOLD) {
                attemptTokenRenewal();
                return;
            }

            if (timeUntilExpiration <= WARNING_THRESHOLD && timeUntilExpiration > 0 && !warningShown) {
                showExpirationWarning(Math.round(timeUntilExpiration / (1000 * 60)));
                return;
            }

            if (timeUntilExpiration <= 0) {
                handleAuthenticationExpired();
                return;
            }
        }
    }

    function scheduleNextCheck() {
        // Verificar novamente em 2 minutos em caso de erro
        setTimeout(checkAuthenticationStatus, 2 * 60 * 1000);
    }

    function showExpirationWarning(minutesRemaining) {
        if (warningShown) return;

        warningShown = true;

        const message = `Sua sessão expirará em ${minutesRemaining} minutos. Deseja renovar automaticamente?`;

        if (confirm(message)) {
            attemptTokenRenewal();
        } else {
            // Usuário optou por não renovar, avisar novamente em 5 minutos se ainda estiver logado
            setTimeout(() => {
                warningShown = false;
            }, 5 * 60 * 1000);
        }
    }

    function attemptTokenRenewal() {

        const xhr = new XMLHttpRequest();
        const formData = new FormData();

        // Adicionar token anti-forgery se disponível
        const token = getAntiForgeToken();
        if (token) {
            formData.append('__RequestVerificationToken', token);
        }

        xhr.open('POST', '/Account/RenewToken', true);
        xhr.setRequestHeader('X-Requested-With', 'XMLHttpRequest');

        xhr.onreadystatechange = function () {
            if (xhr.readyState === 4) {
                if (xhr.status === 200) {
                    try {
                        const data = JSON.parse(xhr.responseText);
                        if (data && data.success) {
                            warningShown = false;
                            showSuccessMessage('Sessão renovada automaticamente');
                        } else {
                            handleRenewalFailure();
                        }
                    } catch (e) {
                        handleRenewalFailure();
                    }
                } else if (xhr.status === 401) {
                    handleAuthenticationExpired();
                } else {
                    handleRenewalFailure();
                }
            }
        };

        xhr.onerror = function () {
            handleRenewalFailure();
        };

        xhr.send(formData);
    }

    function handleRenewalFailure() {
        console.error('❌ Erro ao renovar autenticação');
        showErrorMessage('Erro ao renovar sessão. Você será redirecionado para o login em breve.');
        setTimeout(handleAuthenticationExpired, 30000);
    }

    function showSuccessMessage(message) {
        showAlert(message, 'success');
    }

    function showErrorMessage(message) {
        console.error('❌ Erro:', message);
        showAlert(message, 'warning');
    }

    function showAlert(message, type) {
        // Remover alertas anteriores
        const existingAlerts = document.querySelectorAll('.auth-alert');
        existingAlerts.forEach(alert => alert.remove());

        const alertDiv = document.createElement('div');
        alertDiv.className = `alert alert-${type} alert-dismissible fade show position-fixed auth-alert`;
        alertDiv.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px; max-width: 400px;';

        const iconClass = type === 'success' ? 'fa-check-circle' : 'fa-exclamation-triangle';
        alertDiv.innerHTML = `
            <i class="fas ${iconClass} me-2"></i>${message}
            <button type="button" class="btn-close" onclick="this.parentElement.remove()"></button>
        `;

        document.body.appendChild(alertDiv);

        // Auto-remover
        setTimeout(() => {
            if (alertDiv.parentElement) {
                alertDiv.remove();
            }
        }, type === 'success' ? 5000 : 10000);
    }

    function handleAuthenticationExpired() {
        stopMonitoring();

        try {
            if (typeof sessionStorage !== 'undefined') {
                sessionStorage.clear();
            }
        } catch (e) {
            console.warn('⚠️ Não foi possível limpar sessionStorage:', e);
        }

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
        document.removeEventListener('visibilitychange', handleVisibilityChange);
    }

    function throttle(func, limit) {
        let inThrottle;
        return function () {
            const args = arguments;
            const context = this;
            if (!inThrottle) {
                func.apply(context, args);
                inThrottle = true;
                setTimeout(() => inThrottle = false, limit);
            }
        };
    }

    return {
        initialize: initialize,
        checkAuthenticationStatus: checkAuthenticationStatus,
        stopMonitoring: stopMonitoring,
        attemptTokenRenewal: attemptTokenRenewal
    };
})();

document.addEventListener('DOMContentLoaded', function () {
    if (FinanceSystem.AuthMonitor) {
        FinanceSystem.AuthMonitor.initialize();
    }
});
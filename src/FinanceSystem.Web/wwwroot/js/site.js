// src/FinanceSystem.Web/wwwroot/js/site.js
// Site.js - Funções JavaScript do site

// Inicializar tooltips
document.addEventListener('DOMContentLoaded', function () {
    // Inicializar tooltips do Bootstrap
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl)
    });

    // Auto-hide para alertas
    var alertList = document.querySelectorAll('.alert-dismissible');
    alertList.forEach(function (alert) {
        setTimeout(function () {
            var bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }, 5000); // Alertas desaparecem após 5 segundos
    });
});

// Função para confirmar exclusão
function confirmDelete(formId) {
    if (confirm('Tem certeza que deseja excluir este item? Esta ação não pode ser desfeita.')) {
        document.getElementById(formId).submit();
    }
    return false;
}
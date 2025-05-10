/**
 * Equilibrium+ Finance System - Print Utilities
 */

var FinanceSystem = FinanceSystem || {};

FinanceSystem.Print = (function () {
    /**
     * Inicializa funcionalidades de impressão
     */
    function initialize() {
        if (document.body.classList.contains('print-page')) {
            setTimeout(function () {
                window.print();
            }, 500);
        }

        addPrintButtons();

        setupPrintPreview();
    }

    /**
     * Adiciona botões de impressão
     */
    function addPrintButtons() {
        const printButtons = document.querySelectorAll('.print-button');

        printButtons.forEach(button => {
            button.addEventListener('click', function (e) {
                e.preventDefault();

                const url = this.getAttribute('href') || this.getAttribute('data-url');

                if (url) {
                    const printWindow = window.open(url, '_blank');

                    if (printWindow) {
                        printWindow.addEventListener('load', function () {
                            printWindow.print();
                        });
                    }
                } else {
                    window.print();
                }
            });
        });
    }

    /**
     * Configura pré-visualização de impressão
     */
    function setupPrintPreview() {
        const previewButtons = document.querySelectorAll('.print-preview');

        previewButtons.forEach(button => {
            button.addEventListener('click', function (e) {
                e.preventDefault();

                const targetId = this.getAttribute('data-target');
                const printContent = document.getElementById(targetId);

                if (!printContent) return;

                const iframe = document.createElement('iframe');
                iframe.style.display = 'none';
                document.body.appendChild(iframe);

                const iframeWindow = iframe.contentWindow;
                iframeWindow.document.open();
                iframeWindow.document.write(`
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <title>Pré-visualização de Impressão</title>
                        <link rel="stylesheet" href="/css/print.css">
                        <style>
                            body { 
                                margin: 20px;
                                font-family: Arial, sans-serif;
                            }
                        </style>
                    </head>
                    <body>
                        ${printContent.innerHTML}
                        <script>
                            window.onload = function() {
                                window.print();
                                setTimeout(function() {
                                    window.close();
                                }, 100);
                            };
                        </script>
                    </body>
                    </html>
                `);
                iframeWindow.document.close();
            });
        });
    }

    /**
     * Imprime seção específica da página
     * @param {string} sectionId - ID da seção a ser impressa
     */
    function printSection(sectionId) {
        const section = document.getElementById(sectionId);
        if (!section) return;

        const originalContent = document.body.innerHTML;

        document.body.innerHTML = `
            <div class="printHeader">
                <h1>Equilibrium+</h1>
                <h2>${document.title}</h2>
                <p>Data de Impressão: ${new Date().toLocaleString('pt-BR')}</p>
            </div>
            ${section.innerHTML}
            <div class="printFooter">
                <p>Equilibrium+ Finance System - &copy; ${new Date().getFullYear()}</p>
            </div>
        `;

        window.print();

        document.body.innerHTML = originalContent;

        initialize();
    }

    return {
        initialize: initialize,
        printSection: printSection
    };
})();

document.addEventListener('DOMContentLoaded', function () {
    if (FinanceSystem.Print) {
        FinanceSystem.Print.initialize();
    }

    if (document.body.classList.contains('print-page')) {
        setTimeout(function () {
            window.print();
        }, 500);
    }
});
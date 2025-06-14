var FinanceSystem = FinanceSystem || {};
FinanceSystem.Modules = FinanceSystem.Modules || {};

FinanceSystem.Modules.Version = (function () {
    let versionData = null;
    let isLoading = false;

    function initialize() {
        const versionElements = document.querySelectorAll('[data-version]');
        if (versionElements.length > 0) {
            loadVersion();
        }
    }

    async function loadVersion() {
        if (isLoading || versionData) {
            return versionData;
        }

        try {
            isLoading = true;
            showLoadingState();

            const response = await fetch('/account/GetVersion', {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                }
            });


            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            versionData = await response.json();


            updateVersionElements();
            return versionData;

        } catch (error) {
            showErrorState();
            return null;
        } finally {
            isLoading = false;
        }
    }

    function showLoadingState() {
        const elements = document.querySelectorAll('[data-version]');
        elements.forEach(element => {
            const type = element.dataset.version;

            if (type === 'full') {
                element.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>Carregando versão...';
            } else {
                element.textContent = '...';
            }

            element.classList.add('text-muted');
        });
    }

    function showErrorState() {
        const elements = document.querySelectorAll('[data-version]');
        elements.forEach(element => {
            const type = element.dataset.version;

            if (type === 'full') {
                element.innerHTML = '<i class="fas fa-exclamation-triangle me-1"></i>Erro ao carregar versão';
                element.classList.add('text-warning');
            } else {
                element.textContent = 'N/A';
                element.classList.add('text-muted');
            }
        });
    }

    function updateVersionElements() {
        if (!versionData) {
            return;
        }

        const elements = document.querySelectorAll('[data-version]');

        elements.forEach((element, index) => {
            const type = element.dataset.version;
            element.classList.remove('text-muted', 'text-warning');

            const version = versionData.version || versionData.Version;
            const buildDate = versionData.buildDate || versionData.BuildDate;
            const environment = versionData.environment || versionData.Environment || 'Production';


            switch (type) {
                case 'number':
                    element.textContent = `V${version}`;
                    break;

                case 'build-date':
                    element.textContent = buildDate;
                    break;

                case 'environment':
                    element.textContent = environment;
                    break;

                case 'full':
                    const fullHtml = `
                        <i class="fas fa-code-branch me-1"></i>
                        V${version} 
                        <small class="text-muted">(${buildDate})</small>
                    `;
                    element.innerHTML = fullHtml;
                    break;

                default:
                    element.textContent = `v${version}`;
            }
        });
    }

    function getVersion() {
        return versionData;
    }

    function refreshVersion() {
        versionData = null;
        return loadVersion();
    }

    return {
        initialize: initialize,
        loadVersion: loadVersion,
        getVersion: getVersion,
        refreshVersion: refreshVersion
    };
})();
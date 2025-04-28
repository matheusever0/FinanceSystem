using FinanceSystem.Application.Interfaces;
using FinanceSystem.Domain.Enums;

namespace FinanceSystem.Application.Services
{
    public class CorrectionIndexService : ICorrectionIndexService
    {
        private readonly HttpClient _httpClient;

        // URLs para obtenção de índices (substituir com APIs reais)
        private static readonly Dictionary<CorrectionIndexType, string> IndexUrls = new Dictionary<CorrectionIndexType, string>
        {
            { CorrectionIndexType.IPCA, "https://api.bcb.gov.br/dados/serie/bcdata.sgs.433/dados/ultimos/1?formato=json" },
            { CorrectionIndexType.TR, "https://api.bcb.gov.br/dados/serie/bcdata.sgs.226/dados/ultimos/1?formato=json" },
            { CorrectionIndexType.SELIC, "https://api.bcb.gov.br/dados/serie/bcdata.sgs.11/dados/ultimos/1?formato=json" },
            { CorrectionIndexType.IGPM, "https://api.bcb.gov.br/dados/serie/bcdata.sgs.189/dados/ultimos/1?formato=json" }
        };

        public CorrectionIndexService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<decimal> GetCurrentIndexValueAsync(CorrectionIndexType indexType)
        {
            // Para Fixed, retornamos um valor constante
            if (indexType == CorrectionIndexType.Fixed)
                return 0.005m; // 0.5% ao mês

            try
            {
                // Na implementação real, buscaria o índice de uma API
                // Aqui estamos simulando valores fixos para simplificar
                var values = new Dictionary<CorrectionIndexType, decimal>
                {
                    { CorrectionIndexType.IPCA, 0.004m }, // 0.4% ao mês
                    { CorrectionIndexType.TR, 0.001m },   // 0.1% ao mês
                    { CorrectionIndexType.SELIC, 0.0095m }, // 0.95% ao mês
                    { CorrectionIndexType.IGPM, 0.005m }  // 0.5% ao mês
                };

                return values.ContainsKey(indexType) ? values[indexType] : 0;
            }
            catch (Exception)
            {
                // Em caso de erro na API, retornamos um valor padrão de segurança
                return 0.005m; // 0.5% ao mês
            }
        }

        public async Task<decimal> GetHistoricalIndexValueAsync(CorrectionIndexType indexType, DateTime date)
        {
            // Simplificação: retornar um valor aleatório entre 0.1% e 1% para datas passadas
            Random random = new Random(date.Year * 100 + date.Month);
            return (decimal)(random.NextDouble() * 0.009 + 0.001); // Entre 0.1% e 1%
        }

        public async Task<IEnumerable<KeyValuePair<DateTime, decimal>>> GetHistoricalSeriesAsync(
            CorrectionIndexType indexType,
            DateTime startDate,
            DateTime endDate)
        {
            var result = new List<KeyValuePair<DateTime, decimal>>();

            // Simplificação: gerar valores aleatórios para cada mês no intervalo
            var currentDate = new DateTime(startDate.Year, startDate.Month, 1);
            var lastDate = new DateTime(endDate.Year, endDate.Month, 1);

            Random random = new Random(currentDate.Year * 100 + currentDate.Month);

            while (currentDate <= lastDate)
            {
                decimal value = (decimal)(random.NextDouble() * 0.009 + 0.001); // Entre 0.1% e 1%
                result.Add(new KeyValuePair<DateTime, decimal>(currentDate, value));

                currentDate = currentDate.AddMonths(1);
                random = new Random(currentDate.Year * 100 + currentDate.Month);
            }

            return result;
        }

        public async Task<decimal> GetProjectedIndexValueAsync(CorrectionIndexType indexType, int months)
        {
            // Simplificação: projeção baseada em tendência histórica recente
            // Em uma implementação real, usaríamos modelos preditivos ou fontes oficiais

            var projections = new Dictionary<CorrectionIndexType, decimal>
            {
                { CorrectionIndexType.IPCA, 0.004m + (0.0002m * months) }, // Aumento gradual
                { CorrectionIndexType.TR, 0.001m + (0.0001m * months) },
                { CorrectionIndexType.SELIC, 0.0095m - (0.0001m * months) }, // Redução gradual
                { CorrectionIndexType.IGPM, 0.005m + (0.0003m * months) },
                { CorrectionIndexType.Fixed, 0.005m } // Fixo
            };

            return projections.ContainsKey(indexType) ? projections[indexType] : 0.005m;
        }
    }
}
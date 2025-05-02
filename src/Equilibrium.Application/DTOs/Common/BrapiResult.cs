namespace Equilibrium.Application.DTOs.Common
{
    public class BrapiResult
    {
        public required string Symbol { get; set; }
        public decimal RegularMarketPrice { get; set; }
        public decimal RegularMarketChange { get; set; }
        public decimal RegularMarketChangePercent { get; set; }
        public decimal RegularMarketPreviousClose { get; set; }
        public string ShortName { get; set; }
        public string LongName { get; set; }
        public string Currency { get; set; }
    }
}

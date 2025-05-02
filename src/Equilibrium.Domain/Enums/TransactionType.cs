namespace Equilibrium.Domain.Enums
{
    public enum TransactionType
    {
        Buy = 1,     // Compra
        Sell,        // Venda
        Dividend,    // Dividendo
        Split,       // Desdobramento
        Bonus,       // Bonificação
        JCP,         // Juros sobre capital próprio
        Yield        // Rendimento (para renda fixa)
    }
}

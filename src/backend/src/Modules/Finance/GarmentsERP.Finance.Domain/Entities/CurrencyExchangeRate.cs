using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Finance.Domain.Entities;

/// <summary>
/// ACC-05: Multi-Currency Exchange Rates & Forex Realization Engine.
/// Powers multi-currency valuation (USD, EUR, GBP, BDT) and tracks realized/unrealized Forex exchange variances.
/// </summary>
public class CurrencyExchangeRate : BaseEntity
{
    public string FromCurrency { get; set; } = "USD";
    public string ToCurrency { get; set; } = "BDT";
    public decimal ExchangeRate { get; set; } = 117.50m;
    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow.Date;
    public string Source { get; set; } = "Bangladesh Bank Reference Rate";
    public bool IsActive { get; set; } = true;

    public static decimal CalculateForexGainLoss(
        decimal originalAmountInForeignCurrency,
        decimal bookingExchangeRate,
        decimal settlementExchangeRate)
    {
        // Positive = Forex Gain, Negative = Forex Loss
        return Math.Round(originalAmountInForeignCurrency * (settlementExchangeRate - bookingExchangeRate), 2);
    }
}

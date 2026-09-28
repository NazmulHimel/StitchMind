namespace GarmentsERP.Shared.Extensions;

/// <summary>
/// Price Masking Engine (AC-02 / SEC-03).
/// Strips or masks confidential unit costs, material rates, and FOB figures when queried
/// by shop-floor users (supervisors, operators) who lack 'Permission:ViewFinancialCosts'.
/// </summary>
public static class PriceMaskingExtensions
{
    public const string RestrictedMask = "[RESTRICTED]";

    public static decimal? MaskFinancialCost(this decimal value, bool canViewFinancialCosts)
        => canViewFinancialCosts ? value : null;

    public static decimal? MaskFinancialCost(this decimal? value, bool canViewFinancialCosts)
        => canViewFinancialCosts ? value : null;

    public static string MaskFinancialFormatted(this decimal value, bool canViewFinancialCosts, string currencySymbol = "$")
        => canViewFinancialCosts ? $"{currencySymbol}{value:N2}" : RestrictedMask;

    public static string MaskFinancialFormatted(this decimal? value, bool canViewFinancialCosts, string currencySymbol = "$")
        => canViewFinancialCosts && value.HasValue ? $"{currencySymbol}{value.Value:N2}" : (canViewFinancialCosts ? "-" : RestrictedMask);
}

namespace GarmentsERP.Reporting.Application.Models;

public class CustomsBondReportModel
{
    public string BondRegisterNumber { get; set; } = "BOND-REG-2026-001";
    public string UtilizationPermitNo { get; set; } = "UP-2026-BOND-8891";
    public string BuyerName { get; set; } = "Nordic Fashion Retail AB";
    public string StyleNumber { get; set; } = "ST-2026-CHINO-01";
    public string ItemCode { get; set; } = "FAB-TW-01";
    public string ItemDescription { get; set; } = "98/2 Cotton Spandex Twill 260 GSM (Navy Blue)";
    public decimal TotalCreditedYards { get; set; } = 36750m;
    public decimal TotalDebitedYards { get; set; } = 406m;
    public decimal CurrentBalanceYards { get; set; } = 36344m;
    public decimal DutySavedUSD { get; set; } = 22050m;
    public DateTime ReportDate { get; set; } = DateTime.UtcNow;
    public List<CustomsBondTransactionItem> Transactions { get; set; } = [];
}

public class CustomsBondTransactionItem
{
    public DateTime TransactionDate { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string ReferenceDocType { get; set; } = string.Empty;
    public string ReferenceDocNumber { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal BalanceAfterTransaction { get; set; }
    public string Remarks { get; set; } = string.Empty;
}

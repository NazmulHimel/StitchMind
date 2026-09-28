using GarmentsERP.Inventory.Domain.Enums;
using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Inventory.Domain.Entities;

/// <summary>
/// WHS-03: Customs Bond Register & Utilization Permit (UP) Balance Engine.
/// Tracks duty-free imported raw materials under statutory Bangladesh Customs Bond regulations.
/// Debits are issued against cutting Store Issue Notes (SIN) and reconciled against commercial Bills of Export.
/// </summary>
public class CustomsBondLedger : BaseEntity
{
    public string BondRegisterNumber { get; set; } = string.Empty;
    public string UtilizationPermitNo { get; set; } = string.Empty;
    public BondTransactionType TransactionType { get; set; }
    public string ReferenceDocType { get; set; } = "GRN";
    public string ReferenceDocNumber { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemDescription { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string UnitOfMeasure { get; set; } = "Yds";
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public decimal DutySavedUSD { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public string Remarks { get; set; } = string.Empty;
    public Guid CompanyId { get; set; }

    public static CustomsBondLedger RecordCredit(
        string bondRegisterNumber,
        string upNumber,
        string referenceDocType,
        string referenceDocNumber,
        string itemCode,
        string itemDescription,
        decimal quantity,
        decimal currentBalance,
        decimal dutySavedUSD,
        Guid companyId,
        string remarks = "Raw material inward credit under Customs Bond Utilization Permit")
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Credit quantity must be positive.", nameof(quantity));
        }

        return new CustomsBondLedger
        {
            BondRegisterNumber = bondRegisterNumber.Trim().ToUpper(),
            UtilizationPermitNo = upNumber.Trim().ToUpper(),
            TransactionType = BondTransactionType.ImportCredit,
            ReferenceDocType = referenceDocType.Trim().ToUpper(),
            ReferenceDocNumber = referenceDocNumber.Trim().ToUpper(),
            ItemCode = itemCode.Trim().ToUpper(),
            ItemDescription = itemDescription,
            Quantity = quantity,
            OpeningBalance = currentBalance,
            ClosingBalance = currentBalance + quantity,
            DutySavedUSD = dutySavedUSD,
            TransactionDate = DateTime.UtcNow,
            Remarks = remarks,
            CompanyId = companyId
        };
    }

    public static CustomsBondLedger RecordDebit(
        string bondRegisterNumber,
        string upNumber,
        string referenceDocType,
        string referenceDocNumber,
        string itemCode,
        string itemDescription,
        decimal quantity,
        decimal currentBalance,
        Guid companyId,
        string remarks = "Floor issue debit for cutting allocation")
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Debit quantity must be positive.", nameof(quantity));
        }

        if (quantity > currentBalance)
        {
            throw new InvalidOperationException(
                $"STATUTORY BOND VIOLATION: Requested issue of {quantity:N2} {itemCode} exceeds available Utilization Permit balance ({currentBalance:N2} Yds) under UP '{upNumber}'."
            );
        }

        return new CustomsBondLedger
        {
            BondRegisterNumber = bondRegisterNumber.Trim().ToUpper(),
            UtilizationPermitNo = upNumber.Trim().ToUpper(),
            TransactionType = BondTransactionType.FloorIssueDebit,
            ReferenceDocType = referenceDocType.Trim().ToUpper(),
            ReferenceDocNumber = referenceDocNumber.Trim().ToUpper(),
            ItemCode = itemCode.Trim().ToUpper(),
            ItemDescription = itemDescription,
            Quantity = quantity,
            OpeningBalance = currentBalance,
            ClosingBalance = currentBalance - quantity,
            DutySavedUSD = 0,
            TransactionDate = DateTime.UtcNow,
            Remarks = remarks,
            CompanyId = companyId
        };
    }
}

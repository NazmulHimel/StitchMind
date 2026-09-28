using GarmentsERP.Procurement.Domain.Enums;
using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Procurement.Domain.Entities;

/// <summary>
/// Comparative Statement (CS) evaluating supplier quotations against Pre-Costing budget cap (PRO-02 / AC-03).
/// </summary>
public class ComparativeStatement : BaseEntity
{
    public string CSNumber { get; set; } = string.Empty;
    public Guid? MaterialRequisitionId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemDescription { get; set; } = string.Empty;
    public decimal TotalQuantityRequired { get; set; }
    public decimal PreCostingBudgetRate { get; set; }
    public CSStatus Status { get; set; } = CSStatus.Draft;
    public Guid? AwardedSupplierId { get; set; }
    public string? AwardedSupplierName { get; set; }
    public decimal? AwardedRate { get; set; }
    public string? AwardNotes { get; set; }
    public Guid CompanyId { get; set; }

    public ICollection<ComparativeStatementLine> Lines { get; set; } = new List<ComparativeStatementLine>();

    public void AddSupplierBid(
        Guid supplierId,
        string supplierName,
        decimal quotedRate,
        int leadTimeDays,
        string paymentTerms,
        decimal qualityRating)
    {
        var variance = quotedRate - PreCostingBudgetRate;
        var passesBudget = quotedRate <= PreCostingBudgetRate;

        var line = new ComparativeStatementLine
        {
            ComparativeStatementId = Id,
            SupplierId = supplierId,
            SupplierName = supplierName,
            QuotedRate = quotedRate,
            BudgetVariance = variance,
            LeadTimeDays = leadTimeDays,
            PaymentTerms = paymentTerms,
            QualityRating = qualityRating,
            IsBudgetPassed = passesBudget,
            IsSelected = false
        };

        Lines.Add(line);
    }

    public void AwardContract(Guid supplierId, string? notes = null)
    {
        var line = Lines.FirstOrDefault(l => l.SupplierId == supplierId)
            ?? throw new InvalidOperationException($"Supplier ID '{supplierId}' is not found in the comparative statement lines.");

        foreach (var l in Lines)
        {
            l.IsSelected = (l.SupplierId == supplierId);
        }

        AwardedSupplierId = supplierId;
        AwardedSupplierName = line.SupplierName;
        AwardedRate = line.QuotedRate;
        AwardNotes = notes;
        Status = CSStatus.Awarded;
        UpdatedAt = DateTime.UtcNow;
    }
}

public class ComparativeStatementLine : BaseEntity
{
    public Guid ComparativeStatementId { get; set; }
    public ComparativeStatement? ComparativeStatement { get; set; }
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public decimal QuotedRate { get; set; }
    public decimal BudgetVariance { get; set; }
    public int LeadTimeDays { get; set; }
    public string PaymentTerms { get; set; } = string.Empty;
    public decimal QualityRating { get; set; }
    public bool IsBudgetPassed { get; set; }
    public bool IsSelected { get; set; } = false;
}

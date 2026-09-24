using GarmentsERP.Finance.Domain.Enums;
using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Finance.Domain.Entities;

public class JournalVoucher : BaseEntity
{
    public string VoucherNumber { get; set; } = string.Empty; // e.g. "JV-2026-0001"
    public DateTime VoucherDate { get; set; } = DateTime.UtcNow;
    public VoucherType Type { get; set; } = VoucherType.Journal;
    public string? ReferenceType { get; set; } // PO, GRN, PAYROLL, DISPATCH
    public string? ReferenceNumber { get; set; }
    public string Narration { get; set; } = string.Empty;
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public VoucherStatus Status { get; set; } = VoucherStatus.Draft;
    public DateTime? PostedAt { get; set; }
    public string? PostedBy { get; set; }

    public ICollection<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();
}

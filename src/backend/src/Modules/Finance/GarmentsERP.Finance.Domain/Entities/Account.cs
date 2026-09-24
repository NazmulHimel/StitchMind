using GarmentsERP.Finance.Domain.Enums;
using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Finance.Domain.Entities;

public class Account : BaseEntity
{
    public string AccountCode { get; set; } = string.Empty; // e.g. "1001", "2001", "4001"
    public string AccountName { get; set; } = string.Empty;
    public AccountCategory Category { get; set; }
    public string Currency { get; set; } = "BDT";
    public decimal OpeningBalance { get; set; }
    public decimal CurrentBalance { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }

    public ICollection<JournalEntryLine> JournalLines { get; set; } = new List<JournalEntryLine>();
}

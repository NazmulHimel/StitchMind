using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Finance.Domain.Entities;

public class JournalEntryLine : BaseEntity
{
    public Guid JournalVoucherId { get; set; }
    public JournalVoucher JournalVoucher { get; set; } = null!;
    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public string? LineNarration { get; set; }
}

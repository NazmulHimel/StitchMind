using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Production.Domain.Entities;

public class Buyer : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string PaymentTerms { get; set; } = "LC 90 Days";
    public bool IsActive { get; set; } = true;

    public ICollection<BuyerOrder> Orders { get; set; } = new List<BuyerOrder>();
}

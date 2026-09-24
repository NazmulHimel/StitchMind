using GarmentsERP.Procurement.Domain.Enums;
using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Procurement.Domain.Entities;

public class Supplier : BaseEntity
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public MaterialCategory Category { get; private set; }
    public int PaymentTermsDays { get; private set; } = 30;
    public string? TaxRegistrationNo { get; private set; }
    public decimal Rating { get; private set; } = 5.0m;
    public bool IsActive { get; private set; } = true;
    public Guid CompanyId { get; private set; }

    public ICollection<PurchaseOrder> PurchaseOrders { get; private set; } = [];

    private Supplier() { }

    public static Supplier Create(string code, string name, string email, string phone,
        string address, MaterialCategory category, Guid companyId, int paymentTerms = 30, string? taxNo = null)
    {
        return new Supplier
        {
            Code = code.Trim().ToUpper(),
            Name = name.Trim(),
            Email = email.Trim().ToLower(),
            Phone = phone.Trim(),
            Address = address.Trim(),
            Category = category,
            CompanyId = companyId,
            PaymentTermsDays = paymentTerms,
            TaxRegistrationNo = taxNo
        };
    }

    public void Update(string name, string email, string phone, string address,
        MaterialCategory category, int paymentTerms, string? taxNo)
    {
        Name = name.Trim();
        Email = email.Trim().ToLower();
        Phone = phone.Trim();
        Address = address.Trim();
        Category = category;
        PaymentTermsDays = paymentTerms;
        TaxRegistrationNo = taxNo;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetRating(decimal rating)
    {
        Rating = Math.Clamp(rating, 1.0m, 5.0m);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate() { IsActive = false; UpdatedAt = DateTime.UtcNow; }
    public void Activate() { IsActive = true; UpdatedAt = DateTime.UtcNow; }
}

using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Production.Domain.Entities;

/// <summary>
/// Matrix breakdown of colors and sizes for a buyer order/style (MOD-01).
/// </summary>
public class StyleColorSize : BaseEntity
{
    public Guid BuyerOrderId { get; set; }
    public BuyerOrder? BuyerOrder { get; set; }
    public string ColorName { get; set; } = string.Empty;
    public string PantoneCode { get; set; } = string.Empty;
    public string SizeLabel { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

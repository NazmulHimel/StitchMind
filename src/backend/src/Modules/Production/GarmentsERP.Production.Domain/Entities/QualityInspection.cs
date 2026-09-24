using GarmentsERP.Production.Domain.Enums;
using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Production.Domain.Entities;

public class QualityInspection : BaseEntity
{
    public string InspectionNumber { get; set; } = string.Empty; // e.g. QC-IN-401
    public InspectionType InspectionType { get; set; } = InspectionType.SewingInline;
    public Guid BuyerOrderId { get; set; }
    public BuyerOrder? BuyerOrder { get; set; }
    public string LotOrBatchRef { get; set; } = string.Empty;
    public int SampleSize { get; set; }
    public int MinorDefects { get; set; }
    public int MajorDefects { get; set; }
    public int CriticalDefects { get; set; }
    public decimal TotalPoints { get; set; }
    public InspectionResult Result { get; set; } = InspectionResult.Passed;
    public string InspectorName { get; set; } = string.Empty;
    public string? Comments { get; set; }
    public DateTime InspectedAt { get; set; } = DateTime.UtcNow;
}

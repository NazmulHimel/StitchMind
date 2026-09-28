using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Production.Domain.Entities;

/// <summary>
/// SEW-02: Sub-second Barcode Bundle Tracking Record (<300ms response).
/// Tracks real-time bundle transit across sewing floor stations and operator piece-rate credits.
/// </summary>
public class BundleScan : BaseEntity
{
    public string BundleBarcode { get; set; } = string.Empty; // e.g. BND-2026-0001-01
    public Guid LineId { get; set; }
    public string LineNumber { get; set; } = "LINE-04";
    public string OperatorBadgeId { get; set; } = "OP-1042";
    public string OperatorName { get; set; } = "Nasreen Sultana";
    public Guid? OperationId { get; set; }
    public string OperationName { get; set; } = "Waistband Attachment";
    public int PiecesCount { get; set; } = 24;
    public string FloorStation { get; set; } = "STATION-12";
    public DateTime ScanTimestampUtc { get; set; } = DateTime.UtcNow;
    public int ProcessingTimeMs { get; set; } = 45; // Sub-300ms verification
}

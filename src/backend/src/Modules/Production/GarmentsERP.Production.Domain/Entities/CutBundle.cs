using GarmentsERP.Production.Domain.Enums;
using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Production.Domain.Entities;

/// <summary>
/// CUT-04: Cut Component Bundle Entity with Serialized Barcoding for floor station tracking.
/// </summary>
public class CutBundle : BaseEntity
{
    public string BundleNumber { get; set; } = string.Empty; // e.g. BND-2026-0001-01
    public Guid CuttingOrderId { get; set; }
    public CuttingOrder? CuttingOrder { get; set; }
    public string StyleNumber { get; set; } = string.Empty;
    public string Size { get; set; } = "32/32";
    public string Color { get; set; } = "Khaki Beige";
    public string ShadeGroup { get; set; } = "A";
    public string ComponentName { get; set; } = "FRONT PANEL"; // Front Panel, Back Panel, Waistband, Fly, Pocket
    public int Quantity { get; set; } = 24;
    public string Barcode { get; set; } = string.Empty; // e.g. BND-2026-0001-01
    public int SerialStart { get; set; } = 1;
    public int SerialEnd { get; set; } = 24;
    public int BundleSequence { get; set; } = 1;
    public int TotalBundlesInOrder { get; set; } = 10;
    public string CurrentFloorStation { get; set; } = "CUTTING_OUTPUT";
    public BundleSection CurrentSection { get; set; } = BundleSection.Cutting;
    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Generates raw Zebra ZPL II Thermal Label script for direct industrial printer execution.
    /// </summary>
    public string GenerateZplTicket(string buyerName = "Nordic Fashion", string cutOrderNumber = "CUT-2026-0001")
    {
        var barcode = !string.IsNullOrEmpty(Barcode) ? Barcode : BundleNumber;
        return $"""
            ^XA
            ^PW600
            ^LL400
            ^FO50,30^A0N,28,28^FDSTITCHMIND FACTORY EXECUTION^FS
            ^FO50,65^A0N,22,22^FDSTYLE: {StyleNumber} | BUYER: {buyerName}^FS
            ^FO50,95^A0N,22,22^FDCOLOR: {Color} | SHADE: {ShadeGroup}^FS
            ^FO50,125^A0N,22,22^FDSIZE: {Size} | CUT: {cutOrderNumber}^FS
            ^FO50,155^A0N,24,24^FDCOMPONENT: {ComponentName}^FS
            ^FO50,185^A0N,22,22^FDBUNDLE: {BundleSequence:D2}/{TotalBundlesInOrder:D2} | QTY: {Quantity} PCS ({SerialStart:D3}-{SerialEnd:D3})^FS
            ^FO50,225^BY2,3,70^BCN,70,Y,N,N^FD{barcode}^FS
            ^XZ
            """;
    }
}

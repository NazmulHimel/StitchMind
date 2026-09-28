namespace GarmentsERP.Reporting.Application.Models;

public class BundleTicketReportModel
{
    public string CuttingOrderNumber { get; set; } = "CUT-2026-0001";
    public string StyleNumber { get; set; } = "ST-2026-CHINO-01";
    public string BuyerName { get; set; } = "Nordic Fashion AB";
    public string ColorName { get; set; } = "Khaki Beige";
    public string ShadeGroup { get; set; } = "A";
    public List<BundleTicketItem> Tickets { get; set; } = new();
}

public class BundleTicketItem
{
    public string BundleBarcode { get; set; } = "BND-2026-0001-01";
    public string ComponentName { get; set; } = "FRONT PANEL";
    public string Size { get; set; } = "32/32";
    public int Quantity { get; set; } = 24;
    public int SerialStart { get; set; } = 1;
    public int SerialEnd { get; set; } = 24;
    public int BundleSequence { get; set; } = 1;
    public int TotalBundles { get; set; } = 10;
    public string CurrentFloorStation { get; set; } = "SEWING_INPUT";
}

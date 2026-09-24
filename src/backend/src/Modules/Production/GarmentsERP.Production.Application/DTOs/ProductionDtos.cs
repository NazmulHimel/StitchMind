using GarmentsERP.Production.Domain.Enums;

namespace GarmentsERP.Production.Application.DTOs;

public record BuyerDto(
    Guid Id,
    string Name,
    string Code,
    string Country,
    string ContactPerson,
    string Email,
    string Phone,
    string PaymentTerms,
    bool IsActive,
    int TotalOrdersCount
);

public record CreateBuyerDto(
    string Name,
    string Code,
    string Country,
    string ContactPerson,
    string Email,
    string Phone,
    string PaymentTerms
);

public record BuyerOrderDto(
    Guid Id,
    string OrderNumber,
    Guid BuyerId,
    string BuyerName,
    string StyleNumber,
    string StyleName,
    string Season,
    int OrderQuantity,
    decimal UnitPrice,
    decimal TotalValue,
    DateTime OrderDate,
    DateTime DeliveryDate,
    OrderStatus Status,
    string? Notes,
    int CuttingOrdersCount,
    int TotalProducedQty
);

public record CreateBuyerOrderDto(
    Guid BuyerId,
    string StyleNumber,
    string StyleName,
    string Season,
    int OrderQuantity,
    decimal UnitPrice,
    DateTime DeliveryDate,
    string? Notes
);

public record CutBundleDto(
    Guid Id,
    string BundleNumber,
    Guid CuttingOrderId,
    string Size,
    string Color,
    int Quantity,
    string Barcode,
    int SerialStart,
    int SerialEnd,
    BundleSection CurrentSection,
    bool IsCompleted,
    DateTime? CompletedAt
);

public record CuttingOrderDto(
    Guid Id,
    string CuttingOrderNumber,
    Guid BuyerOrderId,
    string OrderNumber,
    string StyleNumber,
    string FabricType,
    int TotalRequiredQty,
    int TotalPlies,
    decimal MarkerLength,
    decimal FabricWidth,
    decimal EfficiencyPercentage,
    CuttingStatus Status,
    string TableNumber,
    DateTime CuttingDate,
    List<CutBundleDto> Bundles
);

public record CreateCuttingOrderDto(
    Guid BuyerOrderId,
    string StyleNumber,
    string FabricType,
    int TotalRequiredQty,
    int TotalPlies,
    decimal MarkerLength,
    decimal FabricWidth,
    decimal EfficiencyPercentage,
    string TableNumber
);

public record SewingLineDto(
    Guid Id,
    string LineNumber,
    string LineName,
    string Floor,
    decimal TargetEfficiency,
    int CapacityPerDay,
    string Supervisor,
    SewingLineStatus Status,
    int TodayOutput,
    int TodayTarget,
    decimal TodayEfficiency
);

public record CreateSewingLineDto(
    string LineNumber,
    string LineName,
    string Floor,
    decimal TargetEfficiency,
    int CapacityPerDay,
    string Supervisor
);

public record HourlyProductionLogDto(
    Guid Id,
    Guid SewingLineId,
    string LineNumber,
    string LineName,
    Guid BuyerOrderId,
    string OrderNumber,
    string StyleNumber,
    DateTime LogDate,
    int HourSlot,
    int TargetQty,
    int ActualOutputQty,
    int DefectCount,
    int RejectionCount,
    int OperatorCount,
    decimal EfficiencyPercentage,
    string? Notes
);

public record RecordHourlyOutputDto(
    Guid SewingLineId,
    Guid BuyerOrderId,
    DateTime LogDate,
    int HourSlot,
    int TargetQty,
    int ActualOutputQty,
    int DefectCount,
    int RejectionCount,
    int OperatorCount,
    string? Notes
);

public record QualityInspectionDto(
    Guid Id,
    string InspectionNumber,
    InspectionType InspectionType,
    Guid BuyerOrderId,
    string OrderNumber,
    string StyleNumber,
    string LotOrBatchRef,
    int SampleSize,
    int MinorDefects,
    int MajorDefects,
    int CriticalDefects,
    decimal TotalPoints,
    InspectionResult Result,
    string InspectorName,
    string? Comments,
    DateTime InspectedAt
);

public record CreateQualityInspectionDto(
    InspectionType InspectionType,
    Guid BuyerOrderId,
    string LotOrBatchRef,
    int SampleSize,
    int MinorDefects,
    int MajorDefects,
    int CriticalDefects,
    decimal TotalPoints,
    string InspectorName,
    string? Comments
);

public record ProductionMetricsDto(
    int TotalActiveOrders,
    int TotalCuttingOrders,
    int DailyCuttingOutput,
    int DailySewingOutput,
    decimal AverageEfficiency,
    decimal DefectRatePercentage,
    decimal PassRatePercentage,
    int ActiveSewingLines
);

// --- Phase 4 Packing & Dispatch DTOs ---
public record CartonPackDto(
    Guid Id,
    string CartonBarcode,
    Guid BuyerOrderId,
    string OrderNumber,
    string BuyerName,
    string StyleNumber,
    string Color,
    string SizeBreakdownJson,
    int TotalQuantityPcs,
    decimal GrossWeightKg,
    decimal NetWeightKg,
    string StatusName,
    string? AssignedPallet
);

public record DeliveryChallanDto(
    Guid Id,
    string ChallanNumber,
    Guid BuyerId,
    string BuyerName,
    string VehicleNumber,
    string DriverName,
    string DriverPhone,
    string DestinationAddress,
    int TotalCartons,
    int TotalGarmentPcs,
    decimal TotalWeightKg,
    DateTime DispatchDate,
    string StatusName,
    string? GatePassNumber,
    string? Remarks,
    List<DeliveryChallanItemDto> Items
);

public record DeliveryChallanItemDto(
    Guid Id,
    string CartonBarcode,
    string StyleNumber,
    string Color,
    int QuantityPcs
);

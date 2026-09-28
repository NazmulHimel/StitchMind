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

// --- Phase 4 Cutting DTOs ---
public record CADMarkerDto(
    Guid Id,
    string MarkerName,
    Guid? StyleId,
    string StyleNumber,
    string CADSoftware,
    decimal CuttableWidthInches,
    decimal MarkerLengthYards,
    int GarmentsPerMarker,
    string SizeDistributionRatio,
    decimal MarkerEfficiencyPct,
    decimal PlannedConsumptionPerPcYds,
    string FileFormat
);

public record CreateCADMarkerDto(
    string MarkerName,
    Guid? StyleId,
    string StyleNumber,
    string CADSoftware,
    decimal CuttableWidthInches,
    decimal MarkerLengthYards,
    int GarmentsPerMarker,
    string SizeDistributionRatio,
    decimal MarkerEfficiencyPct,
    string FileFormat
);

public record RelaxationSessionDto(
    Guid Id,
    Guid FabricRollId,
    string RollBarcode,
    string DyeLotNumber,
    string FabricType,
    DateTime UnrolledAtUtc,
    int MandatoryDurationHours,
    DateTime UnlockedAtUtc,
    bool IsUnlocked,
    int RemainingHours,
    int RemainingMinutes,
    decimal AmbientTemperatureC,
    decimal RelativeHumidityPct,
    string OperatorId,
    string StatusName
);

public record StartRelaxationSessionDto(
    Guid FabricRollId,
    string RollBarcode,
    string DyeLotNumber,
    string FabricType,
    int DurationHours,
    decimal AmbientTemperatureC,
    decimal RelativeHumidityPct,
    string OperatorId
);

public record AuthorizeSpreadingResultDto(
    bool Allowed,
    string Message,
    string CutOrderNumber,
    string TableNumber
);

public record FusingMachineLogDto(
    Guid Id,
    string CutOrderNumber,
    string MachineId,
    string ComponentName,
    string InterliningType,
    decimal TemperatureC,
    decimal PressureBar,
    decimal SpeedSeconds,
    string OperatorName,
    bool IsApproved,
    string Remarks,
    DateTime LoggedAtUtc
);

public record CreateFusingMachineLogDto(
    string CutOrderNumber,
    string MachineId,
    string ComponentName,
    string InterliningType,
    decimal TemperatureC,
    decimal PressureBar,
    decimal SpeedSeconds,
    string OperatorName,
    string Remarks
);

public record SubcontractCuttingChallanDto(
    Guid Id,
    string ChallanNumber,
    string SubcontractorName,
    string StyleNumber,
    string OrderNumber,
    decimal FabricSentYds,
    int ExpectedCutPanels,
    int ReceivedCutPanels,
    int DefectivePanels,
    int ShortageCount,
    decimal ShortagePercentage,
    string StatusName,
    DateTime DispatchedAtUtc,
    DateTime? ReceivedAtUtc,
    string ReconciledBy,
    string Remarks
);

public record CreateSubcontractCuttingChallanDto(
    string SubcontractorName,
    string StyleNumber,
    string OrderNumber,
    decimal FabricSentYds,
    int ExpectedCutPanels,
    string Remarks
);

public record ReconcileSubcontractDto(
    int ReceivedCutPanels,
    int DefectivePanels,
    string ReconciledBy,
    string? Remarks
);

// Phase 5 Sewing Floor DTOs
public record OperationBulletinDto(
    Guid Id,
    Guid? StyleId,
    string StyleNumber,
    int OperationSequence,
    string OperationName,
    string SectionCategory,
    string MachineType,
    decimal StandardMinuteValueSMV,
    int TargetPerHour,
    string RequiredSkillGrade,
    decimal PieceRatePerDzn
);

public record CreateOperationBulletinDto(
    string StyleNumber,
    int OperationSequence,
    string OperationName,
    string SectionCategory,
    string MachineType,
    decimal StandardMinuteValueSMV,
    string RequiredSkillGrade,
    decimal PieceRatePerDzn
);

public record PitchDiagramOperationDto(
    int Sequence,
    string OperationName,
    string SectionCategory,
    string MachineType,
    decimal SMV,
    int TargetPerHour,
    bool IsBottleneck,
    decimal BottleneckVariancePct
);

public record PitchDiagramDto(
    string StyleNumber,
    int TotalWorkstations,
    decimal TotalSMV,
    decimal PitchTimeMinutes,
    decimal PitchTimeSeconds,
    decimal TargetOutputPerHour,
    decimal LineEfficiencyPct,
    List<PitchDiagramOperationDto> Operations
);

public record BundleScanDto(
    Guid Id,
    string BundleBarcode,
    Guid LineId,
    string LineNumber,
    string OperatorBadgeId,
    string OperatorName,
    string OperationName,
    int PiecesCount,
    string FloorStation,
    DateTime ScanTimestampUtc,
    int ProcessingTimeMs,
    decimal OperatorPieceRateEarning
);

public record RecordBundleScanDto(
    string BundleBarcode,
    string LineNumber,
    string OperatorBadgeId,
    string OperatorName,
    string OperationName,
    int PiecesCount,
    string FloorStation
);

public record AndonBoardFeedDto(
    string LineNumber,
    string StyleNumber,
    int HourlyTarget,
    int HourlyActual,
    decimal EfficiencyPct,
    decimal DefectRateDHU,
    AndonAlertLevel AlertLevel,
    string AlertMessage,
    int ActiveWorkstations,
    int BottleneckStationCount,
    DateTime LastUpdatedUtc
);

public record SewingMachineAssetDto(
    Guid Id,
    string MachineBarcode,
    string Brand,
    string MachineModel,
    string MachineType,
    string LineNumber,
    string StationNumber,
    MachineStatus Status,
    decimal MTBFHours,
    decimal MTTRMinutes,
    DateTime? LastServicedAtUtc
);

public record CreateSewingMachineAssetDto(
    string MachineBarcode,
    string Brand,
    string MachineModel,
    string MachineType,
    string LineNumber,
    string StationNumber,
    decimal MTBFHours,
    decimal MTTRMinutes
);

public record MachineBreakdownTicketDto(
    Guid Id,
    string TicketNumber,
    string MachineBarcode,
    string LineNumber,
    string ReportedIssue,
    DateTime ReportedAtUtc,
    DateTime? ResolvedAtUtc,
    int DowntimeMinutes,
    string MechanicName,
    string RootCause,
    bool IsResolved
);

public record CreateMachineBreakdownTicketDto(
    string MachineBarcode,
    string LineNumber,
    string ReportedIssue,
    string MechanicName
);

public record ResolveBreakdownTicketDto(
    int DowntimeMinutes,
    string RootCause,
    string MechanicName
);

public record BrokenNeedleLogDto(
    Guid Id,
    string LogNumber,
    string LineNumber,
    string MachineBarcode,
    string OperatorBadgeId,
    string OperatorName,
    string BreakageReason,
    bool TipFound,
    bool ShankFound,
    bool EyeFound,
    bool AllFragmentsRecovered,
    string SupervisorVerificationBadge,
    bool ReplacementNeedleIssued,
    string ReplacementNeedleType,
    DateTime LoggedAtUtc,
    int DowntimeMinutes,
    NeedleComplianceStatus Status,
    string Remarks
);

public record CreateBrokenNeedleLogDto(
    string LineNumber,
    string MachineBarcode,
    string OperatorBadgeId,
    string OperatorName,
    string BreakageReason,
    string ReplacementNeedleType
);

public record VerifyNeedleFragmentsDto(
    string SupervisorBadge,
    bool TipFound,
    bool ShankFound,
    bool EyeFound,
    string NeedleType
);

public record VerifyNeedleFragmentsResultDto(
    bool Success,
    string Message,
    NeedleComplianceStatus Status,
    bool ReplacementNeedleIssued
);


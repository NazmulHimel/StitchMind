using GarmentsERP.Inventory.Domain.Enums;

namespace GarmentsERP.Inventory.Application.DTOs;

public record FabricRollDto(
    Guid Id,
    string RollNumber,
    Guid InventoryItemId,
    string ItemCode,
    string ItemName,
    string DyeLotNumber,
    decimal GSM,
    decimal WidthInches,
    decimal TotalLengthYards,
    decimal AvailableLengthYards,
    FabricGrade Grade,
    string GradeName,
    RollStatus Status,
    string StatusName,
    string RackLocation,
    string? SupplierRollNo,
    Guid? GoodsReceiptNoteId,
    Guid CompanyId,
    DateTime CreatedAt
);

public record CreateFabricRollRequest(
    Guid InventoryItemId,
    string DyeLotNumber,
    decimal GSM,
    decimal WidthInches,
    decimal TotalLengthYards,
    string RackLocation,
    string? SupplierRollNo = null,
    FabricGrade Grade = FabricGrade.GradeA
);

public record IssueFabricRollRequest(
    Guid RollId,
    decimal YardsToIssue,
    string CuttingOrderRef
);

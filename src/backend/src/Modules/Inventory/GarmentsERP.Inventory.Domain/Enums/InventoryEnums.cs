namespace GarmentsERP.Inventory.Domain.Enums;

public enum ItemCategory
{
    Fabric = 1,
    Trims = 2,
    Thread = 3,
    Chemical = 4,
    Packaging = 5,
    FinishedGoods = 6
}

public enum StockUOM
{
    Yards = 1,
    Meters = 2,
    Kilograms = 3,
    Pieces = 4,
    Cones = 5,
    Rolls = 6,
    Boxes = 7
}

public enum StockTransactionType
{
    Receipt = 1,
    IssueToCutting = 2,
    IssueToSewing = 3,
    ReturnFromProduction = 4,
    Adjustment = 5,
    Transfer = 6,
    Scrap = 7
}

public enum FabricGrade
{
    GradeA = 1,
    GradeB = 2,
    GradeC = 3,
    Rejected = 4
}

public enum RollStatus
{
    Available = 1,
    Reserved = 2,
    InCutting = 3,
    Consumed = 4,
    Quarantined = 5
}

public enum ThreadStage
{
    RawFiber = 1,
    Spinning = 2,
    Dyeing = 3,
    Winding = 4,
    QualityTesting = 5,
    Packaged = 6
}

public enum ThreadBatchStatus
{
    InProgress = 1,
    OnHold = 2,
    QualityPassed = 3,
    QualityFailed = 4,
    Completed = 5
}

namespace GarmentsERP.Production.Domain.Enums;

public enum OrderStatus
{
    Draft = 0,
    Confirmed = 1,
    InProduction = 2,
    Completed = 3,
    Cancelled = 4
}

public enum CuttingStatus
{
    Draft = 0,
    SpreadingLay = 1,
    InProgress = 2,
    CutCompleted = 3,
    Bundled = 4,
    Relaxing = 5,
    ReadyForCut = 6,
    IssuedToSewing = 7
}

public enum RelaxationStatus
{
    Relaxing = 0,
    Unlocked = 1,
    SpreadingAuthorized = 2
}

public enum SubcontractStatus
{
    Dispatched = 0,
    Reconciled = 1,
    VarianceUnderReview = 2
}

public enum SewingLineStatus
{
    Active = 0,
    Idle = 1,
    Maintenance = 2
}

public enum BundleSection
{
    Cutting = 0,
    Sewing = 1,
    QualityControl = 2,
    Washing = 3,
    Finishing = 4,
    Packed = 5
}

public enum InspectionType
{
    FabricFourPoint = 0,
    SewingInline = 1,
    SewingEndLine = 2,
    WashingAudit = 3,
    FinalAQL = 4
}

public enum InspectionResult
{
    Passed = 0,
    Failed = 1,
    ConditionallyPassed = 2
}

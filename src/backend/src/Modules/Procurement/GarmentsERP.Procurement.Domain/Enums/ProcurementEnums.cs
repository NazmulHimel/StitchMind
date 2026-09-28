namespace GarmentsERP.Procurement.Domain.Enums;

public enum POStatus
{
    Draft = 0,
    Submitted = 1,
    Approved = 2,
    PartiallyReceived = 3,
    Completed = 4,
    Cancelled = 5,
    LockedBudgetExceeded = 6
}

public enum MaterialCategory
{
    Fabric = 1,
    Trims = 2,
    Thread = 3,
    Chemical = 4,
    Packaging = 5,
    Accessories = 6
}

public enum UOM
{
    Yards = 1,
    Meters = 2,
    Kilograms = 3,
    Pieces = 4,
    Cones = 5,
    Rolls = 6,
    Dozens = 7
}

public enum GRNStatus
{
    Draft = 0,
    UnderInspection = 1,
    Accepted = 2,
    Rejected = 3
}

public enum RequisitionStatus
{
    Draft = 0,
    Submitted = 1,
    Approved = 2,
    Cancelled = 3
}

public enum LCType
{
    MasterLC = 1,
    BackToBackLC = 2
}

public enum LCStatus
{
    Draft = 0,
    Opened = 1,
    Transmitted = 2,
    DocumentsReceived = 3,
    Paid = 4,
    Closed = 5
}

public enum CSStatus
{
    Draft = 0,
    Evaluated = 1,
    Awarded = 2,
    Cancelled = 3
}

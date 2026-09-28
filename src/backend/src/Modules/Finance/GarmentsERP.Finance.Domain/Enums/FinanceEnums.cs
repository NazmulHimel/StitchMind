namespace GarmentsERP.Finance.Domain.Enums;

public enum AccountCategory
{
    Asset = 1,
    Liability = 2,
    Equity = 3,
    Revenue = 4,
    Expense = 5
}

public enum VoucherStatus
{
    Draft = 1,
    Posted = 2,
    Cancelled = 3
}

public enum VoucherType
{
    Journal = 1,
    Payment = 2,
    Receipt = 3,
    Contra = 4
}

public enum BillMatchStatus
{
    PendingMatch = 0,
    ApprovedForPayment = 1,
    MatchFailedLocked = 2,
    Paid = 3
}

public enum ThreeWayMatchResult
{
    Passed = 0,
    DiscrepancyFlagged = 1
}

public enum LetterOfCreditStatus
{
    Draft = 0,
    Active = 1,
    PartiallyUtilized = 2,
    FullyRealized = 3,
    Closed = 4
}

public enum BackToBackLCStatus
{
    Issued = 0,
    Accepted = 1,
    Settled = 2,
    Deferred = 3
}

public enum AgingBracket
{
    Current_0_30 = 0,
    Days_31_60 = 1,
    Days_61_90 = 2,
    Days_90_Plus = 3
}


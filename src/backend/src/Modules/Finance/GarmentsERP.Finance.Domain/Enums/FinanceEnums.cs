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

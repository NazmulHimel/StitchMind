using GarmentsERP.Finance.Application.DTOs;
using GarmentsERP.Finance.Application.Interfaces;
using GarmentsERP.Finance.Domain.Entities;
using GarmentsERP.Finance.Domain.Enums;
using GarmentsERP.Shared.Common;
using MediatR;

namespace GarmentsERP.Finance.Application.Queries;

// Accounts
public record GetAccountsQuery : IRequest<Result<List<AccountDto>>>;

public class GetAccountsQueryHandler : IRequestHandler<GetAccountsQuery, Result<List<AccountDto>>>
{
    private readonly IFinanceRepository _repository;
    public GetAccountsQueryHandler(IFinanceRepository repository) => _repository = repository;

    public async Task<Result<List<AccountDto>>> Handle(GetAccountsQuery request, CancellationToken cancellationToken)
    {
        var list = await _repository.GetAllAccountsAsync(cancellationToken);
        var dtos = list.Select(a => new AccountDto(
            a.Id,
            a.AccountCode,
            a.AccountName,
            a.Category,
            a.Category.ToString(),
            a.Currency,
            a.OpeningBalance,
            a.CurrentBalance,
            a.IsActive,
            a.Description
        )).ToList();

        return Result<List<AccountDto>>.Success(dtos);
    }
}

public record GetAccountByIdQuery(Guid Id) : IRequest<Result<AccountDto>>;

public class GetAccountByIdQueryHandler : IRequestHandler<GetAccountByIdQuery, Result<AccountDto>>
{
    private readonly IFinanceRepository _repository;
    public GetAccountByIdQueryHandler(IFinanceRepository repository) => _repository = repository;

    public async Task<Result<AccountDto>> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var a = await _repository.GetAccountByIdAsync(request.Id, cancellationToken);
        if (a == null) return Result<AccountDto>.Failure("Account not found.");

        return Result<AccountDto>.Success(new AccountDto(
            a.Id,
            a.AccountCode,
            a.AccountName,
            a.Category,
            a.Category.ToString(),
            a.Currency,
            a.OpeningBalance,
            a.CurrentBalance,
            a.IsActive,
            a.Description
        ));
    }
}

// Journal Vouchers
public record GetJournalVouchersQuery : IRequest<Result<List<JournalVoucherDto>>>;

public class GetJournalVouchersQueryHandler : IRequestHandler<GetJournalVouchersQuery, Result<List<JournalVoucherDto>>>
{
    private readonly IFinanceRepository _repository;
    public GetJournalVouchersQueryHandler(IFinanceRepository repository) => _repository = repository;

    public async Task<Result<List<JournalVoucherDto>>> Handle(GetJournalVouchersQuery request, CancellationToken cancellationToken)
    {
        var vouchers = await _repository.GetAllJournalVouchersAsync(cancellationToken);
        var dtos = vouchers.Select(v => new JournalVoucherDto(
            v.Id,
            v.VoucherNumber,
            v.VoucherDate,
            v.Type,
            v.Type.ToString(),
            v.ReferenceType,
            v.ReferenceNumber,
            v.Narration,
            v.TotalDebit,
            v.TotalCredit,
            v.Status,
            v.Status.ToString(),
            v.PostedAt,
            v.PostedBy,
            v.Lines?.Select(l => new JournalEntryLineDto(
                l.Id,
                l.JournalVoucherId,
                l.AccountId,
                l.Account?.AccountCode ?? "",
                l.Account?.AccountName ?? "",
                l.Account?.Category ?? AccountCategory.Asset,
                l.DebitAmount,
                l.CreditAmount,
                l.LineNarration
            )).ToList() ?? new()
        )).ToList();

        return Result<List<JournalVoucherDto>>.Success(dtos);
    }
}

public record GetJournalVoucherByIdQuery(Guid Id) : IRequest<Result<JournalVoucherDto>>;

public class GetJournalVoucherByIdQueryHandler : IRequestHandler<GetJournalVoucherByIdQuery, Result<JournalVoucherDto>>
{
    private readonly IFinanceRepository _repository;
    public GetJournalVoucherByIdQueryHandler(IFinanceRepository repository) => _repository = repository;

    public async Task<Result<JournalVoucherDto>> Handle(GetJournalVoucherByIdQuery request, CancellationToken cancellationToken)
    {
        var v = await _repository.GetJournalVoucherByIdAsync(request.Id, cancellationToken);
        if (v == null) return Result<JournalVoucherDto>.Failure("Journal voucher not found.");

        return Result<JournalVoucherDto>.Success(new JournalVoucherDto(
            v.Id,
            v.VoucherNumber,
            v.VoucherDate,
            v.Type,
            v.Type.ToString(),
            v.ReferenceType,
            v.ReferenceNumber,
            v.Narration,
            v.TotalDebit,
            v.TotalCredit,
            v.Status,
            v.Status.ToString(),
            v.PostedAt,
            v.PostedBy,
            v.Lines?.Select(l => new JournalEntryLineDto(
                l.Id,
                l.JournalVoucherId,
                l.AccountId,
                l.Account?.AccountCode ?? "",
                l.Account?.AccountName ?? "",
                l.Account?.Category ?? AccountCategory.Asset,
                l.DebitAmount,
                l.CreditAmount,
                l.LineNarration
            )).ToList() ?? new()
        ));
    }
}

// Reports & Statements
public record GetGeneralLedgerQuery(Guid? AccountId, DateTime? FromDate, DateTime? ToDate) : IRequest<Result<GeneralLedgerDto>>;

public class GetGeneralLedgerQueryHandler : IRequestHandler<GetGeneralLedgerQuery, Result<GeneralLedgerDto>>
{
    private readonly IFinanceRepository _repository;
    public GetGeneralLedgerQueryHandler(IFinanceRepository repository) => _repository = repository;

    public async Task<Result<GeneralLedgerDto>> Handle(GetGeneralLedgerQuery request, CancellationToken cancellationToken)
    {
        var res = await _repository.GetGeneralLedgerAsync(request.AccountId, request.FromDate, request.ToDate, cancellationToken);
        return Result<GeneralLedgerDto>.Success(res);
    }
}

public record GetTrialBalanceQuery(DateTime? AsOfDate) : IRequest<Result<TrialBalanceDto>>;

public class GetTrialBalanceQueryHandler : IRequestHandler<GetTrialBalanceQuery, Result<TrialBalanceDto>>
{
    private readonly IFinanceRepository _repository;
    public GetTrialBalanceQueryHandler(IFinanceRepository repository) => _repository = repository;

    public async Task<Result<TrialBalanceDto>> Handle(GetTrialBalanceQuery request, CancellationToken cancellationToken)
    {
        var res = await _repository.GetTrialBalanceAsync(request.AsOfDate, cancellationToken);
        return Result<TrialBalanceDto>.Success(res);
    }
}

public record GetIncomeStatementQuery(DateTime? FromDate, DateTime? ToDate) : IRequest<Result<IncomeStatementDto>>;

public class GetIncomeStatementQueryHandler : IRequestHandler<GetIncomeStatementQuery, Result<IncomeStatementDto>>
{
    private readonly IFinanceRepository _repository;
    public GetIncomeStatementQueryHandler(IFinanceRepository repository) => _repository = repository;

    public async Task<Result<IncomeStatementDto>> Handle(GetIncomeStatementQuery request, CancellationToken cancellationToken)
    {
        var res = await _repository.GetIncomeStatementAsync(request.FromDate, request.ToDate, cancellationToken);
        return Result<IncomeStatementDto>.Success(res);
    }
}

public record GetBalanceSheetQuery(DateTime? AsOfDate) : IRequest<Result<BalanceSheetDto>>;

public class GetBalanceSheetQueryHandler : IRequestHandler<GetBalanceSheetQuery, Result<BalanceSheetDto>>
{
    private readonly IFinanceRepository _repository;
    public GetBalanceSheetQueryHandler(IFinanceRepository repository) => _repository = repository;

    public async Task<Result<BalanceSheetDto>> Handle(GetBalanceSheetQuery request, CancellationToken cancellationToken)
    {
        var res = await _repository.GetBalanceSheetAsync(request.AsOfDate, cancellationToken);
        return Result<BalanceSheetDto>.Success(res);
    }
}

public record GetFinanceMetricsQuery : IRequest<Result<FinanceMetricsDto>>;

public class GetFinanceMetricsQueryHandler : IRequestHandler<GetFinanceMetricsQuery, Result<FinanceMetricsDto>>
{
    private readonly IFinanceRepository _repository;
    public GetFinanceMetricsQueryHandler(IFinanceRepository repository) => _repository = repository;

    public async Task<Result<FinanceMetricsDto>> Handle(GetFinanceMetricsQuery request, CancellationToken cancellationToken)
    {
        var res = await _repository.GetFinanceMetricsAsync(cancellationToken);
        return Result<FinanceMetricsDto>.Success(res);
    }
}

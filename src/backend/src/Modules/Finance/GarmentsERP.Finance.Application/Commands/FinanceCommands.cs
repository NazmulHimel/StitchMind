using GarmentsERP.Finance.Application.DTOs;
using GarmentsERP.Finance.Application.Interfaces;
using GarmentsERP.Finance.Domain.Entities;
using GarmentsERP.Finance.Domain.Enums;
using GarmentsERP.Shared.Common;
using MediatR;

namespace GarmentsERP.Finance.Application.Commands;

// Create Account
public record CreateAccountCommand(CreateAccountDto Dto) : IRequest<Result<AccountDto>>;

public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, Result<AccountDto>>
{
    private readonly IFinanceRepository _repository;

    public CreateAccountCommandHandler(IFinanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<AccountDto>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetAccountByCodeAsync(request.Dto.AccountCode, cancellationToken);
        if (existing != null)
        {
            return Result<AccountDto>.Failure($"Account with code '{request.Dto.AccountCode}' already exists.");
        }

        var account = new Account
        {
            AccountCode = request.Dto.AccountCode,
            AccountName = request.Dto.AccountName,
            Category = request.Dto.Category,
            Currency = string.IsNullOrWhiteSpace(request.Dto.Currency) ? "BDT" : request.Dto.Currency,
            OpeningBalance = request.Dto.OpeningBalance,
            CurrentBalance = request.Dto.OpeningBalance,
            IsActive = true,
            Description = request.Dto.Description
        };

        var created = await _repository.CreateAccountAsync(account, cancellationToken);
        return Result<AccountDto>.Success(new AccountDto(
            created.Id,
            created.AccountCode,
            created.AccountName,
            created.Category,
            created.Category.ToString(),
            created.Currency,
            created.OpeningBalance,
            created.CurrentBalance,
            created.IsActive,
            created.Description
        ));
    }
}

// Create Journal Voucher
public record CreateJournalVoucherCommand(CreateJournalVoucherDto Dto) : IRequest<Result<JournalVoucherDto>>;

public class CreateJournalVoucherCommandHandler : IRequestHandler<CreateJournalVoucherCommand, Result<JournalVoucherDto>>
{
    private readonly IFinanceRepository _repository;

    public CreateJournalVoucherCommandHandler(IFinanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<JournalVoucherDto>> Handle(CreateJournalVoucherCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        if (dto.Lines == null || dto.Lines.Count < 2)
        {
            return Result<JournalVoucherDto>.Failure("A Journal Voucher must have at least 2 entry lines.");
        }

        decimal totalDebit = dto.Lines.Sum(l => l.DebitAmount);
        decimal totalCredit = dto.Lines.Sum(l => l.CreditAmount);

        if (totalDebit <= 0 || totalCredit <= 0)
        {
            return Result<JournalVoucherDto>.Failure("Debit and Credit amounts must be greater than zero.");
        }

        if (Math.Abs(totalDebit - totalCredit) > 0.001m)
        {
            return Result<JournalVoucherDto>.Failure($"Debit ({totalDebit:N2}) and Credit ({totalCredit:N2}) amounts must balance.");
        }

        var voucher = new JournalVoucher
        {
            VoucherNumber = $"JV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}",
            VoucherDate = dto.VoucherDate,
            Type = dto.Type,
            ReferenceType = dto.ReferenceType,
            ReferenceNumber = dto.ReferenceNumber,
            Narration = dto.Narration,
            TotalDebit = totalDebit,
            TotalCredit = totalCredit,
            Status = VoucherStatus.Draft,
            Lines = dto.Lines.Select(l => new JournalEntryLine
            {
                AccountId = l.AccountId,
                DebitAmount = l.DebitAmount,
                CreditAmount = l.CreditAmount,
                LineNarration = l.LineNarration
            }).ToList()
        };

        var created = await _repository.CreateJournalVoucherAsync(voucher, cancellationToken);
        var res = await _repository.GetJournalVoucherByIdAsync(created.Id, cancellationToken);

        return Result<JournalVoucherDto>.Success(MapToDto(res!));
    }

    private static JournalVoucherDto MapToDto(JournalVoucher v) => new(
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
    );
}

// Post Journal Voucher
public record PostJournalVoucherCommand(Guid VoucherId, string PostedBy = "Admin") : IRequest<Result<JournalVoucherDto>>;

public class PostJournalVoucherCommandHandler : IRequestHandler<PostJournalVoucherCommand, Result<JournalVoucherDto>>
{
    private readonly IFinanceRepository _repository;

    public PostJournalVoucherCommandHandler(IFinanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<JournalVoucherDto>> Handle(PostJournalVoucherCommand request, CancellationToken cancellationToken)
    {
        var result = await _repository.PostJournalVoucherAsync(request.VoucherId, request.PostedBy, cancellationToken);
        if (!result.IsSuccess)
        {
            return Result<JournalVoucherDto>.Failure(result.Error ?? "Failed to post voucher.");
        }

        var v = result.Value!;
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

// Cancel Journal Voucher
public record CancelJournalVoucherCommand(Guid VoucherId) : IRequest<Result<JournalVoucherDto>>;

public class CancelJournalVoucherCommandHandler : IRequestHandler<CancelJournalVoucherCommand, Result<JournalVoucherDto>>
{
    private readonly IFinanceRepository _repository;

    public CancelJournalVoucherCommandHandler(IFinanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<JournalVoucherDto>> Handle(CancelJournalVoucherCommand request, CancellationToken cancellationToken)
    {
        var result = await _repository.CancelJournalVoucherAsync(request.VoucherId, cancellationToken);
        if (!result.IsSuccess)
        {
            return Result<JournalVoucherDto>.Failure(result.Error ?? "Failed to cancel voucher.");
        }

        var v = result.Value!;
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

using GarmentsERP.Finance.Application.DTOs;
using GarmentsERP.Finance.Application.Interfaces;
using GarmentsERP.Shared.Common;
using MediatR;

namespace GarmentsERP.Finance.Application.Queries;

// ACC-03: Get Master LCs
public record GetMasterLCsQuery : IRequest<Result<List<MasterLetterOfCreditDto>>>;

public class GetMasterLCsQueryHandler : IRequestHandler<GetMasterLCsQuery, Result<List<MasterLetterOfCreditDto>>>
{
    private readonly IFinanceRepository _repository;

    public GetMasterLCsQueryHandler(IFinanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<MasterLetterOfCreditDto>>> Handle(GetMasterLCsQuery request, CancellationToken cancellationToken)
    {
        var result = await _repository.GetMasterLCsAsync(cancellationToken);
        return Result<List<MasterLetterOfCreditDto>>.Success(result);
    }
}

public record GetMasterLCByIdQuery(Guid Id) : IRequest<Result<MasterLetterOfCreditDto>>;

public class GetMasterLCByIdQueryHandler : IRequestHandler<GetMasterLCByIdQuery, Result<MasterLetterOfCreditDto>>
{
    private readonly IFinanceRepository _repository;

    public GetMasterLCByIdQueryHandler(IFinanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<MasterLetterOfCreditDto>> Handle(GetMasterLCByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await _repository.GetMasterLCByIdAsync(request.Id, cancellationToken);
        if (result == null)
            return Result<MasterLetterOfCreditDto>.Failure("Master Letter of Credit not found.");

        return Result<MasterLetterOfCreditDto>.Success(result);
    }
}

// ACC-03: Get Back-to-Back LCs
public record GetBackToBackLCsQuery(Guid? MasterLCId = null) : IRequest<Result<List<BackToBackLetterOfCreditDto>>>;

public class GetBackToBackLCsQueryHandler : IRequestHandler<GetBackToBackLCsQuery, Result<List<BackToBackLetterOfCreditDto>>>
{
    private readonly IFinanceRepository _repository;

    public GetBackToBackLCsQueryHandler(IFinanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<BackToBackLetterOfCreditDto>>> Handle(GetBackToBackLCsQuery request, CancellationToken cancellationToken)
    {
        var result = await _repository.GetBackToBackLCsAsync(request.MasterLCId, cancellationToken);
        return Result<List<BackToBackLetterOfCreditDto>>.Success(result);
    }
}

// ACC-03: Get Foreign Bill Discountings (FDBP)
public record GetForeignBillDiscountingsQuery(Guid? MasterLCId = null) : IRequest<Result<List<ForeignBillDiscountingDto>>>;

public class GetForeignBillDiscountingsQueryHandler : IRequestHandler<GetForeignBillDiscountingsQuery, Result<List<ForeignBillDiscountingDto>>>
{
    private readonly IFinanceRepository _repository;

    public GetForeignBillDiscountingsQueryHandler(IFinanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<ForeignBillDiscountingDto>>> Handle(GetForeignBillDiscountingsQuery request, CancellationToken cancellationToken)
    {
        var result = await _repository.GetForeignBillDiscountingsAsync(request.MasterLCId, cancellationToken);
        return Result<List<ForeignBillDiscountingDto>>.Success(result);
    }
}

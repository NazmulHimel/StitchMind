using GarmentsERP.Finance.Application.DTOs;
using GarmentsERP.Finance.Application.Interfaces;
using GarmentsERP.Shared.Common;
using MediatR;

namespace GarmentsERP.Finance.Application.Commands;

// ACC-03: Create Master LC
public record CreateMasterLCCommand(CreateMasterLCDto Dto) : IRequest<Result<MasterLetterOfCreditDto>>;

public class CreateMasterLCCommandHandler : IRequestHandler<CreateMasterLCCommand, Result<MasterLetterOfCreditDto>>
{
    private readonly IFinanceRepository _repository;

    public CreateMasterLCCommandHandler(IFinanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<MasterLetterOfCreditDto>> Handle(CreateMasterLCCommand request, CancellationToken cancellationToken)
    {
        var result = await _repository.CreateMasterLCAsync(request.Dto, cancellationToken);
        return Result<MasterLetterOfCreditDto>.Success(result);
    }
}

// ACC-03: Create Back-to-Back LC
public record CreateBackToBackLCCommand(CreateBackToBackLCDto Dto) : IRequest<Result<BackToBackLetterOfCreditDto>>;

public class CreateBackToBackLCCommandHandler : IRequestHandler<CreateBackToBackLCCommand, Result<BackToBackLetterOfCreditDto>>
{
    private readonly IFinanceRepository _repository;

    public CreateBackToBackLCCommandHandler(IFinanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<BackToBackLetterOfCreditDto>> Handle(CreateBackToBackLCCommand request, CancellationToken cancellationToken)
    {
        return await _repository.CreateBackToBackLCAsync(request.Dto, cancellationToken);
    }
}

// ACC-03: Create Foreign Bill Discounting (FDBP)
public record CreateForeignBillDiscountingCommand(CreateForeignBillDiscountingDto Dto) : IRequest<Result<ForeignBillDiscountingDto>>;

public class CreateForeignBillDiscountingCommandHandler : IRequestHandler<CreateForeignBillDiscountingCommand, Result<ForeignBillDiscountingDto>>
{
    private readonly IFinanceRepository _repository;

    public CreateForeignBillDiscountingCommandHandler(IFinanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ForeignBillDiscountingDto>> Handle(CreateForeignBillDiscountingCommand request, CancellationToken cancellationToken)
    {
        var result = await _repository.CreateForeignBillDiscountingAsync(request.Dto, cancellationToken);
        return Result<ForeignBillDiscountingDto>.Success(result);
    }
}

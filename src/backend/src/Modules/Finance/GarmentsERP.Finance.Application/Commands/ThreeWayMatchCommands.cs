using GarmentsERP.Finance.Application.DTOs;
using GarmentsERP.Finance.Application.Interfaces;
using GarmentsERP.Finance.Domain.Entities;
using GarmentsERP.Shared.Common;
using MediatR;

namespace GarmentsERP.Finance.Application.Commands;

// ACC-02: Create Vendor Bill
public record CreateVendorBillCommand(CreateVendorBillDto Dto) : IRequest<Result<VendorBillDto>>;

public class CreateVendorBillCommandHandler : IRequestHandler<CreateVendorBillCommand, Result<VendorBillDto>>
{
    private readonly IFinanceRepository _repository;

    public CreateVendorBillCommandHandler(IFinanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<VendorBillDto>> Handle(CreateVendorBillCommand request, CancellationToken cancellationToken)
    {
        var result = await _repository.CreateVendorBillAsync(request.Dto, cancellationToken);
        return Result<VendorBillDto>.Success(result);
    }
}

// ACC-02: Execute Three-Way Matching (AC-06)
public record ExecuteThreeWayMatchCommand(
    Guid VendorBillId,
    decimal POUnitPrice,
    decimal GRNPassedQuantity,
    string EvaluatedBy
) : IRequest<Result<ThreeWayMatchLogDto>>;

public class ExecuteThreeWayMatchCommandHandler : IRequestHandler<ExecuteThreeWayMatchCommand, Result<ThreeWayMatchLogDto>>
{
    private readonly IFinanceRepository _repository;

    public ExecuteThreeWayMatchCommandHandler(IFinanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ThreeWayMatchLogDto>> Handle(ExecuteThreeWayMatchCommand request, CancellationToken cancellationToken)
    {
        return await _repository.ExecuteThreeWayMatchAsync(
            request.VendorBillId,
            request.POUnitPrice,
            request.GRNPassedQuantity,
            request.EvaluatedBy,
            cancellationToken
        );
    }
}

// ACC-04: Save Costing Variance
public record SaveCostingVarianceCommand(OrderCostingVariance Variance) : IRequest<Result<OrderCostingVarianceDto>>;

public class SaveCostingVarianceCommandHandler : IRequestHandler<SaveCostingVarianceCommand, Result<OrderCostingVarianceDto>>
{
    private readonly IFinanceRepository _repository;

    public SaveCostingVarianceCommandHandler(IFinanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<OrderCostingVarianceDto>> Handle(SaveCostingVarianceCommand request, CancellationToken cancellationToken)
    {
        var result = await _repository.SaveOrderCostingVarianceAsync(request.Variance, cancellationToken);
        return Result<OrderCostingVarianceDto>.Success(result);
    }
}

// ACC-06: Create Fixed Asset
public record CreateFixedAssetCommand(FixedAsset Asset) : IRequest<Result<FixedAssetDto>>;

public class CreateFixedAssetCommandHandler : IRequestHandler<CreateFixedAssetCommand, Result<FixedAssetDto>>
{
    private readonly IFinanceRepository _repository;

    public CreateFixedAssetCommandHandler(IFinanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<FixedAssetDto>> Handle(CreateFixedAssetCommand request, CancellationToken cancellationToken)
    {
        var result = await _repository.CreateFixedAssetAsync(request.Asset, cancellationToken);
        return Result<FixedAssetDto>.Success(result);
    }
}

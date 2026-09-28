using GarmentsERP.Finance.Application.DTOs;
using GarmentsERP.Finance.Application.Interfaces;
using GarmentsERP.Shared.Common;
using MediatR;

namespace GarmentsERP.Finance.Application.Queries;

// ACC-02: Get Vendor Bills
public record GetVendorBillsQuery : IRequest<Result<List<VendorBillDto>>>;

public class GetVendorBillsQueryHandler : IRequestHandler<GetVendorBillsQuery, Result<List<VendorBillDto>>>
{
    private readonly IFinanceRepository _repository;

    public GetVendorBillsQueryHandler(IFinanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<VendorBillDto>>> Handle(GetVendorBillsQuery request, CancellationToken cancellationToken)
    {
        var result = await _repository.GetVendorBillsAsync(cancellationToken);
        return Result<List<VendorBillDto>>.Success(result);
    }
}

public record GetVendorBillByIdQuery(Guid Id) : IRequest<Result<VendorBillDto>>;

public class GetVendorBillByIdQueryHandler : IRequestHandler<GetVendorBillByIdQuery, Result<VendorBillDto>>
{
    private readonly IFinanceRepository _repository;

    public GetVendorBillByIdQueryHandler(IFinanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<VendorBillDto>> Handle(GetVendorBillByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await _repository.GetVendorBillByIdAsync(request.Id, cancellationToken);
        if (result == null)
            return Result<VendorBillDto>.Failure("Vendor bill record not found.");

        return Result<VendorBillDto>.Success(result);
    }
}

// ACC-04: Costing Variances
public record GetOrderCostingVariancesQuery : IRequest<Result<List<OrderCostingVarianceDto>>>;

public class GetOrderCostingVariancesQueryHandler : IRequestHandler<GetOrderCostingVariancesQuery, Result<List<OrderCostingVarianceDto>>>
{
    private readonly IFinanceRepository _repository;

    public GetOrderCostingVariancesQueryHandler(IFinanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<OrderCostingVarianceDto>>> Handle(GetOrderCostingVariancesQuery request, CancellationToken cancellationToken)
    {
        var result = await _repository.GetOrderCostingVariancesAsync(cancellationToken);
        return Result<List<OrderCostingVarianceDto>>.Success(result);
    }
}

public record GetOrderCostingVarianceByStyleIdQuery(Guid StyleId) : IRequest<Result<OrderCostingVarianceDto>>;

public class GetOrderCostingVarianceByStyleIdQueryHandler : IRequestHandler<GetOrderCostingVarianceByStyleIdQuery, Result<OrderCostingVarianceDto>>
{
    private readonly IFinanceRepository _repository;

    public GetOrderCostingVarianceByStyleIdQueryHandler(IFinanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<OrderCostingVarianceDto>> Handle(GetOrderCostingVarianceByStyleIdQuery request, CancellationToken cancellationToken)
    {
        var result = await _repository.GetOrderCostingVarianceByStyleIdAsync(request.StyleId, cancellationToken);
        if (result == null)
            return Result<OrderCostingVarianceDto>.Failure("Order costing variance not found for specified style.");

        return Result<OrderCostingVarianceDto>.Success(result);
    }
}

// ACC-05: Multi-Currency & Forex
public record GetExchangeRatesQuery : IRequest<Result<List<CurrencyExchangeRateDto>>>;

public class GetExchangeRatesQueryHandler : IRequestHandler<GetExchangeRatesQuery, Result<List<CurrencyExchangeRateDto>>>
{
    private readonly IFinanceRepository _repository;

    public GetExchangeRatesQueryHandler(IFinanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<CurrencyExchangeRateDto>>> Handle(GetExchangeRatesQuery request, CancellationToken cancellationToken)
    {
        var result = await _repository.GetExchangeRatesAsync(cancellationToken);
        return Result<List<CurrencyExchangeRateDto>>.Success(result);
    }
}

public record CalculateForexGainLossQuery(
    decimal Amount,
    string Currency,
    decimal BookingRate,
    decimal SettlementRate
) : IRequest<Result<ForexGainLossCalculationDto>>;

public class CalculateForexGainLossQueryHandler : IRequestHandler<CalculateForexGainLossQuery, Result<ForexGainLossCalculationDto>>
{
    private readonly IFinanceRepository _repository;

    public CalculateForexGainLossQueryHandler(IFinanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ForexGainLossCalculationDto>> Handle(CalculateForexGainLossQuery request, CancellationToken cancellationToken)
    {
        var result = await _repository.CalculateForexGainLossAsync(
            request.Amount,
            request.Currency,
            request.BookingRate,
            request.SettlementRate,
            cancellationToken
        );
        return Result<ForexGainLossCalculationDto>.Success(result);
    }
}

// ACC-06: Fixed Assets
public record GetFixedAssetsQuery : IRequest<Result<List<FixedAssetDto>>>;

public class GetFixedAssetsQueryHandler : IRequestHandler<GetFixedAssetsQuery, Result<List<FixedAssetDto>>>
{
    private readonly IFinanceRepository _repository;

    public GetFixedAssetsQueryHandler(IFinanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<FixedAssetDto>>> Handle(GetFixedAssetsQuery request, CancellationToken cancellationToken)
    {
        var result = await _repository.GetFixedAssetsAsync(cancellationToken);
        return Result<List<FixedAssetDto>>.Success(result);
    }
}

// ACC-07: Aging Analysis
public record GetAgingAnalysisQuery(string Type = "AccountsPayable") : IRequest<Result<AgingAnalysisSummaryDto>>;

public class GetAgingAnalysisQueryHandler : IRequestHandler<GetAgingAnalysisQuery, Result<AgingAnalysisSummaryDto>>
{
    private readonly IFinanceRepository _repository;

    public GetAgingAnalysisQueryHandler(IFinanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<AgingAnalysisSummaryDto>> Handle(GetAgingAnalysisQuery request, CancellationToken cancellationToken)
    {
        var result = await _repository.GetAgingAnalysisAsync(request.Type, cancellationToken);
        return Result<AgingAnalysisSummaryDto>.Success(result);
    }
}

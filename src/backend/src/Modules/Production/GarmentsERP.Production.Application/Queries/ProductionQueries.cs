using GarmentsERP.Production.Application.DTOs;
using GarmentsERP.Production.Application.Interfaces;
using GarmentsERP.Production.Domain.Enums;
using GarmentsERP.Shared.Common;
using MediatR;

namespace GarmentsERP.Production.Application.Queries;

// Buyers Queries
public record GetBuyersQuery : IRequest<Result<List<BuyerDto>>>;

public class GetBuyersQueryHandler : IRequestHandler<GetBuyersQuery, Result<List<BuyerDto>>>
{
    private readonly IProductionRepository _repository;
    public GetBuyersQueryHandler(IProductionRepository repository) => _repository = repository;

    public async Task<Result<List<BuyerDto>>> Handle(GetBuyersQuery request, CancellationToken cancellationToken)
    {
        var buyers = await _repository.GetBuyersAsync(cancellationToken);
        var dtos = buyers.Select(b => new BuyerDto(
            b.Id, b.Name, b.Code, b.Country, b.ContactPerson,
            b.Email, b.Phone, b.PaymentTerms, b.IsActive, b.Orders.Count
        )).ToList();

        return Result<List<BuyerDto>>.Success(dtos);
    }
}

// Buyer Orders Queries
public record GetBuyerOrdersQuery(OrderStatus? Status = null) : IRequest<Result<List<BuyerOrderDto>>>;

public class GetBuyerOrdersQueryHandler : IRequestHandler<GetBuyerOrdersQuery, Result<List<BuyerOrderDto>>>
{
    private readonly IProductionRepository _repository;
    public GetBuyerOrdersQueryHandler(IProductionRepository repository) => _repository = repository;

    public async Task<Result<List<BuyerOrderDto>>> Handle(GetBuyerOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _repository.GetBuyerOrdersAsync(request.Status, cancellationToken);
        var dtos = orders.Select(o => new BuyerOrderDto(
            o.Id, o.OrderNumber, o.BuyerId, o.Buyer?.Name ?? "Unknown Buyer",
            o.StyleNumber, o.StyleName, o.Season, o.OrderQuantity,
            o.UnitPrice, o.TotalValue, o.OrderDate, o.DeliveryDate,
            o.Status, o.Notes, o.CuttingOrders.Count,
            o.ProductionLogs.Sum(l => l.ActualOutputQty)
        )).ToList();

        return Result<List<BuyerOrderDto>>.Success(dtos);
    }
}

// Cutting Orders & Bundles Queries
public record GetCuttingOrdersQuery(CuttingStatus? Status = null) : IRequest<Result<List<CuttingOrderDto>>>;

public class GetCuttingOrdersQueryHandler : IRequestHandler<GetCuttingOrdersQuery, Result<List<CuttingOrderDto>>>
{
    private readonly IProductionRepository _repository;
    public GetCuttingOrdersQueryHandler(IProductionRepository repository) => _repository = repository;

    public async Task<Result<List<CuttingOrderDto>>> Handle(GetCuttingOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _repository.GetCuttingOrdersAsync(request.Status, cancellationToken);
        var dtos = orders.Select(c => new CuttingOrderDto(
            c.Id, c.CuttingOrderNumber, c.BuyerOrderId, c.BuyerOrder?.OrderNumber ?? "N/A",
            c.StyleNumber, c.FabricType, c.TotalRequiredQty, c.TotalPlies,
            c.MarkerLength, c.FabricWidth, c.EfficiencyPercentage,
            c.Status, c.TableNumber, c.CuttingDate,
            c.Bundles.Select(b => new CutBundleDto(
                b.Id, b.BundleNumber, b.CuttingOrderId, b.Size, b.Color,
                b.Quantity, b.Barcode, b.SerialStart, b.SerialEnd,
                b.CurrentSection, b.IsCompleted, b.CompletedAt
            )).ToList()
        )).ToList();

        return Result<List<CuttingOrderDto>>.Success(dtos);
    }
}

public record GetCutBundlesQuery(Guid? CuttingOrderId = null, BundleSection? Section = null) : IRequest<Result<List<CutBundleDto>>>;

public class GetCutBundlesQueryHandler : IRequestHandler<GetCutBundlesQuery, Result<List<CutBundleDto>>>
{
    private readonly IProductionRepository _repository;
    public GetCutBundlesQueryHandler(IProductionRepository repository) => _repository = repository;

    public async Task<Result<List<CutBundleDto>>> Handle(GetCutBundlesQuery request, CancellationToken cancellationToken)
    {
        var bundles = await _repository.GetBundlesAsync(request.CuttingOrderId, request.Section, cancellationToken);
        var dtos = bundles.Select(b => new CutBundleDto(
            b.Id, b.BundleNumber, b.CuttingOrderId, b.Size, b.Color,
            b.Quantity, b.Barcode, b.SerialStart, b.SerialEnd,
            b.CurrentSection, b.IsCompleted, b.CompletedAt
        )).ToList();

        return Result<List<CutBundleDto>>.Success(dtos);
    }
}

// Sewing Queries
public record GetSewingLinesQuery : IRequest<Result<List<SewingLineDto>>>;

public class GetSewingLinesQueryHandler : IRequestHandler<GetSewingLinesQuery, Result<List<SewingLineDto>>>
{
    private readonly IProductionRepository _repository;
    public GetSewingLinesQueryHandler(IProductionRepository repository) => _repository = repository;

    public async Task<Result<List<SewingLineDto>>> Handle(GetSewingLinesQuery request, CancellationToken cancellationToken)
    {
        var lines = await _repository.GetSewingLinesAsync(cancellationToken);
        var today = DateTime.UtcNow.Date;

        var dtos = lines.Select(l =>
        {
            var todayLogs = l.ProductionLogs.Where(p => p.LogDate == today).ToList();
            var todayOutput = todayLogs.Sum(p => p.ActualOutputQty);
            var todayTarget = todayLogs.Sum(p => p.TargetQty);
            var efficiency = todayTarget > 0 ? Math.Round(((decimal)todayOutput / todayTarget) * 100, 2) : l.TargetEfficiency;

            return new SewingLineDto(
                l.Id, l.LineNumber, l.LineName, l.Floor,
                l.TargetEfficiency, l.CapacityPerDay, l.Supervisor,
                l.Status, todayOutput, todayTarget > 0 ? todayTarget : l.CapacityPerDay, efficiency
            );
        }).ToList();

        return Result<List<SewingLineDto>>.Success(dtos);
    }
}

public record GetHourlyProductionLogsQuery(DateTime? Date = null, Guid? LineId = null) : IRequest<Result<List<HourlyProductionLogDto>>>;

public class GetHourlyProductionLogsQueryHandler : IRequestHandler<GetHourlyProductionLogsQuery, Result<List<HourlyProductionLogDto>>>
{
    private readonly IProductionRepository _repository;
    public GetHourlyProductionLogsQueryHandler(IProductionRepository repository) => _repository = repository;

    public async Task<Result<List<HourlyProductionLogDto>>> Handle(GetHourlyProductionLogsQuery request, CancellationToken cancellationToken)
    {
        var logs = await _repository.GetHourlyLogsAsync(request.Date, request.LineId, cancellationToken);
        var dtos = logs.Select(l => new HourlyProductionLogDto(
            l.Id, l.SewingLineId, l.SewingLine?.LineNumber ?? "Line-N/A",
            l.SewingLine?.LineName ?? "Sewing Line",
            l.BuyerOrderId, l.BuyerOrder?.OrderNumber ?? "N/A",
            l.BuyerOrder?.StyleNumber ?? "Style N/A",
            l.LogDate, l.HourSlot, l.TargetQty, l.ActualOutputQty,
            l.DefectCount, l.RejectionCount, l.OperatorCount,
            l.EfficiencyPercentage, l.Notes
        )).ToList();

        return Result<List<HourlyProductionLogDto>>.Success(dtos);
    }
}

// Quality Queries
public record GetQualityInspectionsQuery(InspectionType? Type = null) : IRequest<Result<List<QualityInspectionDto>>>;

public class GetQualityInspectionsQueryHandler : IRequestHandler<GetQualityInspectionsQuery, Result<List<QualityInspectionDto>>>
{
    private readonly IProductionRepository _repository;
    public GetQualityInspectionsQueryHandler(IProductionRepository repository) => _repository = repository;

    public async Task<Result<List<QualityInspectionDto>>> Handle(GetQualityInspectionsQuery request, CancellationToken cancellationToken)
    {
        var inspections = await _repository.GetQualityInspectionsAsync(request.Type, cancellationToken);
        var dtos = inspections.Select(q => new QualityInspectionDto(
            q.Id, q.InspectionNumber, q.InspectionType,
            q.BuyerOrderId, q.BuyerOrder?.OrderNumber ?? "N/A",
            q.BuyerOrder?.StyleNumber ?? "N/A",
            q.LotOrBatchRef, q.SampleSize, q.MinorDefects,
            q.MajorDefects, q.CriticalDefects, q.TotalPoints,
            q.Result, q.InspectorName, q.Comments, q.InspectedAt
        )).ToList();

        return Result<List<QualityInspectionDto>>.Success(dtos);
    }
}

// Metrics Query
public record GetProductionMetricsQuery : IRequest<Result<ProductionMetricsDto>>;

public class GetProductionMetricsQueryHandler : IRequestHandler<GetProductionMetricsQuery, Result<ProductionMetricsDto>>
{
    private readonly IProductionRepository _repository;
    public GetProductionMetricsQueryHandler(IProductionRepository repository) => _repository = repository;

    public async Task<Result<ProductionMetricsDto>> Handle(GetProductionMetricsQuery request, CancellationToken cancellationToken)
    {
        var metrics = await _repository.GetProductionMetricsAsync(cancellationToken);
        return Result<ProductionMetricsDto>.Success(metrics);
    }
}

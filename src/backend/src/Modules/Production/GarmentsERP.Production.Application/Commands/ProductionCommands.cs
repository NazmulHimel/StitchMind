using FluentValidation;
using GarmentsERP.Production.Application.DTOs;
using GarmentsERP.Production.Application.Interfaces;
using GarmentsERP.Production.Domain.Entities;
using GarmentsERP.Production.Domain.Enums;
using GarmentsERP.Shared.Common;
using MediatR;

namespace GarmentsERP.Production.Application.Commands;

// Buyer Commands
public record CreateBuyerCommand(CreateBuyerDto Dto) : IRequest<Result<BuyerDto>>;

public class CreateBuyerCommandHandler : IRequestHandler<CreateBuyerCommand, Result<BuyerDto>>
{
    private readonly IProductionRepository _repository;
    public CreateBuyerCommandHandler(IProductionRepository repository) => _repository = repository;

    public async Task<Result<BuyerDto>> Handle(CreateBuyerCommand request, CancellationToken cancellationToken)
    {
        var d = request.Dto;
        var buyer = new Buyer
        {
            Name = d.Name,
            Code = d.Code.ToUpperInvariant(),
            Country = d.Country,
            ContactPerson = d.ContactPerson,
            Email = d.Email,
            Phone = d.Phone,
            PaymentTerms = d.PaymentTerms,
            IsActive = true
        };

        var created = await _repository.AddBuyerAsync(buyer, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var dto = new BuyerDto(
            created.Id, created.Name, created.Code, created.Country,
            created.ContactPerson, created.Email, created.Phone, created.PaymentTerms,
            created.IsActive, 0
        );
        return Result<BuyerDto>.Success(dto);
    }
}

// Buyer Order Commands
public record CreateBuyerOrderCommand(CreateBuyerOrderDto Dto) : IRequest<Result<BuyerOrderDto>>;

public class CreateBuyerOrderCommandHandler : IRequestHandler<CreateBuyerOrderCommand, Result<BuyerOrderDto>>
{
    private readonly IProductionRepository _repository;
    public CreateBuyerOrderCommandHandler(IProductionRepository repository) => _repository = repository;

    public async Task<Result<BuyerOrderDto>> Handle(CreateBuyerOrderCommand request, CancellationToken cancellationToken)
    {
        var d = request.Dto;
        var buyer = await _repository.GetBuyerByIdAsync(d.BuyerId, cancellationToken);
        if (buyer == null)
            return Result<BuyerOrderDto>.Failure("Buyer not found.");

        var orderCount = (await _repository.GetBuyerOrdersAsync(cancellationToken: cancellationToken)).Count();
        var orderNumber = $"BO-{DateTime.UtcNow:yyMM}-{1001 + orderCount}";

        var order = new BuyerOrder
        {
            OrderNumber = orderNumber,
            BuyerId = d.BuyerId,
            StyleNumber = d.StyleNumber,
            StyleName = d.StyleName,
            Season = d.Season,
            OrderQuantity = d.OrderQuantity,
            UnitPrice = d.UnitPrice,
            OrderDate = DateTime.UtcNow,
            DeliveryDate = d.DeliveryDate,
            Status = OrderStatus.Confirmed,
            Notes = d.Notes
        };

        var created = await _repository.AddBuyerOrderAsync(order, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var dto = new BuyerOrderDto(
            created.Id, created.OrderNumber, created.BuyerId, buyer.Name,
            created.StyleNumber, created.StyleName, created.Season,
            created.OrderQuantity, created.UnitPrice, created.TotalValue,
            created.OrderDate, created.DeliveryDate, created.Status, created.Notes,
            0, 0
        );
        return Result<BuyerOrderDto>.Success(dto);
    }
}

public record UpdateOrderStatusCommand(Guid OrderId, OrderStatus Status) : IRequest<Result<bool>>;

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, Result<bool>>
{
    private readonly IProductionRepository _repository;
    public UpdateOrderStatusCommandHandler(IProductionRepository repository) => _repository = repository;

    public async Task<Result<bool>> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _repository.GetBuyerOrderByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            return Result<bool>.Failure("Order not found.");

        order.Status = request.Status;
        await _repository.UpdateBuyerOrderAsync(order, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}

// Cutting Commands
public record CreateCuttingOrderCommand(CreateCuttingOrderDto Dto) : IRequest<Result<CuttingOrderDto>>;

public class CreateCuttingOrderCommandHandler : IRequestHandler<CreateCuttingOrderCommand, Result<CuttingOrderDto>>
{
    private readonly IProductionRepository _repository;
    public CreateCuttingOrderCommandHandler(IProductionRepository repository) => _repository = repository;

    public async Task<Result<CuttingOrderDto>> Handle(CreateCuttingOrderCommand request, CancellationToken cancellationToken)
    {
        var d = request.Dto;
        var order = await _repository.GetBuyerOrderByIdAsync(d.BuyerOrderId, cancellationToken);
        if (order == null)
            return Result<CuttingOrderDto>.Failure("Buyer order not found.");

        var cuttingOrders = (await _repository.GetCuttingOrdersAsync(cancellationToken: cancellationToken)).Count();
        var cuttingOrderNo = $"CO-{8801 + cuttingOrders}";

        var cuttingOrder = new CuttingOrder
        {
            CuttingOrderNumber = cuttingOrderNo,
            BuyerOrderId = d.BuyerOrderId,
            StyleNumber = d.StyleNumber,
            FabricType = d.FabricType,
            TotalRequiredQty = d.TotalRequiredQty,
            TotalPlies = d.TotalPlies,
            MarkerLength = d.MarkerLength,
            FabricWidth = d.FabricWidth,
            EfficiencyPercentage = d.EfficiencyPercentage,
            Status = CuttingStatus.SpreadingLay,
            TableNumber = d.TableNumber,
            CuttingDate = DateTime.UtcNow
        };

        var created = await _repository.AddCuttingOrderAsync(cuttingOrder, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var dto = new CuttingOrderDto(
            created.Id, created.CuttingOrderNumber, created.BuyerOrderId, order.OrderNumber,
            created.StyleNumber, created.FabricType, created.TotalRequiredQty, created.TotalPlies,
            created.MarkerLength, created.FabricWidth, created.EfficiencyPercentage,
            created.Status, created.TableNumber, created.CuttingDate, new List<CutBundleDto>()
        );
        return Result<CuttingOrderDto>.Success(dto);
    }
}

public record GenerateBundlesCommand(Guid CuttingOrderId, int BundleSize) : IRequest<Result<List<CutBundleDto>>>;

public class GenerateBundlesCommandHandler : IRequestHandler<GenerateBundlesCommand, Result<List<CutBundleDto>>>
{
    private readonly IProductionRepository _repository;
    public GenerateBundlesCommandHandler(IProductionRepository repository) => _repository = repository;

    public async Task<Result<List<CutBundleDto>>> Handle(GenerateBundlesCommand request, CancellationToken cancellationToken)
    {
        var cutting = await _repository.GetCuttingOrderByIdAsync(request.CuttingOrderId, cancellationToken);
        if (cutting == null)
            return Result<List<CutBundleDto>>.Failure("Cutting order not found.");

        var bundleQty = request.BundleSize > 0 ? request.BundleSize : 20;
        var totalQty = cutting.TotalRequiredQty;
        var bundleCount = (int)Math.Ceiling((double)totalQty / bundleQty);
        var sizes = new[] { "S", "M", "L", "XL" };
        var colors = new[] { "Navy Blue", "Heather Grey", "Black", "White" };

        var bundles = new List<CutBundle>();
        var currentSerial = 1;

        for (int i = 1; i <= bundleCount; i++)
        {
            var qty = (i == bundleCount && totalQty % bundleQty != 0) ? (totalQty % bundleQty) : bundleQty;
            var size = sizes[(i - 1) % sizes.Length];
            var color = colors[(i - 1) % colors.Length];
            var bndNumber = $"{cutting.CuttingOrderNumber}-{i:D2}";
            var barcode = $"BND-{cutting.CuttingOrderNumber}-{i:D3}-{size}";

            bundles.Add(new CutBundle
            {
                BundleNumber = bndNumber,
                CuttingOrderId = cutting.Id,
                Size = size,
                Color = color,
                Quantity = qty,
                Barcode = barcode,
                SerialStart = currentSerial,
                SerialEnd = currentSerial + qty - 1,
                CurrentSection = BundleSection.Cutting,
                IsCompleted = false
            });

            currentSerial += qty;
        }

        cutting.Status = CuttingStatus.CutCompleted;
        await _repository.AddBundlesAsync(bundles, cancellationToken);
        await _repository.UpdateCuttingOrderAsync(cutting, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var dtos = bundles.Select(b => new CutBundleDto(
            b.Id, b.BundleNumber, b.CuttingOrderId, b.Size, b.Color,
            b.Quantity, b.Barcode, b.SerialStart, b.SerialEnd,
            b.CurrentSection, b.IsCompleted, b.CompletedAt
        )).ToList();

        return Result<List<CutBundleDto>>.Success(dtos);
    }
}

public record UpdateBundleSectionCommand(Guid BundleId, BundleSection NextSection) : IRequest<Result<CutBundleDto>>;

public class UpdateBundleSectionCommandHandler : IRequestHandler<UpdateBundleSectionCommand, Result<CutBundleDto>>
{
    private readonly IProductionRepository _repository;
    public UpdateBundleSectionCommandHandler(IProductionRepository repository) => _repository = repository;

    public async Task<Result<CutBundleDto>> Handle(UpdateBundleSectionCommand request, CancellationToken cancellationToken)
    {
        var bundle = await _repository.GetBundleByIdAsync(request.BundleId, cancellationToken);
        if (bundle == null)
            return Result<CutBundleDto>.Failure("Bundle not found.");

        bundle.CurrentSection = request.NextSection;
        if (request.NextSection == BundleSection.Packed)
        {
            bundle.IsCompleted = true;
            bundle.CompletedAt = DateTime.UtcNow;
        }

        await _repository.UpdateBundleAsync(bundle, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var dto = new CutBundleDto(
            bundle.Id, bundle.BundleNumber, bundle.CuttingOrderId, bundle.Size,
            bundle.Color, bundle.Quantity, bundle.Barcode, bundle.SerialStart,
            bundle.SerialEnd, bundle.CurrentSection, bundle.IsCompleted, bundle.CompletedAt
        );
        return Result<CutBundleDto>.Success(dto);
    }
}

// Sewing Commands
public record CreateSewingLineCommand(CreateSewingLineDto Dto) : IRequest<Result<SewingLineDto>>;

public class CreateSewingLineCommandHandler : IRequestHandler<CreateSewingLineCommand, Result<SewingLineDto>>
{
    private readonly IProductionRepository _repository;
    public CreateSewingLineCommandHandler(IProductionRepository repository) => _repository = repository;

    public async Task<Result<SewingLineDto>> Handle(CreateSewingLineCommand request, CancellationToken cancellationToken)
    {
        var d = request.Dto;
        var line = new SewingLine
        {
            LineNumber = d.LineNumber,
            LineName = d.LineName,
            Floor = d.Floor,
            TargetEfficiency = d.TargetEfficiency,
            CapacityPerDay = d.CapacityPerDay,
            Supervisor = d.Supervisor,
            Status = SewingLineStatus.Active
        };

        var created = await _repository.AddSewingLineAsync(line, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var dto = new SewingLineDto(
            created.Id, created.LineNumber, created.LineName, created.Floor,
            created.TargetEfficiency, created.CapacityPerDay, created.Supervisor,
            created.Status, 0, created.CapacityPerDay, 0
        );
        return Result<SewingLineDto>.Success(dto);
    }
}

public record RecordHourlyOutputCommand(RecordHourlyOutputDto Dto) : IRequest<Result<HourlyProductionLogDto>>;

public class RecordHourlyOutputCommandHandler : IRequestHandler<RecordHourlyOutputCommand, Result<HourlyProductionLogDto>>
{
    private readonly IProductionRepository _repository;
    public RecordHourlyOutputCommandHandler(IProductionRepository repository) => _repository = repository;

    public async Task<Result<HourlyProductionLogDto>> Handle(RecordHourlyOutputCommand request, CancellationToken cancellationToken)
    {
        var d = request.Dto;
        var line = await _repository.GetSewingLineByIdAsync(d.SewingLineId, cancellationToken);
        if (line == null)
            return Result<HourlyProductionLogDto>.Failure("Sewing line not found.");

        var order = await _repository.GetBuyerOrderByIdAsync(d.BuyerOrderId, cancellationToken);
        if (order == null)
            return Result<HourlyProductionLogDto>.Failure("Buyer order not found.");

        var log = new HourlyProductionLog
        {
            SewingLineId = d.SewingLineId,
            BuyerOrderId = d.BuyerOrderId,
            LogDate = d.LogDate.Date,
            HourSlot = d.HourSlot,
            TargetQty = d.TargetQty,
            ActualOutputQty = d.ActualOutputQty,
            DefectCount = d.DefectCount,
            RejectionCount = d.RejectionCount,
            OperatorCount = d.OperatorCount,
            Notes = d.Notes
        };

        var created = await _repository.AddHourlyLogAsync(log, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var dto = new HourlyProductionLogDto(
            created.Id, created.SewingLineId, line.LineNumber, line.LineName,
            created.BuyerOrderId, order.OrderNumber, order.StyleNumber,
            created.LogDate, created.HourSlot, created.TargetQty, created.ActualOutputQty,
            created.DefectCount, created.RejectionCount, created.OperatorCount,
            created.EfficiencyPercentage, created.Notes
        );
        return Result<HourlyProductionLogDto>.Success(dto);
    }
}

// Quality Inspection Commands
public record CreateQualityInspectionCommand(CreateQualityInspectionDto Dto) : IRequest<Result<QualityInspectionDto>>;

public class CreateQualityInspectionCommandHandler : IRequestHandler<CreateQualityInspectionCommand, Result<QualityInspectionDto>>
{
    private readonly IProductionRepository _repository;
    public CreateQualityInspectionCommandHandler(IProductionRepository repository) => _repository = repository;

    public async Task<Result<QualityInspectionDto>> Handle(CreateQualityInspectionCommand request, CancellationToken cancellationToken)
    {
        var d = request.Dto;
        var order = await _repository.GetBuyerOrderByIdAsync(d.BuyerOrderId, cancellationToken);
        if (order == null)
            return Result<QualityInspectionDto>.Failure("Buyer order not found.");

        var inspectionsCount = (await _repository.GetQualityInspectionsAsync(cancellationToken: cancellationToken)).Count();
        var inspectionNo = $"QC-IN-{401 + inspectionsCount}";

        // Automatic AQL 2.5 / 4-Point Pass-Fail Evaluation
        var result = InspectionResult.Passed;
        if (d.CriticalDefects > 0 || d.MajorDefects > (d.SampleSize * 0.05) || d.TotalPoints > 28)
        {
            result = InspectionResult.Failed;
        }
        else if (d.MajorDefects > (d.SampleSize * 0.025))
        {
            result = InspectionResult.ConditionallyPassed;
        }

        var inspection = new QualityInspection
        {
            InspectionNumber = inspectionNo,
            InspectionType = d.InspectionType,
            BuyerOrderId = d.BuyerOrderId,
            LotOrBatchRef = d.LotOrBatchRef,
            SampleSize = d.SampleSize,
            MinorDefects = d.MinorDefects,
            MajorDefects = d.MajorDefects,
            CriticalDefects = d.CriticalDefects,
            TotalPoints = d.TotalPoints,
            Result = result,
            InspectorName = d.InspectorName,
            Comments = d.Comments,
            InspectedAt = DateTime.UtcNow
        };

        var created = await _repository.AddQualityInspectionAsync(inspection, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var dto = new QualityInspectionDto(
            created.Id, created.InspectionNumber, created.InspectionType,
            created.BuyerOrderId, order.OrderNumber, order.StyleNumber,
            created.LotOrBatchRef, created.SampleSize, created.MinorDefects,
            created.MajorDefects, created.CriticalDefects, created.TotalPoints,
            created.Result, created.InspectorName, created.Comments, created.InspectedAt
        );
        return Result<QualityInspectionDto>.Success(dto);
    }
}

using GarmentsERP.Production.Application.DTOs;
using GarmentsERP.Production.Application.Interfaces;
using GarmentsERP.Production.Domain.Entities;
using GarmentsERP.Production.Domain.Enums;
using GarmentsERP.Shared.Common;
using MediatR;

namespace GarmentsERP.Production.Application.Commands;

// CUT-01: CAD Marker Commands
public record CreateCADMarkerCommand(CreateCADMarkerDto Dto) : IRequest<Result<CADMarkerDto>>;

public class CreateCADMarkerCommandHandler : IRequestHandler<CreateCADMarkerCommand, Result<CADMarkerDto>>
{
    private readonly IProductionRepository _repository;
    public CreateCADMarkerCommandHandler(IProductionRepository repository) => _repository = repository;

    public async Task<Result<CADMarkerDto>> Handle(CreateCADMarkerCommand request, CancellationToken cancellationToken)
    {
        var d = request.Dto;
        var marker = new CADMarker
        {
            MarkerName = d.MarkerName,
            StyleId = d.StyleId,
            StyleNumber = d.StyleNumber,
            CADSoftware = d.CADSoftware,
            CuttableWidthInches = d.CuttableWidthInches,
            MarkerLengthYards = d.MarkerLengthYards,
            GarmentsPerMarker = d.GarmentsPerMarker,
            SizeDistributionRatio = d.SizeDistributionRatio,
            MarkerEfficiencyPct = d.MarkerEfficiencyPct,
            FileFormat = d.FileFormat
        };
        marker.RecalculateConsumption();

        var created = await _repository.AddCADMarkerAsync(marker, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var dto = new CADMarkerDto(
            created.Id, created.MarkerName, created.StyleId, created.StyleNumber,
            created.CADSoftware, created.CuttableWidthInches, created.MarkerLengthYards,
            created.GarmentsPerMarker, created.SizeDistributionRatio, created.MarkerEfficiencyPct,
            created.PlannedConsumptionPerPcYds, created.FileFormat
        );
        return Result<CADMarkerDto>.Success(dto);
    }
}

// CUT-02: Relaxation Time-Lock Commands
public record StartRelaxationSessionCommand(StartRelaxationSessionDto Dto) : IRequest<Result<RelaxationSessionDto>>;

public class StartRelaxationSessionCommandHandler : IRequestHandler<StartRelaxationSessionCommand, Result<RelaxationSessionDto>>
{
    private readonly IProductionRepository _repository;
    public StartRelaxationSessionCommandHandler(IProductionRepository repository) => _repository = repository;

    public async Task<Result<RelaxationSessionDto>> Handle(StartRelaxationSessionCommand request, CancellationToken cancellationToken)
    {
        var d = request.Dto;
        var session = RelaxationSession.StartSession(
            d.FabricRollId, d.RollBarcode, d.DyeLotNumber, d.FabricType,
            d.DurationHours, d.AmbientTemperatureC, d.RelativeHumidityPct,
            d.OperatorId
        );

        var created = await _repository.AddRelaxationSessionAsync(session, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var remaining = created.GetRemainingTime();
        var dto = new RelaxationSessionDto(
            created.Id, created.FabricRollId, created.RollBarcode, created.DyeLotNumber,
            created.FabricType, created.UnrolledAtUtc, created.MandatoryDurationHours,
            created.UnlockedAtUtc, created.IsUnlocked(), remaining.Hours, remaining.Minutes,
            created.AmbientTemperatureC, created.RelativeHumidityPct, created.OperatorId,
            created.Status.ToString()
        );
        return Result<RelaxationSessionDto>.Success(dto);
    }
}

// CUT-02: Authorize Spreading Validation Gate
public record AuthorizeCuttingSpreadingCommand(Guid CuttingOrderId) : IRequest<Result<AuthorizeSpreadingResultDto>>;

public class AuthorizeCuttingSpreadingCommandHandler : IRequestHandler<AuthorizeCuttingSpreadingCommand, Result<AuthorizeSpreadingResultDto>>
{
    private readonly IProductionRepository _repository;
    public AuthorizeCuttingSpreadingCommandHandler(IProductionRepository repository) => _repository = repository;

    public async Task<Result<AuthorizeSpreadingResultDto>> Handle(AuthorizeCuttingSpreadingCommand request, CancellationToken cancellationToken)
    {
        var order = await _repository.GetCuttingOrderByIdAsync(request.CuttingOrderId, cancellationToken);
        if (order == null)
            return Result<AuthorizeSpreadingResultDto>.Failure("Cutting order not found.");

        // Extract roll barcodes
        var rollBarcodes = order.AllocatedRollBarcodes
            .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .ToList();

        if (rollBarcodes.Count == 0)
        {
            return Result<AuthorizeSpreadingResultDto>.Failure("No fabric rolls allocated to this cutting order.");
        }

        var allSessions = await _repository.GetRelaxationSessionsAsync(cancellationToken: cancellationToken);
        var sessionsByRoll = allSessions.ToDictionary(s => s.RollBarcode, StringComparer.OrdinalIgnoreCase);

        foreach (var barcode in rollBarcodes)
        {
            if (!sessionsByRoll.TryGetValue(barcode, out var session))
            {
                return Result<AuthorizeSpreadingResultDto>.Failure(
                    $"RELAXATION LOCK: Roll '{barcode}' has not been registered in an active relaxation session.");
            }

            if (!session.IsUnlocked())
            {
                var remaining = session.GetRemainingTime();
                return Result<AuthorizeSpreadingResultDto>.Failure(
                    $"RELAXATION LOCK ACTIVE: Roll '{barcode}' requires mandatory relaxation. " +
                    $"Remaining time: {remaining.Hours}h {remaining.Minutes}m. Spreading prohibited to prevent post-wash shrinkage defects.");
            }
        }

        // All rolls unlocked! Authorize spreading
        order.Status = CuttingStatus.ReadyForCut;
        await _repository.UpdateCuttingOrderAsync(order, cancellationToken);

        foreach (var barcode in rollBarcodes)
        {
            if (sessionsByRoll.TryGetValue(barcode, out var session))
            {
                session.Status = RelaxationStatus.SpreadingAuthorized;
                await _repository.UpdateRelaxationSessionAsync(session, cancellationToken);
            }
        }

        await _repository.SaveChangesAsync(cancellationToken);

        var result = new AuthorizeSpreadingResultDto(
            true,
            $"All {rollBarcodes.Count} fabric rolls fully relaxed. Spreading authorized on Table {order.TableNumber}.",
            order.CuttingOrderNumber,
            order.TableNumber
        );

        return Result<AuthorizeSpreadingResultDto>.Success(result);
    }
}

// CUT-04: Generate Serialized Component Bundles
public record GenerateThermalBundlesCommand(Guid CuttingOrderId, int BundleSize = 24) : IRequest<Result<List<CutBundleDto>>>;

public class GenerateThermalBundlesCommandHandler : IRequestHandler<GenerateThermalBundlesCommand, Result<List<CutBundleDto>>>
{
    private readonly IProductionRepository _repository;
    public GenerateThermalBundlesCommandHandler(IProductionRepository repository) => _repository = repository;

    public async Task<Result<List<CutBundleDto>>> Handle(GenerateThermalBundlesCommand request, CancellationToken cancellationToken)
    {
        var order = await _repository.GetCuttingOrderByIdAsync(request.CuttingOrderId, cancellationToken);
        if (order == null)
            return Result<List<CutBundleDto>>.Failure("Cutting order not found.");

        var components = new[] { "FRONT PANEL", "BACK PANEL", "WAISTBAND", "FLY & POCKET" };
        var bundles = new List<CutBundle>();
        var totalGarments = order.TotalGarmentsPlanned > 0 ? order.TotalGarmentsPlanned : order.TotalRequiredQty;
        var bundleSize = request.BundleSize > 0 ? request.BundleSize : 24;

        int totalBundleSets = (int)Math.Ceiling((double)totalGarments / bundleSize);
        int totalBundles = totalBundleSets * components.Length;
        int bundleSeq = 1;

        for (int set = 0; set < totalBundleSets; set++)
        {
            int serialStart = (set * bundleSize) + 1;
            int currentQty = Math.Min(bundleSize, totalGarments - (set * bundleSize));
            int serialEnd = serialStart + currentQty - 1;
            string size = (set % 2 == 0) ? "32/32" : "34/32";

            foreach (var comp in components)
            {
                var bundleNumber = $"{order.CuttingOrderNumber}-{bundleSeq:D2}";
                var barcode = $"BND-{order.CuttingOrderNumber}-{bundleSeq:D2}";

                bundles.Add(new CutBundle
                {
                    BundleNumber = bundleNumber,
                    Barcode = barcode,
                    CuttingOrderId = order.Id,
                    StyleNumber = order.StyleNumber,
                    Size = size,
                    Color = "Khaki Beige",
                    ShadeGroup = order.ShadeGroup.Replace("Shade ", "").Trim(),
                    ComponentName = comp,
                    Quantity = currentQty,
                    SerialStart = serialStart,
                    SerialEnd = serialEnd,
                    BundleSequence = bundleSeq,
                    TotalBundlesInOrder = totalBundles,
                    CurrentFloorStation = comp == "WAISTBAND" ? "FUSING_MACHINE" : "CUTTING_OUTPUT",
                    CurrentSection = BundleSection.Cutting
                });

                bundleSeq++;
            }
        }

        order.Status = CuttingStatus.CutCompleted;
        await _repository.AddBundlesAsync(bundles, cancellationToken);
        await _repository.UpdateCuttingOrderAsync(order, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var dtos = bundles.Select(b => new CutBundleDto(
            b.Id, b.BundleNumber, b.CuttingOrderId, b.Size, b.Color,
            b.Quantity, b.Barcode, b.SerialStart, b.SerialEnd,
            b.CurrentSection, b.IsCompleted, b.CompletedAt
        )).ToList();

        return Result<List<CutBundleDto>>.Success(dtos);
    }
}

// CUT-05: Fusing Machine Parameter Logging
public record CreateFusingLogCommand(CreateFusingMachineLogDto Dto) : IRequest<Result<FusingMachineLogDto>>;

public class CreateFusingLogCommandHandler : IRequestHandler<CreateFusingLogCommand, Result<FusingMachineLogDto>>
{
    private readonly IProductionRepository _repository;
    public CreateFusingLogCommandHandler(IProductionRepository repository) => _repository = repository;

    public async Task<Result<FusingMachineLogDto>> Handle(CreateFusingLogCommand request, CancellationToken cancellationToken)
    {
        var d = request.Dto;
        var log = new FusingMachineLog
        {
            CutOrderNumber = d.CutOrderNumber,
            MachineId = d.MachineId,
            ComponentName = d.ComponentName,
            InterliningType = d.InterliningType,
            TemperatureC = d.TemperatureC,
            PressureBar = d.PressureBar,
            SpeedSeconds = d.SpeedSeconds,
            OperatorName = d.OperatorName,
            Remarks = d.Remarks
        };

        log.ValidateFusingParameters(out var warning);
        if (!string.IsNullOrEmpty(warning))
        {
            log.Remarks += $" [Validation: {warning}]";
        }

        var created = await _repository.AddFusingLogAsync(log, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var dto = new FusingMachineLogDto(
            created.Id, created.CutOrderNumber, created.MachineId, created.ComponentName,
            created.InterliningType, created.TemperatureC, created.PressureBar,
            created.SpeedSeconds, created.OperatorName, created.IsApproved,
            created.Remarks, created.LoggedAtUtc
        );
        return Result<FusingMachineLogDto>.Success(dto);
    }
}

// CUT-06: Subcontract Cutting Commands
public record CreateSubcontractChallanCommand(CreateSubcontractCuttingChallanDto Dto) : IRequest<Result<SubcontractCuttingChallanDto>>;

public class CreateSubcontractChallanCommandHandler : IRequestHandler<CreateSubcontractChallanCommand, Result<SubcontractCuttingChallanDto>>
{
    private readonly IProductionRepository _repository;
    public CreateSubcontractChallanCommandHandler(IProductionRepository repository) => _repository = repository;

    public async Task<Result<SubcontractCuttingChallanDto>> Handle(CreateSubcontractChallanCommand request, CancellationToken cancellationToken)
    {
        var d = request.Dto;
        var existingCount = (await _repository.GetSubcontractChallansAsync(cancellationToken: cancellationToken)).Count();
        var challanNumber = $"SCC-2026-{(existingCount + 13):D4}";

        var challan = new SubcontractCuttingChallan
        {
            ChallanNumber = challanNumber,
            SubcontractorName = d.SubcontractorName,
            StyleNumber = d.StyleNumber,
            OrderNumber = d.OrderNumber,
            FabricSentYds = d.FabricSentYds,
            ExpectedCutPanels = d.ExpectedCutPanels,
            Remarks = d.Remarks,
            Status = SubcontractStatus.Dispatched
        };

        var created = await _repository.AddSubcontractChallanAsync(challan, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var dto = new SubcontractCuttingChallanDto(
            created.Id, created.ChallanNumber, created.SubcontractorName, created.StyleNumber,
            created.OrderNumber, created.FabricSentYds, created.ExpectedCutPanels,
            created.ReceivedCutPanels, created.DefectivePanels, created.ShortageCount,
            created.ShortagePercentage, created.Status.ToString(), created.DispatchedAtUtc,
            created.ReceivedAtUtc, created.ReconciledBy, created.Remarks
        );
        return Result<SubcontractCuttingChallanDto>.Success(dto);
    }
}

public record ReconcileSubcontractCommand(Guid Id, ReconcileSubcontractDto Dto) : IRequest<Result<SubcontractCuttingChallanDto>>;

public class ReconcileSubcontractCommandHandler : IRequestHandler<ReconcileSubcontractCommand, Result<SubcontractCuttingChallanDto>>
{
    private readonly IProductionRepository _repository;
    public ReconcileSubcontractCommandHandler(IProductionRepository repository) => _repository = repository;

    public async Task<Result<SubcontractCuttingChallanDto>> Handle(ReconcileSubcontractCommand request, CancellationToken cancellationToken)
    {
        var challan = await _repository.GetSubcontractChallanByIdAsync(request.Id, cancellationToken);
        if (challan == null)
            return Result<SubcontractCuttingChallanDto>.Failure("Subcontract challan not found.");

        challan.Reconcile(request.Dto.ReceivedCutPanels, request.Dto.DefectivePanels, request.Dto.ReconciledBy, request.Dto.Remarks);

        await _repository.UpdateSubcontractChallanAsync(challan, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var dto = new SubcontractCuttingChallanDto(
            challan.Id, challan.ChallanNumber, challan.SubcontractorName, challan.StyleNumber,
            challan.OrderNumber, challan.FabricSentYds, challan.ExpectedCutPanels,
            challan.ReceivedCutPanels, challan.DefectivePanels, challan.ShortageCount,
            challan.ShortagePercentage, challan.Status.ToString(), challan.DispatchedAtUtc,
            challan.ReceivedAtUtc, challan.ReconciledBy, challan.Remarks
        );
        return Result<SubcontractCuttingChallanDto>.Success(dto);
    }
}

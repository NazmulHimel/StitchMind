using GarmentsERP.Production.Application.DTOs;
using GarmentsERP.Production.Application.Interfaces;
using GarmentsERP.Production.Domain.Enums;
using GarmentsERP.Shared.Common;
using MediatR;

namespace GarmentsERP.Production.Application.Queries;

// CUT-01: CAD Markers Queries
public record GetCADMarkersQuery : IRequest<Result<List<CADMarkerDto>>>;

public class GetCADMarkersQueryHandler : IRequestHandler<GetCADMarkersQuery, Result<List<CADMarkerDto>>>
{
    private readonly IProductionRepository _repository;
    public GetCADMarkersQueryHandler(IProductionRepository repository) => _repository = repository;

    public async Task<Result<List<CADMarkerDto>>> Handle(GetCADMarkersQuery request, CancellationToken cancellationToken)
    {
        var markers = await _repository.GetCADMarkersAsync(cancellationToken);
        var dtos = markers.Select(m => new CADMarkerDto(
            m.Id, m.MarkerName, m.StyleId, m.StyleNumber, m.CADSoftware,
            m.CuttableWidthInches, m.MarkerLengthYards, m.GarmentsPerMarker,
            m.SizeDistributionRatio, m.MarkerEfficiencyPct, m.PlannedConsumptionPerPcYds,
            m.FileFormat
        )).ToList();
        return Result<List<CADMarkerDto>>.Success(dtos);
    }
}

// CUT-02: Relaxation Sessions Queries
public record GetRelaxationSessionsQuery(RelaxationStatus? Status = null) : IRequest<Result<List<RelaxationSessionDto>>>;

public class GetRelaxationSessionsQueryHandler : IRequestHandler<GetRelaxationSessionsQuery, Result<List<RelaxationSessionDto>>>
{
    private readonly IProductionRepository _repository;
    public GetRelaxationSessionsQueryHandler(IProductionRepository repository) => _repository = repository;

    public async Task<Result<List<RelaxationSessionDto>>> Handle(GetRelaxationSessionsQuery request, CancellationToken cancellationToken)
    {
        var sessions = await _repository.GetRelaxationSessionsAsync(request.Status, cancellationToken);
        var dtos = sessions.Select(s =>
        {
            var remaining = s.GetRemainingTime();
            return new RelaxationSessionDto(
                s.Id, s.FabricRollId, s.RollBarcode, s.DyeLotNumber, s.FabricType,
                s.UnrolledAtUtc, s.MandatoryDurationHours, s.UnlockedAtUtc,
                s.IsUnlocked(), remaining.Hours, remaining.Minutes,
                s.AmbientTemperatureC, s.RelativeHumidityPct, s.OperatorId,
                s.Status.ToString()
            );
        }).ToList();
        return Result<List<RelaxationSessionDto>>.Success(dtos);
    }
}

// CUT-05: Fusing Logs Queries
public record GetFusingLogsQuery(string? CutOrderNumber = null) : IRequest<Result<List<FusingMachineLogDto>>>;

public class GetFusingLogsQueryHandler : IRequestHandler<GetFusingLogsQuery, Result<List<FusingMachineLogDto>>>
{
    private readonly IProductionRepository _repository;
    public GetFusingLogsQueryHandler(IProductionRepository repository) => _repository = repository;

    public async Task<Result<List<FusingMachineLogDto>>> Handle(GetFusingLogsQuery request, CancellationToken cancellationToken)
    {
        var logs = await _repository.GetFusingLogsAsync(request.CutOrderNumber, cancellationToken);
        var dtos = logs.Select(l => new FusingMachineLogDto(
            l.Id, l.CutOrderNumber, l.MachineId, l.ComponentName, l.InterliningType,
            l.TemperatureC, l.PressureBar, l.SpeedSeconds, l.OperatorName,
            l.IsApproved, l.Remarks, l.LoggedAtUtc
        )).ToList();
        return Result<List<FusingMachineLogDto>>.Success(dtos);
    }
}

// CUT-06: Subcontract Challans Queries
public record GetSubcontractChallansQuery(SubcontractStatus? Status = null) : IRequest<Result<List<SubcontractCuttingChallanDto>>>;

public class GetSubcontractChallansQueryHandler : IRequestHandler<GetSubcontractChallansQuery, Result<List<SubcontractCuttingChallanDto>>>
{
    private readonly IProductionRepository _repository;
    public GetSubcontractChallansQueryHandler(IProductionRepository repository) => _repository = repository;

    public async Task<Result<List<SubcontractCuttingChallanDto>>> Handle(GetSubcontractChallansQuery request, CancellationToken cancellationToken)
    {
        var challans = await _repository.GetSubcontractChallansAsync(request.Status, cancellationToken);
        var dtos = challans.Select(c => new SubcontractCuttingChallanDto(
            c.Id, c.ChallanNumber, c.SubcontractorName, c.StyleNumber, c.OrderNumber,
            c.FabricSentYds, c.ExpectedCutPanels, c.ReceivedCutPanels, c.DefectivePanels,
            c.ShortageCount, c.ShortagePercentage, c.Status.ToString(), c.DispatchedAtUtc,
            c.ReceivedAtUtc, c.ReconciledBy, c.Remarks
        )).ToList();
        return Result<List<SubcontractCuttingChallanDto>>.Success(dtos);
    }
}

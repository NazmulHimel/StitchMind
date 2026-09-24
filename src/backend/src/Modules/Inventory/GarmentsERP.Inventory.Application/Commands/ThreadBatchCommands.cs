using FluentValidation;
using GarmentsERP.Inventory.Application.DTOs;
using GarmentsERP.Inventory.Application.Interfaces;
using GarmentsERP.Inventory.Domain.Entities;
using GarmentsERP.Inventory.Domain.Enums;
using GarmentsERP.Shared.Common;
using MediatR;

namespace GarmentsERP.Inventory.Application.Commands;

public record CreateThreadBatchCommand(
    string YarnCount,
    string FiberComposition,
    string ColorCode,
    string ColorName,
    decimal TargetQuantityKg,
    Guid CompanyId,
    Guid? SupervisorId = null,
    string? Notes = null
) : IRequest<Result<ThreadBatchDto>>;

public class CreateThreadBatchCommandValidator : AbstractValidator<CreateThreadBatchCommand>
{
    public CreateThreadBatchCommandValidator()
    {
        RuleFor(x => x.YarnCount).NotEmpty().MaximumLength(50);
        RuleFor(x => x.FiberComposition).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ColorCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ColorName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TargetQuantityKg).GreaterThan(0);
    }
}

public class CreateThreadBatchCommandHandler(IInventoryRepository repository)
    : IRequestHandler<CreateThreadBatchCommand, Result<ThreadBatchDto>>
{
    public async Task<Result<ThreadBatchDto>> Handle(CreateThreadBatchCommand request, CancellationToken cancellationToken)
    {
        var batchNo = $"YRN-{DateTime.UtcNow:yyyyMM}-{Random.Shared.Next(1000, 9999)}";

        var batch = ThreadBatch.Create(
            batchNo,
            request.YarnCount,
            request.FiberComposition,
            request.ColorCode,
            request.ColorName,
            request.TargetQuantityKg,
            request.CompanyId,
            request.SupervisorId,
            request.Notes
        );

        await repository.AddThreadBatchAsync(batch, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        var dto = MapToDto(batch);
        return Result<ThreadBatchDto>.Success(dto);
    }

    private static ThreadBatchDto MapToDto(ThreadBatch batch)
    {
        var logs = batch.StageLogs.Select(l => new ThreadStageLogDto(
            l.Id,
            l.ThreadBatchId,
            l.FromStage,
            l.FromStage.ToString(),
            l.ToStage,
            l.ToStage.ToString(),
            l.InputQuantityKg,
            l.OutputQuantityKg,
            l.WasteQuantityKg,
            l.MachineId,
            l.OperatorName,
            l.Remarks,
            l.CreatedAt
        )).ToList();

        return new ThreadBatchDto(
            batch.Id,
            batch.BatchNumber,
            batch.YarnCount,
            batch.FiberComposition,
            batch.ColorCode,
            batch.ColorName,
            batch.TargetQuantityKg,
            batch.CurrentQuantityKg,
            batch.TotalWastageKg,
            batch.CurrentStage,
            batch.CurrentStage.ToString(),
            batch.Status,
            batch.Status.ToString(),
            batch.StartDate,
            batch.CompletedDate,
            batch.SupervisorId,
            batch.Notes,
            logs,
            batch.CreatedAt
        );
    }
}

public record AdvanceThreadStageCommand(
    Guid BatchId,
    ThreadStage NextStage,
    decimal OutputQuantityKg,
    decimal WasteQuantityKg,
    string MachineId,
    string OperatorName,
    string? Remarks = null
) : IRequest<Result<ThreadBatchDto>>;

public class AdvanceThreadStageCommandHandler(IInventoryRepository repository)
    : IRequestHandler<AdvanceThreadStageCommand, Result<ThreadBatchDto>>
{
    public async Task<Result<ThreadBatchDto>> Handle(AdvanceThreadStageCommand request, CancellationToken cancellationToken)
    {
        var batch = await repository.GetThreadBatchByIdAsync(request.BatchId, cancellationToken);
        if (batch == null) return Result<ThreadBatchDto>.Failure("Thread batch not found.");

        try
        {
            batch.AdvanceStage(
                request.NextStage,
                request.OutputQuantityKg,
                request.WasteQuantityKg,
                request.MachineId,
                request.OperatorName,
                request.Remarks
            );

            await repository.UpdateThreadBatchAsync(batch, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);

            var logs = batch.StageLogs.Select(l => new ThreadStageLogDto(
                l.Id,
                l.ThreadBatchId,
                l.FromStage,
                l.FromStage.ToString(),
                l.ToStage,
                l.ToStage.ToString(),
                l.InputQuantityKg,
                l.OutputQuantityKg,
                l.WasteQuantityKg,
                l.MachineId,
                l.OperatorName,
                l.Remarks,
                l.CreatedAt
            )).ToList();

            var dto = new ThreadBatchDto(
                batch.Id,
                batch.BatchNumber,
                batch.YarnCount,
                batch.FiberComposition,
                batch.ColorCode,
                batch.ColorName,
                batch.TargetQuantityKg,
                batch.CurrentQuantityKg,
                batch.TotalWastageKg,
                batch.CurrentStage,
                batch.CurrentStage.ToString(),
                batch.Status,
                batch.Status.ToString(),
                batch.StartDate,
                batch.CompletedDate,
                batch.SupervisorId,
                batch.Notes,
                logs,
                batch.CreatedAt
            );

            return Result<ThreadBatchDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<ThreadBatchDto>.Failure(ex.Message);
        }
    }
}

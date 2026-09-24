using GarmentsERP.Inventory.Domain.Enums;
using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Inventory.Domain.Entities;

public class ThreadBatch : BaseEntity
{
    public string BatchNumber { get; private set; } = string.Empty;
    public string YarnCount { get; private set; } = string.Empty;
    public string FiberComposition { get; private set; } = string.Empty;
    public string ColorCode { get; private set; } = string.Empty;
    public string ColorName { get; private set; } = string.Empty;
    public decimal TargetQuantityKg { get; private set; }
    public decimal CurrentQuantityKg { get; private set; }
    public decimal TotalWastageKg { get; private set; } = 0;
    public ThreadStage CurrentStage { get; private set; } = ThreadStage.RawFiber;
    public ThreadBatchStatus Status { get; private set; } = ThreadBatchStatus.InProgress;
    public DateTime StartDate { get; private set; } = DateTime.UtcNow;
    public DateTime? CompletedDate { get; private set; }
    public Guid? SupervisorId { get; private set; }
    public string? Notes { get; private set; }
    public Guid CompanyId { get; private set; }

    public ICollection<ThreadStageLog> StageLogs { get; private set; } = [];

    private ThreadBatch() { }

    public static ThreadBatch Create(
        string batchNumber, string yarnCount, string fiberComposition,
        string colorCode, string colorName, decimal targetKg,
        Guid companyId, Guid? supervisorId = null, string? notes = null)
    {
        return new ThreadBatch
        {
            BatchNumber = batchNumber.Trim().ToUpper(),
            YarnCount = yarnCount.Trim(),
            FiberComposition = fiberComposition.Trim(),
            ColorCode = colorCode.Trim().ToUpper(),
            ColorName = colorName.Trim(),
            TargetQuantityKg = targetKg,
            CurrentQuantityKg = targetKg,
            CompanyId = companyId,
            SupervisorId = supervisorId,
            Notes = notes,
            CurrentStage = ThreadStage.RawFiber,
            Status = ThreadBatchStatus.InProgress
        };
    }

    public void AdvanceStage(ThreadStage nextStage, decimal outputQtyKg, decimal wasteKg, string machineId, string operatorName, string? remarks = null)
    {
        TotalWastageKg += wasteKg;
        CurrentQuantityKg = outputQtyKg;

        var log = ThreadStageLog.Create(Id, CurrentStage, nextStage, outputQtyKg + wasteKg, outputQtyKg, wasteKg, machineId, operatorName, remarks);
        StageLogs.Add(log);

        CurrentStage = nextStage;
        if (nextStage == ThreadStage.Packaged)
        {
            Status = ThreadBatchStatus.Completed;
            CompletedDate = DateTime.UtcNow;
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(ThreadBatchStatus newStatus)
    {
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }
}

public class ThreadStageLog : BaseEntity
{
    public Guid ThreadBatchId { get; private set; }
    public ThreadStage FromStage { get; private set; }
    public ThreadStage ToStage { get; private set; }
    public decimal InputQuantityKg { get; private set; }
    public decimal OutputQuantityKg { get; private set; }
    public decimal WasteQuantityKg { get; private set; }
    public string MachineId { get; private set; } = string.Empty;
    public string OperatorName { get; private set; } = string.Empty;
    public string? Remarks { get; private set; }

    public ThreadBatch ThreadBatch { get; private set; } = null!;

    private ThreadStageLog() { }

    public static ThreadStageLog Create(
        Guid batchId, ThreadStage fromStage, ThreadStage toStage,
        decimal inputKg, decimal outputKg, decimal wasteKg,
        string machineId, string operatorName, string? remarks = null)
    {
        return new ThreadStageLog
        {
            ThreadBatchId = batchId,
            FromStage = fromStage,
            ToStage = toStage,
            InputQuantityKg = inputKg,
            OutputQuantityKg = outputKg,
            WasteQuantityKg = wasteKg,
            MachineId = machineId.Trim().ToUpper(),
            OperatorName = operatorName.Trim(),
            Remarks = remarks
        };
    }
}

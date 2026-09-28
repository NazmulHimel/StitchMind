using GarmentsERP.Production.Domain.Enums;
using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Production.Domain.Entities;

/// <summary>
/// CUT-02: Mandatory Relaxation Time-Lock Gate for Knit and Stretch Woven Fabrics.
/// Prevents cutting floor spreading until the 24-hour unrolled relaxation countdown has elapsed.
/// </summary>
public class RelaxationSession : BaseEntity
{
    public Guid FabricRollId { get; set; }
    public string RollBarcode { get; set; } = string.Empty; // e.g. ROL-2026-00891
    public string DyeLotNumber { get; set; } = string.Empty; // e.g. LOT-TW-2026-01
    public string FabricType { get; set; } = "98/2 Cotton Spandex Twill (260 GSM)";
    public DateTime UnrolledAtUtc { get; set; } = DateTime.UtcNow;
    public int MandatoryDurationHours { get; set; } = 24;
    public DateTime UnlockedAtUtc { get; set; }
    public decimal AmbientTemperatureC { get; set; } = 24.5m;
    public decimal RelativeHumidityPct { get; set; } = 65.0m;
    public string OperatorId { get; set; } = "Cutter-Master-01";
    public RelaxationStatus Status { get; set; } = RelaxationStatus.Relaxing;
    public Guid? CuttingOrderId { get; set; }

    public RelaxationSession()
    {
        UnlockedAtUtc = UnrolledAtUtc.AddHours(MandatoryDurationHours);
    }

    public static RelaxationSession StartSession(
        Guid fabricRollId,
        string rollBarcode,
        string dyeLotNumber,
        string fabricType,
        int durationHours,
        decimal tempC,
        decimal humidityPct,
        string operatorId,
        DateTime? unrolledAt = null)
    {
        var startTime = unrolledAt ?? DateTime.UtcNow;
        return new RelaxationSession
        {
            FabricRollId = fabricRollId,
            RollBarcode = rollBarcode,
            DyeLotNumber = dyeLotNumber,
            FabricType = fabricType,
            MandatoryDurationHours = durationHours,
            UnrolledAtUtc = startTime,
            UnlockedAtUtc = startTime.AddHours(durationHours),
            AmbientTemperatureC = tempC,
            RelativeHumidityPct = humidityPct,
            OperatorId = operatorId,
            Status = (startTime.AddHours(durationHours) <= DateTime.UtcNow)
                ? RelaxationStatus.Unlocked
                : RelaxationStatus.Relaxing
        };
    }

    public bool IsUnlocked(DateTime? asOfUtc = null)
    {
        var now = asOfUtc ?? DateTime.UtcNow;
        return now >= UnlockedAtUtc;
    }

    public TimeSpan GetRemainingTime(DateTime? asOfUtc = null)
    {
        var now = asOfUtc ?? DateTime.UtcNow;
        if (now >= UnlockedAtUtc) return TimeSpan.Zero;
        return UnlockedAtUtc - now;
    }

    public bool TryAuthorizeSpreading(DateTime? asOfUtc, out string reason)
    {
        var now = asOfUtc ?? DateTime.UtcNow;
        if (!IsUnlocked(now))
        {
            var remaining = GetRemainingTime(now);
            reason = $"RELAXATION LOCK ACTIVE: Roll '{RollBarcode}' requires mandatory relaxation. " +
                     $"Remaining time: {remaining.Hours}h {remaining.Minutes}m. Spreading prohibited to prevent post-wash shrinkage defects.";
            return false;
        }

        Status = RelaxationStatus.SpreadingAuthorized;
        reason = $"Roll '{RollBarcode}' is fully relaxed and authorized for table spreading.";
        return true;
    }
}

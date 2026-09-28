using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Production.Domain.Entities;

/// <summary>
/// CUT-05: Fusing Machine Parameter Logging for Interlining adhesion (Temperature, Pressure, Speed/Time).
/// Ensures zero bubbling, delamination, or panel shrinkage.
/// </summary>
public class FusingMachineLog : BaseEntity
{
    public string CutOrderNumber { get; set; } = string.Empty; // e.g. CUT-2026-0001
    public string MachineId { get; set; } = "FUS-02";
    public string ComponentName { get; set; } = "Waistband"; // Waistband, Collar, Cuff, Pocket Flap
    public string InterliningType { get; set; } = "Fusible Microdot Woven 70 GSM";
    public decimal TemperatureC { get; set; } = 150.0m;
    public decimal PressureBar { get; set; } = 3.5m;
    public decimal SpeedSeconds { get; set; } = 12.0m;
    public string OperatorName { get; set; } = "Mizanur Rahman";
    public bool IsApproved { get; set; } = true;
    public string Remarks { get; set; } = "Adhesion peel test passed (>= 1.2 kg/inch).";
    public DateTime LoggedAtUtc { get; set; } = DateTime.UtcNow;

    public bool ValidateFusingParameters(out string warningMessage)
    {
        if (TemperatureC < 135 || TemperatureC > 165)
        {
            warningMessage = $"Temperature {TemperatureC}°C outside optimal range (135°C - 165°C). Delamination risk.";
            IsApproved = false;
            return false;
        }

        if (PressureBar < 2.5m || PressureBar > 4.5m)
        {
            warningMessage = $"Pressure {PressureBar} bar outside optimal range (2.5 - 4.5 bar). Inadequate bond.";
            IsApproved = false;
            return false;
        }

        if (SpeedSeconds < 8 || SpeedSeconds > 20)
        {
            warningMessage = $"Dwell time {SpeedSeconds}s outside optimal range (8 - 20s).";
            IsApproved = false;
            return false;
        }

        warningMessage = "Parameters compliant with technical datasheet.";
        IsApproved = true;
        return true;
    }
}

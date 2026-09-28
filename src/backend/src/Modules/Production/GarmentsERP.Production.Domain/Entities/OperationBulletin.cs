using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Production.Domain.Entities;

/// <summary>
/// IE-01: Industrial Engineering Operation Bulletin & SMV Breakdown.
/// Defines the technical operation sequence, machinery specifications, standard minute values (SMV),
/// and piece-rate benchmarks for style manufacturing.
/// </summary>
public class OperationBulletin : BaseEntity
{
    public Guid? StyleId { get; set; }
    public string StyleNumber { get; set; } = string.Empty; // e.g. ST-2026-CHINO-01
    public int OperationSequence { get; set; } // 1, 2, 3...
    public string OperationName { get; set; } = string.Empty; // e.g. "Waistband Attachment"
    public string SectionCategory { get; set; } = "Assembly"; // Front, Back, Assembly, Finishing
    public string MachineType { get; set; } = "Multi-Needle Chainstitch"; // SNLS, Overlock 4T, Chainstitch, Bar Tack
    public decimal StandardMinuteValueSMV { get; set; } = 1.100m; // in minutes
    public int TargetPerHour { get; set; } = 54; // Formula: 60 / SMV
    public string RequiredSkillGrade { get; set; } = "A"; // A+, A, B, C
    public decimal PieceRatePerDzn { get; set; } = 0.85m;

    public void RecalculateTarget()
    {
        if (StandardMinuteValueSMV > 0)
        {
            TargetPerHour = (int)Math.Round(60.0m / StandardMinuteValueSMV);
        }
    }
}

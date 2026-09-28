using GarmentsERP.Shared.Domain;
using GarmentsERP.HR.Domain.Enums;

namespace GarmentsERP.HR.Domain.Entities;

public class ClinicVisit : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public DateTime VisitDate { get; set; }
    public string ChiefComplaint { get; set; } = string.Empty; // e.g. Dizziness, Needle Puncture, Headache
    public string Diagnosis { get; set; } = string.Empty;
    public string TreatmentGiven { get; set; } = string.Empty; // e.g. Antiseptic dressing, Paracetamol
    public string MedicineDispensed { get; set; } = string.Empty;
    public ClinicDutyFitnessStatus FitnessStatus { get; set; } = ClinicDutyFitnessStatus.FitForNormalDuty;
    public string AttendingNurseOrDoctor { get; set; } = "Dr. Shahnaz Parvin (Factory Medical Officer)";
    public string? Remarks { get; set; }
}

public class WorkerGrievance : BaseEntity
{
    public string TrackingNumber { get; set; } = string.Empty; // e.g. GRV-2026-0042
    public DateTime SubmissionDate { get; set; }
    public bool IsAnonymous { get; set; } = true;
    public Guid? EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public GrievanceCategory Category { get; set; } = GrievanceCategory.WageDispute;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public GrievanceStatus Status { get; set; } = GrievanceStatus.Submitted;

    public string? InvestigatedBy { get; set; }
    public string? WelfareCommitteeFindings { get; set; }
    public string? ResolutionSummary { get; set; }
    public DateTime? ResolvedDate { get; set; }
}

public class DisciplinaryAction : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public string NoticeNumber { get; set; } = string.Empty; // e.g. DISC-2026-0019
    public DateTime IncidentDate { get; set; }
    public DisciplinaryViolationType ViolationType { get; set; } = DisciplinaryViolationType.UnauthorizedAbsence;
    public string AllegationDescription { get; set; } = string.Empty;
    public DateTime? ExplanationSubmissionDeadline { get; set; }
    public string? EmployeeExplanation { get; set; }
    public DateTime? HearingDate { get; set; }
    public DisciplinaryOutcome Outcome { get; set; } = DisciplinaryOutcome.FirstWrittenWarning;
    public string AuthorizedBy { get; set; } = "Head of HR & Social Compliance";
    public DateTime IssuedDate { get; set; }
}

namespace GarmentsERP.HR.Domain.Enums;

public enum EmployeeStatus
{
    Active = 1,
    OnLeave = 2,
    Suspended = 3,
    Terminated = 4
}

public enum AgeVerificationStatus
{
    PendingVerification = 1,
    AgeVerified = 2,
    BlockedUnderageLabor = 3
}

public enum PayStreamType
{
    MonthlyStaff = 1,
    FloorPieceRate = 2
}

public enum PaymentDisbursementChannel
{
    bKash = 1,
    Nagad = 2,
    BankTransfer = 3,
    Cash = 4
}

public enum BiometricProtocolType
{
    ZKTeco_Standalone = 1,
    FacialRecognition_Terminal = 2,
    RFID_CardReader = 3
}

public enum PunchInOutMode
{
    CheckIn = 1,
    CheckOut = 2,
    BreakIn = 3,
    BreakOut = 4
}

public enum GrievanceCategory
{
    Harassment = 1,
    WageDispute = 2,
    CanteenFoodQuality = 3,
    OccupationalSafety = 4,
    SupervisorMisconduct = 5,
    WorkingHoursAndOvertime = 6
}

public enum GrievanceStatus
{
    Submitted = 1,
    UnderInvestigation = 2,
    CommitteeReview = 3,
    Resolved = 4,
    Dismissed = 5
}

public enum DisciplinaryViolationType
{
    UnauthorizedAbsence = 1,
    Insubordination = 2,
    SafetyViolation = 3,
    QualitySabotage = 4,
    TheftOrFraud = 5
}

public enum DisciplinaryOutcome
{
    VerbalWarning = 1,
    FirstWrittenWarning = 2,
    SecondWrittenWarning = 3,
    Suspension = 4,
    Dismissal = 5
}

public enum ClinicDutyFitnessStatus
{
    FitForNormalDuty = 1,
    LightDutyAssigned = 2,
    SentHomeForRest = 3,
    ReferredToHospital = 4
}

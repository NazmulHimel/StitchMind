using GarmentsERP.HR.Domain.Entities;
using GarmentsERP.HR.Domain.Enums;

namespace GarmentsERP.HR.Application.DTOs;

public record EmployeeDto(
    Guid Id,
    string EmployeeCode,
    string FullName,
    string FullNameBangla,
    string Email,
    string PhoneNumber,
    string Department,
    string Designation,
    string? AssignedLine,
    DateTime DateOfBirth,
    decimal CalculatedAgeYears,
    bool IsAgeVerified,
    string AgeVerificationStatus,
    string PayStream,
    decimal PieceRatePerUnit,
    decimal BasicSalary,
    decimal HouseRentAllowance,
    decimal MedicalAllowance,
    decimal ConveyanceAllowance,
    decimal GrossSalary,
    DateTime JoiningDate,
    string StatusName,
    string? NationalId,
    string DisbursementChannel,
    string? MobileBankingNumber,
    string? BiometricBadgeNumber,
    string? EmergencyContact
);

public record AttendanceRecordDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeCode,
    string EmployeeName,
    string Department,
    string? AssignedLine,
    DateTime Date,
    string? CheckInTime,
    string? CheckOutTime,
    decimal OvertimeHours,
    string StatusName,
    string? ShiftName,
    string? Remarks
);

public record BiometricDeviceDto(
    Guid Id,
    string DeviceCode,
    string DeviceName,
    string IpAddress,
    int PortNumber,
    string ProtocolType,
    string PhysicalLocation,
    bool IsOnline,
    DateTime? LastSyncTime,
    int TotalPunchesRecorded
);

public record BiometricPunchLogDto(
    Guid Id,
    Guid BiometricDeviceId,
    string DeviceCode,
    string DeviceName,
    Guid? EmployeeId,
    string? EmployeeName,
    string BiometricBadgeNumber,
    DateTime PunchTimestamp,
    string PunchMode,
    string VerificationType
);

public record WorkShiftDto(
    Guid Id,
    string ShiftCode,
    string ShiftName,
    string StartTime,
    string EndTime,
    int GracePeriodMinutes,
    int HalfDayThresholdMinutes,
    bool IsNightShift,
    decimal ShiftAllowanceDaily,
    bool IsActive
);

public record ShiftRosterDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeCode,
    string EmployeeName,
    Guid WorkShiftId,
    string ShiftCode,
    string ShiftName,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo,
    string AssignedLineOrSection
);

public record LateAttendancePenaltyDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeCode,
    string EmployeeName,
    int PayMonth,
    int PayYear,
    int TotalLateDays,
    int PenaltyDeductionDays,
    decimal DeductionAmount,
    bool IsAppliedToPayroll
);

public record LeaveRequestDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeCode,
    string EmployeeName,
    string Department,
    string LeaveTypeName,
    DateTime StartDate,
    DateTime EndDate,
    int TotalDays,
    string Reason,
    string StatusName,
    string? ApprovedBy,
    DateTime? ApprovedAt
);

public record EmployeeLeaveBalanceDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeCode,
    string EmployeeName,
    int CalendarYear,
    int CasualLeaveTotal,
    int CasualLeaveUsed,
    int CasualLeaveRemaining,
    int SickLeaveTotal,
    int SickLeaveUsed,
    int SickLeaveRemaining,
    int EarnedLeaveTotal,
    int EarnedLeaveUsed,
    int EarnedLeaveRemaining,
    int EarnedLeaveEncashedDays,
    decimal TotalEncashedAmount
);

public record MaternityBenefitDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeCode,
    string EmployeeName,
    string CaseReferenceNumber,
    DateTime NoticeDate,
    DateTime ExpectedDeliveryDate,
    DateTime LeaveStartDate,
    DateTime LeaveEndDate,
    int TotalLeaveDays,
    decimal PriorThreeMonthsTotalWages,
    int PriorThreeMonthsWorkingDays,
    decimal AverageDailyWage,
    decimal FirstInstallmentAmount,
    bool IsFirstInstallmentPaid,
    DateTime? FirstInstallmentPaidDate,
    decimal SecondInstallmentAmount,
    bool IsSecondInstallmentPaid,
    DateTime? SecondInstallmentPaidDate,
    decimal TotalBenefitAmount,
    string MedicalDoctorCertificateNumber,
    string RegisteredClinicName,
    string PaymentDisbursementMethod
);

public record PayrollRecordDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeCode,
    string EmployeeName,
    string FullNameBangla,
    string Department,
    string Designation,
    int PayMonth,
    int PayYear,
    string PayStream,
    decimal BasicSalary,
    decimal Allowances,
    int CompletedGarmentUnits,
    decimal PieceRatePerUnit,
    decimal PieceRateEarnings,
    decimal OvertimeHours,
    decimal OvertimeRatePerHour,
    decimal OvertimeAmount,
    decimal AttendanceBonus,
    decimal AbsentDeduction,
    decimal LatePenaltyDeduction,
    decimal TaxDeduction,
    decimal AdvanceDeduction,
    decimal TotalDeductions,
    decimal GrossEarnings,
    decimal NetPayable,
    string DisbursementChannel,
    string? MfsOrAccountNumber,
    string StatusName,
    DateTime? DisbursedDate,
    string? PaymentReference
);

public record PayrollSheetDto(
    Guid Id,
    string SheetNumber,
    int PayMonth,
    int PayYear,
    int TotalHeadcount,
    int TotalPieceRateWorkers,
    int TotalStaffWorkers,
    decimal TotalBaseOrPieceRateEarnings,
    decimal TotalOvertimeAmount,
    decimal TotalAttendanceBonus,
    decimal TotalGrossEarnings,
    decimal TotalDeductions,
    decimal TotalNetDisbursement,
    string StatusName,
    string? ApprovedBy,
    DateTime? ApprovedAt,
    DateTime? DisbursedAt,
    IReadOnlyList<PayrollRecordDto> Records
);

public record MfsPayoutExportRowDto(
    string EmployeeCode,
    string EmployeeName,
    string AccountNumber,
    decimal Amount,
    string Currency,
    string Reference
);

public record ClinicVisitDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeCode,
    string EmployeeName,
    string Department,
    DateTime VisitDate,
    string ChiefComplaint,
    string Diagnosis,
    string TreatmentGiven,
    string MedicineDispensed,
    string FitnessStatus,
    string AttendingNurseOrDoctor,
    string? Remarks
);

public record WorkerGrievanceDto(
    Guid Id,
    string TrackingNumber,
    DateTime SubmissionDate,
    bool IsAnonymous,
    Guid? EmployeeId,
    string? EmployeeName,
    string Category,
    string Title,
    string Description,
    string Status,
    string? InvestigatedBy,
    string? WelfareCommitteeFindings,
    string? ResolutionSummary,
    DateTime? ResolvedDate
);

public record DisciplinaryActionDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeCode,
    string EmployeeName,
    string Department,
    string NoticeNumber,
    DateTime IncidentDate,
    string ViolationType,
    string AllegationDescription,
    DateTime? HearingDate,
    string Outcome,
    string AuthorizedBy,
    DateTime IssuedDate
);

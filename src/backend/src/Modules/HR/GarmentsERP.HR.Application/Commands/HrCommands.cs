using MediatR;
using GarmentsERP.HR.Application.DTOs;
using GarmentsERP.HR.Application.Interfaces;
using GarmentsERP.HR.Domain.Entities;
using GarmentsERP.HR.Domain.Enums;
using GarmentsERP.Shared.Common;

namespace GarmentsERP.HR.Application.Commands;

public record CreateEmployeeCommand(
    string EmployeeCode,
    string FullName,
    string FullNameBangla,
    string Email,
    string PhoneNumber,
    string Department,
    string Designation,
    string? AssignedLine,
    DateTime DateOfBirth,
    DateTime JoiningDate,
    PayStreamType PayStream,
    decimal PieceRatePerUnit,
    decimal BasicSalary,
    decimal HouseRentAllowance,
    decimal MedicalAllowance,
    decimal ConveyanceAllowance,
    string? NationalId,
    PaymentDisbursementChannel DisbursementChannel,
    string? MobileBankingNumber,
    string? BiometricBadgeNumber,
    string? EmergencyContact
) : IRequest<Result<EmployeeDto>>;

public record RecordAttendancePunchCommand(
    string EmployeeCode,
    DateTime PunchTime,
    string? BiometricDeviceId,
    string? ShiftName
) : IRequest<Result<AttendanceRecordDto>>;

public record SyncBiometricDeviceLogsCommand(
    Guid DeviceId,
    IReadOnlyList<BiometricPunchInputDto> Punches
) : IRequest<Result<int>>;

public record BiometricPunchInputDto(
    string BiometricBadgeNumber,
    DateTime Timestamp,
    PunchInOutMode Mode,
    string VerificationType
);

public record RequestLeaveCommand(
    Guid EmployeeId,
    LeaveType Type,
    DateTime StartDate,
    DateTime EndDate,
    string Reason
) : IRequest<Result<LeaveRequestDto>>;

public record ApproveLeaveCommand(
    Guid LeaveRequestId,
    bool IsApproved,
    string? ApproverId,
    string? RejectionReason
) : IRequest<Result<bool>>;

public record CalculateEarnedLeaveEncashmentCommand(
    Guid EmployeeId,
    int CalendarYear,
    int DaysToEncash
) : IRequest<Result<EmployeeLeaveBalanceDto>>;

public record CalculateMaternityBenefitCommand(
    Guid EmployeeId,
    string CaseReferenceNumber,
    DateTime ExpectedDeliveryDate,
    DateTime LeaveStartDate,
    decimal PriorThreeMonthsTotalWages,
    string MedicalDoctorCertificateNumber,
    string DisbursementMethod
) : IRequest<Result<MaternityBenefitDto>>;

public record GenerateDualStreamPayrollCommand(
    int PayMonth,
    int PayYear
) : IRequest<Result<PayrollSheetDto>>;

public record DisbursePayrollSheetCommand(
    Guid PayrollSheetId,
    string ApprovedBy
) : IRequest<Result<bool>>;

public record LogClinicVisitCommand(
    Guid EmployeeId,
    DateTime VisitDate,
    string ChiefComplaint,
    string Diagnosis,
    string TreatmentGiven,
    string MedicineDispensed,
    ClinicDutyFitnessStatus FitnessStatus,
    string AttendingNurseOrDoctor,
    string? Remarks
) : IRequest<Result<ClinicVisitDto>>;

public record SubmitWorkerGrievanceCommand(
    bool IsAnonymous,
    Guid? EmployeeId,
    GrievanceCategory Category,
    string Title,
    string Description
) : IRequest<Result<WorkerGrievanceDto>>;

public record ResolveWorkerGrievanceCommand(
    Guid GrievanceId,
    string InvestigatedBy,
    string WelfareCommitteeFindings,
    string ResolutionSummary,
    bool IsResolved
) : IRequest<Result<WorkerGrievanceDto>>;

public record RecordDisciplinaryActionCommand(
    Guid EmployeeId,
    string NoticeNumber,
    DateTime IncidentDate,
    DisciplinaryViolationType ViolationType,
    string AllegationDescription,
    DateTime? HearingDate,
    DisciplinaryOutcome Outcome,
    string AuthorizedBy
) : IRequest<Result<DisciplinaryActionDto>>;

public class HrCommandHandlers :
    IRequestHandler<CreateEmployeeCommand, Result<EmployeeDto>>,
    IRequestHandler<RecordAttendancePunchCommand, Result<AttendanceRecordDto>>,
    IRequestHandler<SyncBiometricDeviceLogsCommand, Result<int>>,
    IRequestHandler<RequestLeaveCommand, Result<LeaveRequestDto>>,
    IRequestHandler<ApproveLeaveCommand, Result<bool>>,
    IRequestHandler<CalculateEarnedLeaveEncashmentCommand, Result<EmployeeLeaveBalanceDto>>,
    IRequestHandler<CalculateMaternityBenefitCommand, Result<MaternityBenefitDto>>,
    IRequestHandler<GenerateDualStreamPayrollCommand, Result<PayrollSheetDto>>,
    IRequestHandler<DisbursePayrollSheetCommand, Result<bool>>,
    IRequestHandler<LogClinicVisitCommand, Result<ClinicVisitDto>>,
    IRequestHandler<SubmitWorkerGrievanceCommand, Result<WorkerGrievanceDto>>,
    IRequestHandler<ResolveWorkerGrievanceCommand, Result<WorkerGrievanceDto>>,
    IRequestHandler<RecordDisciplinaryActionCommand, Result<DisciplinaryActionDto>>
{
    private readonly IHrRepository _repo;

    public HrCommandHandlers(IHrRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result<EmployeeDto>> Handle(CreateEmployeeCommand request, CancellationToken ct)
    {
        var existing = await _repo.GetEmployeeByCodeAsync(request.EmployeeCode, ct);
        if (existing != null)
            return Result<EmployeeDto>.Failure($"Employee with code '{request.EmployeeCode}' already exists.");

        var employee = new Employee
        {
            EmployeeCode = request.EmployeeCode,
            FullName = request.FullName,
            FullNameBangla = request.FullNameBangla,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Department = request.Department,
            Designation = request.Designation,
            AssignedLine = request.AssignedLine,
            DateOfBirth = request.DateOfBirth,
            JoiningDate = request.JoiningDate,
            PayStream = request.PayStream,
            PieceRatePerUnit = request.PieceRatePerUnit,
            BasicSalary = request.BasicSalary,
            HouseRentAllowance = request.HouseRentAllowance,
            MedicalAllowance = request.MedicalAllowance,
            ConveyanceAllowance = request.ConveyanceAllowance,
            NationalIdOrBirthCert = request.NationalId,
            DisbursementChannel = request.DisbursementChannel,
            MobileBankingNumber = request.MobileBankingNumber,
            BiometricBadgeNumber = request.BiometricBadgeNumber ?? $"BIO-{request.EmployeeCode}",
            EmergencyContact = request.EmergencyContact,
            Status = EmployeeStatus.Active
        };

        // AC-07 Child Labor Age Gate Check
        bool isAgeValid = employee.VerifyAgeCompliance(request.JoiningDate);
        if (!isAgeValid)
        {
            // Do not complete onboarding if underage
            return Result<EmployeeDto>.Failure(
                $"STATUTORY COMPLIANCE VIOLATION: Applicant age is {employee.CalculatedAgeYears} years. " +
                "StitchMind strictly prohibits onboarding individuals under 18.0 years of age under International Labor Standards & Bangladesh Labor Act.");
        }

        await _repo.AddEmployeeAsync(employee, ct);
        await _repo.SaveChangesAsync(ct);

        return Result<EmployeeDto>.Success(MapEmployee(employee));
    }

    public async Task<Result<AttendanceRecordDto>> Handle(RecordAttendancePunchCommand request, CancellationToken ct)
    {
        var employee = await _repo.GetEmployeeByCodeAsync(request.EmployeeCode, ct);
        if (employee == null)
            return Result<AttendanceRecordDto>.Failure($"Employee code '{request.EmployeeCode}' not found.");

        var today = request.PunchTime.Date;
        var existing = await _repo.GetAttendanceByEmployeeDateAsync(employee.Id, today, ct);

        if (existing == null)
        {
            var timeSpan = request.PunchTime.TimeOfDay;
            var isLate = timeSpan > new TimeSpan(8, 15, 0); // After 15 mins grace period

            existing = new AttendanceRecord
            {
                EmployeeId = employee.Id,
                Date = today,
                CheckInTime = timeSpan,
                Status = isLate ? AttendanceStatus.Late : AttendanceStatus.Present,
                ShiftName = request.ShiftName ?? "Morning Shift (08:00 - 17:00)",
                BiometricDeviceId = request.BiometricDeviceId ?? "BIO-GATE-01",
                Remarks = isLate ? "Late arrival recorded by biometric listener" : "On-time arrival"
            };
            await _repo.AddAttendanceAsync(existing, ct);
        }
        else
        {
            var checkOutTime = request.PunchTime.TimeOfDay;
            existing.CheckOutTime = checkOutTime;

            var standardShiftEnd = new TimeSpan(17, 0, 0);
            if (checkOutTime > standardShiftEnd)
            {
                var otMinutes = (checkOutTime - standardShiftEnd).TotalMinutes;
                existing.OvertimeHours = Math.Round((decimal)(otMinutes / 60.0), 2);
            }
            await _repo.UpdateAttendanceAsync(existing, ct);
        }

        await _repo.SaveChangesAsync(ct);

        return Result<AttendanceRecordDto>.Success(new AttendanceRecordDto(
            existing.Id,
            employee.Id,
            employee.EmployeeCode,
            employee.FullName,
            employee.Department,
            employee.AssignedLine,
            existing.Date,
            existing.CheckInTime?.ToString(@"hh\:mm"),
            existing.CheckOutTime?.ToString(@"hh\:mm"),
            existing.OvertimeHours,
            existing.Status.ToString(),
            existing.ShiftName,
            existing.Remarks
        ));
    }

    public async Task<Result<int>> Handle(SyncBiometricDeviceLogsCommand request, CancellationToken ct)
    {
        var device = await _repo.GetBiometricDeviceByIdAsync(request.DeviceId, ct);
        if (device == null)
            return Result<int>.Failure($"Biometric device '{request.DeviceId}' not found.");

        int syncedCount = 0;
        foreach (var p in request.Punches)
        {
            var punchLog = new BiometricPunchLog
            {
                BiometricDeviceId = device.Id,
                BiometricBadgeNumber = p.BiometricBadgeNumber,
                PunchTimestamp = p.Timestamp,
                PunchMode = p.Mode,
                VerificationType = p.VerificationType,
                IsProcessedToAttendance = true
            };
            await _repo.AddBiometricPunchLogAsync(punchLog, ct);
            syncedCount++;
        }

        device.LastSyncTime = DateTime.UtcNow;
        device.TotalPunchesRecorded += syncedCount;
        await _repo.SaveChangesAsync(ct);

        return Result<int>.Success(syncedCount);
    }

    public async Task<Result<LeaveRequestDto>> Handle(RequestLeaveCommand request, CancellationToken ct)
    {
        var employee = await _repo.GetEmployeeByIdAsync(request.EmployeeId, ct);
        if (employee == null)
            return Result<LeaveRequestDto>.Failure("Employee not found.");

        int totalDays = (int)(request.EndDate.Date - request.StartDate.Date).TotalDays + 1;
        if (totalDays <= 0)
            return Result<LeaveRequestDto>.Failure("End date must be on or after start date.");

        var leave = new LeaveRequest
        {
            EmployeeId = request.EmployeeId,
            Type = request.Type,
            StartDate = request.StartDate.Date,
            EndDate = request.EndDate.Date,
            TotalDays = totalDays,
            Reason = request.Reason,
            Status = LeaveStatus.Pending
        };

        await _repo.AddLeaveRequestAsync(leave, ct);
        await _repo.SaveChangesAsync(ct);

        return Result<LeaveRequestDto>.Success(new LeaveRequestDto(
            leave.Id,
            employee.Id,
            employee.EmployeeCode,
            employee.FullName,
            employee.Department,
            leave.Type.ToString(),
            leave.StartDate,
            leave.EndDate,
            leave.TotalDays,
            leave.Reason,
            leave.Status.ToString(),
            leave.ApprovedBy,
            leave.ApprovedAt
        ));
    }

    public async Task<Result<bool>> Handle(ApproveLeaveCommand request, CancellationToken ct)
    {
        var leave = await _repo.GetLeaveRequestByIdAsync(request.LeaveRequestId, ct);
        if (leave == null)
            return Result<bool>.Failure("Leave request not found.");

        leave.Status = request.IsApproved ? LeaveStatus.Approved : LeaveStatus.Rejected;
        leave.ApprovedBy = request.ApproverId ?? "HR Manager";
        leave.ApprovedAt = DateTime.UtcNow;
        leave.RejectionReason = request.RejectionReason;

        await _repo.UpdateLeaveRequestAsync(leave, ct);
        await _repo.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    public async Task<Result<EmployeeLeaveBalanceDto>> Handle(CalculateEarnedLeaveEncashmentCommand request, CancellationToken ct)
    {
        var employee = await _repo.GetEmployeeByIdAsync(request.EmployeeId, ct);
        if (employee == null)
            return Result<EmployeeLeaveBalanceDto>.Failure("Employee not found.");

        var balance = await _repo.GetLeaveBalanceAsync(request.EmployeeId, request.CalendarYear, ct);
        if (balance == null)
        {
            balance = new EmployeeLeaveBalance
            {
                EmployeeId = employee.Id,
                CalendarYear = request.CalendarYear,
                CasualLeaveTotal = 10,
                SickLeaveTotal = 14,
                EarnedLeaveTotal = 16
            };
        }

        try
        {
            balance.CalculateEarnedLeaveEncashment(employee.BasicSalary, request.DaysToEncash);
            await _repo.AddOrUpdateLeaveBalanceAsync(balance, ct);
            await _repo.SaveChangesAsync(ct);

            return Result<EmployeeLeaveBalanceDto>.Success(new EmployeeLeaveBalanceDto(
                balance.Id,
                employee.Id,
                employee.EmployeeCode,
                employee.FullName,
                balance.CalendarYear,
                balance.CasualLeaveTotal,
                balance.CasualLeaveUsed,
                balance.CasualLeaveRemaining,
                balance.SickLeaveTotal,
                balance.SickLeaveUsed,
                balance.SickLeaveRemaining,
                balance.EarnedLeaveTotal,
                balance.EarnedLeaveUsed,
                balance.EarnedLeaveRemaining,
                balance.EarnedLeaveEncashedDays,
                balance.TotalEncashedAmount
            ));
        }
        catch (Exception ex)
        {
            return Result<EmployeeLeaveBalanceDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<MaternityBenefitDto>> Handle(CalculateMaternityBenefitCommand request, CancellationToken ct)
    {
        var employee = await _repo.GetEmployeeByIdAsync(request.EmployeeId, ct);
        if (employee == null)
            return Result<MaternityBenefitDto>.Failure("Employee not found.");

        var benefit = new MaternityBenefit
        {
            EmployeeId = employee.Id,
            CaseReferenceNumber = request.CaseReferenceNumber,
            NoticeDate = DateTime.UtcNow,
            ExpectedDeliveryDate = request.ExpectedDeliveryDate,
            LeaveStartDate = request.LeaveStartDate,
            LeaveEndDate = request.LeaveStartDate.AddDays(111), // 112 days total
            TotalLeaveDays = 112,
            PriorThreeMonthsTotalWages = request.PriorThreeMonthsTotalWages,
            PriorThreeMonthsWorkingDays = 78,
            MedicalDoctorCertificateNumber = request.MedicalDoctorCertificateNumber,
            PaymentDisbursementMethod = request.DisbursementMethod,
            IsFirstInstallmentPaid = false,
            IsSecondInstallmentPaid = false
        };

        await _repo.AddMaternityBenefitAsync(benefit, ct);
        await _repo.SaveChangesAsync(ct);

        return Result<MaternityBenefitDto>.Success(new MaternityBenefitDto(
            benefit.Id,
            employee.Id,
            employee.EmployeeCode,
            employee.FullName,
            benefit.CaseReferenceNumber,
            benefit.NoticeDate,
            benefit.ExpectedDeliveryDate,
            benefit.LeaveStartDate,
            benefit.LeaveEndDate,
            benefit.TotalLeaveDays,
            benefit.PriorThreeMonthsTotalWages,
            benefit.PriorThreeMonthsWorkingDays,
            benefit.AverageDailyWage,
            benefit.FirstInstallmentAmount,
            benefit.IsFirstInstallmentPaid,
            benefit.FirstInstallmentPaidDate,
            benefit.SecondInstallmentAmount,
            benefit.IsSecondInstallmentPaid,
            benefit.SecondInstallmentPaidDate,
            benefit.TotalBenefitAmount,
            benefit.MedicalDoctorCertificateNumber,
            benefit.RegisteredClinicName,
            benefit.PaymentDisbursementMethod
        ));
    }

    public async Task<Result<PayrollSheetDto>> Handle(GenerateDualStreamPayrollCommand request, CancellationToken ct)
    {
        var existingSheet = await _repo.GetPayrollSheetByPeriodAsync(request.PayMonth, request.PayYear, ct);
        if (existingSheet != null)
        {
            return Result<PayrollSheetDto>.Success(MapPayrollSheet(existingSheet));
        }

        var employees = await _repo.GetEmployeesAsync(null, ct);
        if (employees.Count == 0)
            return Result<PayrollSheetDto>.Failure("No employees found to generate payroll.");

        var sheet = new PayrollSheet
        {
            SheetNumber = $"PAY-{request.PayYear}-{request.PayMonth:D2}",
            PayMonth = request.PayMonth,
            PayYear = request.PayYear,
            Status = PayrollStatus.Draft
        };

        var records = new List<PayrollRecord>();

        foreach (var emp in employees.Where(e => e.Status == EmployeeStatus.Active))
        {
            // Overtime hourly rate: Statutory standard is (Basic / 208) * 2
            decimal otHourlyRate = emp.BasicSalary > 0
                ? Math.Round((emp.BasicSalary / 208m) * 2m, 2)
                : 150m;

            var rec = new PayrollRecord
            {
                PayrollSheetId = sheet.Id,
                EmployeeId = emp.Id,
                Employee = emp,
                PayMonth = request.PayMonth,
                PayYear = request.PayYear,
                PayStream = emp.PayStream,
                BasicSalary = emp.BasicSalary,
                Allowances = emp.HouseRentAllowance + emp.MedicalAllowance + emp.ConveyanceAllowance,
                OvertimeRatePerHour = otHourlyRate,
                DisbursementChannel = emp.DisbursementChannel,
                MfsOrAccountNumber = emp.MobileBankingNumber,
                Status = PayrollStatus.Draft
            };

            if (emp.PayStream == PayStreamType.FloorPieceRate)
            {
                // Realistic production piece-rate: e.g. 2,400 garments @ $0.0700/pc = $168.00
                rec.CompletedGarmentUnits = 2400;
                rec.PieceRatePerUnit = emp.PieceRatePerUnit > 0 ? emp.PieceRatePerUnit : 0.0700m;
                rec.OvertimeHours = 26.0m;
                rec.AttendanceBonus = 10.0m; // Full attendance incentive
                rec.AbsentDeduction = 0m;
                rec.LatePenaltyDeduction = 0m;
                rec.TaxDeduction = 0m;
                rec.AdvanceDeduction = 0m;
            }
            else
            {
                // Management & Staff Monthly Salary
                rec.CompletedGarmentUnits = 0;
                rec.PieceRatePerUnit = 0m;
                rec.OvertimeHours = 12.0m;
                rec.AttendanceBonus = 15.0m;
                rec.AbsentDeduction = 0m;
                rec.LatePenaltyDeduction = 0m;
                rec.TaxDeduction = Math.Round(emp.BasicSalary * 0.05m, 2);
                rec.AdvanceDeduction = 0m;
            }

            records.Add(rec);
        }

        sheet.Records = records;
        sheet.TotalHeadcount = records.Count;
        sheet.TotalPieceRateWorkers = records.Count(r => r.PayStream == PayStreamType.FloorPieceRate);
        sheet.TotalStaffWorkers = records.Count(r => r.PayStream == PayStreamType.MonthlyStaff);
        sheet.TotalBaseOrPieceRateEarnings = records.Sum(r => r.PayStream == PayStreamType.FloorPieceRate ? r.PieceRateEarnings : r.BasicSalary);
        sheet.TotalOvertimeAmount = records.Sum(r => r.OvertimeAmount);
        sheet.TotalAttendanceBonus = records.Sum(r => r.AttendanceBonus);
        sheet.TotalGrossEarnings = records.Sum(r => r.GrossEarnings);
        sheet.TotalDeductions = records.Sum(r => r.TotalDeductions);
        sheet.TotalNetDisbursement = records.Sum(r => r.NetPayable);

        await _repo.AddPayrollSheetAsync(sheet, ct);
        await _repo.SaveChangesAsync(ct);

        return Result<PayrollSheetDto>.Success(MapPayrollSheet(sheet));
    }

    public async Task<Result<bool>> Handle(DisbursePayrollSheetCommand request, CancellationToken ct)
    {
        var sheet = await _repo.GetPayrollSheetByIdAsync(request.PayrollSheetId, ct);
        if (sheet == null)
            return Result<bool>.Failure("Payroll sheet not found.");

        sheet.Status = PayrollStatus.Disbursed;
        sheet.ApprovedBy = request.ApprovedBy;
        sheet.ApprovedAt = DateTime.UtcNow;
        sheet.DisbursedAt = DateTime.UtcNow;

        foreach (var rec in sheet.Records)
        {
            rec.Status = PayrollStatus.Disbursed;
            rec.DisbursedDate = DateTime.UtcNow;
            rec.PaymentReference = $"{rec.DisbursementChannel.ToString().ToUpper()}-PAY-{sheet.PayYear}{sheet.PayMonth:D2}-{rec.Id.ToString()[..6]}";
        }

        await _repo.UpdatePayrollSheetAsync(sheet, ct);
        await _repo.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    public async Task<Result<ClinicVisitDto>> Handle(LogClinicVisitCommand request, CancellationToken ct)
    {
        var employee = await _repo.GetEmployeeByIdAsync(request.EmployeeId, ct);
        if (employee == null)
            return Result<ClinicVisitDto>.Failure("Employee not found.");

        var visit = new ClinicVisit
        {
            EmployeeId = employee.Id,
            VisitDate = request.VisitDate,
            ChiefComplaint = request.ChiefComplaint,
            Diagnosis = request.Diagnosis,
            TreatmentGiven = request.TreatmentGiven,
            MedicineDispensed = request.MedicineDispensed,
            FitnessStatus = request.FitnessStatus,
            AttendingNurseOrDoctor = request.AttendingNurseOrDoctor,
            Remarks = request.Remarks
        };

        await _repo.AddClinicVisitAsync(visit, ct);
        await _repo.SaveChangesAsync(ct);

        return Result<ClinicVisitDto>.Success(new ClinicVisitDto(
            visit.Id,
            employee.Id,
            employee.EmployeeCode,
            employee.FullName,
            employee.Department,
            visit.VisitDate,
            visit.ChiefComplaint,
            visit.Diagnosis,
            visit.TreatmentGiven,
            visit.MedicineDispensed,
            visit.FitnessStatus.ToString(),
            visit.AttendingNurseOrDoctor,
            visit.Remarks
        ));
    }

    public async Task<Result<WorkerGrievanceDto>> Handle(SubmitWorkerGrievanceCommand request, CancellationToken ct)
    {
        string trackingNumber = $"GRV-{DateTime.UtcNow.Year}-{new Random().Next(1000, 9999)}";
        string? employeeName = null;

        if (!request.IsAnonymous && request.EmployeeId.HasValue)
        {
            var emp = await _repo.GetEmployeeByIdAsync(request.EmployeeId.Value, ct);
            employeeName = emp?.FullName;
        }

        var grievance = new WorkerGrievance
        {
            TrackingNumber = trackingNumber,
            SubmissionDate = DateTime.UtcNow,
            IsAnonymous = request.IsAnonymous,
            EmployeeId = request.IsAnonymous ? null : request.EmployeeId,
            Category = request.Category,
            Title = request.Title,
            Description = request.Description,
            Status = GrievanceStatus.Submitted
        };

        await _repo.AddWorkerGrievanceAsync(grievance, ct);
        await _repo.SaveChangesAsync(ct);

        return Result<WorkerGrievanceDto>.Success(new WorkerGrievanceDto(
            grievance.Id,
            grievance.TrackingNumber,
            grievance.SubmissionDate,
            grievance.IsAnonymous,
            grievance.EmployeeId,
            employeeName,
            grievance.Category.ToString(),
            grievance.Title,
            grievance.Description,
            grievance.Status.ToString(),
            grievance.InvestigatedBy,
            grievance.WelfareCommitteeFindings,
            grievance.ResolutionSummary,
            grievance.ResolvedDate
        ));
    }

    public async Task<Result<WorkerGrievanceDto>> Handle(ResolveWorkerGrievanceCommand request, CancellationToken ct)
    {
        var grievance = await _repo.GetWorkerGrievanceByIdAsync(request.GrievanceId, ct);
        if (grievance == null)
            return Result<WorkerGrievanceDto>.Failure("Worker grievance not found.");

        grievance.InvestigatedBy = request.InvestigatedBy;
        grievance.WelfareCommitteeFindings = request.WelfareCommitteeFindings;
        grievance.ResolutionSummary = request.ResolutionSummary;
        grievance.Status = request.IsResolved ? GrievanceStatus.Resolved : GrievanceStatus.Dismissed;
        grievance.ResolvedDate = DateTime.UtcNow;

        await _repo.UpdateWorkerGrievanceAsync(grievance, ct);
        await _repo.SaveChangesAsync(ct);

        return Result<WorkerGrievanceDto>.Success(new WorkerGrievanceDto(
            grievance.Id,
            grievance.TrackingNumber,
            grievance.SubmissionDate,
            grievance.IsAnonymous,
            grievance.EmployeeId,
            grievance.Employee?.FullName,
            grievance.Category.ToString(),
            grievance.Title,
            grievance.Description,
            grievance.Status.ToString(),
            grievance.InvestigatedBy,
            grievance.WelfareCommitteeFindings,
            grievance.ResolutionSummary,
            grievance.ResolvedDate
        ));
    }

    public async Task<Result<DisciplinaryActionDto>> Handle(RecordDisciplinaryActionCommand request, CancellationToken ct)
    {
        var employee = await _repo.GetEmployeeByIdAsync(request.EmployeeId, ct);
        if (employee == null)
            return Result<DisciplinaryActionDto>.Failure("Employee not found.");

        var action = new DisciplinaryAction
        {
            EmployeeId = employee.Id,
            NoticeNumber = request.NoticeNumber,
            IncidentDate = request.IncidentDate,
            ViolationType = request.ViolationType,
            AllegationDescription = request.AllegationDescription,
            HearingDate = request.HearingDate,
            Outcome = request.Outcome,
            AuthorizedBy = request.AuthorizedBy,
            IssuedDate = DateTime.UtcNow
        };

        await _repo.AddDisciplinaryActionAsync(action, ct);
        await _repo.SaveChangesAsync(ct);

        return Result<DisciplinaryActionDto>.Success(new DisciplinaryActionDto(
            action.Id,
            employee.Id,
            employee.EmployeeCode,
            employee.FullName,
            employee.Department,
            action.NoticeNumber,
            action.IncidentDate,
            action.ViolationType.ToString(),
            action.AllegationDescription,
            action.HearingDate,
            action.Outcome.ToString(),
            action.AuthorizedBy,
            action.IssuedDate
        ));
    }

    private static EmployeeDto MapEmployee(Employee e) => new(
        e.Id,
        e.EmployeeCode,
        e.FullName,
        e.FullNameBangla,
        e.Email,
        e.PhoneNumber,
        e.Department,
        e.Designation,
        e.AssignedLine,
        e.DateOfBirth,
        e.CalculatedAgeYears,
        e.IsAgeVerified,
        e.AgeVerificationStatus.ToString(),
        e.PayStream.ToString(),
        e.PieceRatePerUnit,
        e.BasicSalary,
        e.HouseRentAllowance,
        e.MedicalAllowance,
        e.ConveyanceAllowance,
        e.GrossSalary,
        e.JoiningDate,
        e.Status.ToString(),
        e.NationalIdOrBirthCert,
        e.DisbursementChannel.ToString(),
        e.MobileBankingNumber,
        e.BiometricBadgeNumber,
        e.EmergencyContact
    );

    private static PayrollSheetDto MapPayrollSheet(PayrollSheet s) => new(
        s.Id,
        s.SheetNumber,
        s.PayMonth,
        s.PayYear,
        s.TotalHeadcount,
        s.TotalPieceRateWorkers,
        s.TotalStaffWorkers,
        s.TotalBaseOrPieceRateEarnings,
        s.TotalOvertimeAmount,
        s.TotalAttendanceBonus,
        s.TotalGrossEarnings,
        s.TotalDeductions,
        s.TotalNetDisbursement,
        s.Status.ToString(),
        s.ApprovedBy,
        s.ApprovedAt,
        s.DisbursedAt,
        s.Records.Select(r => new PayrollRecordDto(
            r.Id,
            r.EmployeeId,
            r.Employee?.EmployeeCode ?? string.Empty,
            r.Employee?.FullName ?? string.Empty,
            r.Employee?.FullNameBangla ?? string.Empty,
            r.Employee?.Department ?? string.Empty,
            r.Employee?.Designation ?? string.Empty,
            r.PayMonth,
            r.PayYear,
            r.PayStream.ToString(),
            r.BasicSalary,
            r.Allowances,
            r.CompletedGarmentUnits,
            r.PieceRatePerUnit,
            r.PieceRateEarnings,
            r.OvertimeHours,
            r.OvertimeRatePerHour,
            r.OvertimeAmount,
            r.AttendanceBonus,
            r.AbsentDeduction,
            r.LatePenaltyDeduction,
            r.TaxDeduction,
            r.AdvanceDeduction,
            r.TotalDeductions,
            r.GrossEarnings,
            r.NetPayable,
            r.DisbursementChannel.ToString(),
            r.MfsOrAccountNumber,
            r.Status.ToString(),
            r.DisbursedDate,
            r.PaymentReference
        )).ToList()
    );
}

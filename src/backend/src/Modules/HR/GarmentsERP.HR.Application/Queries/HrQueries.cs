using MediatR;
using GarmentsERP.HR.Application.DTOs;
using GarmentsERP.HR.Application.Interfaces;
using GarmentsERP.HR.Domain.Entities;
using GarmentsERP.HR.Domain.Enums;
using GarmentsERP.Shared.Common;

namespace GarmentsERP.HR.Application.Queries;

public record GetEmployeesQuery(string? Department = null) : IRequest<Result<IReadOnlyList<EmployeeDto>>>;

public record GetEmployeeByIdQuery(Guid Id) : IRequest<Result<EmployeeDto>>;

public record GetAttendanceLogsQuery(DateTime? Date = null, Guid? EmployeeId = null) : IRequest<Result<IReadOnlyList<AttendanceRecordDto>>>;

public record GetBiometricDevicesQuery : IRequest<Result<IReadOnlyList<BiometricDeviceDto>>>;

public record GetBiometricPunchLogsQuery(DateTime? Date = null, Guid? DeviceId = null) : IRequest<Result<IReadOnlyList<BiometricPunchLogDto>>>;

public record GetWorkShiftsQuery : IRequest<Result<IReadOnlyList<WorkShiftDto>>>;

public record GetShiftRostersQuery(Guid? EmployeeId = null, DateTime? Date = null) : IRequest<Result<IReadOnlyList<ShiftRosterDto>>>;

public record GetLeaveRequestsQuery(LeaveStatus? Status = null) : IRequest<Result<IReadOnlyList<LeaveRequestDto>>>;

public record GetLeaveBalanceQuery(Guid EmployeeId, int CalendarYear) : IRequest<Result<EmployeeLeaveBalanceDto>>;

public record GetMaternityBenefitsQuery : IRequest<Result<IReadOnlyList<MaternityBenefitDto>>>;

public record GetPayrollRecordsQuery(int? Month = null, int? Year = null) : IRequest<Result<IReadOnlyList<PayrollRecordDto>>>;

public record GetPayrollSheetsQuery : IRequest<Result<IReadOnlyList<PayrollSheetDto>>>;

public record GetPayrollSheetByIdQuery(Guid Id) : IRequest<Result<PayrollSheetDto>>;

public record ExportMfsPayoutBatchQuery(
    Guid PayrollSheetId,
    PaymentDisbursementChannel Channel
) : IRequest<Result<IReadOnlyList<MfsPayoutExportRowDto>>>;

public record GetClinicVisitsQuery(DateTime? FromDate = null, Guid? EmployeeId = null) : IRequest<Result<IReadOnlyList<ClinicVisitDto>>>;

public record GetWorkerGrievancesQuery(GrievanceStatus? Status = null) : IRequest<Result<IReadOnlyList<WorkerGrievanceDto>>>;

public record GetDisciplinaryActionsQuery(Guid? EmployeeId = null) : IRequest<Result<IReadOnlyList<DisciplinaryActionDto>>>;

public class HrQueryHandlers :
    IRequestHandler<GetEmployeesQuery, Result<IReadOnlyList<EmployeeDto>>>,
    IRequestHandler<GetEmployeeByIdQuery, Result<EmployeeDto>>,
    IRequestHandler<GetAttendanceLogsQuery, Result<IReadOnlyList<AttendanceRecordDto>>>,
    IRequestHandler<GetBiometricDevicesQuery, Result<IReadOnlyList<BiometricDeviceDto>>>,
    IRequestHandler<GetBiometricPunchLogsQuery, Result<IReadOnlyList<BiometricPunchLogDto>>>,
    IRequestHandler<GetWorkShiftsQuery, Result<IReadOnlyList<WorkShiftDto>>>,
    IRequestHandler<GetShiftRostersQuery, Result<IReadOnlyList<ShiftRosterDto>>>,
    IRequestHandler<GetLeaveRequestsQuery, Result<IReadOnlyList<LeaveRequestDto>>>,
    IRequestHandler<GetLeaveBalanceQuery, Result<EmployeeLeaveBalanceDto>>,
    IRequestHandler<GetMaternityBenefitsQuery, Result<IReadOnlyList<MaternityBenefitDto>>>,
    IRequestHandler<GetPayrollRecordsQuery, Result<IReadOnlyList<PayrollRecordDto>>>,
    IRequestHandler<GetPayrollSheetsQuery, Result<IReadOnlyList<PayrollSheetDto>>>,
    IRequestHandler<GetPayrollSheetByIdQuery, Result<PayrollSheetDto>>,
    IRequestHandler<ExportMfsPayoutBatchQuery, Result<IReadOnlyList<MfsPayoutExportRowDto>>>,
    IRequestHandler<GetClinicVisitsQuery, Result<IReadOnlyList<ClinicVisitDto>>>,
    IRequestHandler<GetWorkerGrievancesQuery, Result<IReadOnlyList<WorkerGrievanceDto>>>,
    IRequestHandler<GetDisciplinaryActionsQuery, Result<IReadOnlyList<DisciplinaryActionDto>>>
{
    private readonly IHrRepository _repo;

    public HrQueryHandlers(IHrRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result<IReadOnlyList<EmployeeDto>>> Handle(GetEmployeesQuery request, CancellationToken ct)
    {
        var emps = await _repo.GetEmployeesAsync(request.Department, ct);
        var dtos = emps.Select(MapEmployee).ToList();
        return Result<IReadOnlyList<EmployeeDto>>.Success(dtos);
    }

    public async Task<Result<EmployeeDto>> Handle(GetEmployeeByIdQuery request, CancellationToken ct)
    {
        var emp = await _repo.GetEmployeeByIdAsync(request.Id, ct);
        if (emp == null)
            return Result<EmployeeDto>.Failure("Employee not found.");
        return Result<EmployeeDto>.Success(MapEmployee(emp));
    }

    public async Task<Result<IReadOnlyList<AttendanceRecordDto>>> Handle(GetAttendanceLogsQuery request, CancellationToken ct)
    {
        var records = await _repo.GetAttendanceRecordsAsync(request.Date, request.EmployeeId, ct);
        var dtos = records.Select(r => new AttendanceRecordDto(
            r.Id,
            r.EmployeeId,
            r.Employee?.EmployeeCode ?? string.Empty,
            r.Employee?.FullName ?? string.Empty,
            r.Employee?.Department ?? string.Empty,
            r.Employee?.AssignedLine,
            r.Date,
            r.CheckInTime?.ToString(@"hh\:mm"),
            r.CheckOutTime?.ToString(@"hh\:mm"),
            r.OvertimeHours,
            r.Status.ToString(),
            r.ShiftName,
            r.Remarks
        )).ToList();
        return Result<IReadOnlyList<AttendanceRecordDto>>.Success(dtos);
    }

    public async Task<Result<IReadOnlyList<BiometricDeviceDto>>> Handle(GetBiometricDevicesQuery request, CancellationToken ct)
    {
        var devices = await _repo.GetBiometricDevicesAsync(ct);
        var dtos = devices.Select(d => new BiometricDeviceDto(
            d.Id,
            d.DeviceCode,
            d.DeviceName,
            d.IpAddress,
            d.PortNumber,
            d.ProtocolType.ToString(),
            d.PhysicalLocation,
            d.IsOnline,
            d.LastSyncTime,
            d.TotalPunchesRecorded
        )).ToList();
        return Result<IReadOnlyList<BiometricDeviceDto>>.Success(dtos);
    }

    public async Task<Result<IReadOnlyList<BiometricPunchLogDto>>> Handle(GetBiometricPunchLogsQuery request, CancellationToken ct)
    {
        var logs = await _repo.GetBiometricPunchLogsAsync(request.Date, request.DeviceId, ct);
        var dtos = logs.Select(l => new BiometricPunchLogDto(
            l.Id,
            l.BiometricDeviceId,
            l.BiometricDevice?.DeviceCode ?? string.Empty,
            l.BiometricDevice?.DeviceName ?? string.Empty,
            l.EmployeeId,
            l.Employee?.FullName,
            l.BiometricBadgeNumber,
            l.PunchTimestamp,
            l.PunchMode.ToString(),
            l.VerificationType
        )).ToList();
        return Result<IReadOnlyList<BiometricPunchLogDto>>.Success(dtos);
    }

    public async Task<Result<IReadOnlyList<WorkShiftDto>>> Handle(GetWorkShiftsQuery request, CancellationToken ct)
    {
        var shifts = await _repo.GetWorkShiftsAsync(ct);
        var dtos = shifts.Select(s => new WorkShiftDto(
            s.Id,
            s.ShiftCode,
            s.ShiftName,
            s.StartTime.ToString(@"hh\:mm"),
            s.EndTime.ToString(@"hh\:mm"),
            s.GracePeriodMinutes,
            s.HalfDayThresholdMinutes,
            s.IsNightShift,
            s.ShiftAllowanceDaily,
            s.IsActive
        )).ToList();
        return Result<IReadOnlyList<WorkShiftDto>>.Success(dtos);
    }

    public async Task<Result<IReadOnlyList<ShiftRosterDto>>> Handle(GetShiftRostersQuery request, CancellationToken ct)
    {
        var rosters = await _repo.GetShiftRostersAsync(request.EmployeeId, request.Date, ct);
        var dtos = rosters.Select(r => new ShiftRosterDto(
            r.Id,
            r.EmployeeId,
            r.Employee?.EmployeeCode ?? string.Empty,
            r.Employee?.FullName ?? string.Empty,
            r.WorkShiftId,
            r.WorkShift?.ShiftCode ?? string.Empty,
            r.WorkShift?.ShiftName ?? string.Empty,
            r.EffectiveFrom,
            r.EffectiveTo,
            r.AssignedLineOrSection
        )).ToList();
        return Result<IReadOnlyList<ShiftRosterDto>>.Success(dtos);
    }

    public async Task<Result<IReadOnlyList<LeaveRequestDto>>> Handle(GetLeaveRequestsQuery request, CancellationToken ct)
    {
        var leaves = await _repo.GetLeaveRequestsAsync(request.Status, ct);
        var dtos = leaves.Select(l => new LeaveRequestDto(
            l.Id,
            l.EmployeeId,
            l.Employee?.EmployeeCode ?? string.Empty,
            l.Employee?.FullName ?? string.Empty,
            l.Employee?.Department ?? string.Empty,
            l.Type.ToString(),
            l.StartDate,
            l.EndDate,
            l.TotalDays,
            l.Reason,
            l.Status.ToString(),
            l.ApprovedBy,
            l.ApprovedAt
        )).ToList();
        return Result<IReadOnlyList<LeaveRequestDto>>.Success(dtos);
    }

    public async Task<Result<EmployeeLeaveBalanceDto>> Handle(GetLeaveBalanceQuery request, CancellationToken ct)
    {
        var balance = await _repo.GetLeaveBalanceAsync(request.EmployeeId, request.CalendarYear, ct);
        if (balance == null)
        {
            var emp = await _repo.GetEmployeeByIdAsync(request.EmployeeId, ct);
            if (emp == null) return Result<EmployeeLeaveBalanceDto>.Failure("Employee not found.");

            return Result<EmployeeLeaveBalanceDto>.Success(new EmployeeLeaveBalanceDto(
                Guid.Empty,
                emp.Id,
                emp.EmployeeCode,
                emp.FullName,
                request.CalendarYear,
                10, 0, 10,
                14, 0, 14,
                16, 0, 16,
                0, 0m
            ));
        }

        return Result<EmployeeLeaveBalanceDto>.Success(new EmployeeLeaveBalanceDto(
            balance.Id,
            balance.EmployeeId,
            balance.Employee?.EmployeeCode ?? string.Empty,
            balance.Employee?.FullName ?? string.Empty,
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

    public async Task<Result<IReadOnlyList<MaternityBenefitDto>>> Handle(GetMaternityBenefitsQuery request, CancellationToken ct)
    {
        var benefits = await _repo.GetMaternityBenefitsAsync(ct);
        var dtos = benefits.Select(b => new MaternityBenefitDto(
            b.Id,
            b.EmployeeId,
            b.Employee?.EmployeeCode ?? string.Empty,
            b.Employee?.FullName ?? string.Empty,
            b.CaseReferenceNumber,
            b.NoticeDate,
            b.ExpectedDeliveryDate,
            b.LeaveStartDate,
            b.LeaveEndDate,
            b.TotalLeaveDays,
            b.PriorThreeMonthsTotalWages,
            b.PriorThreeMonthsWorkingDays,
            b.AverageDailyWage,
            b.FirstInstallmentAmount,
            b.IsFirstInstallmentPaid,
            b.FirstInstallmentPaidDate,
            b.SecondInstallmentAmount,
            b.IsSecondInstallmentPaid,
            b.SecondInstallmentPaidDate,
            b.TotalBenefitAmount,
            b.MedicalDoctorCertificateNumber,
            b.RegisteredClinicName,
            b.PaymentDisbursementMethod
        )).ToList();
        return Result<IReadOnlyList<MaternityBenefitDto>>.Success(dtos);
    }

    public async Task<Result<IReadOnlyList<PayrollRecordDto>>> Handle(GetPayrollRecordsQuery request, CancellationToken ct)
    {
        var records = await _repo.GetPayrollRecordsAsync(request.Month, request.Year, ct);
        var dtos = records.Select(MapPayrollRecord).ToList();
        return Result<IReadOnlyList<PayrollRecordDto>>.Success(dtos);
    }

    public async Task<Result<IReadOnlyList<PayrollSheetDto>>> Handle(GetPayrollSheetsQuery request, CancellationToken ct)
    {
        var sheets = await _repo.GetPayrollSheetsAsync(ct);
        var dtos = sheets.Select(MapPayrollSheet).ToList();
        return Result<IReadOnlyList<PayrollSheetDto>>.Success(dtos);
    }

    public async Task<Result<PayrollSheetDto>> Handle(GetPayrollSheetByIdQuery request, CancellationToken ct)
    {
        var sheet = await _repo.GetPayrollSheetByIdAsync(request.Id, ct);
        if (sheet == null)
            return Result<PayrollSheetDto>.Failure("Payroll sheet not found.");
        return Result<PayrollSheetDto>.Success(MapPayrollSheet(sheet));
    }

    public async Task<Result<IReadOnlyList<MfsPayoutExportRowDto>>> Handle(ExportMfsPayoutBatchQuery request, CancellationToken ct)
    {
        var sheet = await _repo.GetPayrollSheetByIdAsync(request.PayrollSheetId, ct);
        if (sheet == null)
            return Result<IReadOnlyList<MfsPayoutExportRowDto>>.Failure("Payroll sheet not found.");

        var eligible = sheet.Records
            .Where(r => r.DisbursementChannel == request.Channel && !string.IsNullOrEmpty(r.MfsOrAccountNumber))
            .Select(r => new MfsPayoutExportRowDto(
                r.Employee?.EmployeeCode ?? string.Empty,
                r.Employee?.FullName ?? string.Empty,
                r.MfsOrAccountNumber!,
                r.NetPayable,
                "BDT",
                $"SAL-{sheet.PayYear}{sheet.PayMonth:D2}"
            ))
            .ToList();

        return Result<IReadOnlyList<MfsPayoutExportRowDto>>.Success(eligible);
    }

    public async Task<Result<IReadOnlyList<ClinicVisitDto>>> Handle(GetClinicVisitsQuery request, CancellationToken ct)
    {
        var visits = await _repo.GetClinicVisitsAsync(request.FromDate, request.EmployeeId, ct);
        var dtos = visits.Select(v => new ClinicVisitDto(
            v.Id,
            v.EmployeeId,
            v.Employee?.EmployeeCode ?? string.Empty,
            v.Employee?.FullName ?? string.Empty,
            v.Employee?.Department ?? string.Empty,
            v.VisitDate,
            v.ChiefComplaint,
            v.Diagnosis,
            v.TreatmentGiven,
            v.MedicineDispensed,
            v.FitnessStatus.ToString(),
            v.AttendingNurseOrDoctor,
            v.Remarks
        )).ToList();
        return Result<IReadOnlyList<ClinicVisitDto>>.Success(dtos);
    }

    public async Task<Result<IReadOnlyList<WorkerGrievanceDto>>> Handle(GetWorkerGrievancesQuery request, CancellationToken ct)
    {
        var grievances = await _repo.GetWorkerGrievancesAsync(request.Status, ct);
        var dtos = grievances.Select(g => new WorkerGrievanceDto(
            g.Id,
            g.TrackingNumber,
            g.SubmissionDate,
            g.IsAnonymous,
            g.EmployeeId,
            g.Employee?.FullName,
            g.Category.ToString(),
            g.Title,
            g.Description,
            g.Status.ToString(),
            g.InvestigatedBy,
            g.WelfareCommitteeFindings,
            g.ResolutionSummary,
            g.ResolvedDate
        )).ToList();
        return Result<IReadOnlyList<WorkerGrievanceDto>>.Success(dtos);
    }

    public async Task<Result<IReadOnlyList<DisciplinaryActionDto>>> Handle(GetDisciplinaryActionsQuery request, CancellationToken ct)
    {
        var actions = await _repo.GetDisciplinaryActionsAsync(request.EmployeeId, ct);
        var dtos = actions.Select(a => new DisciplinaryActionDto(
            a.Id,
            a.EmployeeId,
            a.Employee?.EmployeeCode ?? string.Empty,
            a.Employee?.FullName ?? string.Empty,
            a.Employee?.Department ?? string.Empty,
            a.NoticeNumber,
            a.IncidentDate,
            a.ViolationType.ToString(),
            a.AllegationDescription,
            a.HearingDate,
            a.Outcome.ToString(),
            a.AuthorizedBy,
            a.IssuedDate
        )).ToList();
        return Result<IReadOnlyList<DisciplinaryActionDto>>.Success(dtos);
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

    private static PayrollRecordDto MapPayrollRecord(PayrollRecord r) => new(
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
        s.Records.Select(MapPayrollRecord).ToList()
    );
}

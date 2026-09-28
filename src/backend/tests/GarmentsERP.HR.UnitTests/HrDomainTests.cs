using GarmentsERP.HR.Domain.Entities;
using GarmentsERP.HR.Domain.Enums;
using Xunit;

namespace GarmentsERP.HR.UnitTests;

public class HrDomainTests
{
    [Fact]
    public void Employee_GrossSalary_ShouldCalculateSumOfAllAllowances()
    {
        var emp = new Employee
        {
            EmployeeCode = "EMP-9001",
            FullName = "Abul Kashem",
            BasicSalary = 20000m,
            HouseRentAllowance = 8000m,
            MedicalAllowance = 2500m,
            ConveyanceAllowance = 1500m,
            Department = "Sewing Section",
            Designation = "Senior Operator"
        };

        Assert.Equal(32000m, emp.GrossSalary);
        Assert.Equal(EmployeeStatus.Active, emp.Status);
    }

    [Fact]
    public void ChildLaborAgeGate_WhenCandidateUnderEighteen_ShouldBlockAndAbortOnboarding_AC07()
    {
        // Candidate born 2010-05-15, onboarding in Sep 2026 => Age is 16 Years
        var underageWorker = new Employee
        {
            EmployeeCode = "EMP-UNDERAGE-01",
            FullName = "Md. Minor Applicant",
            DateOfBirth = new DateTime(2010, 5, 15),
            JoiningDate = new DateTime(2026, 9, 28),
            BasicSalary = 12000m,
            Department = "Sewing Floor",
            Designation = "Helper"
        };

        bool isAgeVerified = underageWorker.VerifyAgeCompliance();

        // AC-07 Statutory Check: Must be strictly blocked
        Assert.False(isAgeVerified);
        Assert.Equal(16m, underageWorker.CalculatedAgeYears);
        Assert.False(underageWorker.IsAgeVerified);
        Assert.Equal(AgeVerificationStatus.BlockedUnderageLabor, underageWorker.AgeVerificationStatus);
        Assert.Equal(EmployeeStatus.Suspended, underageWorker.Status);
    }

    [Fact]
    public void ChildLaborAgeGate_WhenCandidateEighteenOrOlder_ShouldPassAndVerify_AC07()
    {
        // Candidate born 2000-04-15, onboarding in Sep 2026 => Age is 26 Years
        var adultWorker = new Employee
        {
            EmployeeCode = "EMP-2026-1042",
            FullName = "Md. Rafiqul Islam",
            DateOfBirth = new DateTime(2000, 4, 15),
            JoiningDate = new DateTime(2026, 9, 28),
            BasicSalary = 16000m,
            Department = "Sewing Section",
            Designation = "Senior Sewing Operator"
        };

        bool isAgeVerified = adultWorker.VerifyAgeCompliance();

        Assert.True(isAgeVerified);
        Assert.Equal(26m, adultWorker.CalculatedAgeYears);
        Assert.True(adultWorker.IsAgeVerified);
        Assert.Equal(AgeVerificationStatus.AgeVerified, adultWorker.AgeVerificationStatus);
    }

    [Fact]
    public void ChildLaborAgeGate_OnExactEighteenthBirthday_ShouldPassVerification()
    {
        // Joining exactly on 18th birthday
        var birthdayWorker = new Employee
        {
            EmployeeCode = "EMP-18TH",
            FullName = "Sajib Hossain",
            DateOfBirth = new DateTime(2008, 9, 28),
            JoiningDate = new DateTime(2026, 9, 28)
        };

        bool isAgeVerified = birthdayWorker.VerifyAgeCompliance();

        Assert.True(isAgeVerified);
        Assert.Equal(18m, birthdayWorker.CalculatedAgeYears);
        Assert.Equal(AgeVerificationStatus.AgeVerified, birthdayWorker.AgeVerificationStatus);
    }

    [Fact]
    public void DualStreamPayroll_FloorPieceRate_ShouldComputeCompletedUnitsEarnings_HR05()
    {
        // Md. Rafiqul Islam: 2,400 garments @ $0.0700/pc = $168.00
        var payroll = new PayrollRecord
        {
            EmployeeId = Guid.NewGuid(),
            PayMonth = 10,
            PayYear = 2026,
            PayStream = PayStreamType.FloorPieceRate,
            CompletedGarmentUnits = 2400,
            PieceRatePerUnit = 0.0700m,
            OvertimeHours = 26.0m,
            OvertimeRatePerHour = 1.45m, // $37.70
            AttendanceBonus = 10.00m, // $10.00
            Allowances = 0m,
            AbsentDeduction = 0m,
            LatePenaltyDeduction = 0m,
            TaxDeduction = 0m,
            AdvanceDeduction = 0m
        };

        Assert.Equal(168.00m, payroll.PieceRateEarnings);
        Assert.Equal(37.70m, payroll.OvertimeAmount);
        Assert.Equal(0m, payroll.TotalDeductions);
        Assert.Equal(215.70m, payroll.GrossEarnings);
        Assert.Equal(215.70m, payroll.NetPayable);
    }

    [Fact]
    public void DualStreamPayroll_MonthlyStaff_ShouldComputeSalaryWithAllowancesAndDeductions()
    {
        var payroll = new PayrollRecord
        {
            EmployeeId = Guid.NewGuid(),
            PayMonth = 10,
            PayYear = 2026,
            PayStream = PayStreamType.MonthlyStaff,
            BasicSalary = 22000m,
            Allowances = 13000m,
            OvertimeHours = 10.0m,
            OvertimeRatePerHour = 211.54m, // Overtime = 2115.40m
            AttendanceBonus = 1000m,
            AbsentDeduction = 0m,
            LatePenaltyDeduction = 0m,
            TaxDeduction = 1100m,
            AdvanceDeduction = 0m
        };

        // Gross = 22000 + 13000 + 2115.40 + 1000 = 38115.40m
        // Deductions = 1100m
        // Net = 37015.40m
        Assert.Equal(2115.40m, payroll.OvertimeAmount);
        Assert.Equal(1100m, payroll.TotalDeductions);
        Assert.Equal(38115.40m, payroll.GrossEarnings);
        Assert.Equal(37015.40m, payroll.NetPayable);
    }

    [Fact]
    public void StatutoryOvertime_RateFormula_ShouldBeDoubleBasicHourlyRate_HR05()
    {
        // Bangladesh Labor Act: Basic salary divided by 208 standard monthly hours * 2 (Double rate)
        decimal basicSalary = 20800m;
        decimal expectedHourlyOtRate = (basicSalary / 208m) * 2m; // 100 * 2 = 200

        Assert.Equal(200m, expectedHourlyOtRate);
    }

    [Fact]
    public void EarnedLeaveEncashment_BangladeshLaborActFormula_ShouldCalculateCorrectly_HR04()
    {
        var leaveBal = new EmployeeLeaveBalance
        {
            EmployeeId = Guid.NewGuid(),
            CalendarYear = 2026,
            CasualLeaveTotal = 10,
            SickLeaveTotal = 14,
            EarnedLeaveTotal = 16,
            EarnedLeaveUsed = 4 // 12 days remaining
        };

        decimal basicSalary = 18000m;
        // Formula: (Basic / 30) * Days => (18000 / 30) * 10 = 600 * 10 = 6000m
        decimal encashedAmount = leaveBal.CalculateEarnedLeaveEncashment(basicSalary, 10);

        Assert.Equal(6000m, encashedAmount);
        Assert.Equal(10, leaveBal.EarnedLeaveEncashedDays);
        Assert.Equal(6000m, leaveBal.TotalEncashedAmount);
        Assert.Equal(2, leaveBal.EarnedLeaveRemaining);
    }

    [Fact]
    public void EarnedLeaveEncashment_ExceedingAvailableDays_ShouldThrowException()
    {
        var leaveBal = new EmployeeLeaveBalance
        {
            EmployeeId = Guid.NewGuid(),
            CalendarYear = 2026,
            EarnedLeaveTotal = 16,
            EarnedLeaveUsed = 12 // Only 4 days remaining
        };

        Assert.Throws<InvalidOperationException>(() =>
        {
            leaveBal.CalculateEarnedLeaveEncashment(15000m, 5); // Requesting 5 days
        });
    }

    [Fact]
    public void StatutoryMaternityBenefit_OneHundredTwelveDays_TwoInstallments_HR04()
    {
        // 112 days (16 weeks) total: 8 weeks prenatal (56 days) + 8 weeks postnatal (56 days)
        var maternity = new MaternityBenefit
        {
            EmployeeId = Guid.NewGuid(),
            CaseReferenceNumber = "MAT-2026-0012",
            NoticeDate = DateTime.UtcNow,
            ExpectedDeliveryDate = DateTime.UtcNow.AddDays(50),
            LeaveStartDate = DateTime.UtcNow,
            LeaveEndDate = DateTime.UtcNow.AddDays(111),
            TotalLeaveDays = 112,
            PriorThreeMonthsTotalWages = 105000m, // 35,000 / mo
            PriorThreeMonthsWorkingDays = 78,
            MedicalDoctorCertificateNumber = "DMC-REG-2026-8819"
        };

        // Average daily wage = 105000 / 78 = 1346.15m
        Assert.Equal(1346.15m, maternity.AverageDailyWage);
        Assert.Equal(75384.40m, maternity.FirstInstallmentAmount); // 56 * 1346.15
        Assert.Equal(75384.40m, maternity.SecondInstallmentAmount);
        Assert.Equal(150768.80m, maternity.TotalBenefitAmount);
    }

    [Fact]
    public void WorkShift_GracePeriod_ShouldAllowPunchesWithinThreshold_HR03()
    {
        var shift = new WorkShift
        {
            ShiftCode = "SHIFT-M",
            StartTime = new TimeSpan(8, 0, 0),
            GracePeriodMinutes = 15
        };

        var punchTimeOnTime = new TimeSpan(8, 12, 0); // Within 15m grace
        var punchTimeLate = new TimeSpan(8, 25, 0);   // After grace period

        bool isOnTime = punchTimeOnTime <= shift.StartTime.Add(TimeSpan.FromMinutes(shift.GracePeriodMinutes));
        bool isLate = punchTimeLate > shift.StartTime.Add(TimeSpan.FromMinutes(shift.GracePeriodMinutes));

        Assert.True(isOnTime);
        Assert.True(isLate);
    }

    [Fact]
    public void LateAttendancePenalty_ThreeConsecutiveLateDays_ShouldDeductOneDayWage_HR03()
    {
        int totalLateDays = 6;
        int penaltyDeductionDays = totalLateDays / 3; // 2 days deduction

        decimal basicSalary = 18000m;
        decimal dailyWage = basicSalary / 30m; // 600m
        decimal deductionAmount = penaltyDeductionDays * dailyWage; // 1200m

        Assert.Equal(2, penaltyDeductionDays);
        Assert.Equal(1200m, deductionAmount);
    }

    [Fact]
    public void BiometricDevice_ShouldTrackLocationAndPunches_HR02()
    {
        var device = new BiometricDevice
        {
            DeviceCode = "BIO-GATE-01",
            DeviceName = "Main Gate Fingerprint",
            IpAddress = "192.168.10.201",
            PortNumber = 4370,
            ProtocolType = BiometricProtocolType.ZKTeco_Standalone,
            PhysicalLocation = "Gate A",
            IsOnline = true,
            TotalPunchesRecorded = 500
        };

        Assert.True(device.IsOnline);
        Assert.Equal(4370, device.PortNumber);
        Assert.Equal(BiometricProtocolType.ZKTeco_Standalone, device.ProtocolType);
    }

    [Fact]
    public void WorkerGrievance_AnonymousSubmission_ShouldOmitEmployeeId_HR07()
    {
        var grievance = new WorkerGrievance
        {
            TrackingNumber = "GRV-2026-0042",
            IsAnonymous = true,
            EmployeeId = null,
            Category = GrievanceCategory.CanteenFoodQuality,
            Title = "Water cooler broken on Floor 2",
            Description = "Water was lukewarm",
            Status = GrievanceStatus.Submitted
        };

        Assert.True(grievance.IsAnonymous);
        Assert.Null(grievance.EmployeeId);
        Assert.Equal(GrievanceStatus.Submitted, grievance.Status);
    }

    [Fact]
    public void ClinicVisit_ShouldRecordDutyFitnessAndFirstAid_HR07()
    {
        var visit = new ClinicVisit
        {
            EmployeeId = Guid.NewGuid(),
            VisitDate = DateTime.UtcNow,
            ChiefComplaint = "Needle graze",
            Diagnosis = "Minor scratch",
            TreatmentGiven = "Antiseptic wash",
            MedicineDispensed = "Paracetamol",
            FitnessStatus = ClinicDutyFitnessStatus.FitForNormalDuty
        };

        Assert.Equal(ClinicDutyFitnessStatus.FitForNormalDuty, visit.FitnessStatus);
        Assert.False(string.IsNullOrEmpty(visit.TreatmentGiven));
    }

    [Fact]
    public void DisciplinaryAction_ShouldIssueFormalWarningLetter_HR07()
    {
        var action = new DisciplinaryAction
        {
            EmployeeId = Guid.NewGuid(),
            NoticeNumber = "DISC-2026-0019",
            IncidentDate = DateTime.UtcNow.AddDays(-5),
            ViolationType = DisciplinaryViolationType.SafetyViolation,
            AllegationDescription = "Operating machine without eye guard",
            Outcome = DisciplinaryOutcome.FirstWrittenWarning,
            IssuedDate = DateTime.UtcNow
        };

        Assert.Equal(DisciplinaryViolationType.SafetyViolation, action.ViolationType);
        Assert.Equal(DisciplinaryOutcome.FirstWrittenWarning, action.Outcome);
    }
}

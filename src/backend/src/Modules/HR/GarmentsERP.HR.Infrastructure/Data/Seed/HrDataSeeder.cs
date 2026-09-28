using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using GarmentsERP.HR.Domain.Entities;
using GarmentsERP.HR.Domain.Enums;
using GarmentsERP.HR.Infrastructure.Data;

namespace GarmentsERP.HR.Infrastructure.Data.Seed;

public static class HrDataSeeder
{
    public static async Task SeedAsync(HrDbContext context)
    {
        try
        {
            var dbCreator = context.Database.GetService<IRelationalDatabaseCreator>();
            if (dbCreator.Exists())
            {
                try { dbCreator.CreateTables(); } catch { }
            }
            else
            {
                dbCreator.Create();
                dbCreator.CreateTables();
            }
        }
        catch { }

        // 1. Seed Work Shifts (HR-03)
        WorkShift shiftMorning;
        WorkShift shiftNight;
        if (!await context.WorkShifts.AnyAsync())
        {
            shiftMorning = new WorkShift
            {
                ShiftCode = "SHIFT-M",
                ShiftName = "Standard Morning Shift (08:00 - 17:00)",
                StartTime = new TimeSpan(8, 0, 0),
                EndTime = new TimeSpan(17, 0, 0),
                GracePeriodMinutes = 15,
                HalfDayThresholdMinutes = 60,
                IsNightShift = false,
                ShiftAllowanceDaily = 0m,
                IsActive = true
            };

            shiftNight = new WorkShift
            {
                ShiftCode = "SHIFT-N",
                ShiftName = "Overnight Production Shift (20:00 - 05:00)",
                StartTime = new TimeSpan(20, 0, 0),
                EndTime = new TimeSpan(5, 0, 0),
                GracePeriodMinutes = 15,
                HalfDayThresholdMinutes = 60,
                IsNightShift = true,
                ShiftAllowanceDaily = 150m, // BDT 150 night allowance
                IsActive = true
            };

            await context.WorkShifts.AddRangeAsync(shiftMorning, shiftNight);
            await context.SaveChangesAsync();
        }
        else
        {
            shiftMorning = await context.WorkShifts.FirstAsync(s => s.ShiftCode == "SHIFT-M");
            shiftNight = await context.WorkShifts.FirstAsync(s => s.ShiftCode == "SHIFT-N");
        }

        // 2. Seed Biometric Devices (HR-02)
        BiometricDevice deviceGate1;
        BiometricDevice deviceFacial2;
        if (!await context.BiometricDevices.AnyAsync())
        {
            deviceGate1 = new BiometricDevice
            {
                DeviceCode = "BIO-GATE-01",
                DeviceName = "Main Gate Optical Fingerprint Terminal",
                IpAddress = "192.168.10.201",
                PortNumber = 4370,
                ProtocolType = BiometricProtocolType.ZKTeco_Standalone,
                PhysicalLocation = "Factory Main Entrance Gate A",
                IsOnline = true,
                LastSyncTime = DateTime.UtcNow.AddMinutes(-5),
                TotalPunchesRecorded = 4820
            };

            deviceFacial2 = new BiometricDevice
            {
                DeviceCode = "BIO-FACIAL-02",
                DeviceName = "Sewing Floor Line 04 Facial Recognition Terminal",
                IpAddress = "192.168.10.202",
                PortNumber = 4370,
                ProtocolType = BiometricProtocolType.FacialRecognition_Terminal,
                PhysicalLocation = "Sewing Floor 2, Station 4",
                IsOnline = true,
                LastSyncTime = DateTime.UtcNow.AddMinutes(-2),
                TotalPunchesRecorded = 1940
            };

            await context.BiometricDevices.AddRangeAsync(deviceGate1, deviceFacial2);
            await context.SaveChangesAsync();
        }
        else
        {
            deviceGate1 = await context.BiometricDevices.FirstAsync(d => d.DeviceCode == "BIO-GATE-01");
            deviceFacial2 = await context.BiometricDevices.FirstAsync(d => d.DeviceCode == "BIO-FACIAL-02");
        }

        // 3. Seed Employees with Digital Service Book & AC-07 Age Verification
        if (!await context.Employees.AnyAsync())
        {
            // Sample Operator: Md. Rafiqul Islam from 08_HR_Compliance_DualPayroll.md
            var empRafiqul = new Employee
            {
                EmployeeCode = "EMP-2026-1042",
                FullName = "Md. Rafiqul Islam",
                FullNameBangla = "মোঃ রফিকুল ইসলাম",
                Email = "rafiqul.islam@stitchmind.com",
                PhoneNumber = "+880 1722-445566",
                Department = "Sewing Section",
                Designation = "Senior Sewing Operator",
                AssignedLine = "Line-04",
                DateOfBirth = new DateTime(2000, 4, 15), // Age: 26 Years
                CalculatedAgeYears = 26,
                IsAgeVerified = true,
                AgeVerificationStatus = AgeVerificationStatus.AgeVerified,
                PayStream = PayStreamType.FloorPieceRate,
                PieceRatePerUnit = 0.0700m, // $0.0700 / waistband pc
                BasicSalary = 16000m,
                HouseRentAllowance = 6500m,
                MedicalAllowance = 2000m,
                ConveyanceAllowance = 1500m,
                Status = EmployeeStatus.Active,
                JoiningDate = new DateTime(2023, 1, 10),
                EmergencyContact = "+880 1933-778899",
                NationalIdOrBirthCert = "19951234567890123",
                BiometricBadgeNumber = "BIO-BADGE-1042",
                DisbursementChannel = PaymentDisbursementChannel.bKash,
                MobileBankingNumber = "01722445566"
            };

            var empKarim = new Employee
            {
                EmployeeCode = "EMP-1044",
                FullName = "Md. Karim Ullah",
                FullNameBangla = "মোঃ করিম উল্লাহ",
                Email = "karim.ullah@stitchmind.com",
                PhoneNumber = "+880 1711-234567",
                Department = "Sewing Section",
                Designation = "Machine Operator",
                AssignedLine = "Line-01",
                DateOfBirth = new DateTime(1994, 8, 20),
                CalculatedAgeYears = 32,
                IsAgeVerified = true,
                AgeVerificationStatus = AgeVerificationStatus.AgeVerified,
                PayStream = PayStreamType.FloorPieceRate,
                PieceRatePerUnit = 0.0650m,
                BasicSalary = 18500m,
                HouseRentAllowance = 7500m,
                MedicalAllowance = 2500m,
                ConveyanceAllowance = 1500m,
                Status = EmployeeStatus.Active,
                JoiningDate = DateTime.UtcNow.AddYears(-3),
                EmergencyContact = "+880 1812-987654",
                NationalIdOrBirthCert = "19912693847291029",
                BiometricBadgeNumber = "BIO-BADGE-1044",
                DisbursementChannel = PaymentDisbursementChannel.Nagad,
                MobileBankingNumber = "01711234567"
            };

            var empFatema = new Employee
            {
                EmployeeCode = "EMP-1045",
                FullName = "Fatema Begum",
                FullNameBangla = "ফাতেমা বেগম",
                Email = "fatema.begum@stitchmind.com",
                PhoneNumber = "+880 1912-345678",
                Department = "Cutting Section",
                Designation = "Marker Pattern Specialist",
                AssignedLine = "Cutting Floor 1",
                DateOfBirth = new DateTime(1997, 11, 12),
                CalculatedAgeYears = 28,
                IsAgeVerified = true,
                AgeVerificationStatus = AgeVerificationStatus.AgeVerified,
                PayStream = PayStreamType.MonthlyStaff,
                PieceRatePerUnit = 0m,
                BasicSalary = 22000m,
                HouseRentAllowance = 9000m,
                MedicalAllowance = 2500m,
                ConveyanceAllowance = 1500m,
                Status = EmployeeStatus.Active,
                JoiningDate = DateTime.UtcNow.AddYears(-2),
                EmergencyContact = "+880 1715-112233",
                NationalIdOrBirthCert = "19945693847291030",
                BiometricBadgeNumber = "BIO-BADGE-1045",
                DisbursementChannel = PaymentDisbursementChannel.BankTransfer,
                MobileBankingNumber = "BRAC-20510144910"
            };

            var empTariqul = new Employee
            {
                EmployeeCode = "EMP-1046",
                FullName = "Tariqul Islam",
                FullNameBangla = "তারিকুল ইসলাম",
                Email = "tariqul.islam@stitchmind.com",
                PhoneNumber = "+880 1811-998877",
                Department = "Quality Assurance",
                Designation = "In-Line Senior Inspector",
                AssignedLine = "Line-01 & Line-02",
                DateOfBirth = new DateTime(1992, 3, 5),
                CalculatedAgeYears = 34,
                IsAgeVerified = true,
                AgeVerificationStatus = AgeVerificationStatus.AgeVerified,
                PayStream = PayStreamType.MonthlyStaff,
                PieceRatePerUnit = 0m,
                BasicSalary = 20000m,
                HouseRentAllowance = 8000m,
                MedicalAllowance = 2500m,
                ConveyanceAllowance = 1500m,
                Status = EmployeeStatus.Active,
                JoiningDate = DateTime.UtcNow.AddYears(-1),
                EmergencyContact = "+880 1611-334455",
                NationalIdOrBirthCert = "19967893847291040",
                BiometricBadgeNumber = "BIO-BADGE-1046",
                DisbursementChannel = PaymentDisbursementChannel.BankTransfer,
                MobileBankingNumber = "EBL-10411066723"
            };

            await context.Employees.AddRangeAsync(empRafiqul, empKarim, empFatema, empTariqul);
            await context.SaveChangesAsync();

            // 4. Seed Attendance Records
            var today = DateTime.UtcNow.Date;
            var att1 = new AttendanceRecord
            {
                EmployeeId = empRafiqul.Id,
                Date = today,
                CheckInTime = new TimeSpan(7, 56, 0),
                CheckOutTime = new TimeSpan(19, 0, 0), // 2 hrs Overtime
                OvertimeHours = 2.0m,
                Status = AttendanceStatus.Present,
                ShiftName = shiftMorning.ShiftName,
                BiometricDeviceId = deviceFacial2.DeviceCode,
                Remarks = "Facial recognition match 99.4%"
            };

            var att2 = new AttendanceRecord
            {
                EmployeeId = empKarim.Id,
                Date = today,
                CheckInTime = new TimeSpan(8, 5, 0),
                CheckOutTime = new TimeSpan(17, 30, 0),
                OvertimeHours = 0.5m,
                Status = AttendanceStatus.Present,
                ShiftName = shiftMorning.ShiftName,
                BiometricDeviceId = deviceGate1.DeviceCode,
                Remarks = "Fingerprint punch verified"
            };

            await context.AttendanceRecords.AddRangeAsync(att1, att2);

            // 5. Seed Biometric Punch Logs (HR-02)
            var log1 = new BiometricPunchLog
            {
                BiometricDeviceId = deviceFacial2.Id,
                EmployeeId = empRafiqul.Id,
                BiometricBadgeNumber = empRafiqul.BiometricBadgeNumber!,
                PunchTimestamp = today.AddHours(7).AddMinutes(56),
                PunchMode = PunchInOutMode.CheckIn,
                VerificationType = "FaceScan",
                IsProcessedToAttendance = true
            };

            var log2 = new BiometricPunchLog
            {
                BiometricDeviceId = deviceGate1.Id,
                EmployeeId = empKarim.Id,
                BiometricBadgeNumber = empKarim.BiometricBadgeNumber!,
                PunchTimestamp = today.AddHours(8).AddMinutes(5),
                PunchMode = PunchInOutMode.CheckIn,
                VerificationType = "Fingerprint",
                IsProcessedToAttendance = true
            };

            await context.BiometricPunchLogs.AddRangeAsync(log1, log2);

            // 6. Seed Leave Balances & Maternity Benefit (HR-04)
            var leaveBalRafiqul = new EmployeeLeaveBalance
            {
                EmployeeId = empRafiqul.Id,
                CalendarYear = 2026,
                CasualLeaveTotal = 10,
                CasualLeaveUsed = 2,
                SickLeaveTotal = 14,
                SickLeaveUsed = 1,
                EarnedLeaveTotal = 16,
                EarnedLeaveUsed = 4,
                EarnedLeaveEncashedDays = 0,
                TotalEncashedAmount = 0m
            };

            var leaveBalFatema = new EmployeeLeaveBalance
            {
                EmployeeId = empFatema.Id,
                CalendarYear = 2026,
                CasualLeaveTotal = 10,
                CasualLeaveUsed = 3,
                SickLeaveTotal = 14,
                SickLeaveUsed = 2,
                EarnedLeaveTotal = 16,
                EarnedLeaveUsed = 0,
                EarnedLeaveEncashedDays = 0,
                TotalEncashedAmount = 0m
            };

            await context.EmployeeLeaveBalances.AddRangeAsync(leaveBalRafiqul, leaveBalFatema);

            var maternity = new MaternityBenefit
            {
                EmployeeId = empFatema.Id,
                CaseReferenceNumber = "MAT-2026-0012",
                NoticeDate = DateTime.UtcNow.AddDays(-30),
                ExpectedDeliveryDate = DateTime.UtcNow.AddDays(45),
                LeaveStartDate = DateTime.UtcNow.AddDays(-15),
                LeaveEndDate = DateTime.UtcNow.AddDays(96), // 112 days
                TotalLeaveDays = 112,
                PriorThreeMonthsTotalWages = 105000m, // 35,000 gross * 3
                PriorThreeMonthsWorkingDays = 78,
                MedicalDoctorCertificateNumber = "DMC-REG-2026-8819",
                RegisteredClinicName = "Factory Occupational Health Clinic",
                PaymentDisbursementMethod = "bKash Direct Payout",
                IsFirstInstallmentPaid = true,
                FirstInstallmentPaidDate = DateTime.UtcNow.AddDays(-10),
                IsSecondInstallmentPaid = false
            };

            await context.MaternityBenefits.AddAsync(maternity);

            // 7. Seed October 2026 Dual-Stream Payroll Sheet (HR-05)
            var currentYear = 2026;
            var currentMonth = 10;

            var sheet = new PayrollSheet
            {
                SheetNumber = $"PAY-{currentYear}-{currentMonth:D2}",
                PayMonth = currentMonth,
                PayYear = currentYear,
                TotalHeadcount = 1250,
                TotalPieceRateWorkers = 1100,
                TotalStaffWorkers = 150,
                TotalBaseOrPieceRateEarnings = 108420m,
                TotalOvertimeAmount = 24780m,
                TotalAttendanceBonus = 11200m,
                TotalGrossEarnings = 144400m,
                TotalDeductions = 1900m,
                TotalNetDisbursement = 142500m, // $142,500.00 USD per 08_HR_Compliance_DualPayroll.md
                Status = PayrollStatus.Approved,
                ApprovedBy = "Director of Human Resources & Social Compliance",
                ApprovedAt = DateTime.UtcNow.AddDays(-2)
            };

            // Record 1: Md. Rafiqul Islam ($215.70 net earnings)
            var payRafiqul = new PayrollRecord
            {
                PayrollSheetId = sheet.Id,
                EmployeeId = empRafiqul.Id,
                PayMonth = currentMonth,
                PayYear = currentYear,
                PayStream = PayStreamType.FloorPieceRate,
                BasicSalary = 16000m,
                Allowances = 0m,
                CompletedGarmentUnits = 2400, // 2,400 garments
                PieceRatePerUnit = 0.0700m, // $0.0700 / pc => $168.00
                OvertimeHours = 26.0m,
                OvertimeRatePerHour = 1.45m, // => $37.70
                AttendanceBonus = 10.0m, // => $10.00
                AbsentDeduction = 0m,
                LatePenaltyDeduction = 0m,
                TaxDeduction = 0m,
                AdvanceDeduction = 0m,
                DisbursementChannel = PaymentDisbursementChannel.bKash,
                MfsOrAccountNumber = "01722445566",
                Status = PayrollStatus.Approved,
                PaymentReference = "BKASH-SAL-202610-1042"
            };

            // Record 2: Fatema Begum (Staff)
            var payFatema = new PayrollRecord
            {
                PayrollSheetId = sheet.Id,
                EmployeeId = empFatema.Id,
                PayMonth = currentMonth,
                PayYear = currentYear,
                PayStream = PayStreamType.MonthlyStaff,
                BasicSalary = 22000m,
                Allowances = 13000m,
                CompletedGarmentUnits = 0,
                PieceRatePerUnit = 0m,
                OvertimeHours = 10.0m,
                OvertimeRatePerHour = 211.54m,
                AttendanceBonus = 1000m,
                AbsentDeduction = 0m,
                LatePenaltyDeduction = 0m,
                TaxDeduction = 1100m,
                AdvanceDeduction = 0m,
                DisbursementChannel = PaymentDisbursementChannel.BankTransfer,
                MfsOrAccountNumber = "BRAC-20510144910",
                Status = PayrollStatus.Approved,
                PaymentReference = "BRAC-SAL-202610-1045"
            };

            sheet.Records = new List<PayrollRecord> { payRafiqul, payFatema };
            await context.PayrollSheets.AddAsync(sheet);

            // 8. Seed Clinic Visit, Grievance, and Disciplinary Action (HR-07)
            var clinicVisit = new ClinicVisit
            {
                EmployeeId = empRafiqul.Id,
                VisitDate = DateTime.UtcNow.AddDays(-7),
                ChiefComplaint = "Minor needle deflection graze on right index finger",
                Diagnosis = "Superficial finger abrasion (Broken needle fragment protocol completed)",
                TreatmentGiven = "Antiseptic cleaning, sterile dressing bandage applied",
                MedicineDispensed = "Tetanus toxoid booster, Paracetamol 500mg",
                FitnessStatus = ClinicDutyFitnessStatus.FitForNormalDuty,
                AttendingNurseOrDoctor = "Dr. Shahnaz Parvin (Factory Medical Officer)",
                Remarks = "Returned to Line 04. No needle fragment retained."
            };

            var grievance = new WorkerGrievance
            {
                TrackingNumber = "GRV-2026-0042",
                SubmissionDate = DateTime.UtcNow.AddDays(-12),
                IsAnonymous = true,
                EmployeeId = null,
                Category = GrievanceCategory.CanteenFoodQuality,
                Title = "Drinking water filter cooler maintenance on Floor 2",
                Description = "The RO water dispenser on sewing floor 2 was dispensing lukewarm water during afternoon peak hours.",
                Status = GrievanceStatus.Resolved,
                InvestigatedBy = "Factory Welfare Officer",
                WelfareCommitteeFindings = "Compressor thermostat failed on dispenser # 4.",
                ResolutionSummary = "Maintenance team replaced cooling thermostat; water temperature re-tested at 16°C.",
                ResolvedDate = DateTime.UtcNow.AddDays(-9)
            };

            var disciplinary = new DisciplinaryAction
            {
                EmployeeId = empKarim.Id,
                NoticeNumber = "DISC-2026-0019",
                IncidentDate = DateTime.UtcNow.AddDays(-20),
                ViolationType = DisciplinaryViolationType.SafetyViolation,
                AllegationDescription = "Operating overlock sewing machine without mandatory protective eye shield guard.",
                HearingDate = DateTime.UtcNow.AddDays(-18),
                Outcome = DisciplinaryOutcome.FirstWrittenWarning,
                AuthorizedBy = "Head of HR & Social Compliance",
                IssuedDate = DateTime.UtcNow.AddDays(-17)
            };

            await context.ClinicVisits.AddAsync(clinicVisit);
            await context.WorkerGrievances.AddAsync(grievance);
            await context.DisciplinaryActions.AddAsync(disciplinary);

            await context.SaveChangesAsync();
        }
    }
}

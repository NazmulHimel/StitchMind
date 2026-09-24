using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using GarmentsERP.HR.Domain.Entities;
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

        if (!await context.Employees.AnyAsync())
        {
            var emp1 = new Employee
            {
                EmployeeCode = "EMP-1044",
                FullName = "Md. Karim Ullah",
                Email = "karim.ullah@stitchmind.com",
                PhoneNumber = "+880 1711-234567",
                Department = "Sewing Section",
                Designation = "Senior Machine Operator",
                AssignedLine = "Line-01",
                BasicSalary = 18500m,
                HouseRentAllowance = 7500m,
                MedicalAllowance = 2500m,
                ConveyanceAllowance = 1500m,
                Status = EmployeeStatus.Active,
                JoiningDate = DateTime.UtcNow.AddYears(-3),
                EmergencyContact = "+880 1812-987654",
                NationalId = "19912693847291029",
                MobileBankingNumber = "DBBL: 152.120.98472"
            };

            var emp2 = new Employee
            {
                EmployeeCode = "EMP-1045",
                FullName = "Fatema Begum",
                Email = "fatema.begum@stitchmind.com",
                PhoneNumber = "+880 1912-345678",
                Department = "Cutting Section",
                Designation = "Marker Pattern Specialist",
                AssignedLine = "Cutting Floor 1",
                BasicSalary = 22000m,
                HouseRentAllowance = 9000m,
                MedicalAllowance = 2500m,
                ConveyanceAllowance = 1500m,
                Status = EmployeeStatus.Active,
                JoiningDate = DateTime.UtcNow.AddYears(-2),
                EmergencyContact = "+880 1715-112233",
                NationalId = "19945693847291030",
                MobileBankingNumber = "BRAC: 205.101.44910"
            };

            var emp3 = new Employee
            {
                EmployeeCode = "EMP-1046",
                FullName = "Tariqul Islam",
                Email = "tariqul.islam@stitchmind.com",
                PhoneNumber = "+880 1811-998877",
                Department = "Quality Assurance",
                Designation = "In-Line Senior Inspector",
                AssignedLine = "Line-01 & Line-02",
                BasicSalary = 20000m,
                HouseRentAllowance = 8000m,
                MedicalAllowance = 2500m,
                ConveyanceAllowance = 1500m,
                Status = EmployeeStatus.Active,
                JoiningDate = DateTime.UtcNow.AddYears(-1),
                EmergencyContact = "+880 1611-334455",
                NationalId = "19967893847291040",
                MobileBankingNumber = "EBL: 104.110.66723"
            };

            var emp4 = new Employee
            {
                EmployeeCode = "EMP-1047",
                FullName = "Nasreen Sultana",
                Email = "nasreen.sultana@stitchmind.com",
                PhoneNumber = "+880 1722-445566",
                Department = "Sewing Section",
                Designation = "Overlock Operator",
                AssignedLine = "Line-02",
                BasicSalary = 16000m,
                HouseRentAllowance = 6500m,
                MedicalAllowance = 2000m,
                ConveyanceAllowance = 1500m,
                Status = EmployeeStatus.Active,
                JoiningDate = DateTime.UtcNow.AddMonths(-18),
                EmergencyContact = "+880 1933-778899",
                NationalId = "19951234567890123",
                MobileBankingNumber = "bKash: 01722445566"
            };

            var emp5 = new Employee
            {
                EmployeeCode = "EMP-1048",
                FullName = "Abdur Rahim",
                Email = "abdur.rahim@stitchmind.com",
                PhoneNumber = "+880 1611-123456",
                Department = "Finishing & Packing",
                Designation = "Dispatch Floor Supervisor",
                AssignedLine = "Packing Area",
                BasicSalary = 25000m,
                HouseRentAllowance = 10000m,
                MedicalAllowance = 3000m,
                ConveyanceAllowance = 2000m,
                Status = EmployeeStatus.Active,
                JoiningDate = DateTime.UtcNow.AddYears(-4),
                EmergencyContact = "+880 1711-654321",
                NationalId = "19881234567890124",
                MobileBankingNumber = "Nagad: 01611123456"
            };

            await context.Employees.AddRangeAsync(emp1, emp2, emp3, emp4, emp5);
            await context.SaveChangesAsync();

            // Seed Attendance
            var today = DateTime.UtcNow.Date;
            var att1 = new AttendanceRecord
            {
                EmployeeId = emp1.Id,
                Date = today,
                CheckInTime = new TimeSpan(8, 2, 0),
                CheckOutTime = new TimeSpan(17, 30, 0),
                OvertimeHours = 1.5m,
                Status = AttendanceStatus.Present,
                ShiftName = "Morning Shift (08:00 - 17:00)",
                BiometricDeviceId = "BIO-GATE-01",
                Remarks = "Fingerprint punch verified"
            };

            var att2 = new AttendanceRecord
            {
                EmployeeId = emp2.Id,
                Date = today,
                CheckInTime = new TimeSpan(7, 58, 0),
                CheckOutTime = new TimeSpan(17, 0, 0),
                OvertimeHours = 1.0m,
                Status = AttendanceStatus.Present,
                ShiftName = "Morning Shift (08:00 - 17:00)",
                BiometricDeviceId = "BIO-GATE-01",
                Remarks = "Fingerprint punch verified"
            };

            var att3 = new AttendanceRecord
            {
                EmployeeId = emp3.Id,
                Date = today,
                CheckInTime = new TimeSpan(8, 25, 0),
                CheckOutTime = new TimeSpan(17, 0, 0),
                OvertimeHours = 0.5m,
                Status = AttendanceStatus.Late,
                ShiftName = "Morning Shift (08:00 - 17:00)",
                BiometricDeviceId = "BIO-GATE-02",
                Remarks = "25 mins late due to transport delay"
            };

            var att4 = new AttendanceRecord
            {
                EmployeeId = emp4.Id,
                Date = today,
                CheckInTime = new TimeSpan(8, 0, 0),
                CheckOutTime = new TimeSpan(19, 0, 0),
                OvertimeHours = 3.0m,
                Status = AttendanceStatus.Present,
                ShiftName = "Morning Shift (08:00 - 17:00)",
                BiometricDeviceId = "BIO-GATE-01",
                Remarks = "RFID Card punch"
            };

            await context.AttendanceRecords.AddRangeAsync(att1, att2, att3, att4);

            // Seed Leave Request
            var leave1 = new LeaveRequest
            {
                EmployeeId = emp5.Id,
                Type = LeaveType.Casual,
                StartDate = today.AddDays(2),
                EndDate = today.AddDays(3),
                TotalDays = 2,
                Reason = "Family function in village",
                Status = LeaveStatus.Pending
            };

            await context.LeaveRequests.AddAsync(leave1);

            // Seed Payroll Record
            var prevMonth = DateTime.UtcNow.AddMonths(-1).Month;
            var currentYear = DateTime.UtcNow.Year;
            var payroll1 = new PayrollRecord
            {
                EmployeeId = emp1.Id,
                PayMonth = prevMonth,
                PayYear = currentYear,
                BasicSalary = 18500m,
                Allowances = 11500m,
                OvertimeHours = 24.5m,
                OvertimeRatePerHour = 152m,
                AbsentDeduction = 0m,
                TaxDeduction = 500m,
                AdvanceDeduction = 0m,
                Status = PayrollStatus.Approved,
                PaymentReference = "DBBL-SAL-202609-001"
            };

            await context.PayrollRecords.AddAsync(payroll1);
            await context.SaveChangesAsync();
        }
    }
}

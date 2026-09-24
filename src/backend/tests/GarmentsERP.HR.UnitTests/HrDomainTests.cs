using GarmentsERP.HR.Domain.Entities;
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
    public void PayrollRecord_NetPayable_ShouldCalculateOvertimeAndDeductions()
    {
        var payroll = new PayrollRecord
        {
            EmployeeId = Guid.NewGuid(),
            PayMonth = 9,
            PayYear = 2026,
            BasicSalary = 20000m,
            Allowances = 12000m,
            OvertimeHours = 20m,
            OvertimeRatePerHour = 150m, // OvertimeAmount = 3000m
            AbsentDeduction = 1000m,
            TaxDeduction = 500m,
            AdvanceDeduction = 500m // TotalDeductions = 2000m
        };

        // Gross = 20000 + 12000 + 3000 = 35000m
        // Total Deductions = 2000m
        // Net Payable = 33000m
        Assert.Equal(3000m, payroll.OvertimeAmount);
        Assert.Equal(2000m, payroll.TotalDeductions);
        Assert.Equal(33000m, payroll.NetPayable);
        Assert.Equal(PayrollStatus.Draft, payroll.Status);
    }

    [Fact]
    public void LeaveRequest_ShouldTrackLeaveStatusAndApproval()
    {
        var leave = new LeaveRequest
        {
            EmployeeId = Guid.NewGuid(),
            Type = LeaveType.Casual,
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddDays(2),
            TotalDays = 3,
            Reason = "Family emergency in village",
            Status = LeaveStatus.Pending
        };

        Assert.Equal(3, leave.TotalDays);
        Assert.Equal(LeaveStatus.Pending, leave.Status);
        Assert.Equal(LeaveType.Casual, leave.Type);
    }

    [Fact]
    public void AttendanceRecord_ShouldLogBiometricPunches()
    {
        var today = DateTime.UtcNow.Date;
        var att = new AttendanceRecord
        {
            EmployeeId = Guid.NewGuid(),
            Date = today,
            CheckInTime = new TimeSpan(8, 0, 0),
            CheckOutTime = new TimeSpan(19, 30, 0),
            OvertimeHours = 3.5m,
            Status = AttendanceStatus.Present,
            ShiftName = "Morning Shift",
            BiometricDeviceId = "BIO-GATE-01"
        };

        Assert.Equal(AttendanceStatus.Present, att.Status);
        Assert.Equal(3.5m, att.OvertimeHours);
        Assert.Equal("BIO-GATE-01", att.BiometricDeviceId);
    }
}

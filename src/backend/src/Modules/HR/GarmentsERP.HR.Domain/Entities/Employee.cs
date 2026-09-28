using GarmentsERP.Shared.Domain;
using GarmentsERP.HR.Domain.Enums;

namespace GarmentsERP.HR.Domain.Entities;

public class Employee : BaseEntity
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string FullNameBangla { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty; // Cutting, Sewing, QC, Warehouse, Admin
    public string Designation { get; set; } = string.Empty; // Operator, Helper, Supervisor, Manager
    public string? AssignedLine { get; set; } // e.g. Line 01, Line 02

    // AC-07 Child Labor Age Gate & Compliance
    public DateTime DateOfBirth { get; set; }
    public decimal CalculatedAgeYears { get; set; }
    public bool IsAgeVerified { get; set; }
    public AgeVerificationStatus AgeVerificationStatus { get; set; } = AgeVerificationStatus.PendingVerification;
    public string? NationalIdOrBirthCert { get; set; }
    public string? BiometricBadgeNumber { get; set; }

    // Dual-Stream Payroll Configuration
    public PayStreamType PayStream { get; set; } = PayStreamType.MonthlyStaff;
    public decimal PieceRatePerUnit { get; set; } // e.g. 0.0700 USD per waistband attachment
    public decimal BasicSalary { get; set; }
    public decimal HouseRentAllowance { get; set; }
    public decimal MedicalAllowance { get; set; }
    public decimal ConveyanceAllowance { get; set; }
    public decimal GrossSalary => BasicSalary + HouseRentAllowance + MedicalAllowance + ConveyanceAllowance;

    // Disbursed Payment Details
    public PaymentDisbursementChannel DisbursementChannel { get; set; } = PaymentDisbursementChannel.bKash;
    public string? MobileBankingNumber { get; set; } // bKash / Nagad / Bank Account
    public string? EmergencyContact { get; set; }

    public DateTime JoiningDate { get; set; }
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;

    // Backward-compatible properties
    public string? NationalId 
    { 
        get => NationalIdOrBirthCert; 
        set => NationalIdOrBirthCert = value; 
    }

    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    public ICollection<PayrollRecord> PayrollRecords { get; set; } = new List<PayrollRecord>();

    /// <summary>
    /// AC-07 Child Labor Statutory Verification:
    /// Computes exact completed years on DateOfJoining. Age < 18 is strictly prohibited under international labor standards.
    /// </summary>
    public bool VerifyAgeCompliance(DateTime? asOfDate = null)
    {
        var targetDate = asOfDate ?? JoiningDate;
        int age = targetDate.Year - DateOfBirth.Year;
        if (DateOfBirth.Date > targetDate.AddYears(-age))
            age--;

        CalculatedAgeYears = age;

        if (age < 18)
        {
            IsAgeVerified = false;
            AgeVerificationStatus = AgeVerificationStatus.BlockedUnderageLabor;
            Status = EmployeeStatus.Suspended;
            return false;
        }

        IsAgeVerified = true;
        AgeVerificationStatus = AgeVerificationStatus.AgeVerified;
        return true;
    }
}

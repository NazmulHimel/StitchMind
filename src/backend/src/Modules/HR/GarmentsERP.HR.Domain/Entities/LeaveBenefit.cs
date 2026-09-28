using GarmentsERP.Shared.Domain;

namespace GarmentsERP.HR.Domain.Entities;

public class EmployeeLeaveBalance : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public int CalendarYear { get; set; }
    public int CasualLeaveTotal { get; set; } = 10;
    public int CasualLeaveUsed { get; set; }
    public int CasualLeaveRemaining => CasualLeaveTotal - CasualLeaveUsed;

    public int SickLeaveTotal { get; set; } = 14;
    public int SickLeaveUsed { get; set; }
    public int SickLeaveRemaining => SickLeaveTotal - SickLeaveUsed;

    public int EarnedLeaveTotal { get; set; } = 16;
    public int EarnedLeaveUsed { get; set; }
    public int EarnedLeaveRemaining => EarnedLeaveTotal - EarnedLeaveUsed - EarnedLeaveEncashedDays;
    public int EarnedLeaveEncashedDays { get; set; }
    public decimal TotalEncashedAmount { get; set; }

    /// <summary>
    /// Bangladesh Labor Act formula: (BasicSalary / 30) * UnusedDays
    /// </summary>
    public decimal CalculateEarnedLeaveEncashment(decimal basicSalary, int daysToEncash)
    {
        if (daysToEncash > EarnedLeaveRemaining)
            throw new InvalidOperationException($"Cannot encash {daysToEncash} days. Only {EarnedLeaveRemaining} days available.");

        decimal dailyWage = basicSalary / 30m;
        decimal encashment = Math.Round(dailyWage * daysToEncash, 2);
        EarnedLeaveEncashedDays += daysToEncash;
        TotalEncashedAmount += encashment;
        return encashment;
    }
}

public class MaternityBenefit : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public string CaseReferenceNumber { get; set; } = string.Empty; // e.g. MAT-2026-0012
    public DateTime NoticeDate { get; set; }
    public DateTime ExpectedDeliveryDate { get; set; }
    public DateTime LeaveStartDate { get; set; }
    public DateTime LeaveEndDate { get; set; } // 112 days (16 weeks) total
    public int TotalLeaveDays { get; set; } = 112; // 8 weeks prenatal + 8 weeks postnatal

    public decimal PriorThreeMonthsTotalWages { get; set; }
    public int PriorThreeMonthsWorkingDays { get; set; } = 78; // Approx 26 days * 3
    public decimal AverageDailyWage => PriorThreeMonthsWorkingDays > 0 
        ? Math.Round(PriorThreeMonthsTotalWages / PriorThreeMonthsWorkingDays, 2) 
        : 0m;

    public decimal FirstInstallmentAmount => Math.Round(AverageDailyWage * 56, 2); // 8 weeks prenatal
    public bool IsFirstInstallmentPaid { get; set; }
    public DateTime? FirstInstallmentPaidDate { get; set; }

    public decimal SecondInstallmentAmount => Math.Round(AverageDailyWage * 56, 2); // 8 weeks postnatal
    public bool IsSecondInstallmentPaid { get; set; }
    public DateTime? SecondInstallmentPaidDate { get; set; }

    public decimal TotalBenefitAmount => FirstInstallmentAmount + SecondInstallmentAmount;
    public string MedicalDoctorCertificateNumber { get; set; } = string.Empty;
    public string RegisteredClinicName { get; set; } = "Factory Occupational Health Clinic";
    public string PaymentDisbursementMethod { get; set; } = "bKash Direct Payout";
}

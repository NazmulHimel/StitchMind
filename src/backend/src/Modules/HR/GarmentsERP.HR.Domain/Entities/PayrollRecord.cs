using GarmentsERP.Shared.Domain;

namespace GarmentsERP.HR.Domain.Entities;

public enum PayrollStatus
{
    Draft = 1,
    Approved = 2,
    Disbursed = 3
}

public class PayrollRecord : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public int PayMonth { get; set; } // 1-12
    public int PayYear { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal Allowances { get; set; }
    public decimal OvertimeHours { get; set; }
    public decimal OvertimeRatePerHour { get; set; }
    public decimal OvertimeAmount => OvertimeHours * OvertimeRatePerHour;
    public decimal AbsentDeduction { get; set; }
    public decimal TaxDeduction { get; set; }
    public decimal AdvanceDeduction { get; set; }
    public decimal TotalDeductions => AbsentDeduction + TaxDeduction + AdvanceDeduction;
    public decimal NetPayable => (BasicSalary + Allowances + OvertimeAmount) - TotalDeductions;
    public PayrollStatus Status { get; set; } = PayrollStatus.Draft;
    public DateTime? DisbursedDate { get; set; }
    public string? PaymentReference { get; set; }
}

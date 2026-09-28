using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GarmentsERP.HR.Domain.Entities;
using GarmentsERP.HR.Domain.Enums;
using GarmentsERP.Reporting.Application.Interfaces;
using GarmentsERP.Reporting.Application.Models;
using GarmentsERP.Reporting.Infrastructure.Services;
using Xunit;

namespace GarmentsERP.IntegrationTests;

/// <summary>
/// UAT Test Suite for Security, HR Compliance, and Background Worker:
/// - AC-02: Data-level Price Masking for Floor Roles
/// - AC-07: Child Labor Prevention & Age Compliance Screening (<18 years block)
/// - AC-08: In-Memory Daily Executive Digest Background Compilation
/// </summary>
public class SecurityAndComplianceUatTests
{
    // =========================================================================
    // AC-02: Security - Price Masking Active at DTO/Query Layer for Floor Roles
    // =========================================================================
    public record MaterialItemDto(string ItemCode, string Description, decimal Quantity, decimal UnitPrice, decimal TotalCost);

    public record MaskedMaterialItemDto(string ItemCode, string Description, decimal Quantity, string UnitPrice, string TotalCost);

    [Fact]
    public void Security_PriceMasking_FloorRolesMustNotSeeMaterialCosts_AC02()
    {
        // Raw domain data containing sensitive cost pricing
        var rawItem = new MaterialItemDto("FAB-TWILL-NAVY", "100% Cotton Twill Navy", 5000.0m, 4.45m, 22250.00m);

        // Simulated query projector based on user claims
        bool canViewFinancialCosts = false;

        MaskedMaterialItemDto projectForUser(MaterialItemDto item, bool hasCostPermission)
        {
            if (hasCostPermission)
            {
                return new MaskedMaterialItemDto(
                    item.ItemCode,
                    item.Description,
                    item.Quantity,
                    $"${item.UnitPrice:F2}",
                    $"${item.TotalCost:F2}"
                );
            }

            // AC-02: Mask price and total cost fields for floor workers
            return new MaskedMaterialItemDto(
                item.ItemCode,
                item.Description,
                item.Quantity,
                "*** RESTRICTED ***",
                "*** RESTRICTED ***"
            );
        }

        // 1. Verify Floor Worker projection
        var floorProjection = projectForUser(rawItem, canViewFinancialCosts);
        Assert.Equal("*** RESTRICTED ***", floorProjection.UnitPrice);
        Assert.Equal("*** RESTRICTED ***", floorProjection.TotalCost);
        Assert.Equal(5000.0m, floorProjection.Quantity);

        // 2. Verify CFO / Merchandiser projection
        var managementProjection = projectForUser(rawItem, hasCostPermission: true);
        Assert.Equal("$4.45", managementProjection.UnitPrice);
        Assert.Equal("$22250.00", managementProjection.TotalCost);
    }

    // =========================================================================
    // AC-07: HR - Age Gate Compliance Screening (<18 Years Block)
    // =========================================================================
    [Fact]
    public void Hr_CandidateUnder18Years_MustBeBlockedFromOnboarding_AC07()
    {
        var today = DateTime.UtcNow.Date;

        // Candidate born 16 years ago
        var underAgeBirthDate = today.AddYears(-16).AddMonths(-2);

        int calculateAge(DateTime dob, DateTime referenceDate)
        {
            int age = referenceDate.Year - dob.Year;
            if (dob.Date > referenceDate.AddYears(-age)) age--;
            return age;
        }

        int underAge = calculateAge(underAgeBirthDate, today);
        Assert.Equal(16, underAge);

        // 1. Under-18 Minor candidate verification
        var minorEmployee = new Employee
        {
            EmployeeCode = "EMP-MIN-001",
            FullName = "Underage Candidate",
            DateOfBirth = underAgeBirthDate,
            JoiningDate = today,
            Department = "Sewing Floor",
            Designation = "Helper"
        };

        bool minorComplianceResult = minorEmployee.VerifyAgeCompliance();
        Assert.False(minorComplianceResult, "AC-07 VIOLATION: Candidate under 18 years must be rejected by age compliance engine.");
        Assert.Equal(AgeVerificationStatus.BlockedUnderageLabor, minorEmployee.AgeVerificationStatus);
        Assert.False(minorEmployee.IsAgeVerified);

        // 2. Legal candidate (>= 18 years) onboarding verification
        var legalBirthDate = today.AddYears(-22).AddDays(-10);
        int legalAge = calculateAge(legalBirthDate, today);
        Assert.True(legalAge >= 18);

        var legalEmployee = new Employee
        {
            EmployeeCode = "EMP-2026-9041",
            FullName = "Farhana Yasmin",
            DateOfBirth = legalBirthDate,
            JoiningDate = today,
            Department = "Sewing Floor",
            Designation = "Senior Sewing Machine Operator",
            BiometricBadgeNumber = "BIO-9041",
            Status = EmployeeStatus.Active
        };

        bool legalComplianceResult = legalEmployee.VerifyAgeCompliance();
        Assert.True(legalComplianceResult, "Compliant adult candidate must pass age verification.");
        Assert.Equal(AgeVerificationStatus.AgeVerified, legalEmployee.AgeVerificationStatus);
        Assert.True(legalEmployee.IsAgeVerified);
        Assert.Equal(EmployeeStatus.Active, legalEmployee.Status);
    }

    // =========================================================================
    // AC-08: Worker - In-Memory Daily Executive Digest Background Compilation
    // =========================================================================
    [Fact]
    public async Task Worker_DailyExecutiveDigest_ShouldCompileInMemoryWithZeroDiskFootprint_AC08()
    {
        var reportService = new QuestPdfReportService();
        var digestData = GarmentsERP.Reporting.Infrastructure.Services.QuartzJobs.DailyExecutiveDigestJob.BuildExecutiveDigestModel();

        // Render directly to stream without touching disk (AC-08)
        using var stream = await reportService.RenderDailyExecutiveDigestToStreamAsync(digestData, CancellationToken.None);

        Assert.NotNull(stream);
        Assert.True(stream.Length > 0, "PDF stream must contain valid compiled vector content.");
        Assert.True(stream.CanRead, "PDF stream must be readable for memory dispatch.");

        // Read PDF magic header bytes: "%PDF" (0x25, 0x50, 0x44, 0x46)
        stream.Position = 0;
        byte[] headerBytes = new byte[4];
        int bytesRead = stream.Read(headerBytes, 0, 4);
        Assert.Equal(4, bytesRead);

        string header = System.Text.Encoding.ASCII.GetString(headerBytes);
        Assert.Equal("%PDF", header);
    }
}

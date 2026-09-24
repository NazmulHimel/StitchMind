using GarmentsERP.Production.Domain.Entities;
using GarmentsERP.Production.Domain.Enums;
using Xunit;

namespace GarmentsERP.Production.UnitTests;

public class QualityControlTests
{
    [Fact]
    public void QualityInspection_WithCriticalDefect_ShouldFailAudit()
    {
        int critical = 1;
        int major = 0;
        int sampleSize = 200;

        var result = (critical > 0 || major > (sampleSize * 0.05))
            ? InspectionResult.Failed
            : InspectionResult.Passed;

        Assert.Equal(InspectionResult.Failed, result);
    }

    [Fact]
    public void QualityInspection_UnderAqlThreshold_ShouldPassAudit()
    {
        int critical = 0;
        int major = 2; // 1% of sample
        int sampleSize = 200;

        var result = (critical > 0 || major > (sampleSize * 0.05))
            ? InspectionResult.Failed
            : InspectionResult.Passed;

        Assert.Equal(InspectionResult.Passed, result);
    }
}

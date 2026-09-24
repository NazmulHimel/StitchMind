using GarmentsERP.Production.Domain.Entities;
using Xunit;

namespace GarmentsERP.Production.UnitTests;

public class SewingTests
{
    [Fact]
    public void HourlyProductionLog_ShouldCalculateEfficiencyAccurately()
    {
        var log = new HourlyProductionLog
        {
            TargetQty = 150,
            ActualOutputQty = 165,
            DefectCount = 2,
            RejectionCount = 0,
            OperatorCount = 35
        };

        Assert.Equal(110.0m, log.EfficiencyPercentage);
    }

    [Fact]
    public void HourlyProductionLog_WhenTargetZero_ShouldReturnZeroEfficiency()
    {
        var log = new HourlyProductionLog
        {
            TargetQty = 0,
            ActualOutputQty = 0
        };

        Assert.Equal(0, log.EfficiencyPercentage);
    }
}

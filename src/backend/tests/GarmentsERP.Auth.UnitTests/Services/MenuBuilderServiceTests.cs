using GarmentsERP.Auth.Infrastructure.Services;
using GarmentsERP.Shared.Constants;
using Xunit;

namespace GarmentsERP.Auth.UnitTests.Services;

public class MenuBuilderServiceTests
{
    private readonly MenuBuilderService _service = new();

    [Fact]
    public void SuperAdmin_ShouldReceiveAllMenus()
    {
        var permissions = new[] { $"{AppConstants.Modules.Auth}.SuperAdmin" };
        var menu = _service.BuildMenu(permissions);

        Assert.NotEmpty(menu);
        Assert.Contains(menu, m => m.Module == AppConstants.Modules.Procurement);
        Assert.Contains(menu, m => m.Module == AppConstants.Modules.Cutting);
        Assert.Contains(menu, m => m.Module == AppConstants.Modules.Sewing);
        Assert.Contains(menu, m => m.Module == AppConstants.Modules.Finance);
    }

    [Fact]
    public void CuttingSupervisor_ShouldOnlyReceiveAuthorizedMenus()
    {
        var permissions = new[] { $"{AppConstants.Modules.Cutting}.Read" };
        var menu = _service.BuildMenu(permissions);

        Assert.Contains(menu, m => m.Module == AppConstants.Modules.Dashboard);
        Assert.Contains(menu, m => m.Module == AppConstants.Modules.Cutting);
        Assert.DoesNotContain(menu, m => m.Module == AppConstants.Modules.Finance);
        Assert.DoesNotContain(menu, m => m.Module == AppConstants.Modules.Payroll);
    }
}

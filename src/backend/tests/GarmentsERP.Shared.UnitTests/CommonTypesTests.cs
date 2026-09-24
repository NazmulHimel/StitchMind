using GarmentsERP.Shared.Common;
using Xunit;

namespace GarmentsERP.Shared.UnitTests;

public class CommonTypesTests
{
    [Fact]
    public void Result_Success_ShouldHoldValue()
    {
        var result = Result<string>.Success("OK");
        Assert.True(result.IsSuccess);
        Assert.Equal("OK", result.Value);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Result_Failure_ShouldHoldError()
    {
        var result = Result<string>.Failure("Something went wrong");
        Assert.False(result.IsSuccess);
        Assert.Equal("Something went wrong", result.Error);
    }

    [Fact]
    public void PagedList_Create_ShouldPaginateProperly()
    {
        var source = Enumerable.Range(1, 55).ToList();
        var paged = PagedList<int>.Create(source, page: 2, pageSize: 20);

        Assert.Equal(2, paged.Page);
        Assert.Equal(20, paged.PageSize);
        Assert.Equal(55, paged.TotalCount);
        Assert.Equal(3, paged.TotalPages);
        Assert.True(paged.HasPreviousPage);
        Assert.True(paged.HasNextPage);
        Assert.Equal(20, paged.Items.Count);
        Assert.Equal(21, paged.Items.First());
        Assert.Equal(40, paged.Items.Last());
    }

    [Fact]
    public void ApiResponse_Ok_And_Fail_ShouldSetProperties()
    {
        var ok = ApiResponse<int>.Ok(42, "Success message");
        Assert.True(ok.Success);
        Assert.Equal(42, ok.Data);
        Assert.Equal("Success message", ok.Message);
        Assert.Empty(ok.Errors);

        var fail = ApiResponse<int>.Fail("Error occurred");
        Assert.False(fail.Success);
        Assert.Equal("Error occurred", fail.Message);
        Assert.Single(fail.Errors);
    }
}

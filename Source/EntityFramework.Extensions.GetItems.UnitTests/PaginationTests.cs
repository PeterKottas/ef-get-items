namespace EntityFramework.Extensions.GetItems.UnitTests;

public class PaginationTests
{
    [Fact]
    public void PaginatedData_TotalPages_CalculatesCorrectly()
    {
        var data = new PaginatedData<int>([1, 2, 3], page: 1, count: 10, totalCount: 25);
        
        Assert.Equal(3, data.TotalPages); // ceil(25/10) = 3
    }

    [Fact]
    public void PaginatedData_TotalPages_NullWhenNoTotalCount()
    {
        var data = new PaginatedData<int>([1, 2, 3], page: 1, count: 10);
        
        Assert.Null(data.TotalPages);
    }

    [Fact]
    public void PaginatedData_HasNextPage_Nullable()
    {
        var withNextPage = new PaginatedData<int>([1], page: 1, count: 10, hasNextPage: true);
        var withoutNextPage = new PaginatedData<int>([1], page: 1, count: 10, hasNextPage: false);
        var unknown = new PaginatedData<int>([1], page: 1, count: 10);
        
        Assert.True(withNextPage.HasNextPage);
        Assert.False(withoutNextPage.HasNextPage);
        Assert.Null(unknown.HasNextPage);
    }

    [Fact]
    public async Task Paginate_SkipsAndTakesCorrectly()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(Paginate_SkipsAndTakesCorrectly));
        await using var context = factory.CreateDbContext();
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int> { Page = 2, Count = 2 };
        var result = await context.Entities.OrderBy(e => e.Id).Paginate(request, 2);
        
        Assert.Equal(2, result.Length);
        Assert.Equal(3, result[0].Id); // Skip 2 (page 1), take 2
        Assert.Equal(4, result[1].Id);
    }

    [Fact]
    public async Task Paginate_WithSkipParameter_AddsToPageSkip()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(Paginate_WithSkipParameter_AddsToPageSkip));
        await using var context = factory.CreateDbContext();
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int> { Page = 1, Count = 2, Skip = 1 };
        var result = await context.Entities.OrderBy(e => e.Id).Paginate(request, 2);
        
        Assert.Equal(2, result.Length);
        Assert.Equal(2, result[0].Id); // Skip 1 extra, take 2
    }

    [Fact]
    public async Task GetItems_ExpensiveMode_ReturnsTotalCount()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(GetItems_ExpensiveMode_ReturnsTotalCount));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int> { Page = 1, Count = 2 };
        var options = new GetItemsOptions { PaginationHandling = PaginationHandlingEnum.Expensive };
        
        var result = await factory.GetItems(ctx => ctx.Entities, request, e => e.Id, options);
        
        Assert.Equal(2, result.Items.Length);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.Null(result.HasNextPage);
    }

    [Fact]
    public async Task GetItems_CheapMode_ReturnsHasNextPage()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(GetItems_CheapMode_ReturnsHasNextPage));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int> { Page = 1, Count = 2 };
        var options = new GetItemsOptions { PaginationHandling = PaginationHandlingEnum.Cheap };
        
        var result = await factory.GetItems(ctx => ctx.Entities, request, e => e.Id, options);
        
        Assert.Equal(2, result.Items.Length);
        Assert.True(result.HasNextPage);
        Assert.Null(result.TotalCount);
    }

    [Fact]
    public async Task GetItems_CheapMode_NoNextPage_WhenLastPage()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(GetItems_CheapMode_NoNextPage_WhenLastPage));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int> { Page = 3, Count = 2 };
        var options = new GetItemsOptions { PaginationHandling = PaginationHandlingEnum.Cheap };
        
        var result = await factory.GetItems(ctx => ctx.Entities, request, e => e.Id, options);
        
        Assert.Single(result.Items);
        Assert.False(result.HasNextPage);
    }

    [Fact]
    public async Task GetItems_NoneMode_NoMetadata()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(GetItems_NoneMode_NoMetadata));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int> { Page = 1, Count = 2 };
        var options = new GetItemsOptions { PaginationHandling = PaginationHandlingEnum.None };
        
        var result = await factory.GetItems(ctx => ctx.Entities, request, e => e.Id, options);
        
        Assert.Equal(2, result.Items.Length);
        Assert.Null(result.TotalCount);
        Assert.Null(result.HasNextPage);
    }

    [Fact]
    public async Task GetItems_UsesCachedTotalCount_WhenProvided()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(GetItems_UsesCachedTotalCount_WhenProvided));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int> { Page = 1, Count = 2, TotalCount = 100 };
        var options = new GetItemsOptions { PaginationHandling = PaginationHandlingEnum.Expensive };
        
        var result = await factory.GetItems(ctx => ctx.Entities, request, e => e.Id, options);
        
        Assert.Equal(100, result.TotalCount); // Uses provided value, not actual count
    }
}


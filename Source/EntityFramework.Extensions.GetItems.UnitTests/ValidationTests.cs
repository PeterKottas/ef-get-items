namespace EntityFramework.Extensions.GetItems.UnitTests;

public class ValidationTests
{
    [Fact]
    public async Task GetItems_WithIds_RequiresIdAccessor()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(GetItems_WithIds_RequiresIdAccessor));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            Ids = [1, 2, 3]
        };
        
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            factory.GetItems(ctx => ctx.Entities, request));
        
        Assert.Contains("idAccessor", exception.Message);
    }

    [Fact]
    public async Task GetItems_WithExceptIds_RequiresIdAccessor()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(GetItems_WithExceptIds_RequiresIdAccessor));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            ExceptIds = [1, 2]
        };
        
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            factory.GetItems(ctx => ctx.Entities, request));
        
        Assert.Contains("idAccessor", exception.Message);
    }

    [Fact]
    public async Task GetItems_WithFilters_RequiresPropertyNameToString()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(GetItems_WithFilters_RequiresPropertyNameToString));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            Filters = [new() { Field = TestPropertyEnum.Name, Operator = FilterOperatorEnum.Eq, Value = "Alice" }]
        };
        
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            factory.GetItems(ctx => ctx.Entities, request, e => e.Id));
        
        Assert.Contains("propertyNameToString", exception.Message);
    }

    [Fact]
    public async Task GetItems_WithSort_RequiresPropertyNameToString()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(GetItems_WithSort_RequiresPropertyNameToString));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            Sort = [new() { Field = TestPropertyEnum.Name }]
        };
        
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            factory.GetItems(ctx => ctx.Entities, request, e => e.Id));
        
        Assert.Contains("propertyNameToString", exception.Message);
    }

    [Fact]
    public async Task GetItems_IQueryable_ExpensiveMode_Throws()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(GetItems_IQueryable_ExpensiveMode_Throws));
        await using var context = factory.CreateDbContext();
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>();
        var options = new GetItemsOptions { PaginationHandling = PaginationHandlingEnum.Expensive };
        
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            context.Entities.GetItems(request, options));
        
        Assert.Contains("Expensive", exception.Message);
        Assert.Contains("IDbContextFactory", exception.Message);
    }

    [Fact]
    public async Task GetItems_IQueryable_CheapMode_Works()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(GetItems_IQueryable_CheapMode_Works));
        await using var context = factory.CreateDbContext();
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int> { Count = 2 };
        var options = new GetItemsOptions { PaginationHandling = PaginationHandlingEnum.Cheap };
        
        var result = await context.Entities.GetItems(request, e => e.Id, options);
        
        Assert.Equal(2, result.Items.Length);
        Assert.True(result.HasNextPage);
    }

    [Fact]
    public async Task GetItems_IQueryable_NoneMode_Works()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(GetItems_IQueryable_NoneMode_Works));
        await using var context = factory.CreateDbContext();
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int> { Count = 2 };
        var options = new GetItemsOptions { PaginationHandling = PaginationHandlingEnum.None };
        
        var result = await context.Entities.GetItems(request, e => e.Id, options);
        
        Assert.Equal(2, result.Items.Length);
        Assert.Null(result.HasNextPage);
    }

    [Fact]
    public async Task GetItems_WithIds_FiltersCorrectly()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(GetItems_WithIds_FiltersCorrectly));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            Ids = [1, 3, 5]
        };
        
        var result = await factory.GetItems(ctx => ctx.Entities, request, e => e.Id);
        
        Assert.Equal(3, result.Items.Length);
        Assert.Contains(result.Items, e => e.Id == 1);
        Assert.Contains(result.Items, e => e.Id == 3);
        Assert.Contains(result.Items, e => e.Id == 5);
    }

    [Fact]
    public async Task GetItems_WithExceptIds_ExcludesCorrectly()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(GetItems_WithExceptIds_ExcludesCorrectly));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            ExceptIds = [1, 2]
        };
        
        var result = await factory.GetItems(ctx => ctx.Entities, request, e => e.Id);
        
        Assert.Equal(3, result.Items.Length);
        Assert.DoesNotContain(result.Items, e => e.Id == 1);
        Assert.DoesNotContain(result.Items, e => e.Id == 2);
    }

    [Fact]
    public void GetNestedProperty_ThrowsOnInvalidProperty()
    {
        var exception = Assert.Throws<ArgumentException>(() => 
            GetItemsExtension.GetNestedProperty(typeof(TestEntity), "NonExistentProperty"));
        
        Assert.Contains("NonExistentProperty", exception.Message);
    }
}


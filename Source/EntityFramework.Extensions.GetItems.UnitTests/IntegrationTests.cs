namespace EntityFramework.Extensions.GetItems.UnitTests;

public class IntegrationTests
{
    [Fact]
    public async Task FullWorkflow_FilterSortPaginate()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(FullWorkflow_FilterSortPaginate));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            Filters = [new() { Field = TestPropertyEnum.IsActive, Operator = FilterOperatorEnum.Eq, Value = "true" }],
            Sort = [new() { Field = TestPropertyEnum.Age, Order = OrderByEnum.Descending }],
            Page = 1,
            Count = 2
        };
        
        var result = await factory.GetItems(
            ctx => ctx.Entities, 
            request, 
            e => e.Id, 
            TestHelpers.PropertyNameToString);
        
        Assert.Equal(2, result.Items.Length);
        Assert.Equal(3, result.TotalCount); // 3 active users
        Assert.Equal("Charlie", result.Items[0].Name); // Oldest active
        Assert.Equal("Diana", result.Items[1].Name);
    }

    [Fact]
    public async Task FullWorkflow_IdsWithFilter()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(FullWorkflow_IdsWithFilter));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            Ids = [1, 2, 3, 4],
            Filters = [new() { Field = TestPropertyEnum.IsActive, Operator = FilterOperatorEnum.Eq, Value = "true" }]
        };
        
        var result = await factory.GetItems(
            ctx => ctx.Entities, 
            request, 
            e => e.Id, 
            TestHelpers.PropertyNameToString);
        
        // Alice(1), Charlie(3), Diana(4) are active from the Ids list
        Assert.Equal(3, result.Items.Length);
    }

    [Fact]
    public async Task FullWorkflow_ExceptIdsWithSort()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(FullWorkflow_ExceptIdsWithSort));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            ExceptIds = [1, 5],
            Sort = [new() { Field = TestPropertyEnum.Name, Order = OrderByEnum.Ascending }]
        };
        
        var result = await factory.GetItems(
            ctx => ctx.Entities, 
            request, 
            e => e.Id, 
            TestHelpers.PropertyNameToString);
        
        Assert.Equal(3, result.Items.Length);
        // Bob, Charlie, Diana - sorted by Name ascending
        Assert.Contains(result.Items, e => e.Name == "Bob");
        Assert.Contains(result.Items, e => e.Name == "Charlie");
        Assert.Contains(result.Items, e => e.Name == "Diana");
    }

    [Fact]
    public async Task FullWorkflow_NestedFilters()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(FullWorkflow_NestedFilters));
        
        // (IsActive = true) AND ((Age < 30) OR (Score > 90))
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            Filters = 
            [
                new() 
                { 
                    Field = TestPropertyEnum.IsActive, 
                    Operator = FilterOperatorEnum.Eq, 
                    Value = "true",
                    FiltersLogic = FilterLogicEnum.And,
                    Filters = 
                    [
                        new() { Field = TestPropertyEnum.Age, Operator = FilterOperatorEnum.Lt, Value = "30" },
                        new() { Field = TestPropertyEnum.Score, Operator = FilterOperatorEnum.Gt, Value = "90", Logic = FilterLogicEnum.Or }
                    ]
                }
            ]
        };
        
        var result = await factory.GetItems(
            ctx => ctx.Entities, 
            request, 
            e => e.Id, 
            TestHelpers.PropertyNameToString);
        
        // Alice (active, age 25 < 30)
        // Diana (active, score 95 > 90)
        Assert.Equal(2, result.Items.Length);
    }

    [Fact]
    public async Task FullWorkflow_EmptyRequest_ReturnsAll()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(FullWorkflow_EmptyRequest_ReturnsAll));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>();
        
        var result = await factory.GetItems(ctx => ctx.Entities, request);
        
        Assert.Equal(5, result.Items.Length);
    }

    [Fact]
    public async Task FullWorkflow_DecimalFiltering()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(FullWorkflow_DecimalFiltering));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            Filters = [new() { Field = TestPropertyEnum.Score, Operator = FilterOperatorEnum.Gte, Value = "90" }]
        };
        
        var result = await factory.GetItems(
            ctx => ctx.Entities, 
            request, 
            e => e.Id, 
            TestHelpers.PropertyNameToString);
        
        Assert.Equal(2, result.Items.Length); // Bob (92), Diana (95)
        Assert.All(result.Items, e => Assert.True(e.Score >= 90));
    }

    [Fact]
    public async Task FullWorkflow_DateTimeFiltering()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(FullWorkflow_DateTimeFiltering));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            Filters = [new() { Field = TestPropertyEnum.CreatedAt, Operator = FilterOperatorEnum.Gte, Value = "2024-03-01" }]
        };
        
        var result = await factory.GetItems(
            ctx => ctx.Entities, 
            request, 
            e => e.Id, 
            TestHelpers.PropertyNameToString);
        
        Assert.Equal(3, result.Items.Length); // Charlie, Diana, Eve (March onwards)
    }

    [Fact]
    public async Task FullWorkflow_NotContains()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(FullWorkflow_NotContains));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            Filters = [new() { Field = TestPropertyEnum.Name, Operator = FilterOperatorEnum.NotContains, Value = "li" }]
        };
        
        var result = await factory.GetItems(
            ctx => ctx.Entities, 
            request, 
            e => e.Id, 
            TestHelpers.PropertyNameToString);
        
        // Bob, Diana, Eve don't contain 'li' (Alice and Charlie do)
        Assert.Equal(3, result.Items.Length);
        Assert.All(result.Items, e => Assert.DoesNotContain("li", e.Name));
    }

    [Fact]
    public async Task DebugQuery_WhenEnabled_ReturnsQueryString()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(DebugQuery_WhenEnabled_ReturnsQueryString));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            Filters = [new() { Field = TestPropertyEnum.IsActive, Operator = FilterOperatorEnum.Eq, Value = "true" }]
        };
        var options = new GetItemsOptions { DebugQuery = true };
        
        var result = await factory.GetItems(
            ctx => ctx.Entities, 
            request, 
            e => e.Id, 
            TestHelpers.PropertyNameToString,
            options);
        
        Assert.NotNull(result.QueryDebugView);
        Assert.NotEmpty(result.QueryDebugView);
    }

    [Fact]
    public async Task DebugQuery_WhenDisabled_ReturnsNull()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(DebugQuery_WhenDisabled_ReturnsNull));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>();
        var options = new GetItemsOptions { DebugQuery = false };
        
        var result = await factory.GetItems(ctx => ctx.Entities, request, e => e.Id, options);
        
        Assert.Null(result.QueryDebugView);
    }

    [Fact]
    public async Task DebugQuery_IQueryableExtension_ReturnsQueryString()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(DebugQuery_IQueryableExtension_ReturnsQueryString));
        await using var context = factory.CreateDbContext();
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int> { Count = 2 };
        var options = new GetItemsOptions 
        { 
            PaginationHandling = PaginationHandlingEnum.Cheap,
            DebugQuery = true 
        };
        
        var result = await context.Entities.GetItems(request, e => e.Id, options);
        
        Assert.NotNull(result.QueryDebugView);
        Assert.NotEmpty(result.QueryDebugView);
    }

    [Fact]
    public async Task DebugQuery_ExpensiveMode_ReturnsQueryString()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(DebugQuery_ExpensiveMode_ReturnsQueryString));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int> { Page = 1, Count = 2 };
        var options = new GetItemsOptions 
        { 
            PaginationHandling = PaginationHandlingEnum.Expensive,
            DebugQuery = true 
        };
        
        var result = await factory.GetItems(ctx => ctx.Entities, request, e => e.Id, options);
        
        Assert.NotNull(result.QueryDebugView);
        Assert.NotEmpty(result.QueryDebugView);
        Assert.Equal(5, result.TotalCount); // Verify expensive mode still works
    }
}


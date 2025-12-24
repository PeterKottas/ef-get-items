using System.Linq.Expressions;

namespace EntityFramework.Extensions.GetItems.UnitTests;

public class FilterTests
{
    [Fact]
    public async Task Filter_Eq_MatchesExactValue()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(Filter_Eq_MatchesExactValue));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            Filters = [new() { Field = TestPropertyEnum.Name, Operator = FilterOperatorEnum.Eq, Value = "Alice" }]
        };
        
        var result = await factory.GetItems(ctx => ctx.Entities, request, e => e.Id, TestHelpers.PropertyNameToString);
        
        Assert.Single(result.Items);
        Assert.Equal("Alice", result.Items[0].Name);
    }

    [Fact]
    public async Task Filter_Neq_ExcludesValue()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(Filter_Neq_ExcludesValue));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            Filters = [new() { Field = TestPropertyEnum.Name, Operator = FilterOperatorEnum.Neq, Value = "Alice" }]
        };
        
        var result = await factory.GetItems(ctx => ctx.Entities, request, e => e.Id, TestHelpers.PropertyNameToString);
        
        Assert.Equal(4, result.Items.Length);
        Assert.DoesNotContain(result.Items, e => e.Name == "Alice");
    }

    [Fact]
    public async Task Filter_Lt_LessThan()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(Filter_Lt_LessThan));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            Filters = [new() { Field = TestPropertyEnum.Age, Operator = FilterOperatorEnum.Lt, Value = "28" }]
        };
        
        var result = await factory.GetItems(ctx => ctx.Entities, request, e => e.Id, TestHelpers.PropertyNameToString);
        
        Assert.Equal(2, result.Items.Length);
        Assert.All(result.Items, e => Assert.True(e.Age < 28));
    }

    [Fact]
    public async Task Filter_Gte_GreaterThanOrEqual()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(Filter_Gte_GreaterThanOrEqual));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            Filters = [new() { Field = TestPropertyEnum.Age, Operator = FilterOperatorEnum.Gte, Value = "30" }]
        };
        
        var result = await factory.GetItems(ctx => ctx.Entities, request, e => e.Id, TestHelpers.PropertyNameToString);
        
        Assert.Equal(2, result.Items.Length);
        Assert.All(result.Items, e => Assert.True(e.Age >= 30));
    }

    [Fact]
    public async Task Filter_Contains_StringSubstring()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(Filter_Contains_StringSubstring));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            Filters = [new() { Field = TestPropertyEnum.Name, Operator = FilterOperatorEnum.Contains, Value = "li" }]
        };
        
        var result = await factory.GetItems(ctx => ctx.Entities, request, e => e.Id, TestHelpers.PropertyNameToString);
        
        Assert.Equal(2, result.Items.Length); // Alice, Charlie
    }

    [Fact]
    public async Task Filter_Contains_WithValues_InList()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(Filter_Contains_WithValues_InList));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            Filters = [new() { Field = TestPropertyEnum.Age, Operator = FilterOperatorEnum.Contains, Values = ["25", "30", "35"] }]
        };
        
        var result = await factory.GetItems(ctx => ctx.Entities, request, e => e.Id, TestHelpers.PropertyNameToString);
        
        Assert.Equal(3, result.Items.Length);
    }

    [Fact]
    public async Task Filter_StartsWith()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(Filter_StartsWith));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            Filters = [new() { Field = TestPropertyEnum.Name, Operator = FilterOperatorEnum.StartsWith, Value = "A" }]
        };
        
        var result = await factory.GetItems(ctx => ctx.Entities, request, e => e.Id, TestHelpers.PropertyNameToString);
        
        Assert.Single(result.Items);
        Assert.Equal("Alice", result.Items[0].Name);
    }

    [Fact]
    public async Task Filter_EndsWith()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(Filter_EndsWith));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            Filters = [new() { Field = TestPropertyEnum.Name, Operator = FilterOperatorEnum.EndsWith, Value = "e" }]
        };
        
        var result = await factory.GetItems(ctx => ctx.Entities, request, e => e.Id, TestHelpers.PropertyNameToString);
        
        Assert.Equal(3, result.Items.Length); // Alice, Charlie, Eve
    }

    [Fact]
    public async Task Filter_Boolean_True()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(Filter_Boolean_True));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            Filters = [new() { Field = TestPropertyEnum.IsActive, Operator = FilterOperatorEnum.Eq, Value = "true" }]
        };
        
        var result = await factory.GetItems(ctx => ctx.Entities, request, e => e.Id, TestHelpers.PropertyNameToString);
        
        Assert.Equal(3, result.Items.Length);
        Assert.All(result.Items, e => Assert.True(e.IsActive));
    }

    [Fact]
    public async Task Filter_MultipleFilters_AndLogic()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(Filter_MultipleFilters_AndLogic));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            Filters = 
            [
                new() { Field = TestPropertyEnum.IsActive, Operator = FilterOperatorEnum.Eq, Value = "true" },
                new() { Field = TestPropertyEnum.Age, Operator = FilterOperatorEnum.Gte, Value = "30", Logic = FilterLogicEnum.And }
            ]
        };
        
        var result = await factory.GetItems(ctx => ctx.Entities, request, e => e.Id, TestHelpers.PropertyNameToString);
        
        Assert.Single(result.Items);
        Assert.Equal("Charlie", result.Items[0].Name);
    }

    [Fact]
    public async Task Filter_MultipleFilters_OrLogic()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(Filter_MultipleFilters_OrLogic));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            Filters = 
            [
                new() { Field = TestPropertyEnum.Name, Operator = FilterOperatorEnum.Eq, Value = "Alice" },
                new() { Field = TestPropertyEnum.Name, Operator = FilterOperatorEnum.Eq, Value = "Bob", Logic = FilterLogicEnum.Or }
            ]
        };
        
        var result = await factory.GetItems(ctx => ctx.Entities, request, e => e.Id, TestHelpers.PropertyNameToString);
        
        Assert.Equal(2, result.Items.Length);
    }

    [Fact]
    public async Task Filter_Flag_MatchesSingleFlag()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(Filter_Flag_MatchesSingleFlag));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            Filters = [new() { Field = TestPropertyEnum.Category, Operator = FilterOperatorEnum.Flag, Value = "1" }] // CategoryA
        };
        
        var result = await factory.GetItems(ctx => ctx.Entities, request, e => e.Id, TestHelpers.PropertyNameToString);
        
        Assert.Equal(2, result.Items.Length); // Alice (A), Charlie (A|B)
    }

    [Fact]
    public async Task Filter_AnyFlag_MatchesAnyOfFlags()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(Filter_AnyFlag_MatchesAnyOfFlags));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            Filters = [new() { Field = TestPropertyEnum.Category, Operator = FilterOperatorEnum.AnyFlag, Value = "3" }] // CategoryA | CategoryB
        };
        
        var result = await factory.GetItems(ctx => ctx.Entities, request, e => e.Id, TestHelpers.PropertyNameToString);
        
        Assert.Equal(3, result.Items.Length); // Alice, Bob, Charlie
    }

    [Fact]
    public void GetFiltersExpression_EmptyFilters_ReturnsTrue()
    {
        var param = Expression.Parameter(typeof(TestEntity), "e");
        var result = GetItemsExtension.GetFiltersExpression<TestPropertyEnum>(param, null, null, GetItemsOptions.Default);
        
        Assert.Equal(ExpressionType.Constant, result.NodeType);
        Assert.Equal(true, ((ConstantExpression)result).Value);
    }
}


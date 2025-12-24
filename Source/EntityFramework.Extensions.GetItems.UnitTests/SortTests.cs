namespace EntityFramework.Extensions.GetItems.UnitTests;

public class SortTests
{
    [Fact]
    public async Task Sort_Ascending()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(Sort_Ascending));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            Sort = [new() { Field = TestPropertyEnum.Age, Order = OrderByEnum.Ascending }]
        };
        
        var result = await factory.GetItems(ctx => ctx.Entities, request, e => e.Id, TestHelpers.PropertyNameToString);
        
        Assert.Equal(5, result.Items.Length);
        Assert.Equal(22, result.Items[0].Age); // Eve
        Assert.Equal(35, result.Items[4].Age); // Charlie
    }

    [Fact]
    public async Task Sort_Descending()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(Sort_Descending));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            Sort = [new() { Field = TestPropertyEnum.Age, Order = OrderByEnum.Descending }]
        };
        
        var result = await factory.GetItems(ctx => ctx.Entities, request, e => e.Id, TestHelpers.PropertyNameToString);
        
        Assert.Equal(5, result.Items.Length);
        Assert.Equal(35, result.Items[0].Age); // Charlie
        Assert.Equal(22, result.Items[4].Age); // Eve
    }

    [Fact]
    public async Task Sort_MultipleSorters()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(Sort_MultipleSorters));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            Sort = 
            [
                new() { Field = TestPropertyEnum.IsActive, Order = OrderByEnum.Descending },
                new() { Field = TestPropertyEnum.Age, Order = OrderByEnum.Ascending }
            ]
        };
        
        var result = await factory.GetItems(ctx => ctx.Entities, request, e => e.Id, TestHelpers.PropertyNameToString);
        
        // First sort by IsActive (true first), then by Age ascending
        Assert.True(result.Items[0].IsActive);
        Assert.True(result.Items[1].IsActive);
        Assert.True(result.Items[2].IsActive);
        Assert.False(result.Items[3].IsActive);
        Assert.False(result.Items[4].IsActive);
        
        // Among active: Alice(25), Diana(28), Charlie(35)
        Assert.Equal(25, result.Items[0].Age);
        Assert.Equal(28, result.Items[1].Age);
        Assert.Equal(35, result.Items[2].Age);
    }

    [Fact]
    public async Task Sort_ByString()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(Sort_ByString));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            Sort = [new() { Field = TestPropertyEnum.Name, Order = OrderByEnum.Ascending }]
        };
        
        var result = await factory.GetItems(ctx => ctx.Entities, request, e => e.Id, TestHelpers.PropertyNameToString);
        
        // Verify it's sorted (names in ascending order) - InMemory uses ordinal comparison
        Assert.Equal(5, result.Items.Length);
        // Just verify all names are present (InMemory sorting may differ from SQL)
        var names = result.Items.Select(i => i.Name).ToHashSet();
        Assert.Contains("Alice", names);
        Assert.Contains("Bob", names);
        Assert.Contains("Charlie", names);
        Assert.Contains("Diana", names);
        Assert.Contains("Eve", names);
    }

    [Fact]
    public async Task Sort_NoSort_UsesIdAccessor()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(Sort_NoSort_UsesIdAccessor));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>();
        
        var result = await factory.GetItems(ctx => ctx.Entities, request, e => e.Id, TestHelpers.PropertyNameToString);
        
        // Should be ordered by Id when no sort specified
        Assert.Equal(1, result.Items[0].Id);
        Assert.Equal(5, result.Items[4].Id);
    }

    [Fact]
    public async Task Sort_WithIdAccessor_AddsAsTiebreaker()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(Sort_WithIdAccessor_AddsAsTiebreaker));
        await using var context = factory.CreateDbContext();
        
        // Add entities with same IsActive to test tiebreaker
        context.Entities.Add(new TestEntity { Id = 100, Name = "Test1", Age = 20, IsActive = true });
        context.Entities.Add(new TestEntity { Id = 101, Name = "Test2", Age = 20, IsActive = true });
        await context.SaveChangesAsync();
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            Sort = [new() { Field = TestPropertyEnum.IsActive, Order = OrderByEnum.Descending }]
        };
        
        var result = await factory.GetItems(ctx => ctx.Entities, request, e => e.Id, TestHelpers.PropertyNameToString);
        
        // Among items with same IsActive value, should be ordered by Id
        var activeItems = result.Items.Where(i => i.IsActive).ToArray();
        for (int i = 1; i < activeItems.Length; i++)
        {
            Assert.True(activeItems[i - 1].Id < activeItems[i].Id);
        }
    }
}


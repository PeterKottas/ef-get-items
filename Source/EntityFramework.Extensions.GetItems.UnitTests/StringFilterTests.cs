namespace EntityFramework.Extensions.GetItems.UnitTests;

public class StringFilterTests : IDisposable
{
    public StringFilterTests()
    {
        // Reset to default before each test
        GetItemsOptions.ResetDefault();
    }
    
    public void Dispose()
    {
        // Reset after each test to not affect other tests
        GetItemsOptions.ResetDefault();
    }

    [Fact]
    public void ConfigureDefault_SetsGlobalOptions()
    {
        GetItemsOptions.ConfigureDefault(options =>
        {
            options.DbProvider = DbProviderEnum.SqlServer;
            options.PaginationHandling = PaginationHandlingEnum.Expensive;
        });
        
        var defaults = GetItemsOptions.Default;
        
        Assert.Equal(DbProviderEnum.SqlServer, defaults.DbProvider);
        Assert.Equal(PaginationHandlingEnum.Expensive, defaults.PaginationHandling);
    }

    [Fact]
    public void ResetDefault_ClearsGlobalOptions()
    {
        GetItemsOptions.ConfigureDefault(options =>
        {
            options.DbProvider = DbProviderEnum.SqlServer;
        });
        
        GetItemsOptions.ResetDefault();
        
        var defaults = GetItemsOptions.Default;
        
        // After reset, defaults go back to initial values
        Assert.Equal(DbProviderEnum.PostgreSql, defaults.DbProvider); // Default is PostgreSql
    }

    [Fact]
    public void Default_WithoutConfigure_ReturnsNewInstance()
    {
        var options1 = GetItemsOptions.Default;
        var options2 = GetItemsOptions.Default;
        
        // Without ConfigureDefault, each call returns a new instance
        Assert.NotSame(options1, options2);
    }

    [Fact]
    public void Default_WithConfigure_ReturnsSameInstance()
    {
        GetItemsOptions.ConfigureDefault(options =>
        {
            options.DebugQuery = true;
        });
        
        var options1 = GetItemsOptions.Default;
        var options2 = GetItemsOptions.Default;
        
        // With ConfigureDefault, returns the same configured instance
        Assert.Same(options1, options2);
    }

    [Fact]
    public void DbProvider_DefaultValue_IsPostgreSql()
    {
        var options = new GetItemsOptions();
        Assert.Equal(DbProviderEnum.PostgreSql, options.DbProvider);
    }

    [Fact]
    public void DbProvider_CanBeSetToSqlServer()
    {
        var options = new GetItemsOptions { DbProvider = DbProviderEnum.SqlServer };
        Assert.Equal(DbProviderEnum.SqlServer, options.DbProvider);
    }

    [Fact]
    public void DbProvider_CanBeSetToInMemory()
    {
        var options = new GetItemsOptions { DbProvider = DbProviderEnum.InMemory };
        Assert.Equal(DbProviderEnum.InMemory, options.DbProvider);
    }

    // Case-sensitive string operators (existing behavior)
    
    [Fact]
    public async Task Filter_Contains_CaseSensitive()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(Filter_Contains_CaseSensitive));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            Filters = [new() { Field = TestPropertyEnum.Name, Operator = FilterOperatorEnum.Contains, Value = "LI" }]
        };
        
        // Case-sensitive: "LI" should not match "Alice" or "Charlie"
        var result = await factory.GetItems(ctx => ctx.Entities, request, e => e.Id, TestHelpers.PropertyNameToString);
        
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task Filter_StartsWith_CaseSensitive()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(Filter_StartsWith_CaseSensitive));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            Filters = [new() { Field = TestPropertyEnum.Name, Operator = FilterOperatorEnum.StartsWith, Value = "a" }]
        };
        
        // Case-sensitive: "a" should not match "Alice"
        var result = await factory.GetItems(ctx => ctx.Entities, request, e => e.Id, TestHelpers.PropertyNameToString);
        
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task Filter_EndsWith_CaseSensitive()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(Filter_EndsWith_CaseSensitive));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            Filters = [new() { Field = TestPropertyEnum.Name, Operator = FilterOperatorEnum.EndsWith, Value = "E" }]
        };
        
        // Case-sensitive: "E" should not match "Alice", "Charlie", "Eve"
        var result = await factory.GetItems(ctx => ctx.Entities, request, e => e.Id, TestHelpers.PropertyNameToString);
        
        Assert.Empty(result.Items);
    }

    // Case-insensitive string operators (new operators)
    
    [Fact]
    public async Task Filter_IContains_CaseInsensitive()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(Filter_IContains_CaseInsensitive));
        
        var options = new GetItemsOptions { DbProvider = DbProviderEnum.InMemory };
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            Filters = [new() { Field = TestPropertyEnum.Name, Operator = FilterOperatorEnum.IContains, Value = "LI" }]
        };
        
        // Case-insensitive: "LI" should match "Alice" and "Charlie"
        var result = await factory.GetItems(ctx => ctx.Entities, request, e => e.Id, TestHelpers.PropertyNameToString, options);
        
        Assert.Equal(2, result.Items.Length);
        Assert.Contains(result.Items, e => e.Name == "Alice");
        Assert.Contains(result.Items, e => e.Name == "Charlie");
    }

    [Fact]
    public async Task Filter_IStartsWith_CaseInsensitive()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(Filter_IStartsWith_CaseInsensitive));
        
        var options = new GetItemsOptions { DbProvider = DbProviderEnum.InMemory };
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            Filters = [new() { Field = TestPropertyEnum.Name, Operator = FilterOperatorEnum.IStartsWith, Value = "a" }]
        };
        
        // Case-insensitive: "a" should match "Alice"
        var result = await factory.GetItems(ctx => ctx.Entities, request, e => e.Id, TestHelpers.PropertyNameToString, options);
        
        Assert.Single(result.Items);
        Assert.Equal("Alice", result.Items[0].Name);
    }

    [Fact]
    public async Task Filter_IEndsWith_CaseInsensitive()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(Filter_IEndsWith_CaseInsensitive));
        
        var options = new GetItemsOptions { DbProvider = DbProviderEnum.InMemory };
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            Filters = [new() { Field = TestPropertyEnum.Name, Operator = FilterOperatorEnum.IEndsWith, Value = "E" }]
        };
        
        // Case-insensitive: "E" should match "Alice", "Charlie", "Eve"
        var result = await factory.GetItems(ctx => ctx.Entities, request, e => e.Id, TestHelpers.PropertyNameToString, options);
        
        Assert.Equal(3, result.Items.Length);
    }

    [Fact]
    public async Task Filter_INotContains_CaseInsensitive()
    {
        var factory = await TestHelpers.CreateSeededFactory(nameof(Filter_INotContains_CaseInsensitive));
        
        var options = new GetItemsOptions { DbProvider = DbProviderEnum.InMemory };
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            Filters = [new() { Field = TestPropertyEnum.Name, Operator = FilterOperatorEnum.INotContains, Value = "LI" }]
        };
        
        // Case-insensitive INotContains: should exclude "Alice" and "Charlie"
        var result = await factory.GetItems(ctx => ctx.Entities, request, e => e.Id, TestHelpers.PropertyNameToString, options);
        
        Assert.Equal(3, result.Items.Length);
        Assert.DoesNotContain(result.Items, e => e.Name == "Alice");
        Assert.DoesNotContain(result.Items, e => e.Name == "Charlie");
    }

    [Fact]
    public async Task Filter_IContains_UsesGlobalDbProvider()
    {
        GetItemsOptions.ConfigureDefault(options =>
        {
            options.DbProvider = DbProviderEnum.InMemory;
        });
        
        var factory = await TestHelpers.CreateSeededFactory(nameof(Filter_IContains_UsesGlobalDbProvider));
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            Filters = [new() { Field = TestPropertyEnum.Name, Operator = FilterOperatorEnum.IContains, Value = "LI" }]
        };
        
        // No options passed - should use global config for DbProvider
        var result = await factory.GetItems(ctx => ctx.Entities, request, e => e.Id, TestHelpers.PropertyNameToString);
        
        Assert.Equal(2, result.Items.Length);
    }

    [Fact]
    public async Task Filter_IContains_PostgreSqlProvider_ThrowsWithoutNpgsql()
    {
        // Test that PostgreSQL provider throws when Npgsql is not installed
        var factory = await TestHelpers.CreateSeededFactory(nameof(Filter_IContains_PostgreSqlProvider_ThrowsWithoutNpgsql));
        
        var options = new GetItemsOptions { DbProvider = DbProviderEnum.PostgreSql };
        
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            Filters = [new() { Field = TestPropertyEnum.Name, Operator = FilterOperatorEnum.IContains, Value = "ali" }]
        };
        
        // Without Npgsql package installed, should throw with helpful message
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await factory.GetItems(ctx => ctx.Entities, request, e => e.Id, TestHelpers.PropertyNameToString, options));
        
        Assert.Contains("Npgsql.EntityFrameworkCore.PostgreSQL", exception.Message);
        Assert.Contains("ILike", exception.Message);
    }

    [Fact]
    public async Task Filter_IContains_WithSpecialCharacters_DoesNotThrow()
    {
        var factory = new TestDbContextFactory(nameof(Filter_IContains_WithSpecialCharacters_DoesNotThrow));
        await using var context = factory.CreateDbContext();
        context.Entities.AddRange(
        [
            new TestEntity { Id = 1, Name = "Test%Name", Age = 25, IsActive = true, CreatedAt = DateTime.Now, Score = 80m },
            new TestEntity { Id = 2, Name = "Test_Name", Age = 30, IsActive = true, CreatedAt = DateTime.Now, Score = 85m },
            new TestEntity { Id = 3, Name = "TestName", Age = 35, IsActive = true, CreatedAt = DateTime.Now, Score = 90m }
        ]);
        await context.SaveChangesAsync();
        
        var options = new GetItemsOptions { DbProvider = DbProviderEnum.InMemory };
        
        // Search for literal "%" - verify the query executes without error
        var request = new BaseGetItemsRequest<TestPropertyEnum, int>
        {
            Filters = [new() { Field = TestPropertyEnum.Name, Operator = FilterOperatorEnum.IContains, Value = "%" }]
        };
        
        var result = await factory.GetItems(ctx => ctx.Entities, request, e => e.Id, TestHelpers.PropertyNameToString, options);
        
        // Just verify query executes - escaping behavior is DB-specific
        Assert.NotNull(result);
    }
}

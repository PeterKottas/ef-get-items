using Microsoft.EntityFrameworkCore;

namespace EntityFramework.Extensions.GetItems.UnitTests;

public enum TestPropertyEnum
{
    Id,
    Name,
    Age,
    IsActive,
    CreatedAt,
    Score,
    Category
}

public class TestEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal Score { get; set; }
    public TestCategoryEnum Category { get; set; }
    public int? NullableValue { get; set; }
}

[Flags]
public enum TestCategoryEnum
{
    None = 0,
    CategoryA = 1,
    CategoryB = 2,
    CategoryC = 4
}

public class TestDbContext : DbContext
{
    public DbSet<TestEntity> Entities { get; set; } = null!;

    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
}

public class TestDbContextFactory : IDbContextFactory<TestDbContext>
{
    private readonly DbContextOptions<TestDbContext> _options;

    public TestDbContextFactory(string databaseName)
    {
        _options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;
    }

    public TestDbContext CreateDbContext() => new(_options);
}

public static class TestHelpers
{
    public static string[] PropertyNameToString(TestPropertyEnum field) => field switch
    {
        TestPropertyEnum.Id => ["Id"],
        TestPropertyEnum.Name => ["Name"],
        TestPropertyEnum.Age => ["Age"],
        TestPropertyEnum.IsActive => ["IsActive"],
        TestPropertyEnum.CreatedAt => ["CreatedAt"],
        TestPropertyEnum.Score => ["Score"],
        TestPropertyEnum.Category => ["Category"],
        _ => [field.ToString()]
    };

    public static List<TestEntity> GetTestData() =>
    [
        new() { Id = 1, Name = "Alice", Age = 25, IsActive = true, CreatedAt = new DateTime(2024, 1, 1), Score = 85.5m, Category = TestCategoryEnum.CategoryA },
        new() { Id = 2, Name = "Bob", Age = 30, IsActive = false, CreatedAt = new DateTime(2024, 2, 1), Score = 92.0m, Category = TestCategoryEnum.CategoryB },
        new() { Id = 3, Name = "Charlie", Age = 35, IsActive = true, CreatedAt = new DateTime(2024, 3, 1), Score = 78.5m, Category = TestCategoryEnum.CategoryA | TestCategoryEnum.CategoryB },
        new() { Id = 4, Name = "Diana", Age = 28, IsActive = true, CreatedAt = new DateTime(2024, 4, 1), Score = 95.0m, Category = TestCategoryEnum.CategoryC },
        new() { Id = 5, Name = "Eve", Age = 22, IsActive = false, CreatedAt = new DateTime(2024, 5, 1), Score = 88.0m, Category = TestCategoryEnum.None }
    ];

    public static async Task<TestDbContextFactory> CreateSeededFactory(string dbName)
    {
        var factory = new TestDbContextFactory(dbName);
        await using var context = factory.CreateDbContext();
        context.Entities.AddRange(GetTestData());
        await context.SaveChangesAsync();
        return factory;
    }
}


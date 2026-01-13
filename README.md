# PK.EntityFramework.Extensions.GetItems

A simple, expressive way to paginate, filter, and sort data in Entity Framework. Works great with APIs and directly in your business logic.

## Installation

```bash
dotnet add package PK.EntityFramework.Extensions.GetItems
```

## Quick Start

```csharp
// Define your filterable properties
public enum BookPropertyNames { Name, PublishedOn, AuthorName }

// Map enum values to property paths
static string[] PropertyNameMapper(BookPropertyNames field) => field switch
{
    BookPropertyNames.Name => [nameof(Book.Name)],
    BookPropertyNames.PublishedOn => [nameof(Book.PublishedOn)],
    BookPropertyNames.AuthorName => [nameof(Book.Author), nameof(Author.Name)],
    _ => throw new ArgumentOutOfRangeException()
};

// Create a request
var request = new BaseGetItemsRequest<BookPropertyNames, Guid>
{
    Page = 1,
    Count = 10,
    Filters = [
        new() { Field = BookPropertyNames.Name, Operator = FilterOperatorEnum.Contains, Value = "Great" }
    ],
    Sort = [
        new() { Field = BookPropertyNames.PublishedOn, Order = OrderByEnum.Descending }
    ]
};

// Execute with IDbContextFactory
var result = await dbContextFactory.GetItems(
    ctx => ctx.Books.AsQueryable(),
    request,
    b => b.Id,
    PropertyNameMapper
);

// Or directly on IQueryable (Cheap/None pagination modes only)
var result = await dbContext.Books.GetItems(request, b => b.Id, PropertyNameMapper);

// result.Items, result.TotalCount, result.Page, result.TotalPages
```

## Features

- **Pagination** — Three modes: Expensive (full count), Cheap (has next page), None
- **Filtering** — Rich operators with AND/OR logic and nested filters
- **Case-insensitive filtering** — Dedicated operators using LIKE/ILIKE for SQL Server and PostgreSQL
- **Sorting** — Multi-level sorting on any property
- **ID filtering** — Include/exclude specific IDs with `Ids` and `ExceptIds`
- **Nested properties** — Filter/sort through relationships
- **Type-safe** — Enum-based property names, compile-time checking
- **Query debugging** — Inspect generated expression trees

## Pagination Modes

```csharp
var options = new GetItemsOptions
{
    PaginationHandling = PaginationHandlingEnum.Cheap // default
};
```

| Mode        | Description                          | Returns                    |
| ----------- | ------------------------------------ | -------------------------- |
| `Cheap`     | Fetches N+1 items to check next page | `HasNextPage` (default)    |
| `Expensive` | Runs parallel COUNT query            | `TotalCount`, `TotalPages` |
| `None`      | No pagination metadata               | Items only                 |

> **Note:** `Expensive` mode requires `IDbContextFactory`. Use `Cheap` or `None` with direct `IQueryable` extensions.

## Filter Operators

| Operator                                   | Description                                            |
| ------------------------------------------ | ------------------------------------------------------ |
| `Eq`, `Neq`                                | Equal / Not equal                                      |
| `Lt`, `Lte`, `Gt`, `Gte`                   | Comparison operators                                   |
| `StartsWith`, `EndsWith`, `Contains`       | String matching (case-sensitive)                       |
| `IStartsWith`, `IEndsWith`, `IContains`    | String matching (case-insensitive, uses LIKE/ILIKE)    |
| `NotContains`, `INotContains`              | Does not contain (case-sensitive / case-insensitive)   |
| `ContainsAll`, `NotContainsAll`            | Collection operations                                  |
| `Flag`, `AnyFlag`, `NotFlag`, `NotAnyFlag` | Bitwise flag operations                                |

## Request Options

```csharp
var request = new BaseGetItemsRequest<PropertyEnum, Guid>
{
    // Pagination
    Page = 1,
    Count = 25,
    Skip = 0,              // Additional skip offset

    // ID filtering
    Ids = [guid1, guid2],       // Include only these IDs
    ExceptIds = [guid3],        // Exclude these IDs

    // Filters and sorting
    Filters = [...],
    Sort = [...],

    // Optimization: pass cached total to skip COUNT query
    TotalCount = cachedTotal
};
```

## Property Mapping

Map enum values to property paths, including nested properties:

```csharp
public static string[] PropertyNameMapper(BookPropertyNames field) => field switch
{
    BookPropertyNames.Name => [nameof(Book.Name)],
    BookPropertyNames.PublishedOn => [nameof(Book.PublishedOn)],
    BookPropertyNames.AuthorName => [nameof(Book.Author), nameof(Author.Name)],
    _ => throw new ArgumentOutOfRangeException()
};
```

## Response

```csharp
public class PaginatedData<T>
{
    public T[] Items { get; }
    public int Page { get; }
    public int Count { get; }
    public long? TotalCount { get; }      // Expensive mode only
    public int? TotalPages { get; }       // Expensive mode only
    public bool? HasNextPage { get; }     // Cheap mode only
    public string? QueryDebugView { get; } // When DebugQuery = true
}
```

## Query Debugging

Enable debug mode to inspect the generated expression:

```csharp
var options = new GetItemsOptions { DebugQuery = true };
var result = await factory.GetItems(ctx => ctx.Books, request, b => b.Id, mapper, options);

Console.WriteLine(result.QueryDebugView);
// Output:
// DbSet<Book>()
//     .Where(b => b.Name.Contains("Great"))
//     .OrderByDescending(b => b.PublishedOn)
```

## Case-Insensitive String Filtering

Use the `I`-prefixed operators (`IStartsWith`, `IEndsWith`, `IContains`, `INotContains`) for case-insensitive string matching:

```csharp
var request = new BaseGetItemsRequest<BookPropertyNames, Guid>
{
    Filters = [
        new() { Field = BookPropertyNames.Name, Operator = FilterOperatorEnum.IContains, Value = "great" }
    ]
};

var result = await factory.GetItems(ctx => ctx.Books, request, b => b.Id, mapper);
```

The `DbProvider` option controls which SQL function is used:

| Provider     | Function | Behavior                                      |
| ------------ | -------- | --------------------------------------------- |
| `PostgreSql` | `ILIKE`  | Explicitly case-insensitive (default)         |
| `SqlServer`  | `LIKE`   | Case-insensitive (depends on collation)       |
| `InMemory`   | `LIKE`   | For testing purposes                          |

```csharp
// Configure DbProvider globally if not using PostgreSQL
GetItemsOptions.ConfigureDefault(options =>
{
    options.DbProvider = DbProviderEnum.SqlServer;
});
```

> **Note:** PostgreSQL requires `Npgsql.EntityFrameworkCore.PostgreSQL` package. An exception is thrown if the package is not installed.

> **Note:** Special characters (`%`, `_`, `[`) in filter values are automatically escaped.

## Global Configuration

Configure default options once at application startup:

```csharp
// In Program.cs or startup
GetItemsOptions.ConfigureDefault(options =>
{
    options.PaginationHandling = PaginationHandlingEnum.Cheap;
    options.DbProvider = DbProviderEnum.SqlServer; // Default is PostgreSql
});

// Reset to default behavior if needed
GetItemsOptions.ResetDefault();
```

## Example Project

Check out `EntityFramework.Extensions.GetItems.Example` for a complete working API.

```bash
cd Source/EntityFramework.Extensions.GetItems.Example
dotnet run
# Open http://localhost:5000/scalar/v1
```

## License

MIT

namespace EntityFramework.Extensions.GetItems;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

/// <summary>
/// Represents paginated data returned from a GetItems query.
/// </summary>
/// <typeparam name="TEntity">The type of entities in the result set.</typeparam>
public class PaginatedData<TEntity>
{
    /// <summary>
    /// The array of items for the current page.
    /// </summary>
    public TEntity[] Items { get; }
    
    /// <summary>
    /// The current page number (1-based).
    /// </summary>
    public int Page { get; }
    
    /// <summary>
    /// The number of items requested per page.
    /// </summary>
    public int Count { get; }
    
    /// <summary>
    /// The total count of items matching the query. Only populated when using <see cref="PaginationHandlingEnum.Expensive"/>.
    /// </summary>
    public long? TotalCount { get; }
    
    /// <summary>
    /// The total number of pages. Only populated when <see cref="TotalCount"/> is available.
    /// </summary>
    public int? TotalPages => TotalCount.HasValue ? (int)Math.Ceiling((double)TotalCount.Value / Count) : null;
    
    /// <summary>
    /// Indicates whether there is a next page. Only populated when using <see cref="PaginationHandlingEnum.Cheap"/>.
    /// </summary>
    public bool? HasNextPage { get; }
    
    /// <summary>
    /// Debug view of the generated query expression. Only populated when <see cref="GetItemsOptions.DebugQuery"/> is true.
    /// </summary>
    public string? QueryDebugView { get; }

    /// <summary>
    /// Creates a new instance of <see cref="PaginatedData{TEntity}"/>.
    /// </summary>
    /// <param name="items">The array of items for the current page.</param>
    /// <param name="page">The current page number.</param>
    /// <param name="count">The number of items per page.</param>
    /// <param name="totalCount">The total count of matching items (optional).</param>
    /// <param name="hasNextPage">Whether there is a next page (optional).</param>
    /// <param name="queryDebugView">Debug view of the query expression (optional).</param>
    public PaginatedData(TEntity[] items, int page, int count, long? totalCount = null, bool? hasNextPage = null, string? queryDebugView = null)
    {
        Items = items;
        Page = page;
        Count = count;
        TotalCount = totalCount;
        HasNextPage = hasNextPage;
        QueryDebugView = queryDebugView;
    }
}

/// <summary>
/// Configuration options for GetItems queries.
/// </summary>
public class GetItemsOptions
{
    private static GetItemsOptions? _globalDefault;
    private static readonly object _lock = new();
    
    /// <summary>
    /// Specifies how pagination metadata should be calculated. Default is <see cref="PaginationHandlingEnum.Cheap"/>.
    /// </summary>
    public PaginationHandlingEnum PaginationHandling { get; set; } = PaginationHandlingEnum.Cheap;
    
    /// <summary>
    /// Custom expression transformations for properties marked with <see cref="LinqExpressionAttribute"/>.
    /// The key is the expression name, and the value is a function that transforms the property access expression.
    /// </summary>
    public Dictionary<string, Func<Expression, Expression>> Expressions { get; set; } = new();
    
    /// <summary>
    /// When true, captures the query expression and includes it in <see cref="PaginatedData{TEntity}.QueryDebugView"/>.
    /// Useful for debugging and verifying generated queries.
    /// </summary>
    public bool DebugQuery { get; set; }
    
    /// <summary>
    /// Specifies the database provider for provider-specific query optimizations.
    /// Used by case-insensitive operators (IStartsWith, IEndsWith, IContains, INotContains).
    /// Default is <see cref="DbProviderEnum.PostgreSql"/>.
    /// </summary>
    public DbProviderEnum DbProvider { get; set; } = DbProviderEnum.PostgreSql;
    
    /// <summary>
    /// Gets the default instance of <see cref="GetItemsOptions"/>.
    /// Returns the globally configured default if <see cref="ConfigureDefault"/> was called, otherwise a new instance.
    /// </summary>
    public static GetItemsOptions Default => _globalDefault ?? new GetItemsOptions();
    
    /// <summary>
    /// Configures the global default options. Call this once at application startup to set defaults for all GetItems calls.
    /// </summary>
    /// <param name="configure">An action to configure the default options.</param>
    /// <example>
    /// <code>
    /// GetItemsOptions.ConfigureDefault(options =>
    /// {
    ///     options.DbProvider = DbProviderEnum.SqlServer;
    ///     options.PaginationHandling = PaginationHandlingEnum.Cheap;
    /// });
    /// </code>
    /// </example>
    public static void ConfigureDefault(Action<GetItemsOptions> configure)
    {
        lock (_lock)
        {
            _globalDefault = new GetItemsOptions();
            configure(_globalDefault);
        }
    }
    
    /// <summary>
    /// Resets the global default options to null, causing <see cref="Default"/> to return a new instance.
    /// </summary>
    public static void ResetDefault()
    {
        lock (_lock)
        {
            _globalDefault = null;
        }
    }
}

/// <summary>
/// Base request class for GetItems queries containing pagination, filtering, and sorting parameters.
/// </summary>
/// <typeparam name="TPropertyNameEnum">An enum type representing the filterable/sortable property names.</typeparam>
/// <typeparam name="TId">The type of the entity's primary key.</typeparam>
public partial class BaseGetItemsRequest<TPropertyNameEnum, TId>
    where TPropertyNameEnum : struct, IConvertible
{
    /// <summary>
    /// Filter to only include entities with these IDs.
    /// </summary>
    public TId[]? Ids { get; set; }
    
    /// <summary>
    /// Exclude entities with these IDs from the results.
    /// </summary>
    public TId[]? ExceptIds { get; set; }
    
    /// <summary>
    /// The page number to retrieve (1-based). Default is 1.
    /// </summary>
    public int? Page { get; set; }
    
    /// <summary>
    /// The number of items per page. Default is 25.
    /// </summary>
    public int? Count { get; set; }
    
    /// <summary>
    /// Additional items to skip after page calculation. Default is 0.
    /// </summary>
    public int? Skip { get; set; }
    
    /// <summary>
    /// Array of filters to apply to the query.
    /// </summary>
    public GetItemsFilter<TPropertyNameEnum>[]? Filters { get; set; }
    
    /// <summary>
    /// Array of sorters to apply to the query.
    /// </summary>
    public GetItemsSorter<TPropertyNameEnum>[]? Sort { get; set; }
    
    /// <summary>
    /// Optional pre-computed total count. If provided, skips the count query in <see cref="PaginationHandlingEnum.Expensive"/> mode.
    /// </summary>
    public long? TotalCount { get; set; }
}

/// <summary>
/// Represents a filter condition for GetItems queries.
/// </summary>
/// <typeparam name="TPropertyNameEnum">An enum type representing the filterable property names.</typeparam>
public class GetItemsFilter<TPropertyNameEnum>
    where TPropertyNameEnum : struct, IConvertible
{
    /// <summary>
    /// The property to filter on. Can be null for nested filter groups.
    /// </summary>
    public TPropertyNameEnum? Field { get; set; }
    
    /// <summary>
    /// The comparison operator to use.
    /// </summary>
    public FilterOperatorEnum Operator { get; set; }
    
    /// <summary>
    /// The value to compare against. Used for single-value comparisons.
    /// </summary>
    public string? Value { get; set; }
    
    /// <summary>
    /// Multiple values to compare against. Used for Contains, ContainsAll, and similar operators.
    /// </summary>
    public string[]? Values { get; set; }
    
    /// <summary>
    /// How this filter combines with the previous filter. Default is <see cref="FilterLogicEnum.And"/>.
    /// </summary>
    public FilterLogicEnum Logic { get; set; }
    
    /// <summary>
    /// How nested <see cref="Filters"/> combine with this filter's condition. Default is <see cref="FilterLogicEnum.And"/>.
    /// </summary>
    public FilterLogicEnum FiltersLogic { get; set; } = FilterLogicEnum.And;
    
    /// <summary>
    /// For collection properties, specifies whether Any or All items must match. Default is <see cref="FilterArrayAccessorLogic.Any"/>.
    /// </summary>
    public FilterArrayAccessorLogic ArrayAccessorLogic { get; set; } = FilterArrayAccessorLogic.Any;
    
    /// <summary>
    /// Nested filters for creating complex filter groups.
    /// </summary>
    public GetItemsFilter<TPropertyNameEnum>[]? Filters { get; set; }
}

/// <summary>
/// Represents a sort condition for GetItems queries.
/// </summary>
/// <typeparam name="TPropertyNameEnum">An enum type representing the sortable property names.</typeparam>
public class GetItemsSorter<TPropertyNameEnum>
    where TPropertyNameEnum : struct, IConvertible
{
    /// <summary>
    /// The property to sort by.
    /// </summary>
    public TPropertyNameEnum Field { get; set; }
    
    /// <summary>
    /// The sort direction. Default is <see cref="OrderByEnum.Ascending"/>.
    /// </summary>
    public OrderByEnum Order { get; set; } = OrderByEnum.Ascending;
}

/// <summary>
/// Default values for pagination parameters.
/// </summary>
public class PaginationConstants
{
    /// <summary>
    /// Default number of items per page (25).
    /// </summary>
    public const int DefaultCount = 25;

    /// <summary>
    /// Default page number (1).
    /// </summary>
    public const int DefaultPage = 1;

    /// <summary>
    /// Default number of items to skip (0).
    /// </summary>
    public const int DefaultSkip = 0;
}

/// <summary>
/// Marks a property for custom LINQ expression transformation during query building.
/// Use this attribute to define computed properties that require special handling.
/// </summary>
/// <remarks>
/// The transformation function must be registered in <see cref="GetItemsOptions.Expressions"/>
/// with a key matching <see cref="ExpressionName"/>.
/// </remarks>
[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public class LinqExpressionAttribute : Attribute
{
    /// <summary>
    /// The name used to look up the transformation function in <see cref="GetItemsOptions.Expressions"/>.
    /// </summary>
    public string ExpressionName { get; }

    /// <summary>
    /// Creates a new instance of <see cref="LinqExpressionAttribute"/>.
    /// </summary>
    /// <param name="expressionName">The name used to look up the transformation function.</param>
    public LinqExpressionAttribute(string expressionName)
    {
        ExpressionName = expressionName;
    }
}

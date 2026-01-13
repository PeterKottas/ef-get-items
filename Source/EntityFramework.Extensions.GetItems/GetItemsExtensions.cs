namespace EntityFramework.Extensions.GetItems;

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

/// <summary>
/// Provides extension methods for querying Entity Framework Core entities with pagination, filtering, and sorting.
/// </summary>
public static class GetItemsExtension
{
    internal static readonly MethodInfo AnyMethod = typeof(Enumerable).GetMethods()
                .First(m => m.Name == "Any" && m.GetParameters().Length == 2);

    internal static readonly MethodInfo AllMethod = typeof(Enumerable).GetMethods()
                .First(m => m.Name == "All" && m.GetParameters().Length == 2);

    internal static readonly MethodInfo WhereMethod = typeof(Enumerable).GetMethods()
                .First(m => m.Name == "Where" && m.GetParameters().Length == 2);

    internal static readonly MethodInfo CountMethod = typeof(Enumerable).GetMethods()
                .First(m => m.Name == "Count" && m.GetParameters().Length == 1);

    internal static readonly MethodInfo ContainsMethod = typeof(Enumerable).GetMethods()
                .First(m => m.Name == "Contains" && m.GetParameters().Length == 2);

    /// <summary>
    /// Queries entities with pagination, filtering, and sorting support using an <see cref="IDbContextFactory{TContext}"/>.
    /// </summary>
    /// <typeparam name="TDBContext">The DbContext type.</typeparam>
    /// <typeparam name="TEntity">The entity type to query.</typeparam>
    /// <typeparam name="TPropertyNameEnum">An enum type representing filterable/sortable property names.</typeparam>
    /// <typeparam name="TId">The type of the entity's primary key.</typeparam>
    /// <param name="contextFactory">The DbContext factory for creating database contexts.</param>
    /// <param name="query">A function that returns the base queryable from a DbContext.</param>
    /// <param name="request">The request containing pagination, filter, and sort parameters.</param>
    /// <param name="options">Optional configuration options. Uses defaults if not specified.</param>
    /// <returns>A <see cref="PaginatedData{TEntity}"/> containing the query results and pagination metadata.</returns>
    public static async Task<PaginatedData<TEntity>> GetItems<TDBContext, TEntity, TPropertyNameEnum, TId>(
               this IDbContextFactory<TDBContext> contextFactory,
               Func<TDBContext, IQueryable<TEntity>> query,
               BaseGetItemsRequest<TPropertyNameEnum, TId> request,
               GetItemsOptions? options = null
        ) where TPropertyNameEnum : struct, IConvertible where TDBContext : DbContext where TEntity : class
    {
        return await contextFactory.GetItems(query, request, null, null, options ?? GetItemsOptions.Default);
    }

    /// <summary>
    /// Queries entities with pagination, filtering, and sorting support using an <see cref="IDbContextFactory{TContext}"/>.
    /// </summary>
    /// <typeparam name="TDBContext">The DbContext type.</typeparam>
    /// <typeparam name="TEntity">The entity type to query.</typeparam>
    /// <typeparam name="TPropertyNameEnum">An enum type representing filterable/sortable property names.</typeparam>
    /// <typeparam name="TId">The type of the entity's primary key.</typeparam>
    /// <param name="contextFactory">The DbContext factory for creating database contexts.</param>
    /// <param name="query">A function that returns the base queryable from a DbContext.</param>
    /// <param name="request">The request containing pagination, filter, and sort parameters.</param>
    /// <param name="idAccessor">An expression to access the entity's primary key. Required when using Ids or ExceptIds.</param>
    /// <param name="options">Optional configuration options. Uses defaults if not specified.</param>
    /// <returns>A <see cref="PaginatedData{TEntity}"/> containing the query results and pagination metadata.</returns>
    public static async Task<PaginatedData<TEntity>> GetItems<TDBContext, TEntity, TPropertyNameEnum, TId>(
               this IDbContextFactory<TDBContext> contextFactory,
               Func<TDBContext, IQueryable<TEntity>> query,
               BaseGetItemsRequest<TPropertyNameEnum, TId> request,
               Expression<Func<TEntity, TId>>? idAccessor,
               GetItemsOptions? options = null
        ) where TPropertyNameEnum : struct, IConvertible where TDBContext : DbContext where TEntity : class
    {
        return await contextFactory.GetItems(query, request, idAccessor, null, options ?? GetItemsOptions.Default);
    }
    
    /// <summary>
    /// Queries entities with pagination, filtering, and sorting support using an <see cref="IDbContextFactory{TContext}"/>.
    /// </summary>
    /// <typeparam name="TDBContext">The DbContext type.</typeparam>
    /// <typeparam name="TEntity">The entity type to query.</typeparam>
    /// <typeparam name="TPropertyNameEnum">An enum type representing filterable/sortable property names.</typeparam>
    /// <typeparam name="TId">The type of the entity's primary key.</typeparam>
    /// <param name="contextFactory">The DbContext factory for creating database contexts.</param>
    /// <param name="query">A function that returns the base queryable from a DbContext.</param>
    /// <param name="request">The request containing pagination, filter, and sort parameters.</param>
    /// <param name="idAccessor">An expression to access the entity's primary key. Required when using Ids or ExceptIds.</param>
    /// <param name="propertyNameToString">A function that maps property enum values to property path arrays. Required when using Filters or Sort.</param>
    /// <returns>A <see cref="PaginatedData{TEntity}"/> containing the query results and pagination metadata.</returns>
    public static async Task<PaginatedData<TEntity>> GetItems<TDBContext, TEntity, TPropertyNameEnum, TId>(
               this IDbContextFactory<TDBContext> contextFactory,
               Func<TDBContext, IQueryable<TEntity>> query,
               BaseGetItemsRequest<TPropertyNameEnum, TId> request,
               Expression<Func<TEntity, TId>>? idAccessor,
               Func<TPropertyNameEnum, string[]>? propertyNameToString
        ) where TPropertyNameEnum : struct, IConvertible where TDBContext : DbContext where TEntity : class
    {
        return await contextFactory.GetItems(query, request, idAccessor, propertyNameToString, null);
    }

    /// <summary>
    /// Queries entities with pagination, filtering, and sorting support using an <see cref="IDbContextFactory{TContext}"/>.
    /// This overload provides full control over all parameters.
    /// </summary>
    /// <typeparam name="TDBContext">The DbContext type.</typeparam>
    /// <typeparam name="TEntity">The entity type to query.</typeparam>
    /// <typeparam name="TPropertyNameEnum">An enum type representing filterable/sortable property names.</typeparam>
    /// <typeparam name="TId">The type of the entity's primary key.</typeparam>
    /// <param name="contextFactory">The DbContext factory for creating database contexts.</param>
    /// <param name="query">A function that returns the base queryable from a DbContext.</param>
    /// <param name="request">The request containing pagination, filter, and sort parameters.</param>
    /// <param name="idAccessor">An expression to access the entity's primary key. Required when using Ids or ExceptIds.</param>
    /// <param name="propertyNameToString">A function that maps property enum values to property path arrays. Required when using Filters or Sort.</param>
    /// <param name="options">Configuration options for pagination handling and query debugging.</param>
    /// <returns>A <see cref="PaginatedData{TEntity}"/> containing the query results and pagination metadata.</returns>
    /// <exception cref="ArgumentException">Thrown when idAccessor is null but Ids/ExceptIds are used, or when propertyNameToString is null but Filters/Sort are used.</exception>
    public static async Task<PaginatedData<TEntity>> GetItems<TDBContext, TEntity, TPropertyNameEnum, TId>(
               this IDbContextFactory<TDBContext> contextFactory,
               Func<TDBContext, IQueryable<TEntity>> query,
               BaseGetItemsRequest<TPropertyNameEnum, TId> request,
               Expression<Func<TEntity, TId>>? idAccessor,
               Func<TPropertyNameEnum, string[]>? propertyNameToString,
               GetItemsOptions? options
        ) where TPropertyNameEnum : struct, IConvertible where TDBContext : DbContext where TEntity : class
    {
        options ??= GetItemsOptions.Default;
        
        var filteredQuery = BuildFilteredQuery(request, idAccessor, propertyNameToString, options);

        var count = request.Count ?? PaginationConstants.DefaultCount;

        switch (options.PaginationHandling)
        {
            case PaginationHandlingEnum.Expensive:
            {
                await using var totalContext = await contextFactory.CreateDbContextAsync();
                await using var itemsContext = await contextFactory.CreateDbContextAsync();
                
                var itemsQuery = filteredQuery(query(itemsContext));
                var debugView = GetQueryDebugView(itemsQuery, options);
                
                var totalCountTask = !request.TotalCount.HasValue
                    ? filteredQuery(query(totalContext)).LongCountAsync()
                    : Task.FromResult(request.TotalCount.Value);

                var totalItemsTask = itemsQuery.Paginate(request, count);

                await Task.WhenAll(totalCountTask, totalItemsTask);
                
                return new PaginatedData<TEntity>(
                    totalItemsTask.Result,
                    request.Page ?? PaginationConstants.DefaultPage,
                    count,
                    totalCount: totalCountTask.Result,
                    queryDebugView: debugView
                );
            }
            
            case PaginationHandlingEnum.Cheap:
            case PaginationHandlingEnum.None:
            default:
            {
                await using var itemsContext = await contextFactory.CreateDbContextAsync();
                return await ExecuteNonExpensiveQuery(filteredQuery(query(itemsContext)), request, options, count);
            }
        }
    }

    /// <summary>
    /// Queries entities with pagination, filtering, and sorting support directly on an <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <remarks>
    /// This overload does not support <see cref="PaginationHandlingEnum.Expensive"/> pagination mode.
    /// Use the <see cref="IDbContextFactory{TContext}"/> overloads for expensive pagination.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type to query.</typeparam>
    /// <typeparam name="TPropertyNameEnum">An enum type representing filterable/sortable property names.</typeparam>
    /// <typeparam name="TId">The type of the entity's primary key.</typeparam>
    /// <param name="query">The base queryable to apply filters, sorting, and pagination to.</param>
    /// <param name="request">The request containing pagination, filter, and sort parameters.</param>
    /// <param name="options">Optional configuration options. Uses defaults if not specified. Cannot use Expensive pagination.</param>
    /// <returns>A <see cref="PaginatedData{TEntity}"/> containing the query results and pagination metadata.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="PaginationHandlingEnum.Expensive"/> is used.</exception>
    public static Task<PaginatedData<TEntity>> GetItems<TEntity, TPropertyNameEnum, TId>(
               this IQueryable<TEntity> query,
               BaseGetItemsRequest<TPropertyNameEnum, TId> request,
               GetItemsOptions? options = null
        ) where TPropertyNameEnum : struct, IConvertible where TEntity : class
    {
        return query.GetItems(request, null, null, options);
    }

    /// <summary>
    /// Queries entities with pagination, filtering, and sorting support directly on an <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <remarks>
    /// This overload does not support <see cref="PaginationHandlingEnum.Expensive"/> pagination mode.
    /// Use the <see cref="IDbContextFactory{TContext}"/> overloads for expensive pagination.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type to query.</typeparam>
    /// <typeparam name="TPropertyNameEnum">An enum type representing filterable/sortable property names.</typeparam>
    /// <typeparam name="TId">The type of the entity's primary key.</typeparam>
    /// <param name="query">The base queryable to apply filters, sorting, and pagination to.</param>
    /// <param name="request">The request containing pagination, filter, and sort parameters.</param>
    /// <param name="idAccessor">An expression to access the entity's primary key. Required when using Ids or ExceptIds.</param>
    /// <param name="options">Optional configuration options. Uses defaults if not specified. Cannot use Expensive pagination.</param>
    /// <returns>A <see cref="PaginatedData{TEntity}"/> containing the query results and pagination metadata.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="PaginationHandlingEnum.Expensive"/> is used.</exception>
    public static Task<PaginatedData<TEntity>> GetItems<TEntity, TPropertyNameEnum, TId>(
               this IQueryable<TEntity> query,
               BaseGetItemsRequest<TPropertyNameEnum, TId> request,
               Expression<Func<TEntity, TId>>? idAccessor,
               GetItemsOptions? options = null
        ) where TPropertyNameEnum : struct, IConvertible where TEntity : class
    {
        return query.GetItems(request, idAccessor, null, options);
    }

    /// <summary>
    /// Queries entities with pagination, filtering, and sorting support directly on an <see cref="IQueryable{T}"/>.
    /// This overload provides full control over all parameters except pagination mode.
    /// </summary>
    /// <remarks>
    /// This overload does not support <see cref="PaginationHandlingEnum.Expensive"/> pagination mode because
    /// it requires parallel queries which need separate DbContext instances from an <see cref="IDbContextFactory{TContext}"/>.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type to query.</typeparam>
    /// <typeparam name="TPropertyNameEnum">An enum type representing filterable/sortable property names.</typeparam>
    /// <typeparam name="TId">The type of the entity's primary key.</typeparam>
    /// <param name="query">The base queryable to apply filters, sorting, and pagination to.</param>
    /// <param name="request">The request containing pagination, filter, and sort parameters.</param>
    /// <param name="idAccessor">An expression to access the entity's primary key. Required when using Ids or ExceptIds.</param>
    /// <param name="propertyNameToString">A function that maps property enum values to property path arrays. Required when using Filters or Sort.</param>
    /// <param name="options">Optional configuration options. Cannot use <see cref="PaginationHandlingEnum.Expensive"/>.</param>
    /// <returns>A <see cref="PaginatedData{TEntity}"/> containing the query results and pagination metadata.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="PaginationHandlingEnum.Expensive"/> is used.</exception>
    /// <exception cref="ArgumentException">Thrown when idAccessor is null but Ids/ExceptIds are used, or when propertyNameToString is null but Filters/Sort are used.</exception>
    public static Task<PaginatedData<TEntity>> GetItems<TEntity, TPropertyNameEnum, TId>(
               this IQueryable<TEntity> query,
               BaseGetItemsRequest<TPropertyNameEnum, TId> request,
               Expression<Func<TEntity, TId>>? idAccessor,
               Func<TPropertyNameEnum, string[]>? propertyNameToString,
               GetItemsOptions? options = null
        ) where TPropertyNameEnum : struct, IConvertible where TEntity : class
    {
        options ??= GetItemsOptions.Default;
        
        if (options.PaginationHandling == PaginationHandlingEnum.Expensive)
        {
            throw new InvalidOperationException(
                "Expensive pagination requires IDbContextFactory to run parallel queries. " +
                "Use IDbContextFactory.GetItems() or switch to Cheap/None pagination.");
        }

        var filteredQuery = BuildFilteredQuery(request, idAccessor, propertyNameToString, options);
        var count = request.Count ?? PaginationConstants.DefaultCount;
        
        return ExecuteNonExpensiveQuery(filteredQuery(query), request, options, count);
    }

    private static Func<IQueryable<TEntity>, IQueryable<TEntity>> BuildFilteredQuery<TEntity, TPropertyNameEnum, TId>(
        BaseGetItemsRequest<TPropertyNameEnum, TId> request,
        Expression<Func<TEntity, TId>>? idAccessor,
        Func<TPropertyNameEnum, string[]>? propertyNameToString,
        GetItemsOptions options
    ) where TPropertyNameEnum : struct, IConvertible where TEntity : class
    {
        var hasIds = request.Ids is { Length: > 0 };
        var hasExceptIds = request.ExceptIds is { Length: > 0 };
        var hasFilters = request.Filters is { Length: > 0 };
        var hasSort = request.Sort?.Any() == true;
        
        if ((hasIds || hasExceptIds) && idAccessor is null)
        {
            throw new ArgumentException(
                "An idAccessor must be provided when using Ids or ExceptIds in the request. " +
                "Please provide an expression that identifies the entity's primary key (e.g., e => e.Id).",
                nameof(idAccessor));
        }
        
        if ((hasFilters || hasSort) && propertyNameToString is null)
        {
            throw new ArgumentException(
                "A propertyNameToString mapping function must be provided when using Filters or Sort in the request. " +
                "Please provide a function that maps property enum values to property path arrays.",
                nameof(propertyNameToString));
        }
        
        var idsHashSet = new HashSet<TId>(request.Ids ?? []);
        var exceptIdsHashSet = new HashSet<TId>(request.ExceptIds ?? []);

        return baseQuery => baseQuery
            .FilterByIds(idsHashSet, exceptIdsHashSet, idAccessor)
            .Filters(request.Filters, propertyNameToString, options)
            .SortBy(request.Sort, propertyNameToString, idAccessor, options);
    }

    private static async Task<PaginatedData<TEntity>> ExecuteNonExpensiveQuery<TEntity, TPropertyNameEnum, TId>(
        IQueryable<TEntity> query,
        BaseGetItemsRequest<TPropertyNameEnum, TId> request,
        GetItemsOptions options,
        int count
    ) where TPropertyNameEnum : struct, IConvertible where TEntity : class
    {
        var debugView = GetQueryDebugView(query, options);
        
        if (options.PaginationHandling == PaginationHandlingEnum.Cheap)
        {
            var items = await query.Paginate(request, count + 1);
            var hasNextPage = items.Length > count;
            var resultItems = hasNextPage ? items.Take(count).ToArray() : items;
            
            return new PaginatedData<TEntity>(
                resultItems,
                request.Page ?? PaginationConstants.DefaultPage,
                count,
                hasNextPage: hasNextPage,
                queryDebugView: debugView
            );
        }
        else // None
        {
            var items = await query.Paginate(request, count);
            
            return new PaginatedData<TEntity>(
                items,
                request.Page ?? PaginationConstants.DefaultPage,
                count,
                queryDebugView: debugView
            );
        }
    }

    private static string? GetQueryDebugView<TEntity>(IQueryable<TEntity> query, GetItemsOptions options)
    {
        if (!options.DebugQuery) return null;
        
        // Use EF Core's ExpressionPrinter for clean readable output like:
        // DbSet<Entity>().Where(e => e.IsActive).OrderBy(e => e.Id)
        return Microsoft.EntityFrameworkCore.Query.ExpressionPrinter.Print(query.Expression);
    }

    internal static Task<TEntity[]> Paginate<TEntity, TPropertyNameEnum, TId>(
        this IQueryable<TEntity> query,
        BaseGetItemsRequest<TPropertyNameEnum, TId> request,
        int take) where TPropertyNameEnum : struct, IConvertible
    {
        var page = request.Page ?? PaginationConstants.DefaultPage;
        var count = request.Count ?? PaginationConstants.DefaultCount;
        var skip = request.Skip ?? PaginationConstants.DefaultSkip;
        var finalSkip = (page > 1 ? count * (page - 1) : 0) + skip;
        return query
            .Skip(finalSkip)
            .Take(take)
            .ToArrayAsync();
    }

    internal static IQueryable<TEntity> FilterByIds<TEntity, TId>(
        this IQueryable<TEntity> query,
        HashSet<TId> idsHashSet,
        HashSet<TId> exceptIdsHashSet,
        Expression<Func<TEntity, TId>>? idAccessor)
    {
        if (idsHashSet.Count == 0 && exceptIdsHashSet.Count == 0)
        {
            return query;
        }
        
        // idAccessor is validated in GetItems, so it should never be null here if we have ids
        if (idAccessor is null)
        {
            return query;
        }

        // Construct the IN clause
        if (idsHashSet.Count != 0)
        {
            var item = Expression.Parameter(typeof(TEntity), "item");
            var inExpression = Expression.Call(
                Expression.Constant(idsHashSet),
                nameof(HashSet<TId>.Contains),
                Type.EmptyTypes,
                Expression.Invoke(idAccessor, item)
            );

            var lambdaExpression = Expression.Lambda<Func<TEntity, bool>>(inExpression, item);
            query = query.Where(lambdaExpression);
        }

        // Construct the NOT IN clause
        if (exceptIdsHashSet.Count != 0)
        {
            var item = Expression.Parameter(typeof(TEntity), "item");
            var notInExpression = Expression.Not(Expression.Call(
                Expression.Constant(exceptIdsHashSet),
                nameof(HashSet<TId>.Contains),
                Type.EmptyTypes,
                Expression.Invoke(idAccessor, item)
            ));

            var lambdaExpression = Expression.Lambda<Func<TEntity, bool>>(notInExpression, item);
            query = query.Where(lambdaExpression);
        }

        return query;
    }

    internal static IQueryable<TEntity> Filters<TEntity, TPropertyNameEnum>(
        this IQueryable<TEntity> query,
        GetItemsFilter<TPropertyNameEnum>[]? filters,
        Func<TPropertyNameEnum, string[]>? propertyNameToString,
        GetItemsOptions options
    ) where TPropertyNameEnum : struct, IConvertible
    {
        // Ignore other code in the other methods, what I want to do here is create one single lambda expression that will be used in the Where method
        // To achieve this, I will first create a param. Then I will map the array of entities to a list of expressions . Then I will use the Aggregate method to combine all the expressions into one
        if (filters != null)
        {
            var param = Expression.Parameter(typeof(TEntity), "e");
            var expression = GetFiltersExpression(param, filters, propertyNameToString, options);
            var expressionLambda = Expression.Lambda<Func<TEntity, bool>>(expression, param);
            var whereQuery = query.Where(expressionLambda);
            return whereQuery;
        }
        return query;
    }

    internal static Expression GetFiltersExpression<TPropertyNameEnum>(
        Expression param,
        GetItemsFilter<TPropertyNameEnum>[]? filters,
        Func<TPropertyNameEnum, string[]>? propertyNameToString,
        GetItemsOptions options) where TPropertyNameEnum : struct, IConvertible
    {
        if (filters != null)
        {
            var expressions = filters.Select(filter => new { Expression = GetFilterExpression(filter, param, propertyNameToString, options), Filter = filter }).ToList();
            //var expression = expressions.Aggregate((left, right) => right.Filter.Logic == FilterLogicEnum.And ? Expression.AndAlso(left.Expression, right.Expression) : Expression.OrElse(left.Expression, right.Expression));
            var expression = expressions.Aggregate((Expression?)null, (left, right) =>
            {
                if (left == null)
                    return right.Expression;
                return right.Filter.Logic == FilterLogicEnum.And ? Expression.AndAlso(left, right.Expression) : Expression.OrElse(left, right.Expression);
            }) ?? Expression.Constant(true);
            return expression;
        }
        return Expression.Constant(true);
    }

    internal static Expression GetFilterExpression<TPropertyNameEnum>(
        GetItemsFilter<TPropertyNameEnum> filter,
        Expression param,
        Func<TPropertyNameEnum, string[]>? propertyNameToString,
        GetItemsOptions options) where TPropertyNameEnum : struct, IConvertible
    {
        var filtersExpression = (filter.Filters is not null && filter.Filters.Length > 0)
            ? GetFiltersExpression(param, filter.Filters, propertyNameToString, options)
            : null;

        if (!filter.Field.HasValue)
        {
            return filtersExpression ?? Expression.Constant(true);
        }

        var propertyNames = filter.Field.Value.GetPropertyNameString(propertyNameToString);

        if (propertyNames.Length == 0)
        {
            return filtersExpression ?? Expression.Constant(true);
        }

        var propertyNamesByArrayAccessor = NestedPropertyByArrayAccessor(param, propertyNames, options);
        if (propertyNamesByArrayAccessor.Length > 2)
        {
            throw new ArgumentException("Nested array accessor is not supported. Hint: In your GetPropertyNameString, you included more than 1 array.");
        }

        if (propertyNamesByArrayAccessor.Length == 1)
        {
            var member = propertyNamesByArrayAccessor.First();
            var body = GetBodyExpression(filter, member, GetConstantExpression(filter, member), options);

            return filtersExpression is not null
                ? LogicExpression(body, filtersExpression, filter.FiltersLogic)
                : body;
        }
        else
        {
            var hasValues = filter.Values is not null && filter.Values.Length != 0;
            var member = propertyNamesByArrayAccessor.Last();
            var body = GetBodyExpression(filter, member, GetConstantExpression(filter, member), options);
            var arrayAccessor = propertyNamesByArrayAccessor.First();
            var arrayAccessorType = arrayAccessor.Type;
            var elementType = arrayAccessorType.GetInterfaces()
                .First(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .GetGenericArguments()[0];
            var originalParam = member;
            while (originalParam!.NodeType != ExpressionType.Parameter)
            {
                originalParam = (originalParam as MemberExpression)!.Expression;
            }
            var bodyLambda = Expression.Lambda(body, (originalParam as ParameterExpression)!);
            Expression finalExpression;
            if (hasValues && filter.Operator == FilterOperatorEnum.ContainsAll)
            {
                // We do something like
                /*
                var items2 = await context.CompanyUsers
                    .Where(e => e.UserData.Skills
                        .Where(e1 => vals.Contains(e1.Skill))
                        .Count() == vals.Length)
                    .ToListAsync();
                */

                var whereExpression = Expression.Call(
                    WhereMethod.MakeGenericMethod(originalParam.Type),
                    arrayAccessor,
                    bodyLambda
                );
                var countExpression = Expression.Call(
                    CountMethod.MakeGenericMethod(elementType),
                    whereExpression
                );
                finalExpression = Expression.Equal(countExpression, Expression.Constant(filter.Values!.Length));
            }
            else
            {
                // We do something like
                /*
                var items2 = await context.CompanyUsers
                    .Where(e => e.UserData.Skills
                        .Any(e1 => e1.Skill == UserSkillEnum.Acting))
                    .ToListAsync
                or for values
                var items2 = await context.CompanyUsers
                    .Where(e => e.UserData.Skills
                        .Any(e1 => vals.Contains(e1.Skill)))
                    .ToListAsync();
                */

                var method = (filter.ArrayAccessorLogic == FilterArrayAccessorLogic.All ? AllMethod : AnyMethod).MakeGenericMethod(elementType);
                finalExpression = Expression.Call(
                    method,
                    arrayAccessor,
                    bodyLambda
                );
            }
            return filtersExpression is not null
                    ? LogicExpression(finalExpression, filtersExpression, filter.FiltersLogic)
                    : finalExpression;
        }
    }

    internal static Expression[] NestedPropertyByArrayAccessor(Expression param, string[] propertyNames, GetItemsOptions options)
    {
        List<Expression> propertyNamesList = new() { param };

        foreach (var propertyName in propertyNames)
        {
            var result = propertyNamesList[^1]; // Get last expression

            // First, check if the property has a LinqExpression attribute
            var propertyInfo = result.Type.GetProperty(propertyName);
            if (propertyInfo == null)
                throw new ArgumentException($"Property '{propertyName}' not found on type '{result.Type.Name}'");

            var linqExpressionAttribute = propertyInfo.GetCustomAttribute<LinqExpressionAttribute>();
            if (linqExpressionAttribute != null)
            {
                var expressionName = linqExpressionAttribute.ExpressionName;
                if (options.Expressions.TryGetValue(expressionName, out var transformExpression))
                {
                    // Apply transformation function
                    propertyNamesList[^1] = transformExpression(result);
                    continue;
                }
                else
                {
                    throw new ArgumentException($"Expression '{expressionName}' not found in options.Expressions.");
                }
            }

            // If no special transformation, continue with standard property access
            propertyNamesList[^1] = Expression.PropertyOrField(result, propertyName);

            // Handle collections
            if (propertyInfo.PropertyType.GetInterfaces()
                .Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                && propertyInfo.PropertyType != typeof(string))
            {
                var elementType = propertyInfo.PropertyType.GetInterfaces()
                    .First(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    .GetGenericArguments()[0];

                var elementParam = Expression.Parameter(elementType, "e" + propertyNamesList.Count);
                propertyNamesList.Add(elementParam);
            }
        }

        return propertyNamesList.ToArray();
    }

    internal static Expression LogicExpression(Expression left, Expression right, FilterLogicEnum logic)
    {
        return logic == FilterLogicEnum.And ? Expression.AndAlso(left, right) : Expression.OrElse(left, right);
    }

    internal static IQueryable<TEntity> SortBy<TEntity, TPropertyNameEnum, TId>(
        this IQueryable<TEntity> source,
        IEnumerable<GetItemsSorter<TPropertyNameEnum>>? sort,
        Func<TPropertyNameEnum, string[]>? propertyNameToString,
        Expression<Func<TEntity, TId>>? idAccessor,
        GetItemsOptions options) where TPropertyNameEnum : struct, IConvertible
    {
        if (sort != null && sort.Any())
        {
            int count = 0;
            foreach (var s in sort)
            {
                string command = s.Order == OrderByEnum.Descending ? "OrderByDescending" : "OrderBy";
                if (count >= 1)
                    command = s.Order == OrderByEnum.Descending ? "ThenByDescending" : "ThenBy";

                var type = typeof(TEntity);
                var propertyNames = s.Field.GetPropertyNameString(propertyNameToString);
                var property = GetNestedProperty(type, propertyNames);
                if (property != null)
                {
                    var parameter = Expression.Parameter(type, "p");
                    var propertyAccess = NestedPropertyByArrayAccessor(parameter, propertyNames, options);
                    if (propertyAccess.Length > 1)
                    {
                        throw new ArgumentException("Array accessor is not supported when sorting. Hint: In your GetPropertyNameString, you included more than 1 array.");
                    }
                    var orderByExpression = Expression.Lambda(propertyAccess.First(), parameter);
                    var resultExpression = Expression.Call(
                        typeof(Queryable),
                        command,
                        new[] { type, property.PropertyType },
                        source.Expression,
                        Expression.Quote(orderByExpression));

                    source = source.Provider.CreateQuery<TEntity>(resultExpression);
                    count++;
                }
            }
            if (idAccessor != null)
            {
                // We should always do this because the order from above might not be fully deterministic
                source = ((IOrderedQueryable<TEntity>)source).ThenBy(idAccessor);
            }
        }
        else if (idAccessor != null)
        {
            source = source.OrderBy(idAccessor);
        }

        return source;
    }

    // This method is used to get string array from propertyNameToString and field name
    internal static string[] GetPropertyNameString<TPropertyNameEnum>(
               this TPropertyNameEnum field,
                      Func<TPropertyNameEnum, string[]>? propertyNameToString) where TPropertyNameEnum : struct, IConvertible
    {
        return propertyNameToString?.Invoke(field) ?? [field.ToString()!];
    }

    private static Expression GetBodyExpression<TPropertyNameEnum>(
        GetItemsFilter<TPropertyNameEnum> filter, 
        Expression member, 
        Expression constant,
        GetItemsOptions options)
    where TPropertyNameEnum : struct, IConvertible
    {
        var isNullable = IsNullable(member.Type, out var underlyingType);
        var isNumeric = IsNumericType(underlyingType);
        var isDateTime = underlyingType == typeof(DateTime);
        var isDateOnly = underlyingType == typeof(DateOnly);
        var isEnum = underlyingType.IsEnum;
        var isBoolean = underlyingType == typeof(bool);

        // Separating the logic into small reusable pieces
        var memberValue = GetMemberValueExpression(member, isNullable);
        var memberHasValue = GetMemberHasValueExpression(member, isNullable);
        var constantIsNull = filter.Value is null;

        return filter.Operator switch
        {
            FilterOperatorEnum.Eq => GetEqualityExpression(memberValue, constant, memberHasValue, isNullable, isDateTime, isDateOnly, constantIsNull),
            FilterOperatorEnum.Neq => GetInequalityExpression(memberValue, constant, memberHasValue, isNullable, constantIsNull),
            FilterOperatorEnum.Lt => GetComparisonExpression(Expression.LessThan, memberValue, constant, memberHasValue, underlyingType, isNullable, isNumeric, isDateTime, isDateOnly, isEnum),
            FilterOperatorEnum.Lte => GetComparisonExpression(Expression.LessThanOrEqual, memberValue, constant, memberHasValue, underlyingType, isNullable, isNumeric, isDateTime, isDateOnly, isEnum),
            FilterOperatorEnum.Gt => GetComparisonExpression(Expression.GreaterThan, memberValue, constant, memberHasValue, underlyingType, isNullable, isNumeric, isDateTime, isDateOnly, isEnum),
            FilterOperatorEnum.Gte => GetComparisonExpression(Expression.GreaterThanOrEqual, memberValue, constant, memberHasValue, underlyingType, isNullable, isNumeric, isDateTime, isDateOnly, isEnum),
            FilterOperatorEnum.StartsWith => GetStringOperationExpression("StartsWith", memberValue, constant, filter.Values),
            FilterOperatorEnum.EndsWith => GetStringOperationExpression("EndsWith", memberValue, constant, filter.Values),
            FilterOperatorEnum.Contains => GetContainsExpression(memberValue, constant, memberHasValue, filter.Values, underlyingType, isNullable),
            FilterOperatorEnum.NotContains => Expression.Not(GetContainsExpression(memberValue, constant, memberHasValue, filter.Values, underlyingType, isNullable)),
            FilterOperatorEnum.IStartsWith => GetCaseInsensitiveStringExpression("StartsWith", memberValue, filter.Value, filter.Values, options.DbProvider),
            FilterOperatorEnum.IEndsWith => GetCaseInsensitiveStringExpression("EndsWith", memberValue, filter.Value, filter.Values, options.DbProvider),
            FilterOperatorEnum.IContains => GetCaseInsensitiveStringExpression("Contains", memberValue, filter.Value, filter.Values, options.DbProvider),
            FilterOperatorEnum.INotContains => Expression.Not(GetCaseInsensitiveStringExpression("Contains", memberValue, filter.Value, filter.Values, options.DbProvider)),
            FilterOperatorEnum.ContainsAll => GetContainsAllExpression(memberValue, constant, memberHasValue, filter.Values, underlyingType, isNullable),
            FilterOperatorEnum.NotContainsAll => Expression.Not(GetContainsAllExpression(memberValue, constant, memberHasValue, filter.Values, underlyingType, isNullable)),
            FilterOperatorEnum.Flag => GetFlagExpression(memberValue, constant, memberHasValue, isNullable, isEnum || isNumeric, isBoolean),
            FilterOperatorEnum.NotFlag => GetNotFlagExpression(memberValue, constant, memberHasValue, isNullable, isEnum || isNumeric, isBoolean),
            FilterOperatorEnum.AnyFlag => GetAnyFlagExpression(memberValue, constant, memberHasValue, isNullable, isEnum || isNumeric, isBoolean),
            FilterOperatorEnum.NotAnyFlag => GetNotAnyFlagExpression(memberValue, constant, memberHasValue, isNullable, isEnum || isNumeric, isBoolean),
            _ => throw new ArgumentOutOfRangeException(nameof(filter.Operator), filter.Operator, "Unsupported filter operator.")
        };
    }

    internal static PropertyInfo? GetNestedProperty(Type type, params string[] propertyNames)
    {
        PropertyInfo? property = null;
        foreach (string propertyName in propertyNames)
        {
            property = type.GetProperty(propertyName);
            if (property == null)
            {
                throw new ArgumentException($"Property {propertyName} not found on type {type.Name}");
            }
            type = property.PropertyType;
        }
        return property;
    }

    private static Expression GetMemberValueExpression(Expression member, bool isNullable)
    {
        return isNullable ? Expression.Property(member, "Value") : member;
    }

    private static Expression GetMemberHasValueExpression(Expression member, bool isNullable)
    {
        return isNullable ? Expression.Property(member, "HasValue") : Expression.Constant(true);
    }

    private static Expression GetFlagExpression(Expression memberValue, Expression constant, Expression memberHasValue, bool isNullable, bool isNumeric, bool isBoolean)
    {
        if (isBoolean)
        {
            var memberValueInt = Expression.Convert(memberValue, typeof(bool));
            var constantInt = Expression.Convert(constant, typeof(bool));
            return isNullable
                ? Expression.Condition(memberHasValue, Expression.Equal(memberValueInt, constantInt), Expression.Constant(false))
                : Expression.Equal(memberValueInt, constantInt);
        }
        if (isNumeric)
        {
            var memberValueInt = Expression.Convert(memberValue, typeof(int));
            var constantInt = Expression.Convert(constant, typeof(int));
            return isNullable
                ? Expression.Condition(memberHasValue, Expression.Equal(Expression.And(memberValueInt, constantInt), constantInt), Expression.Constant(false))
                : Expression.Equal(Expression.And(memberValueInt, constantInt), constantInt);
        }
        return Expression.Constant(false);
    }

    private static Expression GetNotFlagExpression(Expression memberValue, Expression constant, Expression memberHasValue, bool isNullable, bool isNumeric, bool isBoolean)
    {
        if (isBoolean)
        {
            var memberValueBool = Expression.Convert(memberValue, typeof(bool));
            var constantBool = Expression.Convert(constant, typeof(bool));
            var notEqual = Expression.NotEqual(memberValueBool, constantBool);
            return isNullable
                ? Expression.Condition(memberHasValue, notEqual, Expression.Constant(false))
                : notEqual;
        }

        if (!isNumeric)
        {
            return Expression.Constant(false);
        }
        var memberValueInt = Expression.Convert(memberValue, typeof(int));
        var constantInt = Expression.Convert(constant, typeof(int));

        var noFlagSet = Expression.Equal(Expression.And(memberValueInt, constantInt), Expression.Constant(0, typeof(int)));

        return isNullable
            ? Expression.Condition(memberHasValue, noFlagSet, Expression.Constant(false))
            : noFlagSet;
    }

    private static Expression GetAnyFlagExpression(Expression memberValue, Expression constant, Expression memberHasValue, bool isNullable, bool isNumeric, bool isBoolean)
    {
        if (isBoolean)
        {
            var memberValueBool = Expression.Convert(memberValue, typeof(bool));
            var constantBool = Expression.Convert(constant, typeof(bool));
            var equal = Expression.Equal(memberValueBool, constantBool);
            return isNullable
                ? Expression.Condition(memberHasValue, equal, Expression.Constant(false))
                : equal;
        }

        if (!isNumeric)
        {
            return Expression.Constant(false);
        }

        var memberValueInt = Expression.Convert(memberValue, typeof(int));
        var constantInt = Expression.Convert(constant, typeof(int));

        var hasAnyFlag = Expression.NotEqual(Expression.And(memberValueInt, constantInt), Expression.Constant(0, typeof(int)));

        return isNullable
            ? Expression.Condition(memberHasValue, hasAnyFlag, Expression.Constant(false))
            : hasAnyFlag;
    }

    private static Expression GetNotAnyFlagExpression(Expression memberValue, Expression constant, Expression memberHasValue, bool isNullable, bool isNumeric, bool isBoolean)
    {
        if (isBoolean)
        {
            var memberValueBool = Expression.Convert(memberValue, typeof(bool));
            var constantBool = Expression.Convert(constant, typeof(bool));
            var notEqual = Expression.NotEqual(memberValueBool, constantBool);
            return isNullable
                ? Expression.Condition(memberHasValue, notEqual, Expression.Constant(false))
                : notEqual;
        }

        if (!isNumeric)
        {
            return Expression.Constant(false);
        }

        var memberValueInt = Expression.Convert(memberValue, typeof(int));
        var constantInt = Expression.Convert(constant, typeof(int));

        var atLeastOneFlagNotSet = Expression.NotEqual(Expression.And(memberValueInt, constantInt), constantInt);

        return isNullable
            ? Expression.Condition(memberHasValue, atLeastOneFlagNotSet, Expression.Constant(false))
            : atLeastOneFlagNotSet;
    }

    private static Expression GetEqualityExpression(Expression memberValue, Expression constant, Expression memberHasValue, bool isNullable, bool isDateTime, bool isDateOnly, bool constantIsNull)
    {
        if (isDateTime || isDateOnly)
        {
            return isNullable
                ? Expression.Condition(memberHasValue, Expression.Equal(memberValue, constant), Expression.Constant(false))
                : Expression.Equal(memberValue, constant);
        }
        return Expression.Not(GetInequalityExpression(memberValue, constant, memberHasValue, isNullable, constantIsNull));
    }

    private static Expression GetInequalityExpression(Expression memberValue, Expression constant, Expression memberHasValue, bool isNullable, bool constantIsNull)
    {
        Expression notEqualCondition = isNullable && constantIsNull ? Expression.Constant(false) : Expression.NotEqual(memberValue, constant);
        var hasValueCondition = constantIsNull ? Expression.Constant(true) : notEqualCondition;
        var noValueCondition = constantIsNull ? Expression.Constant(false) : Expression.Constant(true);
        return isNullable
            ? Expression.Condition(
                memberHasValue,
                hasValueCondition,
                noValueCondition
              )
            : notEqualCondition;
    }

    private static Expression GetComparisonExpression(Func<Expression, Expression, Expression> comparisonFunc, Expression memberValue, Expression constant, Expression memberHasValue, Type underlyingType, bool isNullable, bool isNumeric, bool isDateTime, bool isDateOnly, bool isEnum)
    {
        if (!isNumeric && !isDateTime && !isDateOnly && !isEnum)
        {
            return Expression.Constant(false);
        }

        if (isEnum)
        {
            memberValue = Expression.Convert(memberValue, typeof(int));
            constant = Expression.Convert(constant, typeof(int));
        }

        return isNullable
            ? Expression.Condition(memberHasValue, comparisonFunc(memberValue, constant), Expression.Constant(false))
            : comparisonFunc(memberValue, constant);
    }

    // Cache the MethodInfo for EF.Functions.Like
    private static readonly MethodInfo EfLikeMethod = typeof(DbFunctionsExtensions)
        .GetMethod(nameof(DbFunctionsExtensions.Like), new[] { typeof(DbFunctions), typeof(string), typeof(string) })!;
    
    // ILike is in Npgsql.EntityFrameworkCore.PostgreSQL package (NpgsqlDbFunctionsExtensions class)
    // We use lazy initialization to find it at runtime if the Npgsql assembly is loaded
    private static MethodInfo? _efILikeMethod;
    
    private static MethodInfo? GetEfILikeMethod()
    {
        if (_efILikeMethod != null)
            return _efILikeMethod;
        
        // Try to find NpgsqlDbFunctionsExtensions from loaded assemblies
        var npgsqlType = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name == "Npgsql.EntityFrameworkCore.PostgreSQL")
            .SelectMany(a => 
            {
                try { return a.GetTypes(); }
                catch { return Array.Empty<Type>(); }
            })
            .FirstOrDefault(t => t.Name == "NpgsqlDbFunctionsExtensions");
        
        if (npgsqlType != null)
        {
            // ILike signature: ILike(DbFunctions, string matchExpression, string pattern)
            _efILikeMethod = npgsqlType.GetMethods()
                .FirstOrDefault(m => m.Name == "ILike" 
                    && m.GetParameters().Length == 3
                    && m.GetParameters()[0].ParameterType == typeof(DbFunctions)
                    && m.GetParameters()[1].ParameterType == typeof(string)
                    && m.GetParameters()[2].ParameterType == typeof(string));
        }
        
        return _efILikeMethod;
    }
    
    private static Expression GetStringOperationExpression(
        string methodName, 
        Expression memberValue, 
        Expression constant,
        string[]? values)
    {
        if (values is { Length: > 0 } || memberValue.Type != typeof(string))
        {
            return Expression.Constant(false);
        }

        return Expression.Call(memberValue, methodName, Type.EmptyTypes, constant);
    }
    
    private static Expression GetCaseInsensitiveStringExpression(
        string operationType,
        Expression memberValue,
        string? value,
        string[]? values,
        DbProviderEnum dbProvider)
    {
        if (values is { Length: > 0 } || memberValue.Type != typeof(string) || value == null)
        {
            return Expression.Constant(false);
        }

        return GetLikeExpression(memberValue, value, operationType, dbProvider);
    }
    
    private static Expression GetLikeExpression(Expression memberValue, string value, string operationType, DbProviderEnum dbProvider)
    {
        // Escape LIKE wildcards in the value
        var escapedValue = EscapeLikePattern(value);
        
        // Build the pattern based on operation type
        var pattern = operationType switch
        {
            "StartsWith" => $"{escapedValue}%",
            "EndsWith" => $"%{escapedValue}",
            "Contains" => $"%{escapedValue}%",
            _ => escapedValue
        };
        
        var patternExpression = Expression.Constant(pattern);
        var efFunctionsExpression = Expression.Property(null, typeof(EF), nameof(EF.Functions));
        
        // Use ILike for PostgreSQL, Like for SQL Server and InMemory
        if (dbProvider == DbProviderEnum.PostgreSql)
        {
            var iLikeMethod = GetEfILikeMethod();
            if (iLikeMethod == null)
            {
                throw new InvalidOperationException(
                    "PostgreSQL case-insensitive operators (IStartsWith, IEndsWith, IContains, INotContains) require the " +
                    "'Npgsql.EntityFrameworkCore.PostgreSQL' package to be installed. The ILike method was not found. " +
                    "Either install the Npgsql package or use DbProviderEnum.SqlServer if targeting SQL Server.");
            }
            return Expression.Call(iLikeMethod, efFunctionsExpression, memberValue, patternExpression);
        }
        
        return Expression.Call(EfLikeMethod, efFunctionsExpression, memberValue, patternExpression);
    }
    
    /// <summary>
    /// Escapes special LIKE pattern characters (%, _, [) in the input value.
    /// </summary>
    private static string EscapeLikePattern(string value)
    {
        // Escape the special characters used in LIKE patterns
        // The escape character itself doesn't need escaping as we're using the default
        return value
            .Replace("[", "[[]")
            .Replace("%", "[%]")
            .Replace("_", "[_]");
    }

    private static Expression GetContainsExpression(
        Expression memberValue, 
        Expression constant, 
        Expression memberHasValue, 
        string[]? values, 
        Type underlyingType, 
        bool isNullable)
    {
        var hasValues = values is { Length: > 0 };
        var isString = underlyingType == typeof(string);

        if (hasValues)
        {
            return isNullable ?
                Expression.Condition(memberHasValue, Expression.Call(constant, "Contains", Type.EmptyTypes, memberValue), Expression.Constant(false))
                :
                Expression.Call(constant, "Contains", Type.EmptyTypes, memberValue);
        }
        else
        {
            return isString
                ? Expression.Call(memberValue, "Contains", Type.EmptyTypes, constant)
                : Expression.Constant(false);
        }
    }

    private static Expression GetContainsAllExpression(Expression memberValue, Expression constant, Expression memberHasValue, string[]? values, Type underlyingType, bool isNullable)
    {
        var hasValues = values is { Length: > 0 };
        var isString = underlyingType == typeof(string);

        if (hasValues)
        {
            return isNullable ?
                Expression.Condition(memberHasValue, Expression.Call(constant, "Contains", Type.EmptyTypes, memberValue), Expression.Constant(false))
                :
                Expression.Call(constant, "Contains", Type.EmptyTypes, memberValue);
        }
        else
        {
            throw new ArgumentException("ContainsAll operator only works for values array, not a single value.");
        }
    }

    private static Expression GetConstantExpression<TPropertyNameEnum>(GetItemsFilter<TPropertyNameEnum> filter, Expression member)
    where TPropertyNameEnum : struct, IConvertible
    {
        bool isNullable = IsNullable(member.Type, out var underlyingType);
        var hasValues = filter.Values is not null && filter.Values.Length != 0;
        var listType = typeof(List<>).MakeGenericType(underlyingType);
        var typedList = Activator.CreateInstance(listType)!;
        if (hasValues)
        {
            foreach (var value in filter.Values!)
            {
                var parsedValue = ParseSingleValue(value, underlyingType);
                listType.GetMethod("Add")!.Invoke(typedList, new[] { parsedValue });
            }
        }

        Expression constant = hasValues ? Expression.Constant(typedList)
            : Expression.Constant(ParseSingleValue(filter.Value, underlyingType));
        return constant;
    }

    private static bool IsNullable(Type type, out Type finalType)
    {
        var underlyingType = Nullable.GetUnderlyingType(type);
        if (underlyingType != null)
        {
            finalType = underlyingType;
            return true;
        }
        finalType = type;
        return false;
    }

    private static bool IsNumericType(Type type)
    {
        return type == typeof(int) || type == typeof(long) ||
               type == typeof(float) || type == typeof(double) ||
               type == typeof(short) || type == typeof(decimal) ||
               type == typeof(byte) || type == typeof(sbyte) ||
               type == typeof(ushort) || type == typeof(uint) ||
               type == typeof(ulong) || type == typeof(char);
    }

    internal static object? ParseSingleValue(string? value, Type type)
    {
        if (value is null)
        {
            return null;
        }
        return type switch
        {
            _ when type == typeof(Guid) => ParseFlexibleGuid(value),
            _ when type == typeof(float) => float.Parse(value, CultureInfo.InvariantCulture),
            _ when type == typeof(double) => double.Parse(value, CultureInfo.InvariantCulture),
            _ when type == typeof(short) => short.Parse(value, CultureInfo.InvariantCulture),
            _ when type == typeof(int) => int.Parse(value, CultureInfo.InvariantCulture),
            _ when type == typeof(long) => long.Parse(value, CultureInfo.InvariantCulture),
            _ when type == typeof(bool) => value == "1" || (value == "0" ? false : bool.Parse(value)),
            _ when type == typeof(char) => char.Parse(value),
            _ when type == typeof(byte) => byte.Parse(value, CultureInfo.InvariantCulture),
            _ when type == typeof(sbyte) => sbyte.Parse(value, CultureInfo.InvariantCulture),
            _ when type == typeof(ushort) => ushort.Parse(value, CultureInfo.InvariantCulture),
            _ when type == typeof(uint) => uint.Parse(value, CultureInfo.InvariantCulture),
            _ when type == typeof(decimal) => decimal.Parse(value, CultureInfo.InvariantCulture),
            _ when type == typeof(ulong) => ulong.Parse(value, CultureInfo.InvariantCulture),
            _ when type == typeof(DateTime) => DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal),
            _ when type == typeof(DateOnly) => DateOnly.FromDateTime(DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal)),
            _ when type.IsEnum => Enum.Parse(type, value),
            _ => value,
        };
    }

    private static Guid ParseFlexibleGuid(string value)
    {
        if (value is null)
        {
            return Guid.Empty;
        }

        string hex = new string(value.Where(c => Uri.IsHexDigit(c)).ToArray()).ToUpperInvariant();

        if (hex.Length == 0)
        {
            return Guid.Empty;
        }

        if (hex.Length > 32)
        {
            return Guid.ParseExact(hex.Substring(0, 32), "N");
        }

        hex = hex.PadRight(32, '0');

        return Guid.ParseExact(hex, "N");
    }
}

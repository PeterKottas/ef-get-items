namespace FilteringTest.Utils;

using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

public class PaginatedData<TEntity>
{
    public TEntity[] Items { get; }
    public int Page { get; }
    public int Count { get; }
    public long TotalCount { get; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / Count);

    public PaginatedData(TEntity[] items, int page, int count, long totalCount)
    {
        Items = items;
        Page = page;
        Count = count;
        TotalCount = totalCount;
    }
}

public class BaseGetItemsRequest<TPropertyNameEnum, TId>
    where TPropertyNameEnum : struct, IConvertible
{
    public TId[]? Ids { get; set; }
    public TId[]? ExceptIds { get; set; }
    public int? Page { get; set; }
    public int? Count { get; set; }
    public int? Skip { get; set; }
    public GetItemsFilter<TPropertyNameEnum>[]? Filters { get; set; }
    public IEnumerable<GetItemsSorter<TPropertyNameEnum>>? Sort { get; set; }
    public long? TotalCount { get; set; }
}

public class GetItemsFilter<TPropertyNameEnum>
    where TPropertyNameEnum : struct, IConvertible
{
    public TPropertyNameEnum? Field { get; set; }
    public FilterOperatorEnum Operator { get; set; }
    public string? Value { get; set; }
    public string[]? Values { get; set; }
    public FilterLogicEnum Logic { get; set; }
    public FilterLogicEnum FiltersLogic { get; set; } = FilterLogicEnum.And;
    public FilterArrayAccessorLogic ArrayAccessorLogic { get; set; } = FilterArrayAccessorLogic.Any;
    public GetItemsFilter<TPropertyNameEnum>[]? Filters { get; set; }
}

public class GetItemsSorter<TPropertyNameEnum>
    where TPropertyNameEnum : struct, IConvertible
{
    public TPropertyNameEnum Field { get; set; }
    public OrderByEnum Order { get; set; } = OrderByEnum.Ascending;
}

public enum OrderByEnum
{
    Ascending,
    Descending
}

public enum FilterOperatorEnum
{
    Eq,
    Neq,
    Lt,
    Lte,
    Gt,
    Gte,
    StartsWith,
    EndsWith,
    Contains,
    NotContains,
    ContainsAll,
    NotContainsAll,
    Flag,
    NotFlag,
    AnyFlag,
    NotAnyFlag
}

public enum FilterLogicEnum
{
    And,
    Or
}

public enum FilterArrayAccessorLogic
{
    Any,
    All
}

public class PaginationConstants
{
    public const int DefaultCount = 25;

    public const int DefaultPage = 1;

    public const int DefaultSkip = 0;
}

[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public class LinqExpressionAttribute : System.Attribute
{
    public string ExpressionName { get; }

    public LinqExpressionAttribute(string expressionName)
    {
        ExpressionName = expressionName;
    }
}

public class ExpressionsRepository
{
    public static readonly Dictionary<string, Func<Expression, Expression>> Expressions = new()
    {
        /*{
            nameof(CompanyUserModelBase) + "_" + nameof(CompanyUserModelBase.EffectiveVisibility),
            prev => {
                var hiddenUntilProp = Expression.Property(prev, nameof(CompanyUserModelBase.HiddenUntil));
                var hiddenUntilHasValue = Expression.Property(hiddenUntilProp, nameof(Nullable<DateTime>.HasValue));
                var hiddenUntilValue = Expression.Property(hiddenUntilProp, nameof(Nullable<DateTime>.Value));

                var condition = Expression.AndAlso(
                    hiddenUntilHasValue, // Check if HiddenUntil.HasValue == true
                    Expression.GreaterThan(hiddenUntilValue, Expression.Constant(DateTime.UtcNow)) // Compare value
                );

                return Expression.Condition(
                    condition,
                    Expression.Constant(CompanyUserVisibilityEnum.Hidden),
                    Expression.Property(prev, nameof(CompanyUserModelBase.Visibility))
                );
            }
        }*/
    };
}

public static class GetItemsExtension
{
    public static readonly MethodInfo AnyMethod = typeof(Enumerable).GetMethods()
                .First(m => m.Name == "Any" && m.GetParameters().Length == 2);

    public static readonly MethodInfo AllMethod = typeof(Enumerable).GetMethods()
                .First(m => m.Name == "All" && m.GetParameters().Length == 2);

    public static readonly MethodInfo WhereMethod = typeof(Enumerable).GetMethods()
                .First(m => m.Name == "Where" && m.GetParameters().Length == 2);

    public static readonly MethodInfo CountMethod = typeof(Enumerable).GetMethods()
                .First(m => m.Name == "Count" && m.GetParameters().Length == 1);

    public static readonly MethodInfo ContainsMethod = typeof(Enumerable).GetMethods()
                .First(m => m.Name == "Contains" && m.GetParameters().Length == 2);

    public static async Task<PaginatedData<TEntity>> GetItems<TDBContext, TEntity, TPropertyNameEnum, TId>(
               this IDbContextFactory<TDBContext> contextFactory,
               Func<TDBContext, IQueryable<TEntity>> query,
               BaseGetItemsRequest<TPropertyNameEnum, TId> request
        ) where TPropertyNameEnum : struct, IConvertible where TDBContext : DbContext where TEntity : class
    {
        return await contextFactory.GetItems(query, request, (item) => default, null);
    }

    public static async Task<PaginatedData<TEntity>> GetItems<TDBContext, TEntity, TPropertyNameEnum, TId>(
               this IDbContextFactory<TDBContext> contextFactory,
               Func<TDBContext, IQueryable<TEntity>> query,
               BaseGetItemsRequest<TPropertyNameEnum, TId> request,
               Expression<Func<TEntity, TId>> idAccessor,
               Func<TPropertyNameEnum, string[]>? propertyNameToString
        ) where TPropertyNameEnum : struct, IConvertible where TDBContext : DbContext where TEntity : class
    {
        await using var totalContext = await contextFactory.CreateDbContextAsync();
        await using var itemsContext = await contextFactory.CreateDbContextAsync();
        var idsHashSet = new HashSet<TId>(request.Ids ?? []);
        var exceptIdsHashSet = new HashSet<TId>(request.ExceptIds ?? []);

        IQueryable<TEntity> ApplyQueryFilters(IQueryable<TEntity> baseQuery) => baseQuery
            .FilterByIds(idsHashSet, exceptIdsHashSet, idAccessor)
            .Filters(request.Filters, propertyNameToString)
            .SortBy(request.Sort, propertyNameToString, idAccessor);

        var totalCountTask = !request.TotalCount.HasValue
            ? ApplyQueryFilters(query(totalContext)).TotalCount(request, propertyNameToString)
            : Task.FromResult(request.TotalCount.Value);

        var totalItemsQuery = ApplyQueryFilters(query(itemsContext));
        var totalItemsTask = totalItemsQuery
            .Paginate(request, propertyNameToString, idAccessor);

        await Task.WhenAll(totalCountTask, totalItemsTask);
        return new Tuple<TEntity[], long>(totalItemsTask.Result, totalCountTask.Result).ToPaginatedData(request);
    }

    public static PaginatedData<TEntity> ToPaginatedData<TEntity, TPropertyNameEnum, TId>(
               this Tuple<TEntity[], long> paginated,
                      BaseGetItemsRequest<TPropertyNameEnum, TId> request) where TPropertyNameEnum : struct, IConvertible
    {
        return new PaginatedData<TEntity>(paginated.Item1, request.Page ?? PaginationConstants.DefaultPage, request.Count ?? PaginationConstants.DefaultCount, paginated.Item2);
    }

    public static Task<TEntity[]> Paginate<TEntity, TPropertyNameEnum, TId>(
    this IQueryable<TEntity> query,
        BaseGetItemsRequest<TPropertyNameEnum, TId> request,
        Func<TPropertyNameEnum, string[]>? propertyNameToString,
        Expression<Func<TEntity, TId>> idAccessor) where TPropertyNameEnum : struct, IConvertible
    {
        var page = request.Page ?? PaginationConstants.DefaultPage;
        var count = request.Count ?? PaginationConstants.DefaultCount;
        var skip = request.Skip ?? PaginationConstants.DefaultSkip;
        var finalSkip = (page > 1 ? count * (page - 1) : 0) + skip;
        return query
            .Skip(finalSkip)
            .Take(count)
            .ToArrayAsync();
    }

    public static IQueryable<TEntity> FilterByIds<TEntity, TId>(
        this IQueryable<TEntity> query,
        HashSet<TId> idsHashSet,
        HashSet<TId> exceptIdsHashSet,
        Expression<Func<TEntity, TId>> idAccessor)
    {
        if (idsHashSet.Count == 0 && exceptIdsHashSet.Count == 0)
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

    public static Task<long> TotalCount<TEntity, TPropertyNameEnum, TId>(
        this IQueryable<TEntity> query,
        BaseGetItemsRequest<TPropertyNameEnum, TId> request,
        Func<TPropertyNameEnum, string[]>? propertyNameToString) where TPropertyNameEnum : struct, IConvertible
        => query
            .LongCountAsync();

    public static IQueryable<TEntity> Filters<TEntity, TPropertyNameEnum>(
        this IQueryable<TEntity> query,
        GetItemsFilter<TPropertyNameEnum>[]? filters,
        Func<TPropertyNameEnum, string[]>? propertyNameToString
    ) where TPropertyNameEnum : struct, IConvertible
    {
        // Ignore other code in the other methods, what I want to do here is create one single lambda expression that will be used in the Where method
        // To achieve this, I will first create a param. Then I will map the array of entities to a list of expressions . Then I will use the Aggregate method to combine all the expressions into one
        if (filters != null)
        {
            var param = Expression.Parameter(typeof(TEntity), "e");
            var expression = GetFiltersExpression(param, filters, propertyNameToString);
            var expressionLambda = Expression.Lambda<Func<TEntity, bool>>(expression, param);
            var whereQuery = query.Where(expressionLambda);
            return whereQuery;
        }
        return query;
    }

    public static Expression GetFiltersExpression<TPropertyNameEnum>(
        Expression param,
        GetItemsFilter<TPropertyNameEnum>[]? filters,
        Func<TPropertyNameEnum, string[]>? propertyNameToString) where TPropertyNameEnum : struct, IConvertible
    {
        if (filters != null)
        {
            var expressions = filters.Select(filter => new { Expression = GetFilterExpression(filter, param, propertyNameToString), Filter = filter }).ToList();
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

    public static Expression GetFilterExpression<TPropertyNameEnum>(
        GetItemsFilter<TPropertyNameEnum> filter,
        Expression param,
        Func<TPropertyNameEnum, string[]>? propertyNameToString) where TPropertyNameEnum : struct, IConvertible
    {
        var filtersExpression = (filter.Filters is not null && filter.Filters.Length > 0)
            ? GetFiltersExpression(param, filter.Filters, propertyNameToString)
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

        var propertyNamesByArrayAccessor = NestedPropertyByArrayAccessor(param, propertyNames);
        if (propertyNamesByArrayAccessor.Length > 2)
        {
            throw new ArgumentException("Nested array accessor is not supported. Hint: In your GetPropertyNameString, you included more than 1 array.");
        }

        if (propertyNamesByArrayAccessor.Length == 1)
        {
            var member = propertyNamesByArrayAccessor.First();
            var body = GetBodyExpression(filter, member, GetConstantExpression(filter, member));

            return filtersExpression is not null
                ? LogicExpression(body, filtersExpression, filter.FiltersLogic)
                : body;
        }
        else
        {
            var hasValues = filter.Values is not null && filter.Values.Length != 0;
            var member = propertyNamesByArrayAccessor.Last();
            var body = GetBodyExpression(filter, member, GetConstantExpression(filter, member));
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

    public static Expression[] NestedPropertyByArrayAccessor(Expression param, string[] propertyNames)
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
                if (ExpressionsRepository.Expressions.TryGetValue(expressionName, out var transformExpression))
                {
                    // Apply transformation function
                    propertyNamesList[^1] = transformExpression(result);
                    continue;
                }
                else
                {
                    throw new ArgumentException($"Expression '{expressionName}' not found in repository.");
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

    public static Expression LogicExpression(Expression left, Expression right, FilterLogicEnum logic)
    {
        return logic == FilterLogicEnum.And ? Expression.AndAlso(left, right) : Expression.OrElse(left, right);
    }

    public static IQueryable<TEntity> SortBy<TEntity, TPropertyNameEnum, TId>(
        this IQueryable<TEntity> source,
        IEnumerable<GetItemsSorter<TPropertyNameEnum>>? sort,
        Func<TPropertyNameEnum, string[]>? propertyNameToString,
        Expression<Func<TEntity, TId>>? idAccessor) where TPropertyNameEnum : struct, IConvertible
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
                    var propertyAccess = NestedPropertyByArrayAccessor(parameter, propertyNames);
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
    public static string[] GetPropertyNameString<TPropertyNameEnum>(
               this TPropertyNameEnum field,
                      Func<TPropertyNameEnum, string[]>? propertyNameToString) where TPropertyNameEnum : struct, IConvertible
    {
        return propertyNameToString?.Invoke(field) ?? [field.ToString()];
    }

    private static Expression GetBodyExpression<TPropertyNameEnum>(GetItemsFilter<TPropertyNameEnum> filter, Expression member, Expression constant)
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
            FilterOperatorEnum.ContainsAll => GetContainsAllExpression(memberValue, constant, memberHasValue, filter.Values, underlyingType, isNullable),
            FilterOperatorEnum.NotContainsAll => Expression.Not(GetContainsAllExpression(memberValue, constant, memberHasValue, filter.Values, underlyingType, isNullable)),
            FilterOperatorEnum.Flag => GetFlagExpression(memberValue, constant, memberHasValue, isNullable, isEnum || isNumeric, isBoolean),
            FilterOperatorEnum.NotFlag => GetNotFlagExpression(memberValue, constant, memberHasValue, isNullable, isEnum || isNumeric, isBoolean),
            FilterOperatorEnum.AnyFlag => GetAnyFlagExpression(memberValue, constant, memberHasValue, isNullable, isEnum || isNumeric, isBoolean),
            FilterOperatorEnum.NotAnyFlag => GetNotAnyFlagExpression(memberValue, constant, memberHasValue, isNullable, isEnum || isNumeric, isBoolean),
            _ => throw new ArgumentOutOfRangeException(nameof(filter.Operator), filter.Operator, "Unsupported filter operator.")
        };
    }

    public static PropertyInfo? GetNestedProperty(Type type, params string[] propertyNames)
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

        var hasAnyFlag = Expression.NotEqual(Expression.And(memberValueInt, constantInt), Expression.Constant(0, typeof(int)));

        var atLeastOneFlagNotSet = Expression.NotEqual(Expression.And(memberValueInt, constantInt), constantInt);

        return isNullable
            ? Expression.Condition(memberHasValue, atLeastOneFlagNotSet, Expression.Constant(false))
            : atLeastOneFlagNotSet;
    }

    private static Expression GetEqualityExpression(Expression memberValue, Expression constant, Expression memberHasValue, bool isNullable, bool isDateTime, bool isDateOnly, bool constantIsNull)
    {
        if (isDateTime)
        {
            var memberTicks = Expression.Property(memberValue, "Ticks");
            var constantTicks = Expression.Property(constant, "Ticks");
            var memberTicksFloored = Expression.Divide(memberTicks, Expression.Constant(TimeSpan.TicksPerMillisecond));
            var constantTicksFloored = Expression.Divide(constantTicks, Expression.Constant(TimeSpan.TicksPerMillisecond));
            return isNullable
                ? Expression.Condition(memberHasValue, Expression.Equal(memberTicksFloored, constantTicksFloored), Expression.Constant(false))
                : Expression.Equal(memberTicksFloored, constantTicksFloored);
        }
        if (isDateOnly)
        {
            var memberDayNumber = Expression.Property(memberValue, "DayNumber");
            var constantDayNumber = Expression.Property(constant, "DayNumber");

            return isNullable
                ? Expression.Condition(memberHasValue, Expression.Equal(memberDayNumber, constantDayNumber), Expression.Constant(false))
                : Expression.Equal(memberDayNumber, constantDayNumber);
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

    private static Expression GetStringOperationExpression(string methodName, Expression memberValue, Expression constant, string[]? values)
    {
        if (values != null && values.Any() || memberValue.Type != typeof(string))
        {
            return Expression.Constant(false);
        }

        return Expression.Call(memberValue, methodName, Type.EmptyTypes, constant);
    }

    private static Expression GetContainsExpression(Expression memberValue, Expression constant, Expression memberHasValue, string[]? values, Type underlyingType, bool isNullable)
    {
        var hasValues = values != null && values.Any();
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
        var hasValues = values != null && values.Any();
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
               type == typeof(ulong) || type == typeof(char) || type == typeof(DateTime);
    }

    private static object ParseSingleValue(string value, Type type)
    {
        if (value is null)
        {
            return null;
        }
        return type switch
        {
            _ when type == typeof(Guid) => ParseFlexibleGuid(value),
            _ when type == typeof(float) => Convert.ToSingle(value),
            _ when type == typeof(double) => Convert.ToDouble(value),
            _ when type == typeof(short) => Convert.ToInt16(value),
            _ when type == typeof(int) => Convert.ToInt32(value),
            _ when type == typeof(long) => Convert.ToInt64(value),
            _ when type == typeof(bool) => value == "1" ? true : value == "0" ? false : Convert.ToBoolean(value),
            _ when type == typeof(char) => Convert.ToChar(value),
            _ when type == typeof(byte) => Convert.ToByte(value),
            _ when type == typeof(sbyte) => Convert.ToSByte(value),
            _ when type == typeof(ushort) => Convert.ToUInt16(value),
            _ when type == typeof(uint) => Convert.ToUInt32(value),
            _ when type == typeof(decimal) => Convert.ToDecimal(value),
            _ when type == typeof(ulong) => Convert.ToUInt64(value),
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
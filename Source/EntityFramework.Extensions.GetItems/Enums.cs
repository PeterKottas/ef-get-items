namespace EntityFramework.Extensions.GetItems;

/// <summary>
/// Specifies how pagination metadata should be calculated.
/// </summary>
public enum PaginationHandlingEnum
{
    /// <summary>
    /// Runs a separate COUNT query to get the total count in parallel with the data query.
    /// Most accurate but requires an additional database round-trip.
    /// Provides <see cref="PaginatedData{TEntity}.TotalCount"/> and <see cref="PaginatedData{TEntity}.TotalPages"/>.
    /// </summary>
    Expensive,
    
    /// <summary>
    /// Fetches one extra item to determine if there's a next page.
    /// More efficient than Expensive but doesn't provide total count.
    /// Provides <see cref="PaginatedData{TEntity}.HasNextPage"/>.
    /// </summary>
    Cheap,
    
    /// <summary>
    /// No pagination metadata is calculated. Only returns the requested items.
    /// Most efficient when pagination metadata is not needed.
    /// </summary>
    None
}

/// <summary>
/// Specifies the sort direction.
/// </summary>
public enum OrderByEnum
{
    /// <summary>
    /// Sort in ascending order (A-Z, 0-9, oldest first).
    /// </summary>
    Ascending,
    
    /// <summary>
    /// Sort in descending order (Z-A, 9-0, newest first).
    /// </summary>
    Descending
}

/// <summary>
/// Specifies the comparison operator for filter conditions.
/// </summary>
public enum FilterOperatorEnum
{
    /// <summary>
    /// Equal to. Matches when the property value equals the filter value.
    /// </summary>
    Eq,
    
    /// <summary>
    /// Not equal to. Matches when the property value does not equal the filter value.
    /// </summary>
    Neq,
    
    /// <summary>
    /// Less than. For numeric, date, and enum comparisons.
    /// </summary>
    Lt,
    
    /// <summary>
    /// Less than or equal to. For numeric, date, and enum comparisons.
    /// </summary>
    Lte,
    
    /// <summary>
    /// Greater than. For numeric, date, and enum comparisons.
    /// </summary>
    Gt,
    
    /// <summary>
    /// Greater than or equal to. For numeric, date, and enum comparisons.
    /// </summary>
    Gte,
    
    /// <summary>
    /// String starts with. For string properties only.
    /// </summary>
    StartsWith,
    
    /// <summary>
    /// String ends with. For string properties only.
    /// </summary>
    EndsWith,
    
    /// <summary>
    /// Contains. For strings: substring match. With Values array: checks if value is in the list.
    /// </summary>
    Contains,
    
    /// <summary>
    /// Does not contain. Inverse of <see cref="Contains"/>.
    /// </summary>
    NotContains,
    
    /// <summary>
    /// Contains all values. For collection properties, checks if all specified values are present.
    /// Requires <see cref="GetItemsFilter{TPropertyNameEnum}.Values"/> to be set.
    /// </summary>
    ContainsAll,
    
    /// <summary>
    /// Does not contain all values. Inverse of <see cref="ContainsAll"/>.
    /// </summary>
    NotContainsAll,
    
    /// <summary>
    /// Bitwise flag check. Matches when the specified flag is set (value &amp; flag == flag).
    /// For enum and numeric properties with [Flags] attribute.
    /// </summary>
    Flag,
    
    /// <summary>
    /// Bitwise flag not set. Matches when the specified flag is not set (value &amp; flag == 0).
    /// </summary>
    NotFlag,
    
    /// <summary>
    /// Any flag set. Matches when any of the specified flags are set (value &amp; flag != 0).
    /// </summary>
    AnyFlag,
    
    /// <summary>
    /// No flags set. Matches when at least one of the specified flags is not set.
    /// </summary>
    NotAnyFlag
}

/// <summary>
/// Specifies how filter conditions are combined.
/// </summary>
public enum FilterLogicEnum
{
    /// <summary>
    /// Logical AND. Both conditions must be true.
    /// </summary>
    And,
    
    /// <summary>
    /// Logical OR. Either condition can be true.
    /// </summary>
    Or
}

/// <summary>
/// Specifies how collection properties are filtered.
/// </summary>
public enum FilterArrayAccessorLogic
{
    /// <summary>
    /// Match if ANY item in the collection satisfies the condition (LINQ Any).
    /// </summary>
    Any,
    
    /// <summary>
    /// Match if ALL items in the collection satisfy the condition (LINQ All).
    /// </summary>
    All
}

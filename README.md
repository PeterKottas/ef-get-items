# GetItemsExtensions Example Showcase

This project demonstrates the usage of the `GetItemsExtensions` utility for filtering, sorting, and paginating database queries using LINQ expressions.

## Project Structure

### Models
- **Author**: Represents an author with properties:
  - `Id` (Guid)
  - `Name` (string)
  - `Surname` (string)
  - `DateOfBirth` (DateTime)
  - `Gender` (enum: Male | Female)
  - `Books` (Collection of Book entities)

- **Book**: Represents a book with properties:
  - `Id` (Guid)
  - `Name` (string)
  - `PublishedOn` (DateTime)
  - `Authors` (Collection of Author entities)

### Database
- Uses **in-memory database** for easy testing without external dependencies
- Many-to-many relationship between Authors and Books
- Pre-seeded with **20 classic authors** and **40 books** for comprehensive testing

### Controllers
- **AuthorsController**: Provides `/api/authors/get` endpoint
- **BooksController**: Provides `/api/books/get` endpoint

Both endpoints accept POST requests with filtering, sorting, and pagination parameters.

## API Usage Examples

### Dashboard Endpoint

Get a comprehensive dashboard showcasing advanced filtering, sorting, and pagination:

```http
GET /api/dashboard
```

**Response:**
```json
{
  "lastPublishedBooks": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "name": "Kafka on the Shore",
      "publishedOn": "2002-09-12T00:00:00"
    },
    {
      "id": "550e8400-e29b-41d4-a716-446655440001",
      "name": "The Da Vinci Code",
      "publishedOn": "2003-03-18T00:00:00"
    }
  ],
  "prolificAuthorBooks": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440200",
      "name": "1984",
      "publishedOn": "1949-06-08T00:00:00"
    },
    {
      "id": "550e8400-e29b-41d4-a716-446655440201",
      "name": "Animal Farm",
      "publishedOn": "1945-08-17T00:00:00"
    }
  ],
  "classicAuthorsByAge": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440300",
      "name": "Emily",
      "surname": "Brontë",
      "dateOfBirth": "1818-07-30T00:00:00"
    },
    {
      "id": "550e8400-e29b-41d4-a716-446655440301",
      "name": "Charlotte",
      "surname": "Brontë",
      "dateOfBirth": "1816-04-21T00:00:00"
    }
  ]
}
```

The Dashboard endpoint showcases 3 advanced query patterns:

1. **Multi-Level Sorting** - Books sorted by publication date (newest first), then alphabetically by name
2. **Nested Property Filtering** - Books filtered by author names using the nested Authors relationship (contains "Orwell")
3. **Complex Multi-Filter with Date Range** - Female authors born in the 19th century using chained AND conditions, sorted by birth date and name

---

### Example 1: Get Authors with Gender Filter and Pagination

```json
POST /api/authors/get
{
  "page": 1,
  "count": 10,
  "filters": [
    {
      "field": "Gender",
      "operator": "Eq",
      "value": "Male"
    }
  ],
  "sort": [
    {
      "field": "Surname",
      "order": "Ascending"
    }
  ]
}
```

### Example 2: Get Books Published in a Specific Date Range

```json
POST /api/books/get
{
  "page": 1,
  "count": 15,
  "filters": [
    {
      "field": "PublishedOn",
      "operator": "Gte",
      "value": "1900-01-01"
    },
    {
      "field": "PublishedOn",
      "operator": "Lte",
      "value": "2000-12-31"
    }
  ],
  "sort": [
    {
      "field": "PublishedOn",
      "order": "Descending"
    }
  ]
}
```

### Example 3: Find Authors by Name Pattern

```json
POST /api/authors/get
{
  "page": 1,
  "count": 10,
  "filters": [
    {
      "field": "Name",
      "operator": "StartsWith",
      "value": "George"
    }
  ]
}
```

### Example 4: Find Authors by Their Books (Nested Property Filter)

Filter authors who have written books with names containing specific text:

```json
POST /api/authors/get
{
  "page": 1,
  "count": 10,
  "filters": [
    {
      "field": "BookName",
      "operator": "Contains",
      "value": "Love"
    }
  ],
  "sort": [
    {
      "field": "Name",
      "order": "Ascending"
    }
  ]
}
```

**Result**: Returns all authors who have written books with "Love" in the title (e.g., "Love in the Time of Cholera")

### Example 5: Find Books by Author Names (Nested Property Filter)

Filter books written by specific authors using nested property access:

```json
POST /api/books/get
{
  "page": 1,
  "count": 20,
  "filters": [
    {
      "field": "AuthorName",
      "operator": "Eq",
      "value": "Orwell"
    }
  ],
  "sort": [
    {
      "field": "PublishedOn",
      "order": "Ascending"
    }
  ]
}
```

**Result**: Returns all books written by George Orwell, sorted by publication date

### Example 6: Complex Multi-Filter with OR Logic

Find female authors born in the 19th century:

```json
POST /api/authors/get
{
  "page": 1,
  "count": 10,
  "filters": [
    {
      "field": "Gender",
      "operator": "Eq",
      "value": "Female"
    },
    {
      "field": "DateOfBirth",
      "operator": "Gte",
      "value": "1800-01-01",
      "logic": "And"
    },
    {
      "field": "DateOfBirth",
      "operator": "Lt",
      "value": "1900-01-01",
      "logic": "And"
    }
  ],
  "sort": [
    {
      "field": "DateOfBirth",
      "order": "Descending"
    }
  ]
}
```

### Example 7: Case-Insensitive Author Search

Find authors by surname using partial match:

```json
POST /api/authors/get
{
  "page": 1,
  "count": 25,
  "filters": [
    {
      "field": "Surname",
      "operator": "Contains",
      "value": "Brontë"
    }
  ]
}
```

**Result**: Returns all authors with "Brontë" in their surname (Charlotte and Emily Brontë)

### Example 8: Pagination Example - Get Second Page

```json
POST /api/authors/get
{
  "page": 2,
  "count": 5,
  "sort": [
    {
      "field": "Name",
      "order": "Ascending"
    }
  ]
}
```

**Result**: Returns 5 authors per page, page 2 (items 6-10), sorted alphabetically by first name

### Example 9: Find Books by Author - Multiple Conditions

```json
POST /api/books/get
{
  "page": 1,
  "count": 10,
  "filters": [
    {
      "field": "AuthorName",
      "operator": "StartsWith",
      "value": "Stephen"
    },
    {
      "field": "PublishedOn",
      "operator": "Gte",
      "value": "1977-01-01",
      "logic": "And"
    }
  ]
}
```

**Result**: Returns all books by authors whose first name starts with "Stephen", published after 1977

### Example 10: Get Most Recent Books with Multi-level Sorting

```json
POST /api/books/get
{
  "page": 1,
  "count": 20,
  "filters": [
    {
      "field": "PublishedOn",
      "operator": "Gte",
      "value": "1980-01-01"
    }
  ],
  "sort": [
    {
      "field": "PublishedOn",
      "order": "Descending"
    },
    {
      "field": "Name",
      "order": "Ascending"
    }
  ]
}
```

**Result**: Returns books published since 1980, sorted by publication date (newest first), then alphabetically by name

## Supported Filter Operators

The `GetItemsExtensions` supports the following operators:
- `Eq` - Equal
- `Neq` - Not Equal
- `Lt` - Less Than
- `Lte` - Less Than or Equal
- `Gt` - Greater Than
- `Gte` - Greater Than or Equal
- `StartsWith` - String starts with
- `EndsWith` - String ends with
- `Contains` - Contains substring
- `NotContains` - Does not contain
- `ContainsAll` - Contains all values (for arrays)
- `NotContainsAll` - Does not contain all values
- `Flag` - Bitwise flag check
- `NotFlag` - Bitwise flag not set
- `AnyFlag` - Has any flag set
- `NotAnyFlag` - Has no flags set

## Advanced Features

### Nested Property Filtering
The extension supports filtering on **nested/related entity properties** using a property accessor pattern:

```json
{
  "field": "BookName",
  "operator": "Contains",
  "value": "Love"
}
```

This allows you to filter:
- **Authors** by properties of their related **Books** (e.g., `BookName`)
- **Books** by properties of their related **Authors** (e.g., `AuthorName`)

### Complex Filter Logic
Combine multiple filters with AND/OR logic:

```json
"filters": [
  {
    "field": "Gender",
    "operator": "Eq",
    "value": "Female"
  },
  {
    "field": "DateOfBirth",
    "operator": "Gte",
    "value": "1800-01-01",
    "logic": "And"
  }
]
```

Each filter's `logic` property determines how it combines with the previous filter:
- `And` - Both conditions must be true
- `Or` - Either condition can be true (default: `And`)

### Multi-Level Sorting
Sort by multiple properties in sequence:

```json
"sort": [
  {
    "field": "PublishedOn",
    "order": "Descending"
  },
  {
    "field": "Name",
    "order": "Ascending"
  }
]
```

This sorts by publication date first (newest), then alphabetically by name for ties.

### Array Accessor Logic
When filtering nested collections, control matching behavior:
- `Any` - Match if ANY item in the collection satisfies the condition
- `All` - Match if ALL items in the collection satisfy the condition

## Sorting

You can sort by any property in ascending or descending order:

```json
"sort": [
  {
    "field": "DateOfBirth",
    "order": "Descending"
  }
]
```

## Pagination

Control pagination with `page` and `count` parameters:

```json
{
  "page": 1,
  "count": 25
}
```

- `page`: 1-based page number (default: 1)
- `count`: Items per page (default: 25)
- `skip`: Additional items to skip (default: 0)

## Key Features

? **Filtering**: Complex filter expressions with AND/OR logic
? **Sorting**: Multi-level sorting with ascending/descending order
? **Pagination**: Offset-based pagination with configurable page size
? **Type-Safe**: Enum-based property access
? **Expression Trees**: Uses LINQ expression trees for efficient database queries
? **In-Memory Database**: No external database setup required

## Implementation Details

The `GetItemsExtensions` class provides extension methods on `IDbContextFactory<TDBContext>`:
- `GetItems<TEntity>()` - Main method that applies filters, sorting, and pagination
- `Filters()` - Applies filter expressions
- `SortBy()` - Applies sorting
- `Paginate()` - Applies pagination
- `TotalCount()` - Gets total count of filtered items

All database operations are executed asynchronously for better performance.

## Usage in Code

The `DashboardController` demonstrates how to use the GetItemsExtensions within C# code. It showcases three advanced patterns:

### Pattern 1: Multi-Level Sorting
Sort by multiple properties in sequence (publication date DESC, then name ASC):

```csharp
var booksRequest = new GetBooksRequest
{
    Page = 1,
    Count = 5,
    Sort = new[]
    {
        new GetItemsSorter<BookPropertyNames>
        {
            Field = BookPropertyNames.PublishedOn,
            Order = OrderByEnum.Descending
        },
        new GetItemsSorter<BookPropertyNames>
        {
            Field = BookPropertyNames.Name,
            Order = OrderByEnum.Ascending
        }
    }
};

var booksPaginatedData = await _contextFactory.GetItems(
    context => context.Books.AsQueryable(),
    booksRequest,
    b => b.Id,
    PropertyNameMappersUtil.BookPropertyNameMapper
);
```

### Pattern 2: Nested Property Filtering
Filter books by author names using the nested Authors relationship:

```csharp
var orwellBooksRequest = new GetBooksRequest
{
    Page = 1,
    Count = 10,
    Filters = new[]
    {
        new GetItemsFilter<BookPropertyNames>
        {
            Field = BookPropertyNames.AuthorName,
            Operator = FilterOperatorEnum.Contains,
            Value = "Orwell"
        }
    },
    Sort = new[]
    {
        new GetItemsSorter<BookPropertyNames>
        {
            Field = BookPropertyNames.PublishedOn,
            Order = OrderByEnum.Ascending
        }
    }
};

var orwellBooksPaginatedData = await _contextFactory.GetItems(
    context => context.Books.AsQueryable(),
    orwellBooksRequest,
    b => b.Id,
    PropertyNameMappersUtil.BookPropertyNameMapper
);
```

### Pattern 3: Complex Multi-Filter with Date Range
Find female authors born in the 19th century using chained AND conditions:

```csharp
var classicAuthorsRequest = new GetAuthorsRequest
{
    Page = 1,
    Count = 10,
    Filters = new[]
    {
        new GetItemsFilter<AuthorPropertyNames>
        {
            Field = AuthorPropertyNames.Gender,
            Operator = FilterOperatorEnum.Eq,
            Value = "Female"
        },
        new GetItemsFilter<AuthorPropertyNames>
        {
            Field = AuthorPropertyNames.DateOfBirth,
            Operator = FilterOperatorEnum.Gte,
            Value = "1800-01-01",
            Logic = FilterLogicEnum.And
        },
        new GetItemsFilter<AuthorPropertyNames>
        {
            Field = AuthorPropertyNames.DateOfBirth,
            Operator = FilterOperatorEnum.Lt,
            Value = "1900-01-01",
            Logic = FilterLogicEnum.And
        }
    },
    Sort = new[]
    {
        new GetItemsSorter<AuthorPropertyNames>
        {
            Field = AuthorPropertyNames.DateOfBirth,
            Order = OrderByEnum.Descending
        },
        new GetItemsSorter<AuthorPropertyNames>
        {
            Field = AuthorPropertyNames.Name,
            Order = OrderByEnum.Ascending
        }
    }
};

var classicAuthorsPaginatedData = await _contextFactory.GetItems(
    context => context.Authors.AsQueryable(),
    classicAuthorsRequest,
    a => a.Id,
    PropertyNameMappersUtil.AuthorPropertyNameMapper
);
```

The `DashboardController` endpoint (`GET /api/dashboard`) combines all three patterns to return a comprehensive summary of library data, demonstrating the power and flexibility of the GetItemsExtensions library.

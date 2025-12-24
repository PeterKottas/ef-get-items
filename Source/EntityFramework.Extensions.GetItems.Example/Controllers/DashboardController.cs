namespace EntityFramework.Extensions.GetItems.Example.Controllers;

using EntityFramework.Extensions.GetItems;
using EntityFramework.Extensions.GetItems.Example.Data;
using EntityFramework.Extensions.GetItems.Example.Models;
using EntityFramework.Extensions.GetItems.Example.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class DashboardResponse
{
    public Book[] LastPublishedBooks { get; set; } = Array.Empty<Book>();
    public Book[] OrwellBooks { get; set; } = Array.Empty<Book>();
    public Author[] ClassicAuthorsByAge { get; set; } = Array.Empty<Author>();
}

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDbContextFactory<LibraryDbContext> _contextFactory;

    public DashboardController(IDbContextFactory<LibraryDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    [HttpGet]
    [ProducesResponseType(typeof(DashboardResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard()
    {
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
            PropertyNameMappersUtil.BookPropertyNameToPath
        );

        var orwellBooksRequest = new GetBooksRequest
        {
            Page = 1,
            Count = 10,
            Filters = new[]
            {
                new GetItemsFilter<BookPropertyNames>
                {
                    Field = BookPropertyNames.AuthorName,
                    Operator = FilterOperatorEnum.Eq,
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
            PropertyNameMappersUtil.BookPropertyNameToPath
        );

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
            PropertyNameMappersUtil.AuthorPropertyNameToPath
        );

        var response = new DashboardResponse
        {
            LastPublishedBooks = booksPaginatedData.Items,
            OrwellBooks = orwellBooksPaginatedData.Items,
            ClassicAuthorsByAge = classicAuthorsPaginatedData.Items
        };

        return Ok(response);
    }
}


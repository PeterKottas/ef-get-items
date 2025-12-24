namespace FilteringTest.Controllers;

using FilteringTest.Data;
using FilteringTest.Models;
using FilteringTest.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public enum BookPropertyNames
{
    Name,
    PublishedOn,
    AuthorName,
    AuthorSurname
}

// Request/Response DTOs for Books
public class GetBooksRequest : BaseGetItemsRequest<BookPropertyNames, Guid>
{
}

public class GetBooksResponse
{
    public PaginatedData<Book> Items { get; set; } = null!;
}

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IDbContextFactory<LibraryDbContext> _contextFactory;

    public BooksController(IDbContextFactory<LibraryDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    [HttpPost("get")]
    [ProducesResponseType(typeof(GetBooksResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBooks([FromBody] GetBooksRequest request)
    {
        var paginatedData = await _contextFactory.GetItems(
            context => context.Books.AsQueryable(),
            request,
            b => b.Id,
            PropertyNameMappersUtil.BookPropertyNameToPath
        );

        var response = new GetBooksResponse
        {
            Items = paginatedData
        };
        return Ok(response);
    }
}
namespace FilteringTest.Controllers;

using FilteringTest.Data;
using FilteringTest.Models;
using FilteringTest.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// Enums for property names
public enum AuthorPropertyNames
{
    Name,
    Surname,
    DateOfBirth,
    Gender,
    BookName
}

// Request/Response DTOs for Authors
public class GetAuthorsRequest : BaseGetItemsRequest<AuthorPropertyNames, Guid>
{
}

public class GetAuthorsResponse
{
    public PaginatedData<Author> Items { get; set; } = null!;
}

[ApiController]
[Route("api/[controller]")]
public class AuthorsController : ControllerBase
{
    private readonly IDbContextFactory<LibraryDbContext> _contextFactory;

    public AuthorsController(IDbContextFactory<LibraryDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    [HttpPost("get")]
    [ProducesResponseType(typeof(GetAuthorsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuthors([FromBody] GetAuthorsRequest request)
    {
        var paginatedData = await _contextFactory.GetItems(
            context => context.Authors.AsQueryable(),
            new GetAuthorsRequest
            {
                Count = 25,
                Filters = [
                    new(){
                        Field = AuthorPropertyNames.Name,
                        Value = "John",
                        Operator = FilterOperatorEnum.StartsWith
                    }
                    ]
            },
            a => a.Id,
            PropertyNameMappersUtil.AuthorPropertyNameToPath
        );

        var response = new GetAuthorsResponse
        {
            Items = paginatedData
        };
        return Ok(response);
    }
}
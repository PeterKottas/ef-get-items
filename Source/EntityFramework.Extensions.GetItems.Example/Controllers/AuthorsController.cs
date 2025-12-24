namespace EntityFramework.Extensions.GetItems.Example.Controllers;

using System.Threading.Tasks;
using EntityFramework.Extensions.GetItems;
using EntityFramework.Extensions.GetItems.Example.Data;
using EntityFramework.Extensions.GetItems.Example.Models;
using EntityFramework.Extensions.GetItems.Example.Utils;
using Microsoft.AspNetCore.Http;
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
            request,
            a => a.Id,
            PropertyNameMappersUtil.AuthorPropertyNameToPath,
            new GetItemsOptions()
            {
                DebugQuery = true,
            }
        );

        var response = new GetAuthorsResponse
        {
            Items = paginatedData
        };
        return Ok(response);
    }
}


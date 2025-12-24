namespace FilteringTest.Utils;

using FilteringTest.Controllers;
using FilteringTest.Models;

public static class PropertyNameMappersUtil
{
    public static string[] AuthorPropertyNameToPath(AuthorPropertyNames field)
    {
        return field switch
        {
            AuthorPropertyNames.Name => [nameof(Author.Name)],
            AuthorPropertyNames.Surname => [nameof(Author.Surname)],
            AuthorPropertyNames.DateOfBirth => [nameof(Author.DateOfBirth)],
            AuthorPropertyNames.Gender => [nameof(Author.Gender)],
            AuthorPropertyNames.BookName => [nameof(Author.Books), nameof(Book.Name)],
            _ => throw new ArgumentOutOfRangeException(nameof(field), field, "Unsupported Author property name")
        };
    }

    public static string[] BookPropertyNameToPath(BookPropertyNames field)
    {
        return field switch
        {
            BookPropertyNames.Name => [nameof(Book.Name)],
            BookPropertyNames.PublishedOn => [nameof(Book.PublishedOn)],
            BookPropertyNames.AuthorName => [nameof(Book.Authors), nameof(Author.Name)],
            BookPropertyNames.AuthorSurname => [nameof(Book.Authors), nameof(Author.Surname)],
            _ => throw new ArgumentOutOfRangeException(nameof(field), field, "Unsupported Book property name")
        };
    }
}
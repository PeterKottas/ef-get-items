namespace FilteringTest.Models;

public enum Gender
{
    Male,
    Female
}

public class Author
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public ICollection<Book> Books { get; set; } = new List<Book>();
}

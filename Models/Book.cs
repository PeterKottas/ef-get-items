namespace FilteringTest.Models;

public class Book
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime PublishedOn { get; set; }
    public ICollection<Author> Authors { get; set; } = new List<Author>();
}

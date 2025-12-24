namespace EntityFramework.Extensions.GetItems.Example.Data;

using Microsoft.EntityFrameworkCore;
using EntityFramework.Extensions.GetItems.Example.Models;
using System.Linq;
using System.Collections.Generic;
using System;

public class LibraryDbContext : DbContext
{
    public DbSet<Author> Authors { get; set; }
    public DbSet<Book> Books { get; set; }

    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Author entity
        modelBuilder.Entity<Author>()
            .HasKey(a => a.Id);

        modelBuilder.Entity<Author>()
            .Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(255);

        modelBuilder.Entity<Author>()
            .Property(a => a.Surname)
            .IsRequired()
            .HasMaxLength(255);

        // Configure Book entity
        modelBuilder.Entity<Book>()
            .HasKey(b => b.Id);

        modelBuilder.Entity<Book>()
            .Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(255);

        // Configure many-to-many relationship
        modelBuilder.Entity<Book>()
            .HasMany(b => b.Authors)
            .WithMany(a => a.Books)
            .UsingEntity<Dictionary<string, object>>(
                "BookAuthor",
                r => r
                    .HasOne<Author>()
                    .WithMany()
                    .HasForeignKey("AuthorId"),
                l => l
                    .HasOne<Book>()
                    .WithMany()
                    .HasForeignKey("BookId"),
                je =>
                {
                    je.HasKey("BookId", "AuthorId");
                });

        // Seed data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Generate author IDs
        var authorIds = Enumerable.Range(0, 20).Select(_ => Guid.NewGuid()).ToArray();
        
        // Generate book IDs
        var bookIds = Enumerable.Range(0, 40).Select(_ => Guid.NewGuid()).ToArray();

        // Seed 20 Authors
        var authors = new List<Author>
        {
            new() { Id = authorIds[0], Name = "George", Surname = "Orwell", DateOfBirth = new DateTime(1903, 6, 25), Gender = Gender.Male },
            new() { Id = authorIds[1], Name = "Jane", Surname = "Austen", DateOfBirth = new DateTime(1775, 12, 16), Gender = Gender.Female },
            new() { Id = authorIds[2], Name = "Ernest", Surname = "Hemingway", DateOfBirth = new DateTime(1899, 7, 21), Gender = Gender.Male },
            new() { Id = authorIds[3], Name = "F. Scott", Surname = "Fitzgerald", DateOfBirth = new DateTime(1896, 9, 24), Gender = Gender.Male },
            new() { Id = authorIds[4], Name = "Charlotte", Surname = "Brontë", DateOfBirth = new DateTime(1816, 4, 21), Gender = Gender.Female },
            new() { Id = authorIds[5], Name = "Emily", Surname = "Brontë", DateOfBirth = new DateTime(1818, 7, 30), Gender = Gender.Female },
            new() { Id = authorIds[6], Name = "Charles", Surname = "Dickens", DateOfBirth = new DateTime(1812, 2, 7), Gender = Gender.Male },
            new() { Id = authorIds[7], Name = "Leo", Surname = "Tolstoy", DateOfBirth = new DateTime(1828, 9, 9), Gender = Gender.Male },
            new() { Id = authorIds[8], Name = "Fyodor", Surname = "Dostoevsky", DateOfBirth = new DateTime(1821, 11, 11), Gender = Gender.Male },
            new() { Id = authorIds[9], Name = "Mark", Surname = "Twain", DateOfBirth = new DateTime(1835, 11, 30), Gender = Gender.Male },
            new() { Id = authorIds[10], Name = "Oscar", Surname = "Wilde", DateOfBirth = new DateTime(1854, 10, 16), Gender = Gender.Male },
            new() { Id = authorIds[11], Name = "Virginia", Surname = "Woolf", DateOfBirth = new DateTime(1882, 1, 25), Gender = Gender.Female },
            new() { Id = authorIds[12], Name = "James", Surname = "Joyce", DateOfBirth = new DateTime(1882, 2, 2), Gender = Gender.Male },
            new() { Id = authorIds[13], Name = "Gabriel García", Surname = "Márquez", DateOfBirth = new DateTime(1927, 3, 6), Gender = Gender.Male },
            new() { Id = authorIds[14], Name = "Isabel", Surname = "Allende", DateOfBirth = new DateTime(1942, 8, 2), Gender = Gender.Female },
            new() { Id = authorIds[15], Name = "Haruki", Surname = "Murakami", DateOfBirth = new DateTime(1949, 1, 12), Gender = Gender.Male },
            new() { Id = authorIds[16], Name = "J.K.", Surname = "Rowling", DateOfBirth = new DateTime(1965, 7, 31), Gender = Gender.Female },
            new() { Id = authorIds[17], Name = "Stephen", Surname = "King", DateOfBirth = new DateTime(1947, 9, 21), Gender = Gender.Male },
            new() { Id = authorIds[18], Name = "Agatha", Surname = "Christie", DateOfBirth = new DateTime(1890, 1, 15), Gender = Gender.Female },
            new() { Id = authorIds[19], Name = "Dan", Surname = "Brown", DateOfBirth = new DateTime(1964, 6, 22), Gender = Gender.Male }
        };

        modelBuilder.Entity<Author>().HasData(authors);

        // Seed 40 Books
        var books = new List<Book>
        {
            new() { Id = bookIds[0], Name = "1984", PublishedOn = new DateTime(1949, 6, 8) },
            new() { Id = bookIds[1], Name = "Animal Farm", PublishedOn = new DateTime(1945, 8, 17) },
            new() { Id = bookIds[2], Name = "Pride and Prejudice", PublishedOn = new DateTime(1813, 1, 28) },
            new() { Id = bookIds[3], Name = "Emma", PublishedOn = new DateTime(1815, 12, 24) },
            new() { Id = bookIds[4], Name = "Sense and Sensibility", PublishedOn = new DateTime(1811, 10, 30) },
            new() { Id = bookIds[5], Name = "The Old Man and the Sea", PublishedOn = new DateTime(1952, 9, 1) },
            new() { Id = bookIds[6], Name = "For Whom the Bell Tolls", PublishedOn = new DateTime(1940, 10, 21) },
            new() { Id = bookIds[7], Name = "The Great Gatsby", PublishedOn = new DateTime(1925, 4, 10) },
            new() { Id = bookIds[8], Name = "Jane Eyre", PublishedOn = new DateTime(1847, 10, 19) },
            new() { Id = bookIds[9], Name = "Wuthering Heights", PublishedOn = new DateTime(1847, 12, 19) },
            new() { Id = bookIds[10], Name = "Oliver Twist", PublishedOn = new DateTime(1838, 3, 1) },
            new() { Id = bookIds[11], Name = "Great Expectations", PublishedOn = new DateTime(1860, 7, 1) },
            new() { Id = bookIds[12], Name = "War and Peace", PublishedOn = new DateTime(1869, 1, 1) },
            new() { Id = bookIds[13], Name = "Anna Karenina", PublishedOn = new DateTime(1877, 1, 1) },
            new() { Id = bookIds[14], Name = "Crime and Punishment", PublishedOn = new DateTime(1866, 1, 1) },
            new() { Id = bookIds[15], Name = "The Brothers Karamazov", PublishedOn = new DateTime(1879, 1, 1) },
            new() { Id = bookIds[16], Name = "The Adventures of Huckleberry Finn", PublishedOn = new DateTime(1884, 12, 10) },
            new() { Id = bookIds[17], Name = "The Picture of Dorian Gray", PublishedOn = new DateTime(1890, 7, 1) },
            new() { Id = bookIds[18], Name = "Mrs. Dalloway", PublishedOn = new DateTime(1925, 5, 14) },
            new() { Id = bookIds[19], Name = "Ulysses", PublishedOn = new DateTime(1922, 2, 2) },
            new() { Id = bookIds[20], Name = "One Hundred Years of Solitude", PublishedOn = new DateTime(1967, 5, 30) },
            new() { Id = bookIds[21], Name = "Love in the Time of Cholera", PublishedOn = new DateTime(1985, 12, 6) },
            new() { Id = bookIds[22], Name = "The House of the Spirits", PublishedOn = new DateTime(1982, 10, 6) },
            new() { Id = bookIds[23], Name = "Paula", PublishedOn = new DateTime(1994, 1, 1) },
            new() { Id = bookIds[24], Name = "Norwegian Wood", PublishedOn = new DateTime(1987, 9, 4) },
            new() { Id = bookIds[25], Name = "Kafka on the Shore", PublishedOn = new DateTime(2002, 9, 12) },
            new() { Id = bookIds[26], Name = "Harry Potter and the Philosopher's Stone", PublishedOn = new DateTime(1997, 6, 26) },
            new() { Id = bookIds[27], Name = "Harry Potter and the Chamber of Secrets", PublishedOn = new DateTime(1998, 7, 2) },
            new() { Id = bookIds[28], Name = "The Shining", PublishedOn = new DateTime(1977, 5, 28) },
            new() { Id = bookIds[29], Name = "It", PublishedOn = new DateTime(1986, 9, 15) },
            new() { Id = bookIds[30], Name = "Murder on the Orient Express", PublishedOn = new DateTime(1934, 1, 1) },
            new() { Id = bookIds[31], Name = "And Then There Were None", PublishedOn = new DateTime(1939, 11, 6) },
            new() { Id = bookIds[32], Name = "The Da Vinci Code", PublishedOn = new DateTime(2003, 3, 18) },
            new() { Id = bookIds[33], Name = "Angels & Demons", PublishedOn = new DateTime(2000, 5, 1) },
            new() { Id = bookIds[34], Name = "The Catcher in the Rye", PublishedOn = new DateTime(1951, 7, 16) },
            new() { Id = bookIds[35], Name = "To Kill a Mockingbird", PublishedOn = new DateTime(1960, 7, 11) },
            new() { Id = bookIds[36], Name = "The Lord of the Rings", PublishedOn = new DateTime(1954, 7, 29) },
            new() { Id = bookIds[37], Name = "The Hobbit", PublishedOn = new DateTime(1937, 9, 21) },
            new() { Id = bookIds[38], Name = "Moby Dick", PublishedOn = new DateTime(1851, 10, 18) },
            new() { Id = bookIds[39], Name = "The Odyssey", PublishedOn = new DateTime(1614, 1, 1) }
        };

        modelBuilder.Entity<Book>().HasData(books);

        // Create diverse many-to-many relationships
        var bookAuthorRelationships = new List<Dictionary<string, object>>()
        {
            new() { { "BookId", bookIds[0] }, { "AuthorId", authorIds[0] } }, // 1984 - Orwell
            new() { { "BookId", bookIds[1] }, { "AuthorId", authorIds[0] } }, // Animal Farm - Orwell
            new() { { "BookId", bookIds[2] }, { "AuthorId", authorIds[1] } }, // Pride and Prejudice - Austen
            new() { { "BookId", bookIds[3] }, { "AuthorId", authorIds[1] } }, // Emma - Austen
            new() { { "BookId", bookIds[4] }, { "AuthorId", authorIds[1] } }, // Sense and Sensibility - Austen
            new() { { "BookId", bookIds[5] }, { "AuthorId", authorIds[2] } }, // The Old Man and the Sea - Hemingway
            new() { { "BookId", bookIds[6] }, { "AuthorId", authorIds[2] } }, // For Whom the Bell Tolls - Hemingway
            new() { { "BookId", bookIds[7] }, { "AuthorId", authorIds[3] } }, // The Great Gatsby - Fitzgerald
            new() { { "BookId", bookIds[8] }, { "AuthorId", authorIds[4] } }, // Jane Eyre - Charlotte Brontë
            new() { { "BookId", bookIds[9] }, { "AuthorId", authorIds[5] } }, // Wuthering Heights - Emily Brontë
            new() { { "BookId", bookIds[10] }, { "AuthorId", authorIds[6] } }, // Oliver Twist - Dickens
            new() { { "BookId", bookIds[11] }, { "AuthorId", authorIds[6] } }, // Great Expectations - Dickens
            new() { { "BookId", bookIds[12] }, { "AuthorId", authorIds[7] } }, // War and Peace - Tolstoy
            new() { { "BookId", bookIds[13] }, { "AuthorId", authorIds[7] } }, // Anna Karenina - Tolstoy
            new() { { "BookId", bookIds[14] }, { "AuthorId", authorIds[8] } }, // Crime and Punishment - Dostoevsky
            new() { { "BookId", bookIds[15] }, { "AuthorId", authorIds[8] } }, // The Brothers Karamazov - Dostoevsky
            new() { { "BookId", bookIds[16] }, { "AuthorId", authorIds[9] } }, // Huckleberry Finn - Mark Twain
            new() { { "BookId", bookIds[17] }, { "AuthorId", authorIds[10] } }, // The Picture of Dorian Gray - Wilde
            new() { { "BookId", bookIds[18] }, { "AuthorId", authorIds[11] } }, // Mrs. Dalloway - Woolf
            new() { { "BookId", bookIds[19] }, { "AuthorId", authorIds[12] } }, // Ulysses - Joyce
            new() { { "BookId", bookIds[20] }, { "AuthorId", authorIds[13] } }, // One Hundred Years - Márquez
            new() { { "BookId", bookIds[21] }, { "AuthorId", authorIds[13] } }, // Love in the Time of Cholera - Márquez
            new() { { "BookId", bookIds[22] }, { "AuthorId", authorIds[14] } }, // The House of the Spirits - Allende
            new() { { "BookId", bookIds[23] }, { "AuthorId", authorIds[14] } }, // Paula - Allende
            new() { { "BookId", bookIds[24] }, { "AuthorId", authorIds[15] } }, // Norwegian Wood - Murakami
            new() { { "BookId", bookIds[25] }, { "AuthorId", authorIds[15] } }, // Kafka on the Shore - Murakami
            new() { { "BookId", bookIds[26] }, { "AuthorId", authorIds[16] } }, // Harry Potter 1 - Rowling
            new() { { "BookId", bookIds[27] }, { "AuthorId", authorIds[16] } }, // Harry Potter 2 - Rowling
            new() { { "BookId", bookIds[28] }, { "AuthorId", authorIds[17] } }, // The Shining - King
            new() { { "BookId", bookIds[29] }, { "AuthorId", authorIds[17] } }, // It - King
            new() { { "BookId", bookIds[30] }, { "AuthorId", authorIds[18] } }, // Murder on the Orient Express - Christie
            new() { { "BookId", bookIds[31] }, { "AuthorId", authorIds[18] } }, // And Then There Were None - Christie
            new() { { "BookId", bookIds[32] }, { "AuthorId", authorIds[19] } }, // The Da Vinci Code - Brown
            new() { { "BookId", bookIds[33] }, { "AuthorId", authorIds[19] } }, // Angels & Demons - Brown
            new() { { "BookId", bookIds[34] }, { "AuthorId", authorIds[3] } }, // The Catcher in the Rye - Fitzgerald
            new() { { "BookId", bookIds[35] }, { "AuthorId", authorIds[9] } }, // To Kill a Mockingbird - Twain
            new() { { "BookId", bookIds[36] }, { "AuthorId", authorIds[6] } }, // The Lord of the Rings - Dickens
            new() { { "BookId", bookIds[37] }, { "AuthorId", authorIds[11] } }, // The Hobbit - Woolf
            new() { { "BookId", bookIds[38] }, { "AuthorId", authorIds[10] } }, // Moby Dick - Wilde
            new() { { "BookId", bookIds[39] }, { "AuthorId", authorIds[0] } }  // The Odyssey - Orwell
        };

        modelBuilder.Entity("BookAuthor").HasData(bookAuthorRelationships);
    }
}


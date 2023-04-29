namespace Domain.Book;

public class Book
{
    public Book()
    {
        Id = IdGenerator.GenerateId();
    }
    public string Id { get; set; }
    public string? ISBN { get; set; }
    public string? Title { get; set; }
    public string? Author { get; set; }
}
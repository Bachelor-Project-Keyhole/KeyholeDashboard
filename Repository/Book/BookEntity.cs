namespace Repository.Book;

[BsonCollection("books")]
public class BookEntity : Document
{
    public string ISBN { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
}
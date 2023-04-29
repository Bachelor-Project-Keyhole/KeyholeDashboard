namespace Domain.Book;

public interface IBookRepository
{
    Task<Book> AddBook(Book book);
    Task<Book[]> GetAllBooks();
}
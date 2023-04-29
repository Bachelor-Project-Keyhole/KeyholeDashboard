using AutoMapper;
using Domain.Book;
using Microsoft.Extensions.Options;

namespace Repository.Book;

public class BookRepository : MongoRepository<BookEntity>, IBookRepository
{
    private readonly IMapper _mapper;

    public BookRepository(IOptions<DatabaseOptions> dataBaseOptions, IMapper mapper): base(dataBaseOptions)
    {
        _mapper = mapper;
    }
    
    public async Task<Domain.Book.Book> AddBook(Domain.Book.Book book)
    {
        var bookEntity = _mapper.Map<BookEntity>(book);
        await InsertOneAsync(bookEntity);
        return book;
    }

    public async Task<Domain.Book.Book[]> GetAllBooks()
    {
        var bookEntities = AsQueryable().Take(10).ToArray();
        return _mapper.Map<Domain.Book.Book[]>(bookEntities);
    }
}
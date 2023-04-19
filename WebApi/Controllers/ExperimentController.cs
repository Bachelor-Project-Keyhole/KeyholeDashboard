using Domain.Book;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class ExperimentController: ControllerBase
{
   private readonly IBookRepository _bookRepository;

   public ExperimentController(IBookRepository bookRepository)
   {
      _bookRepository = bookRepository;
   }

   [HttpPost]
   [Route("book")]
   public async Task<Book> AddBook([FromQuery] string isbn, [FromQuery] string title, [FromQuery] string author)
   {
      var book = new Book
      {
         ISBN = isbn,
         Title = title,
         Author = author
      };

      return await _bookRepository.AddBook(book);
   }
   
   [HttpGet]
   [Route("book")]
   public async Task<Book[]> GetAllBooks()
   {
      return await _bookRepository.GetAllBooks();
   }
}
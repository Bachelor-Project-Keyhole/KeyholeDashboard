using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Repository;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class ExperimentController: ControllerBase   
{
   [HttpPost]
   [Route("book")]
   public async Task<List<BookEntity>> AddBook([FromQuery] string isbn, [FromQuery] string title, [FromQuery] string author)
   {
      var bookEntity = new BookEntity
      {
         ISBN = isbn,
         Title = title,
         Author = author
      };

      var databaseDriver = new DatabaseDriver();
      var mongoDatabase = databaseDriver.Client.GetDatabase("keyhole-dashboard-db");
      var mongoCollection = mongoDatabase.GetCollection<BookEntity>("books");
      await mongoCollection.InsertOneAsync(bookEntity);
      var result = await mongoCollection.FindAsync(_ => true);

      return result.ToList();
   }  
}
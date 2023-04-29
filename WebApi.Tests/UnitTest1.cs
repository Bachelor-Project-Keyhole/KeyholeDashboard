using Domain.Book;
using FluentAssertions;
using Newtonsoft.Json;

namespace WebApi.Tests;

public class UnitTest1 : IntegrationTest
{
    [Fact]
    public async Task Test1()
    {
        await TestClient.PostAsync(new Uri("experiment/book?isbn=123&title=456&author=789", UriKind.Relative), null);
        var httpResponseMessage = 
            await TestClient.GetAsync(new Uri("experiment/book", UriKind.Relative));
        var deserializeObject = 
            JsonConvert.DeserializeObject<Book[]>(await httpResponseMessage.Content.ReadAsStringAsync());
        deserializeObject.Should().HaveCount(1);
    }
}
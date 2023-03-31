using System.Net;

namespace Service.ExceptionHandling.BaseExceptionHandlingService;

//TODO: WIP

public class BaseExceptionHandlingService : IBaseExceptionHandlingService
{
    public async Task<HttpResponseMessage> ExceptionMessage(string message)
    {
        // var errorEnum = (BaseExceptionMessage) Enum.Parse(typeof(BaseExceptionMessage), message);
        // if (Enum.IsDefined(typeof(BaseExceptionMessage), errorEnum))
        // {
        //     // Implement exception response
        // }
        return new HttpResponseMessage(HttpStatusCode.OK);
    }
}
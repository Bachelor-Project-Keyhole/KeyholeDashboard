namespace Service.ExceptionHandling.BaseExceptionHandlingService;

public interface IBaseExceptionHandlingService
{
    Task<HttpResponseMessage> ExceptionMessage(string message);
}
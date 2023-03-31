using Service.Email.EmailService;
using Service.ExceptionHandling.BaseExceptionHandlingService;

namespace WebApi.Registry;

static class ServiceRegistry
{
    public static void RegisterPersistence(this IServiceCollection collection)
    {
        #region Services

        #region Email Service

        collection.AddTransient<IEmailService, EmailService>();

        #endregion

        #region Exception Handeling

        collection.AddTransient<IBaseExceptionHandlingService, BaseExceptionHandlingService>();

        #endregion
        
        #endregion
    }
}
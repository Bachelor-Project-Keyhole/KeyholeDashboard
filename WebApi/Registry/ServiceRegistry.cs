using Google.Apis.Auth.AspNetCore3;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.Cookies;
using WebApi.Helper;
using WebApi.Services.MailKit;

namespace WebApi.Registry;

static class ServiceRegistry
{
    public static void RegisterPersistence(this IServiceCollection collection)
    {
        collection.AddTransient<IMailKitService, MailKitService>();
    }
}
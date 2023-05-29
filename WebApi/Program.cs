using System.Reflection;
using System.Text.Json.Serialization;
using Application.Email.Helper;
using Application.JWT.Model;
using Domain.Organization.OrganizationUserInvite;
using Domain.User;
using Infrastructure.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.OpenApi.Models;
using Repository;
using WebApi.Controllers.V1.Organization;
using WebApi.Registry;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services
    .AddControllers()
    .AddJsonOptions(opts =>
    {
        var enumConverter = new JsonStringEnumConverter();
        opts.JsonSerializerOptions.Converters.Add(enumConverter);
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

#region Config setup
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json").Build();

// Configure database options
builder.Services.Configure<DatabaseOptions>(configuration.GetSection("DatabaseOptions"));

if (builder.Environment.IsDevelopment())
{
    builder.Services.Configure<InvitationBaseRoute>(builder.Configuration.GetSection("BaseRouteForDev"));
    builder.Services.Configure<UserPasswordResetRoute>(builder.Configuration.GetSection("TwoFactorRouteForDev"));
}
else
{
    builder.Services.Configure<InvitationBaseRoute>(builder.Configuration.GetSection("BaseRouteForProd"));
    builder.Services.Configure<UserPasswordResetRoute>(builder.Configuration.GetSection("TwoFactorRouteForProd"));
}
    

#endregion

#region Register dependecy injections and automapper

builder.Services.RegisterAutoMapper();
builder.Services.RegisterPersistence();

#endregion

builder.Services.Configure<EmailAuth>(builder.Configuration.GetSection("MailKit"));
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("internal", new OpenApiInfo { Title = "Internal Dashboards API", Version = "v1" });
    c.SwaggerDoc("public", new OpenApiInfo { Title = "Public Dashboards API", Version = "v1" });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Id = "Bearer",
                        Type = ReferenceType.SecurityScheme,
                    }
                },
                new string[] { }
            }
        }
    );
});

builder.Services.AddApiVersioning(opt =>
{
    opt.DefaultApiVersion = new ApiVersion(1, 0);
    opt.AssumeDefaultVersionWhenUnspecified = true;
    opt.ReportApiVersions = true;
    opt.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("x-api-version"),
        new MediaTypeApiVersionReader("x-api-version"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        corsPolicyBuilder => corsPolicyBuilder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Enforce routing lowercase
builder.Services.AddRouting(options => options.LowercaseUrls = true);

var app = builder.Build();

// Build HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/public/swagger.json", "Public Dashboards API");

        if (builder.Environment.IsDevelopment())
        {
            c.SwaggerEndpoint("/swagger/internal/swagger.json", "Internal Dashboards API");
        }
    }
);

app.UseHttpsRedirection();
app.UseErrorHandlerMiddleware();
app.UseMiddleware<JwtMiddleware>();
app.UseCors("CorsPolicy");
app.UseAuthorization();
app.MapControllers();
app.Run();

// Make the implicit Program class public so test projects can access it
namespace WebApi
{
    public class Program
    {
    }
}
using System.Reflection;
using Application.Email.Helper;
using Application.JWT.Model;
using Infrastructure.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Repository;
using WebApi.Registry;

var myAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));


#region Config setup

//WE WILL NEED TO ADD A CONDITION TO ONLY TRIGGER THESE IF WE ARE IN DEV ENVIRONMENT

/*var appSettingsFile = string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIROMENT"))
    ? "appsettings.json" : $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIROMENT")}.json";*/

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json").Build();

// Configure database options
builder.Services.Configure<DatabaseOptions>(configuration.GetSection("DatabaseOptions"));

var client = new MongoClient(configuration.GetConnectionString("MongoDbConnString"));
var database = client.GetDatabase(configuration.GetConnectionString("MongoDbConnSchema"));

#endregion

#region Register dependecy injections and automapper

builder.Services.RegisterAutoMapper();
builder.Services.RegisterPersistence(database);

#endregion

builder.Services.Configure<EmailAuth>(builder.Configuration.GetSection("MailKit"));
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo {Title = "Web Dashboards API", Version = "V1"});

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
    options.AddPolicy(name: myAllowSpecificOrigins,
                      policy =>
                      {
                          //   policy.WithOrigins("http://localhost:8080/",
                          //                       "https://localhost:7173", "http://localhost:5161").AllowAnyMethod();
                          //Console.WriteLine("CORS policy");
                          policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
                      });
});

// Enforce routing lowercase
builder.Services.AddRouting(options => options.LowercaseUrls = true);

var app = builder.Build();

// Build HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseErrorHandlerMiddleware();
app.UseCors(myAllowSpecificOrigins);
app.UseAuthorization();
app.MapControllers();
app.Run();

// Make the implicit Program class public so test projects can access it
namespace WebApi
{
    public partial class Program { }
}
using GioAPI.Models;
using Microsoft.Extensions.Options;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure settings - ensure environment-specific configs are loaded
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Configure settings
builder.Services.Configure<Settings>(
    builder.Configuration.GetSection("Settings")
);

// Configure authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var settings = context.HttpContext.RequestServices.GetRequiredService<IOptions<Settings>>().Value;
            var authHeader = context.Request.Headers["Authorization"].ToString();
            
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Task.CompletedTask;
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            if (token == settings.BearerToken)
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, "API User"),
                    new Claim(ClaimTypes.Role, "ApiAccess")
                };

                var identity = new ClaimsIdentity(claims, "Bearer");
                context.Principal = new ClaimsPrincipal(identity);
                context.Success();
            }

            return Task.CompletedTask;
        }
    };
});

// Add AWS Lambda support. When application is run in Lambda Kestrel is swapped out as the web server with Amazon.Lambda.AspNetCoreServer. This
// package will act as the webserver translating request and responses between the Lambda event source and ASP.NET Core.
builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);

var app = builder.Build();

// Ensure we're using the correct environment
if (app.Environment.IsDevelopment())
{
    Console.WriteLine("Running in Development environment");
    Console.WriteLine($"Bearer Token from config: {app.Configuration.GetSection("Settings:BearerToken").Value}");
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

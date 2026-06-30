using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

using Microsoft.EntityFrameworkCore;
using System.Data;
using core10_grpc_mysql.Services;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureEndpointDefaults(listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
    });
});

builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
builder.Services.AddTransient<IDbConnection>(_ => new MySqlConnection(connectionString));
builder.Services.AddTransient<DbInitializer>();


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddAuthorization(); 


var app = builder.Build();

// 1. Serve static files from wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

app.UseAuthentication();
app.UseAuthorization();


// 2. Configure the HTTP request pipeline
app.MapGrpcService<RegistrationService>();
app.MapGrpcService<LoginService>();
app.MapGrpcService<UserService>();
app.MapGrpcService<MfaService>();
app.MapGrpcService<FileUploadService>();
app.MapGrpcService<ProductService>();

app.MapGrpcReflectionService();

app.MapFallbackToFile("{*path:nonfile}", "index.html");

app.Run();

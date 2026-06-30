using Microsoft.EntityFrameworkCore;
using System.Data;
using core10_grpc_mysql.Services;
using MySql.Data.MySqlClient;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddGrpcReflection(); 

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
builder.Services.AddTransient<IDbConnection>(_ => new MySqlConnection(connectionString));

builder.Services.AddTransient<DbInitializer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<RegistrationService>();
app.MapGrpcService<LoginService>();
app.MapGrpcService<UserService>();
app.MapGrpcReflectionService();       
// app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();


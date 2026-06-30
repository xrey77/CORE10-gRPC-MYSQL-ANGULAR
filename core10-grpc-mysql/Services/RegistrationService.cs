// Services/AuthService.cs
using System.Data;
using Dapper;
using Grpc.Core;

namespace core10_grpc_mysql.Services;

public class RegistrationService(ILogger<RegistrationService> logger, IDbConnection dbConnection) : Register.RegisterBase
{
    public override async Task<RegisterResponse> CreateUser(RegisterRequest request, ServerCallContext context)
    {
        var fname = request.Firstname;
        var lname = request.Lastname;
        var email = request.Email;
        var mobile = request.Mobile;
        var username = request.Username;
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

        logger.LogInformation("Registering user: {Username} ({Email})", username, email);

        // 1. Pre-flight check to see if the email already exists
        const string checkEmailSql = "SELECT COUNT(1) FROM users WHERE email = @Email";

        // 1. Pre-flight check to see if the username already exists
        const string checkUsernameSql = "SELECT COUNT(1) FROM users WHERE username = @Username";

        // 2. Insert statement 
        const string insertSql = @"
            INSERT INTO users (firstname, lastname, email, mobile, username, password, role_id) 
            VALUES (@Fname, @Lname, @Email, @Mobile, @Username, @Password, @Roleid); 
            SELECT LAST_INSERT_ID();";

        try
        {
            // Execute the check using Dapper's ExecuteScalar
            var emailExists = await dbConnection.ExecuteScalarAsync<int>(checkEmailSql, new { Email = email }) > 0;

            if (emailExists)
            {
                logger.LogWarning("Registration failed. Email {Email} already exists.", email);
                throw new RpcException(new Status(StatusCode.AlreadyExists, $"The email address '{email}' is already registered."));
            }

            var usernameExists = await dbConnection.ExecuteScalarAsync<int>(checkUsernameSql, new { Username = username }) > 0;

            if (usernameExists)
            {
                logger.LogWarning("Registration failed. Username {Username} already exists.", username);
                throw new RpcException(new Status(StatusCode.AlreadyExists, $"The username '{username}' is already registered."));
            }

            // Proceed to insert the user if the email is unique
            var insertedId = await dbConnection.ExecuteScalarAsync<int>(insertSql, new { 
                Fname = fname, 
                Lname = lname, 
                Email = email, 
                Mobile = mobile, 
                Username = username, 
                Password = hashedPassword, 
                Roleid = 2 
            });

            return new RegisterResponse 
            { 
                UserId = insertedId, 
                TextContent = "You have registered successfully, please login now." 
            };
        }
        catch (RpcException)
        {
            // Re-throw gRPC exceptions so they propagate properly to the client
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database insertion failed for user {Username}", username);
            throw new RpcException(new Status(StatusCode.Internal, "Failed to register user due to an internal error."));
        }
    }
}

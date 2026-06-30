// Services/LoginService.cs
using System.Data;
using Dapper;
using Grpc.Core;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims; 

namespace core10_grpc_mysql.Services;

public class LoginService(ILogger<LoginService> logger, IDbConnection dbConnection, IConfiguration configuration) : Login.LoginBase
{
    private readonly IConfiguration _configuration = configuration;  
    public override async Task<LoginResponse> UserLogin(LoginRequest request, ServerCallContext context)
    {
        const string sql = "SELECT Id, Firstname, Lastname, Email, Mobile, Username, Password, Isactivated, Isblocked, Mailtoken, Userpicture, Qrcodeurl, Role_id FROM users WHERE username = @Username";
        var Usrname = request.Username;
        var Pword = request.Password;

        var user = await dbConnection.QueryFirstOrDefaultAsync<dynamic>(sql, new { Username = Usrname });

        if (user != null)
        {

            if (BCrypt.Net.BCrypt.Verify(Pword, user.Password))
            {

            const string rolename = "SELECT Name FROM roles WHERE Id = @Id";            
            var roleName = await dbConnection.ExecuteScalarAsync<string>(rolename, new { Id = user.Role_id });



                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
                
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.Username)
                    }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                    Issuer = _configuration["Jwt:Issuer"],
                    Audience = _configuration["Jwt:Audience"]
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                return new LoginResponse 
                { 
                    Id = (int)user.Id, 
                    Firstname = user.Firstname,
                    Lastname = user.Lastname,
                    Email = user.Email,
                    Mobile = user.Mobile,
                    Username = user.Username,
                    Isactivated = (int)user.Isactivated,
                    Isblocked = (int)user.Isblocked,
                    Mailtoken = (int)user.Mailtoken,
                    Userpicture = user.Userpicture ?? "",
                    Qrcodeurl = user.Qrcodeurl ?? null,
                    Roles = roleName,
                    Token = tokenString,
                    TextContent = "You have logged-in successfully, please wait."
                };
            }
            else
            {
                logger.LogError("Invalid Password attempt for user {Username}", Usrname);
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Password, please try again."));
            }
        }
        else
        {
            logger.LogError("Login failed: Username does not exist for {Username}", Usrname);
            throw new RpcException(new Status(StatusCode.NotFound, "Username does not exist, please register."));
        }
    }
}

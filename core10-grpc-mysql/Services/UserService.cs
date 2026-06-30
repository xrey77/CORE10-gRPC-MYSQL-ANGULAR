// Services/UserService.cs
using System.Data;
using Dapper;
using Grpc.Core;
using System.Text;
using System.Security.Claims; 
using Microsoft.AspNetCore.Authorization;

namespace core10_grpc_mysql.Services;

 [Authorize] 
public class UserService(ILogger<UserService> logger, IDbConnection dbConnection) : GetUser.GetUserBase
{
    public override async Task<GetUserResponse> GetUserById(GetUserRequest request, ServerCallContext context)
    {
        const string sql = "SELECT Id, Firstname, Lastname, Email, Mobile, Username, Password, Isactivated, Isblocked, Mailtoken, Userpicture, Qrcodeurl, Role_id FROM users WHERE id = @Id";
        var Idno = request.Id;

        var user = await dbConnection.QueryFirstOrDefaultAsync<dynamic>(sql, new { Id = Idno });

        if (user != null)
        {
            string qrcode = "";
            if (user.Qrcodeurl != null)
            {
                qrcode = user.Qrcodeurl;
            }

            return new GetUserResponse
            {
                Data = new UserData
                {
                    Id = user.Id,
                    Firstname = user.Firstname,
                    Lastname = user.Lastname,
                    Email = user.Email,
                    Mobile = user.Mobile,
                    Username = user.Username,
                    Isactived = user.Isactivated,
                    Isblocked = user.Isblocked,
                    Mailtoken = user.Mailtoken,
                    Userpicture = user.Userpicture,
                    Qrcodeurl = qrcode 
                }
            };
        }
        else
        {
            logger.LogError("User ID not found.");
            throw new RpcException(new Status(StatusCode.NotFound, "User ID not found."));
        }
    }


    public override async Task<GetAllUsersResponse> GetAllUsers(GetAllUsersRequest request, ServerCallContext context)
    {
        const string sql = "SELECT Id, Firstname, Lastname, Email, Mobile, Username, Password, Isactivated, Isblocked, Mailtoken, Userpicture, Qrcodeurl, Role_id FROM users";

        var dbUsers = await dbConnection.QueryAsync<dynamic>(sql);


        var response = new GetAllUsersResponse();

        if (dbUsers != null)
        {
            // Create a temporary list to hold our mapped records
            var userList = new List<UserData>();

            foreach (var user in dbUsers)
            {
                userList.Add(new UserData
                {
                    Id = user.Id,
                    Firstname = user.Firstname ?? string.Empty,
                    Lastname = user.Lastname ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    Mobile = user.Mobile ?? string.Empty,
                    Username = user.Username ?? string.Empty,
                    Isactived = user.Isactivated,
                    Isblocked = user.Isblocked,
                    Mailtoken = user.Mailtoken,
                    Userpicture = user.Userpicture ?? string.Empty,
                    Qrcodeurl = user.Qrcodeurl ?? string.Empty
                });
            }

            response.Data.AddRange(userList);
        }

        return response;

    }

    public override async Task<ProfileUpdateResponse> UpdateProfile(ProfileUpdateRequest request, ServerCallContext context)
    {
        // Check if user exists
        const string sql = "SELECT COUNT(1) FROM users WHERE id = @Id";
        var Idno = request.Id;
        var user = await dbConnection.QueryFirstOrDefaultAsync<dynamic>(sql, new { Id = Idno });

        if (user != null)
        {
            var fname = request.Firstname;
            var lname = request.Lastname;
            var mobile = request.Mobile;

            // FIX: Added @Id to the parameter object
            const string updateSql = @" UPDATE users SET firstname = @Fname, lastname = @Lname, mobile = @Mobile WHERE id = @Id";
            await dbConnection.ExecuteScalarAsync<int>(updateSql, new { Fname = fname, Lname = lname, Mobile = mobile, Id = Idno });

            return new ProfileUpdateResponse { TextContent = "You have changed your profile successfully." };
        }
        else
        {
            logger.LogError("User ID not found.");
            throw new RpcException(new Status(StatusCode.NotFound, "User ID not found."));
        }
    }

    public override async Task<UpdatePasswordResponse> ChangePassword(UpdatePasswordRequest request, ServerCallContext context)
    {
        // Check if user exists
        const string sql = "SELECT COUNT(1) FROM users WHERE id = @Id";
        var Idno = request.Id;
        var user = await dbConnection.QueryFirstOrDefaultAsync<dynamic>(sql, new { Id = Idno });

        if (user != null)
        {
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // FIX: Corrected @password to @Pword to match SQL, and added @Id to the parameter object
            const string updateSql = @" UPDATE users SET password = @Pword WHERE id = @Id";
            await dbConnection.ExecuteScalarAsync<int>(updateSql, new { Pword = hashedPassword, Id = Idno });

            return new UpdatePasswordResponse { TextContent = "You have changed your password successfully." };
        }
        else
        {
            logger.LogError("User ID not found.");
            throw new RpcException(new Status(StatusCode.NotFound, "User ID not found."));
        }
    }
}

// Services/UserService.cs
using System.Data;
using Dapper;
using Grpc.Core;
using System.Text;
using System.Security.Claims; 

namespace core10_grpc_mysql.Services;

public class UserService(ILogger<UserService> logger, IDbConnection dbConnection) : GetUser.GetUserBase
{
    public override async Task<GetUserResponse> GetUserById(GetUserRequest request, ServerCallContext context)
    {
        const string sql = "SELECT Id, Firstname, Lastname, Email, Mobile, Username, Password, Isactivated, Isblocked, Mailtoken, Userpicture, Qrcodeurl, Role_id FROM users WHERE id = @Id";
        var Idno = request.Id;

        var user = await dbConnection.QueryFirstOrDefaultAsync<dynamic>(sql, new { Id = Idno });

        if (user != null)
        {

            var response = new GetUserResponse
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
                    Qrcodeurl = user.Qrcodeurl ?? null 
                }
            };
            return response;
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
                    Qrcodeurl = user.Qrcodeurl ?? null
                });
            }

            // Populate the repeated field using AddRange
            response.Data.AddRange(userList);
        }

        return response;

    }


}

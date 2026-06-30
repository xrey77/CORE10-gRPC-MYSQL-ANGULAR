// Services/MfaService.cs 
using System.Data;
using Dapper;
using Grpc.Core;
using Google.Authenticator;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

namespace core10_grpc_mysql.Services;

public class MfaService(ILogger<MfaService> logger, IDbConnection dbConnection, IConfiguration configuration) : Mfa.MfaBase 
{
    private readonly IConfiguration _configuration = configuration;

    public override async Task<MfaActivationResponse> ActivateMfa(MfaActivationRequest request, ServerCallContext context) 
    {
        const string sql = "SELECT COUNT(1) FROM users WHERE id = @Id";
        var Idno = request.Id;
        bool Istwofactorenabled = request.Twofactorenabled;

        var userExists = await dbConnection.QueryFirstOrDefaultAsync<int>(sql, new { Id = Idno });

        if (userExists > 0) 
        {
            if (Istwofactorenabled) 
            {
                const string getSecretSql = "SELECT Secret FROM users WHERE id = @Id";
                var userSecret = await dbConnection.QueryFirstOrDefaultAsync<string>(getSecretSql, new { Id = Idno });

                var tokenHandler = new JwtSecurityTokenHandler();
                var xkey = _configuration["Jwt:Key"];
                var key = Encoding.ASCII.GetBytes(xkey); 

                var tokenDescriptor = new SecurityTokenDescriptor 
                {
                    Subject = new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, request.Id.ToString()) }), 
                    Expires = DateTime.UtcNow.AddHours(4),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var secret = tokenHandler.CreateToken(tokenDescriptor);
                var secretkey = tokenHandler.WriteToken(secret);
                
                var fullname = "Apple Inc."; 
                TwoFactorAuthenticator twoFactor = new(); 
                var setupInfo = twoFactor.GenerateSetupCode(fullname, request.Id.ToString(), secretkey, false, 3);
                var imageUrl = setupInfo.QrCodeSetupImageUrl;

                const string updateSql = @" UPDATE users SET secret = @Secret, qrcodeurl = @Qrcodeurl WHERE id = @Id";
                await dbConnection.ExecuteAsync(updateSql, new { Secret = secretkey.ToUpper(), Qrcodeurl = imageUrl, Id = Idno }); 

                return new MfaActivationResponse 
                { 
                    Qrcodeurl = imageUrl, 
                    TextContent = "Multi-Factor has been enabled successfully." 
                };
            } 
            else 
            {
                const string updateSql = @" UPDATE users SET secret = @Secret, qrcodeurl = @Qrcoderul WHERE id = @Id";
                await dbConnection.ExecuteAsync(updateSql, new { Secret = (string)null, Qrcoderul = (string)null, Id = Idno }); 

                return new MfaActivationResponse 
                { 
                    TextContent = "Multi-Factor has been disabled successfully." 
                };
            }
        } 
        else 
        {
            logger.LogError("User ID not found.");
            throw new RpcException(new Status(StatusCode.NotFound, "User ID not found."));
        }
    }

    public override async Task<MfaVerificationResponse> VerifyTotp(MfaVerificationRequest request, ServerCallContext context) 
    {
        const string sql = "SELECT Username, Secret FROM users WHERE id = @Id"; 
        var Idno = request.Id;
        var otp = request.Otp;

        var dbUser = await dbConnection.QueryFirstOrDefaultAsync<dynamic>(sql, new { Id = Idno });

        if (dbUser != null) 
        {
            TwoFactorAuthenticator twoFactor = new();
            // Google Authenticator pins are usually Base32 strings, so the 3rd parameter is set to true
            bool isValid = twoFactor.ValidateTwoFactorPIN(dbUser.Secret, otp, true); 

            if (isValid) 
            {
                return new MfaVerificationResponse 
                { 
                    Username = dbUser.Username, 
                    TextContent = "OTP code validation successful." 
                };
            } 
            else 
            {
                logger.LogError("Invalid OTP code, please enter 6 digits number from Authenticator App.");
                throw new RpcException(new Status(StatusCode.NotFound, "Invalid OTP code, please enter 6 digits number from Authenticator App."));
            }
        } 
        else 
        {
            logger.LogError("User ID not found.");
            throw new RpcException(new Status(StatusCode.NotFound, "User ID not found."));
        }
    }
}


using LG_projects.ResponseModel.Auth;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LG_projects.Classes.Token
{
    public class TokenService : ITokenService
    {
        private const double EXPIRY_DURATION_MINUTES = 5; //change 15 to 120;
        private readonly IConfiguration configuration;
        public TokenService(IConfiguration _configuration)
        {
            configuration = _configuration;
        }

        public string BuildToken(UserVm user)
        {
            string name = user.NameUr;
            string key = configuration["Jwt:Key"]!.ToString();
            string issuer = configuration["Jwt:Issuer"]!.ToString();

            var claims = new[]
              {
                new Claim("id", user.id.ToString()),
                new Claim("name", user.NameEn ?? ""),
                new Claim("phone", user.phone ?? "")
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
            var tokenDescriptor = new JwtSecurityToken(issuer, issuer, claims,
                expires: DateTime.UtcNow.AddMinutes(5).AddMinutes(EXPIRY_DURATION_MINUTES), signingCredentials: credentials);// use the const for expiry and not the static value
            foreach (var claim in claims)
            {
                Console.WriteLine($"{claim.Type} = {claim.Value}");
            }
            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
        public bool IsTokenValid(string token)
        {

            string key = configuration["Jwt:Key"]!.ToString();
            string issuer = configuration["Jwt:Issuer"]!.ToString();

            var mySecret = Encoding.UTF8.GetBytes(key);
            var mySecurityKey = new SymmetricSecurityKey(mySecret);

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = issuer,
                    ValidAudience = issuer,
                    IssuerSigningKey = mySecurityKey,
                    ValidateLifetime = true
                }, out SecurityToken validatedToken);
            }
            catch
            {
                return false;
            }
            return true;
        }


        public List<string?> DecodeJWTToken(string token)
        {
            var result = new List<string?> { null, null, null };

            if (string.IsNullOrWhiteSpace(token))
                return result;

            var handler = new JwtSecurityTokenHandler();

            try
            {
                var jwtToken = handler.ReadJwtToken(token);
                result[0] = jwtToken.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
                result[1] = jwtToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
                result[2] = jwtToken.Claims.FirstOrDefault(c => c.Type == "phone")?.Value;
                return result;
            }
            catch
            {
                return result;
            }
        }
    }
}

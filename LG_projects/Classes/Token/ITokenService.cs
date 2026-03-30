
using LG_projects.ResponseModel.Auth;

namespace LG_projects.Classes.Token
{
    public interface ITokenService
    {
        string BuildToken(UserVm user);
        bool IsTokenValid(string token);
        List<string?> DecodeJWTToken(string token);
    }
}

using Microsoft.Data.SqlClient;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Cryptography;

namespace LG_projects.Classes
{
    public class Settings
    {
        public string DateFormat()
        {
            string Format = "dd-MM-yyyy";
            return Format;
        }
        public string GenerateAlphaNumericOtp(int length = 6)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var result = new char[length];
            var bytes = new byte[length];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            for (int i = 0; i < length; i++)
            {
                result[i] = chars[bytes[i] % chars.Length];
            }

            return new string(result);
        }
    }
}

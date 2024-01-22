using System.Security.Cryptography;
using System.Text;

namespace webApiTemplate.src.App.Provider
{
    public class Hmac512Provider
    {
        private static readonly string _key = "Hdasdcnmz213*";


        public static string Compute(string value)
        {
            var keyBytes = Encoding.UTF8.GetBytes(_key);
            using var hmac = new HMACSHA512(keyBytes);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(value));
            return Convert.ToBase64String(hash);
        }
    }
}
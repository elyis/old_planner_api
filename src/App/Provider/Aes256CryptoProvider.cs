using System.Security.Cryptography;
using System.Text;
using webApiTemplate.src.Domain.Entities.Config;

namespace webApiTemplate.src.App.Provider
{
    public class Aes256CryptoProvider : IDisposable
    {
        private readonly Aes _aes;

        public Aes256CryptoProvider(Aes256Settings settings)
        {
            _aes = Aes.Create();
            _aes.Key = Encoding.UTF8.GetBytes(settings.Key);
            _aes.IV = Encoding.UTF8.GetBytes(settings.IV);
        }

        public string Decrypt(string input)
        {

            var decryptor = _aes.CreateDecryptor(_aes.Key, _aes.IV);
            var cipherText = Convert.FromBase64String(input);

            using MemoryStream msDecrypt = new(cipherText);
            using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);

            using StreamReader srDecrypt = new(csDecrypt);
            return srDecrypt.ReadToEnd();
        }

        public string Encrypt(string input)
        {
            var encryptor = _aes.CreateEncryptor(_aes.Key, _aes.IV);

            using MemoryStream msEncrypt = new();
            using CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write);
            using StreamWriter swEncrypt = new(csEncrypt);
            swEncrypt.Write(input);

            return Convert.ToBase64String(msEncrypt.ToArray());
        }

        public void Dispose() => _aes.Dispose();
    }
}
using System;
using System.Security.Cryptography;
using System.Text;

namespace LG_projects.Common.EncryptionDecryption
{
    public  class EncryptionHelper
    {
        // 🔑 Secret key & IV (store securely in appsettings.json for production)
        private static readonly string Key = "dofkrfaosrdedofkrfaosrdedofkrfao"; // 32 chars for AES-256
        private static readonly string IV = "zxcvbnmdfrasdfgh"; // 16 chars

        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;

            using var aes = Aes.Create(); // Modern cross-platform AES
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Key = Encoding.UTF8.GetBytes(Key);
            aes.IV = Encoding.UTF8.GetBytes(IV);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            return Convert.ToBase64String(encryptedBytes);
        }

        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;

            using var aes = Aes.Create(); // Modern cross-platform AES
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Key = Encoding.UTF8.GetBytes(Key);
            aes.IV = Encoding.UTF8.GetBytes(IV);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}
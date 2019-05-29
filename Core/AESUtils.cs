using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Core
{
    /// <summary>
    /// 256位(32字节)Key，128位（16字节）IV AES加密/解密
    /// </summary>
    public static class AESUtils
    {
        static public Tuple<string, string, string> Encrypt(string content)
        {
            byte[] input = Encoding.UTF8.GetBytes(content);
            AesCryptoServiceProvider aesProvider = new AesCryptoServiceProvider();
            aesProvider.GenerateIV();
            aesProvider.GenerateKey();

            ICryptoTransform encryptor = aesProvider.CreateEncryptor();
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            cs.Write(input, 0, input.Length);
            cs.FlushFinalBlock();
            byte[] data = ms.ToArray();

            Tuple<string, string, string> result = Tuple.Create(Convert.ToBase64String(data), Convert.ToBase64String(aesProvider.Key), Convert.ToBase64String(aesProvider.IV));
            return result;
        }
        public static string Decrypt(string data, string key, string iv)
        {
            byte[] input = Convert.FromBase64String(data);
            AesCryptoServiceProvider aesProvider = new AesCryptoServiceProvider()
            {
                Key = Convert.FromBase64String(key),
                IV = Convert.FromBase64String(iv)
            };
            ICryptoTransform decryptor = aesProvider.CreateDecryptor();
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write);
            cs.Write(input, 0, input.Length);
            cs.FlushFinalBlock();
            byte[] result = ms.ToArray();
            return Encoding.UTF8.GetString(result);
        }
    }
}

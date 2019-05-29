﻿using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Core
{
    /// <summary>
    /// 64位（8字节）Key，64位IV DES加密/解密
    /// </summary>
    public static class DESUtils
    {
        /// <summary>
        /// 64位DES加密       
        /// </summary>
        /// <returns></returns>
        public static Tuple<string, string, string> Encrypt(string content)
        {
            byte[] input = Encoding.UTF8.GetBytes(content);
            DESCryptoServiceProvider desProvider = new DESCryptoServiceProvider();
            desProvider.GenerateKey();
            desProvider.GenerateIV();

            MemoryStream ms = new MemoryStream();
            ICryptoTransform encryptor = desProvider.CreateEncryptor();
            CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            cs.Write(input, 0, input.Length);
            cs.FlushFinalBlock();
            byte[] data = ms.ToArray();
            Tuple<string, string, string> result = Tuple.Create(Convert.ToBase64String(data), Convert.ToBase64String(desProvider.Key), Convert.ToBase64String(desProvider.IV));
            return result;
        }
        /// <summary>
        /// 64位DES解密       
        /// </summary>
        /// <returns></returns>
        public static string Decrypt(string data, string key, string iv)
        {
            DESCryptoServiceProvider desProvider = new DESCryptoServiceProvider()
            {
                Key = Convert.FromBase64String(key),
                IV = Convert.FromBase64String(iv)
            };
            byte[] input = Convert.FromBase64String(data);
            MemoryStream ms = new MemoryStream();
            ICryptoTransform decryptor = desProvider.CreateDecryptor();
            CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write);
            cs.Write(input, 0, input.Length);
            cs.FlushFinalBlock();
            byte[] result = ms.ToArray();
            return Encoding.UTF8.GetString(result);
        }
    }
}

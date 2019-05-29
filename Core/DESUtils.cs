using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Core
{
    public static class DESUtils
    {
        #region 加密、解密
        /// <summary>
        /// 64位DES加密       
        /// </summary>
        /// <returns></returns>
        public static Tuple<string, string, string> DESEncrypt(string content)
        {
            byte[] inputByteArray = Encoding.UTF8.GetBytes(content);
            DESCryptoServiceProvider desProvider = new DESCryptoServiceProvider();
            desProvider.GenerateKey();
            desProvider.GenerateIV();
            
            MemoryStream ms = new MemoryStream();
            ICryptoTransform encryptor = desProvider.CreateEncryptor();
            CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            byte[] data = ms.ToArray();
            Tuple<string, string, string> result = Tuple.Create(Convert.ToBase64String(data), Convert.ToBase64String(desProvider.Key), Convert.ToBase64String(desProvider.IV));
            return result;
        }
        /// <summary>
        /// 64位DES解密       
        /// </summary>
        /// <returns></returns>
        public static string DESDecrypt(string data, string key, string iv)
        {
            DESCryptoServiceProvider desProvider = new DESCryptoServiceProvider()
            {
                Key = Convert.FromBase64String(key),
                IV = Convert.FromBase64String(iv)
            };
            byte[] inputByteArray = Convert.FromBase64String(data);
            MemoryStream ms = new MemoryStream();
            ICryptoTransform decryptor = desProvider.CreateDecryptor();
            CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            byte[] result = ms.ToArray();
            return Encoding.UTF8.GetString(result);
        }
        #endregion
    }
}

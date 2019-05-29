using System;
using System.Security.Cryptography;
using System.Text;

namespace Core
{
    /// <summary>
    ///公钥:供任何人用于编码发送给接收方的数据。 
    ///私钥:必须保留为私有的接收方,用于解码消息编码
    ///https://docs.microsoft.com/zh-cn/dotnet/api/system.security.cryptography.asymmetricalgorithm?view=netframework-4.8
    /// </summary>
    public static class RSAUtils
    {
        static public Tuple<RSAParameters, byte[]> Encrypt(string content)
        {
            byte[] input = Encoding.UTF8.GetBytes(content);
            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider();
            RSAParameters rsaExportKeyInfo = rsaProvider.ExportParameters(true);

            rsaProvider.ImportParameters(rsaExportKeyInfo);

            byte[] data = rsaProvider.Encrypt(input, false);
            return Tuple.Create(rsaExportKeyInfo, data);
        }
        public static string Decrypt(string data, RSAParameters rsaKeyInfo)
        {
            byte[] input = Convert.FromBase64String(data);
            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider();
            rsaProvider.ImportParameters(rsaKeyInfo);

            byte[] result = rsaProvider.Decrypt(input, false);

            return Encoding.UTF8.GetString(result);
        }
    }
}

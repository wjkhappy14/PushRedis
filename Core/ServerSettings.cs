using System.Configuration;

namespace Core
{
    public static class ServerSettings
    {
        public static bool IsSsl
        {
            get
            {
                string ssl = ConfigurationManager.ConnectionStrings["ssl"].ConnectionString;
                return !string.IsNullOrEmpty(ssl) && bool.Parse(ssl);
            }
        }
        public static string Host =>ConfigurationManager.ConnectionStrings["Host"].ConnectionString;
        public static int Port => int.Parse(ConfigurationManager.ConnectionStrings["Port"].ConnectionString);
        public static string Bind => ConfigurationManager.ConnectionStrings["Bind"].ConnectionString;
        public static string X509Cert => ConfigurationManager.ConnectionStrings["X509Cert"].ConnectionString;
    }

}

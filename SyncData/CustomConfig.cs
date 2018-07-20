using System.Configuration;

namespace SyncData
{
    public class CustomConfig
    {
        public static string ConnStrBaan;
        public static string ConnStrMain;
        public static int nCacheExpiredMin;
        public static bool PullBaanData=true;
        public static bool bDeleteTempFile;
        public static bool bSendMail;
        public static string SmtpConnStr;
        public static string EmailReceiver;
        public static string EmailCC;
        public static string EmailSubject;

        static CustomConfig()
        {
            ConnStrBaan = ConfigurationManager.ConnectionStrings["baan_ConnStr"].ConnectionString;
            ConnStrMain = ConfigurationManager.ConnectionStrings["main_ConnStr"].ConnectionString;
            var sTemp = ConfigurationManager.AppSettings["CacheExpiredMin"].Trim();
            int.TryParse(sTemp, out nCacheExpiredMin);

            sTemp = ConfigurationManager.AppSettings["PullBaanData"].Trim();
            bool.TryParse(sTemp, out PullBaanData);
            sTemp = ConfigurationManager.AppSettings["DeleteTempFile"].Trim();
            bool.TryParse(sTemp, out bDeleteTempFile);
            sTemp = ConfigurationManager.AppSettings["SendEmail"].Trim();
            bool.TryParse(sTemp, out bSendMail);
            SmtpConnStr = ConfigurationManager.AppSettings["SMTPConnection"].Trim();
            EmailReceiver = ConfigurationManager.AppSettings["EmailReceiver"].Trim();
            EmailCC = ConfigurationManager.AppSettings["EmailCC"].Trim();
            EmailSubject = ConfigurationManager.AppSettings["EmailSubject"].Trim();
        }
    }
}

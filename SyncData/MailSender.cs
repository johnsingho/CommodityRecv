using Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SyncData
{
    public class MailSender
    {
        private string SmtpConnStr;
        private string EmailCC;
        private string EmailReceiver;
        private string EmailSubject;

        public MailSender(string sSmtpConn, string sEmailRecv, string sEmailCc, string sEmailSub)
        {
            SmtpConnStr = sSmtpConn;
            EmailReceiver = sEmailRecv;
            EmailCC = sEmailCc;
            EmailSubject = sEmailSub;
        }

        public bool SendEmail(string sBody, string sFilePath)
        {
            using (var eh = new EmailHelper(SmtpConnStr))
            {
                eh.AddToAddress(EmailReceiver);
                eh.AddCcAddress(EmailCC);
                if (System.IO.File.Exists(sFilePath))
                {
                    eh.AddAttachment(new System.Net.Mail.Attachment(sFilePath));
                }

                var dt = DateTime.Now;
                eh.Body = sBody;
                eh.Subject = MakeEmailSubject(dt);
                try
                {
                    eh.SendMail();
                }
                catch (Exception ex)
                {
                    ErrorOut(ex);
                    return false;
                }
            }

            return true;
        }

        public bool SendEmail(string sFilePath)
        {
            using (var eh = new EmailHelper(SmtpConnStr))
            {
                eh.AddToAddress(EmailReceiver);
                eh.AddCcAddress(EmailCC);
                eh.AddAttachment(new System.Net.Mail.Attachment(sFilePath));

                var dt = DateTime.Now;
                eh.Body = MakeEmailBody(dt);
                eh.Subject = MakeEmailSubject(dt);
                try
                {
                    eh.SendMail();
                }
                catch (Exception ex)
                {
                    ErrorOut(ex);
                    return false;
                }
            }

            return true;
        }

        private static void InfoOut(string str)
        {
            LogHelper.WriteInfo(typeof(MailSender), str);
        }
        private static void ErrorOut(Exception ex)
        {
            LogHelper.WriteError(typeof(MailSender), ex);
        }

        private string MakeEmailSubject(DateTime dt)
        {
            var stm = dt.ToString("yyyy-MM-dd HH:mm");
            var str = string.Format("Automatic Email for {0} {1}", EmailSubject, stm);
            return str;
        }

        private string MakeEmailBody(DateTime dt)
        {
            var stm = dt.ToString("F", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
            var str = string.Format("Automatic Email for {0} {1}", EmailSubject, stm);
            return str;
        }
    }
}

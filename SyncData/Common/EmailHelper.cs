using System;
using System.IO;
using System.Net;
using System.Net.Mail;

namespace Common
{
    public class EmailHelper : IDisposable {
        private MailMessage Message = null;
        private SmtpClient smtpClient = null;
        private string _connectionString;
        public MailAddress FromAddress { get; set; }

        public string ConnectionString {
            get { return _connectionString; }
            set {
                _connectionString = value;
                ParseConnectionString(value);
            }
        }

        public string DisplayName { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        public EmailHelper(string ConnectionString) {
            smtpClient = new SmtpClient();
            _connectionString = ConnectionString; // ConfigurationManager.AppSettings["SMTPConnection"];
            ParseConnectionString(_connectionString);
            Message = new MailMessage();
        }
        public EmailHelper(string host, int port, string userName, string password, bool ssl) {
            smtpClient.Host = host;
            smtpClient.Port = port;
            smtpClient.EnableSsl = ssl;
            smtpClient.Credentials = new NetworkCredential(userName, password);
        }

        public void AddToAddress(string email, string name = null) {
            if(!string.IsNullOrEmpty(email)) {
                email = email.Replace(",", ";");
                string[] emailList = email.Split(';');
                for(int i = 0; i < emailList.Length; i++) {
                    if(!string.IsNullOrEmpty(emailList[i]))
                        Message.To.Add(new MailAddress(emailList[i], name));
                }
            }
        }
        public void AddCcAddress(string email, string name = null) {
            if(!string.IsNullOrEmpty(email)) {
                email = email.Replace(",", ";");
                string[] emailList = email.Split(';');
                for(int i = 0; i < emailList.Length; i++) {
                    if(!string.IsNullOrEmpty(emailList[i]))
                        Message.CC.Add(new MailAddress(emailList[i], name));
                }
            }
        }
        public void AddBccAddress(string email, string name = null) {
            if(!string.IsNullOrEmpty(email)) {
                email = email.Replace(",", ";");
                string[] emailList = email.Split(';');
                for(int i = 0; i < emailList.Length; i++) {
                    if(!string.IsNullOrEmpty(emailList[i]))
                        Message.Bcc.Add(new MailAddress(emailList[i], name));
                }
            }
        }
        public void AddAttachment(string file, string mimeType) {
            Attachment attachment = new Attachment(file, mimeType);
            Message.Attachments.Add(attachment);
        }
        public void AddAttachment(Attachment objAttachment) {
            Message.Attachments.Add(objAttachment);
        }
        public void SendMail() {
            if(!string.IsNullOrEmpty(DisplayName)) {
                FromAddress = new MailAddress(FromAddress.Address, DisplayName);
            }
            Message.From = FromAddress;
            if(Message.To.Count <= 0) { throw new Exception("To address not defined"); }
            Message.Subject = Subject;
            Message.IsBodyHtml = true;
            Message.Body = Body;
            smtpClient.Send(Message);
        }
        public static string GetFileMimeType(string fileName) {
            string fileExt = Path.GetExtension(fileName.ToLower());
            string mimeType = string.Empty;
            switch(fileExt) {
                case ".htm":
                case ".html":
                    mimeType = "text/html";
                    break;
                case ".xml":
                    mimeType = "text/xml";
                    break;
                case ".jpg":
                case ".jpeg":
                    mimeType = "image/jpeg";
                    break;
                case ".gif":
                    mimeType = "image/gif";
                    break;
                case ".png":
                    mimeType = "image/png";
                    break;
                case ".bmp":
                    mimeType = "image/bmp";
                    break;
                case ".pdf":
                    mimeType = "application/pdf";
                    break;
                case ".doc":
                    mimeType = "application/msword";
                    break;
                case ".docx":
                    mimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                    break;
                case ".xls":
                    mimeType = "application/x-msexcel";
                    break;
                case ".xlsx":
                    mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    break;
                case ".csv":
                    mimeType = "application/csv";
                    break;
                case ".ppt":
                    mimeType = "application/vnd.ms-powerpoint";
                    break;
                case ".pptx":
                    mimeType = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                    break;
                case ".rar":
                    mimeType = "application/x-rar-compressed";
                    break;
                case ".zip":
                    mimeType = "application/x-zip-compressed";
                    break;
                default:
                    mimeType = "text/plain";
                    break;
            }
            return mimeType;
        }
        #region Privates
        private void ParseConnectionString(string connectionString) {
            var conn = connectionString;
            string[] connArr = conn.Split(';');
            string[] valueArr = null;
            string key = null; string value = null;

            foreach(string s in connArr) {
                valueArr = s.Split('=');
                key = valueArr[0].Trim().ToUpper();
                value = valueArr[1].Trim();
                if(key == "SERVER") {
                    smtpClient.Host = value;
                }
                else if(key == "PORT") {
                    try { smtpClient.Port = int.Parse(value); } catch(FormatException) { }
                }
                else if(key == "USER ID") {
                    SetCredentials(value, null);
                }
                else if(key == "PASSWORD") {
                    SetCredentials(null, value);
                }
                else if(key == "SSL") {
                    try { smtpClient.EnableSsl = bool.Parse(value); } catch(FormatException) { }
                }
                else if(key == "FROM ADDRESS") {
                    this.FromAddress = new MailAddress(value);
                }
            }
        }

        private void SetCredentials(string username, string password) {
            if(smtpClient.Credentials == null) {
                smtpClient.Credentials = new NetworkCredential();
            }
            if(username != null) {
                ((NetworkCredential)smtpClient.Credentials).UserName = username;
            }
            if(password != null) {
                ((NetworkCredential)smtpClient.Credentials).Password = password;
            }
        }
        #endregion

        public void Dispose()
        {
            foreach(var att in Message.Attachments)
            {
                att.Dispose();
            }
        }

        //public void Test() {
        //    EmailHelper h = new EmailHelper();
        //    h.AddToAddress("");
        //    h.AddCcAddress("");
        //   // h.AddAttachment("");
        //    h.SendMail();
        //}        
    }

}




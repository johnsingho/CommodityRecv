using System;
using System.IO;

namespace Common
{
    public class LogHelper {

        public static void SetConfig()
        {
            //use exe's
            log4net.Config.XmlConfigurator.Configure();
        }

        public static void SetConfig(FileInfo configFile) {
            log4net.Config.XmlConfigurator.Configure(configFile);
        }

        public static void WriteInfo(Type type, string info) {
            SetConfig();
            log4net.ILog loginfo = log4net.LogManager.GetLogger(type);
            if (loginfo.IsInfoEnabled)
            {
                loginfo.Info(info);
            }
        }

        public static void WriteError(Type type, Exception ex) {
            WriteError(type, ex.Message, ex);
        }
        public static void WriteError(Type type, string message, Exception ex) {
            SetConfig();
            log4net.ILog logerror = log4net.LogManager.GetLogger(type);
            if (logerror.IsErrorEnabled)
            {
                logerror.Error(message, ex);
            }
        }
    }
}

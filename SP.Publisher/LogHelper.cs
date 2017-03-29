using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Publisher
{
    public enum LogType
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3
    }

    public class LogHelper
    {
        public static void Raw(string sMsg)
        {
            Console.WriteLine(sMsg);
        }

        public static void Info(string sMsg, object context = null)
        {
            LogHelper.Write(context, LogType.Info, sMsg);
        }

        public static void Warning(string sMsg, object context = null)
        {
            LogHelper.Write(context, LogType.Warning, sMsg);
        }

        public static void Error(Exception ex, object context = null)
        {
            if (null != ex)
            {
                string message = ex.Message + ex.StackTrace;
                if (null != ex.InnerException)
                {
                    message += "Inner Exception Information: " + ex.InnerException.Message + ex.InnerException.StackTrace;
                }
                LogHelper.Write(context, LogType.Error, message);
            }
        }

        public static void Error(string message, Exception ex, object context = null)
        {
            LogHelper.Write(context, LogType.Error, $"message: {message}; exception:{ex}");
        }

        private static void Write(object context, LogType type, string sMsg)
        {
            var ctx = context ?? "SP.Publisher";
            var msg = $"[{DateTime.UtcNow}][{type}][{ctx}]: {sMsg}";
            Console.WriteLine(msg);
        }
    }
}

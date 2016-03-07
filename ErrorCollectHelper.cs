using System;
using System.IO;
using System.Text;

namespace WatcherAsFileService
{
    public class ErrorCollectHelper
    {
        public readonly static string errorParent = MonitoringService.errorPath;
        private readonly static string errorPath = errorParent + "Image_[" + DateTime.Now.ToString("yyyyMMdd")+"]"+"_Error.log";
        private static object lockObj = new object();
        private static object lockObjAsError = new object();
        /// <summary>
        /// 错误图片记录
        /// </summary>
        /// <param name="question">错误图片</param>
        public static void ImgErrorLog(QuestionModel question)
        {
            lock (lockObj)
            {
                
                string imageErrorPath = errorParent + question.fileName +@"\["+DateTime.Now.ToString("yyyyMMdd") +"]_Error.txt";
                string imgErrorPathParent = errorParent + question.fileName;
                if (!Directory.Exists(imgErrorPathParent))
                {
                    Directory.CreateDirectory(imgErrorPathParent);
                }
                using (StreamWriter file = new StreamWriter(imageErrorPath, true, Encoding.UTF8))
                {
                    string logMaster = "[{0}][{1}]:{2}\r";
                    string errorlog = string.Format(logMaster, DateTime.Now.ToString("HH:mm:ss"), question.path, question.error);
                    file.WriteLine(errorlog);
                    file.Close();
                }
            }
        }

        /// <summary>
        /// 错误日志记录
        /// </summary>
        /// <param name="count">错误标题</param>
        /// <param name="title">错误内容</param>
        public static void ErrorLog(string count)
        {
            lock (lockObjAsError)
            {
                if (!Directory.Exists(errorParent))
                {
                    Directory.CreateDirectory(errorParent);
                }
                using (StreamWriter file = new StreamWriter(errorPath, true, Encoding.UTF8))
                {
                    string logMaster = "[{0}]:\r{1}\r";
                    string errorlog = string.Format(logMaster, DateTime.Now.ToString("HH:mm:ss"), count);
                    file.WriteLine(errorlog);
                    file.Close();
                }
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WatcherAsFileService
{
    public class ImgsDetectionHelper
    {
        private readonly static int minSize = Int32.Parse(MonitoringService.minSize) * 1024;
        private readonly static int maxSize = Int32.Parse(MonitoringService.maxSize) * 1024;
        public readonly static string postfix = MonitoringService.postfix;

        public bool Monitor(string path)
        {
            try
            {
                DecideImageNorms(path);
            }
            catch (Exception ex)
            {
                ErrorCollectHelper.ErrorLog(ex.ToString());
                return false;
            }
            return true;
        }
        /// <summary>
        /// 图片检测，大小和格式检测
        /// </summary>
        private void DecideImageNorms(string imgPath)
        {
            bool isAccordWithSize = true;
            QuestionModel QuestionPath = new QuestionModel();
            if (!File.Exists(imgPath))
            {
                return;
            }

            using (FileStream fs = new FileStream(imgPath, FileMode.Open, FileAccess.Read))
            {
                if (fs.Length < minSize || fs.Length > maxSize)
                {
                    QuestionPath.error = "[文件大小 不符规范]";
                    QuestionPath.path = imgPath;
                    QuestionPath.fileName = FieldInterception(imgPath);
                    ErrorCollectHelper.ImgErrorLog(QuestionPath);
                    isAccordWithSize = false;
                }
                fs.Close();
                fs.Dispose();
            }

            if (!isAccordWithSize)
            {
                CopyFile(QuestionPath);
                File.Delete(QuestionPath.path);
                isAccordWithSize = true;
                return;
            }

            if (postfix == null || postfix.ToUpper() != "*.JPG")
            {
                return;
            }

            Image img = Image.FromFile(imgPath);
            if (img.RawFormat.Equals(ImageFormat.Jpeg))
            {
                return;
            }
            else if (img.RawFormat.Equals(ImageFormat.Png))
            {
                QuestionPath.error = "[图片本质为PNG格式]";
                QuestionPath.path = imgPath;
            }
            else if (img.RawFormat.Equals(ImageFormat.Bmp))
            {
                QuestionPath.error = "[图片本质为BMP格式]";
                QuestionPath.path = imgPath;
            }
            else
            {
                QuestionPath.error = "[图片的本质格式未知]";
                QuestionPath.path = imgPath;
            }

            QuestionPath.fileName = FieldInterception(imgPath);
            img.Dispose();
            CopyFile(QuestionPath);
            File.Delete(QuestionPath.path);
            ErrorCollectHelper.ImgErrorLog(QuestionPath);
        }

        /// <summary>
        /// 字符串截取考场号
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns></returns>
        private string FieldInterception(string path)
        {
            string[] paths = path.Split('\\');
            return paths[paths.Length - 2];
        }

        private static void CopyFile(QuestionModel question)
        {
            string errorpath = ErrorCollectHelper.errorParent + question.fileName;
            string[] path1 = question.path.Split('\\');
            string path2 = path1[path1.Length - 1];
            string path3 = errorpath + @"\" + path2;
            if (!Directory.Exists(errorpath))
            {
                Directory.CreateDirectory(errorpath);
            }

            FileInfo file = new FileInfo(question.path);
            //File.Delete(path3);
            //file.MoveTo(path3);
            File.Copy(question.path, path3, true);
        }
    }
}

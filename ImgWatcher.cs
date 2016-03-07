using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace WatcherAsFileService
{
    public class ImgWatcher
    {
        private static ImgsDetectionHelper imageHelper = new ImgsDetectionHelper();
        private static readonly string postfix = MonitoringService.postfix;
        public static List<string> pathList = new List<string>();
        public void WatcherStart()
        {
            //FileSystemWatcher使用详解：http://www.cnblogs.com/springyangwc/archive/2011/08/27/2155547.html
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = MonitoringService.imagePath;
            watcher.Filter = postfix;
            watcher.Created += new FileSystemEventHandler(OnProcess);
            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = true;
            watcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastAccess
                                   | NotifyFilters.LastWrite | NotifyFilters.Size;
            watcher.IncludeSubdirectories = true;
        }
        private static void OnProcess(object source, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                lock ("HGHsd")
                {
                    OnCreated(source, e);
                }
            }
        }
        private static void OnCreated(object source, FileSystemEventArgs e)
        {
            DataRow r = MonitoringService.empTable.NewRow();
            r["path"] = e.FullPath;
            MonitoringService.empTable.Rows.Add(r);
            //pathList.Add(e.FullPath);
            //if (pathList.Count > 5000)
            //{
            //    MonitoringService.addList.AddRange(pathList);
            //    pathList.Clear();
            //}
        }
    }
}

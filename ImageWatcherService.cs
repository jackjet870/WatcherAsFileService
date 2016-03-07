using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Timers;

namespace WatcherAsFileService
{
    public partial class MonitoringService : ServiceBase
    {
        private readonly static int timelag = 1000 * 30;//30s执行一次
        private static readonly string timelagStr = ConfigurationManager.ConnectionStrings["timelag"].ConnectionString;
        public static readonly string minSize = ConfigurationManager.ConnectionStrings["minSize"].ConnectionString;
        public static readonly string maxSize = ConfigurationManager.ConnectionStrings["maxSize"].ConnectionString;
        public static readonly string errorPath = ConfigurationManager.ConnectionStrings["errorPath"].ConnectionString;
        public static readonly string imagePath = ConfigurationManager.ConnectionStrings["imagePath"].ConnectionString;
        public static readonly string postfix = ConfigurationManager.ConnectionStrings["postfix"].ConnectionString;
        private ImgWatcher imgWatcher = new ImgWatcher();
        private Timer time;
        private static ImgsDetectionHelper ImD;
        private List<string> pathList = new List<string>();
        private List<string> errorList = new List<string>();
        public static List<string> addList = new List<string>();

        public static DataTable empTable;
        private static DataTable thisTable;
        public MonitoringService()
        {
            InitializeComponent();
            if (ImD == null)
            {
                ImD = new ImgsDetectionHelper();
            }

            int timelast = 0;
            if (int.TryParse(timelagStr, out timelast))
            {
                if (timelast > 1)
                {
                    time = new Timer(timelast * 1000);
                }
                else
                {
                    time = new Timer(timelag);
                    ErrorCollectHelper.ErrorLog("配置文件错误！timelag配置不符合条件，采取默认30s内置配置");
                }
            }
            else
            {
                time = new Timer(timelag);
                ErrorCollectHelper.ErrorLog("配置文件错误！timelag配置不符合条件，采取默认30s内置配置");
            }

            empTable = new DataTable();
            empTable.Columns.Add("path", typeof(string));
            thisTable = empTable;
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                //这里是监测操作
                imgWatcher.WatcherStart();
                //这里是处理操作
                StartSomething();
            }
            catch (Exception ex)
            {
                ErrorCollectHelper.ErrorLog(ex.ToString());
            }

        }
        private void StartSomething()
        {
            time.AutoReset = true;
            time.Enabled = false;
            time.Elapsed += Time_Elapsed;
            time.Start();
        }

        private void Time_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock ("asdad")
            {
                thisTable = empTable;
                //empTable.Clear();
                ErrorCollectHelper.ErrorLog("当前操作数据量：" + thisTable.Rows.Count.ToString());

                //foreach (DataRow item in thisTable.Rows)
                //{
                //    try
                //    {
                //        ImD.Monitor(item["path"].ToString());
                //        empTable.Rows.Remove(item);
                //    }
                //    catch (Exception ex)
                //    {
                //        empTable.Rows.Add(item);
                //        ErrorCollectHelper.ErrorLog(ex.ToString());
                //    }
                //}

                for (int i = 0; i < thisTable.Rows.Count; i++)
                {
                    try
                    {
                        if (ImD.Monitor(thisTable.Rows[i]["path"].ToString()))
                            empTable.Rows.Remove(thisTable.Rows[i]);
                    }
                    catch (Exception ex)
                    {
                        ErrorCollectHelper.ErrorLog(ex.ToString());
                    }
                }
            }


            //pathList.AddRange(addList);
            //addList.Clear();
            //foreach (var itemPath in pathList)
            //{
            //    try
            //    {
            //        ImD.Monitor(itemPath);
            //    }
            //    catch (Exception ex)
            //    {
            //        errorList.Add(itemPath);
            //        ErrorCollectHelper.ErrorLog(ex.ToString());
            //    }
            //}
            //pathList.Clear();
            //pathList.AddRange(errorList);
            //errorList.Clear();
        }
        protected override void OnStop()
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace readXML
{
    public partial class Service1 : ServiceBase
    {
        public static MySqlConnection conn;
        public static MySqlDataAdapter adapter;
        public static DataSet dt = new DataSet();
        static string conStr = "";
        static string report = "";
        static string backup = "";
        static string error = "";


        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            using (StreamReader r = new StreamReader(@"C:\Users\Admin\Documents\Zalo Received Files\configure.json"))
            {
                string json = r.ReadToEnd();
                //JavaScriptSerializer jss = new JavaScriptSerializer();
                //List<readJson> items = jss.Deserialize<List<readJson>>(json);
                List<readJson> array = JsonConvert.DeserializeObject<List<readJson>>(json);

                foreach (var item in array)
                {
                    report = item.Report_His;
                    conStr = item.Connect_DataBase;
                    backup = item.Backup_Report_His;
                    error = item.Error_Report_His;
                }
            }

            FileSystemWatcher watcher = new FileSystemWatcher(report);
            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = true;

            //xu ly su thay doi cua file
            watcher.Changed += watcher_Changer;
            watcher.Created += watcher_Created;
            watcher.Deleted += watcher_Delete;
            watcher.Renamed += watcher_Renamed;
            Service1.readFileXML();
            Console.Read();
        }

        protected override void OnStop()
        {
        }

        public static void RunCmd(Service1 obj)
        {
            obj.OnStart(null);
            obj.OnStop();
        }

        static void readFileXML()   
        {
            var path = report;
            foreach (var file in System.IO.Directory.GetFiles(path))
            {
                ProcessFile(file);
            }
        }

        static void ProcessFile(string Filename)
        {
            conn = new MySqlConnection(conStr);
            //Console.Read();
            dt.ReadXml(Filename);
            string userID = "";
            string result = "";
            string proposal = "";
            string descttext = "";
            string aprotime = DateTime.Now.ToString("MM/dd/yyyy");
            string aprovebyID = "";

            foreach (DataRow item in dt.Tables[0].Rows)
            {
                userID = item["MaBacSiKetLuan"].ToString();
                result = item["KetLuan"].ToString();
                proposal = item["DeNghi"].ToString();
                descttext = item["MoTa"].ToString();
                aprotime = item["ThoiGianThucHien"].ToString();
                aprovebyID = item["MaBacSiKetLuan"].ToString();
            }
            conn.Open();
            string queryselect = "select * from m_study where OrgCode = " + userID;
            MySqlCommand mySql = new MySqlCommand(queryselect, conn);
            var data = mySql.ExecuteReader();

            //tra ve datatable
            if (!data.HasRows)
            {
                string sourcePath = report;
                string targetPath = error;
                string sourceFile = Path.GetFileName(Filename);
                if (File.Exists(targetPath + "\\" + sourceFile))
                {
                    File.Delete(targetPath + "\\" + sourceFile);
                }
                File.Move(Filename, targetPath + "\\" + sourceFile);
            }

            //conn.Open();
            while (data.Read())
            {
                //conn.Open();
                int id = Convert.ToInt32(data["id"]);
                if (id > 0)
                {
                    conn = new MySqlConnection(conStr);
                    var query = "update m_service_ex set UserID = '" + userID + "',Result = '" + result + "',Proposal = '" + proposal + "',DescTxt ='" + descttext + "',AproTime = '" + aprotime + "',AproveByID = '" + aprovebyID + "'";
                    string sourcePath = report;
                    string targetPath = backup;
                    string sourceFile = Path.GetFileName(Filename);
                    if (File.Exists(targetPath + "\\" + sourceFile))
                    {
                        File.Delete(targetPath + "\\" + sourceFile);
                    }
                    File.Move(Filename, targetPath + "\\" + sourceFile);
                }
                else
                {
                    string sourcePath = report;
                    string targetPath = error;
                    string sourceFile = Path.GetFileName(Filename);
                    if (File.Exists(targetPath + "\\" + sourceFile))
                    {
                        File.Delete(targetPath + "\\" + sourceFile);
                    }
                    File.Move(Filename, targetPath + "\\" + sourceFile);
                }
            }
            conn.Close();
            conn.Dispose();
        }
        private static void watcher_Renamed(object sender, RenamedEventArgs e)
        {
            //ProcessFile(e.Name);
            writeLog.write("Bạn vừa đổi tên file : ", e.Name, null, DateTime.Now);
        }

        private static void watcher_Delete(object sender, FileSystemEventArgs e)
        {
            writeLog.write("Bạn vừa xóa file : ", e.Name, null, DateTime.Now);
        }

        private static void watcher_Created(object sender, FileSystemEventArgs e)
        {
            ProcessFile(report + "\\" + e.Name);
            writeLog.write("Bạn vừa tạo file : ", e.Name, null, DateTime.Now);
        }

        private static void watcher_Changer(object sender, FileSystemEventArgs e)
        {
            //ProcessFile(e.Name);
            writeLog.write("Bạn vừa thay đổi file : ", e.Name, null, DateTime.Now);
        }
    }

    public class writeLog
    {
        public static void write(string text, string name, string oldname, DateTime date)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "log.txt";

            if (System.IO.File.Exists(path))
            {
                Console.WriteLine(text + " " + name + " " + oldname + " " + DateTime.Now.ToString() + Environment.NewLine);
                System.IO.File.AppendAllText(path, text + " " + name + " " + oldname + " " + DateTime.Now.ToString() + Environment.NewLine);
            }
        }
    }


    public class readJson
    {
        public string Connect_DataBase;
        public string Report_His;
        public string Backup_Report_His;
        public string Error_Report_His;
    }
}

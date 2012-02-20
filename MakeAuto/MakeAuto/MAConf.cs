using System;
using System.Text;
using System.Xml;
using System.Data.OleDb;
using System.IO;
using System.Windows.Forms;
using System.Data;
using EnterpriseDT.Net.Ftp;
using System.Diagnostics;

namespace MakeAuto
{
    public enum InfoType
    {
        Nothing = 0,
        FileLog = 1,
        Info,
        Warning,
        Error,
    }

    public class FtpConf
    {
        public string host { get; set; }
        public int port { get; set; }
        public string user { get; set; }
        public string pass { get; set; }
        // ftp初始化路径
        public string ServerDir { get; set; }
        // 本地初始化路径
        public string LocalDir { get; set; }
    }

    public delegate void InfoHandler(string info, InfoType type = InfoType.Info);    //定义信息输出委托

    sealed class MAConf
    {
        // 详细设计说明说配置 
        public string DetailFile { get; set; }
        public string SrcDir { get; set; }
        public string DetailList { get; set; }
 
        // 修改单配置
        public string BaseDir { get; set; }
        public string Author { get; set; }
        public string ModuleList { get; set; }

        // winrar路径
        public string rar { get; private set; }

        public SshConns Conns;

        public Details Dls;
        public FtpConf fc;
        public FTPConnection ftp;

        public SAWVList SAWs;

        // 是否显示对应模块
        public bool ShowSecu;
        public bool ShowCrdt;
        public bool ShowFutu;

        public event InfoHandler LogInfo;    //基本信息实现事件

        private static string LogDir = "Log";
        private static string LogFile = LogDir + "\\" + "MA" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".Log";
        private FileStream filestream;
        private StreamWriter writer;

        // 取配置文件名称
        private readonly string conf = "MAConf.xml";

        private MAConf()
        {
            Conns = new SshConns();
            SAWs = new SAWVList();
            Dls = new Details();
            fc = new FtpConf();
            ftp = new FTPConnection();

            // 初始化日志文件
            InitLog();

            // 添加调试
            //Debug.Listeners.Add(new TextWriterTraceListener(
            //    "Log" + "\\" + "MakeAuto" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".log"));
            // 自动写入文件
            //Debug.AutoFlush = true;

            // 注册事件
            LogInfo += new InfoHandler(WriteLogFile);

            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(conf);

            XmlElement root = xmldoc.DocumentElement;

            // 读取显示属性
            XmlNode xn = root.SelectSingleNode("Part");
            ShowSecu = bool.Parse(xn.Attributes["showsecu"].InnerText);
            ShowFutu = bool.Parse(xn.Attributes["showfutu"].InnerText);
            ShowCrdt = bool.Parse(xn.Attributes["showcrdt"].InnerText);

            // 读取节点配置明细
            xn = root.SelectSingleNode("Detail");
            DetailFile = xn.Attributes["DetailFile"].InnerText;
            SrcDir = xn.Attributes["SrcDir"].InnerText;
            DetailList = xn.Attributes["DetailList"].InnerText;

            // 读取修改单配置明细
            xn = root.SelectSingleNode("Amend");
            BaseDir = xn.Attributes["BaseDir"].InnerText;
            Author = xn.Attributes["Author"].InnerText;
            ModuleList = xn.Attributes["ModuleList"].InnerText;

            // 读取Ssh连接配置
            xn = root.SelectSingleNode("Conns");
            XmlNodeList xnl = xn.ChildNodes;
            foreach (XmlNode x in xnl)
            {
                SshConn sc = new SshConn(x.Attributes["name"].InnerText,
                    x.Attributes["host"].InnerText,
                    int.Parse(x.Attributes["port"].InnerText),
                    x.Attributes["user"].InnerText,
                    x.Attributes["pass"].InnerText);

                // 读取FBASE节点，以决定是否进行编译和重启AS
                xn = x.FirstChild;
                sc.compile = bool.Parse(xn.Attributes["compile"].InnerText);
                sc.restartAs = bool.Parse(xn.Attributes["restartas"].InnerText);

                // 添加此连接到配置组
                Conns.Add(sc);
            }

            // 读取小球FTP路径递交配置
            xn = root.SelectSingleNode("Smallball");
            fc.host = xn.Attributes["host"].InnerText;
            fc.port = int.Parse(xn.Attributes["port"].InnerText);
            fc.user = xn.Attributes["user"].InnerText;
            fc.pass = xn.Attributes["pass"].InnerText;
            fc.ServerDir = xn.ChildNodes[0].Attributes["ServerDir"].InnerText;
            fc.LocalDir = xn.ChildNodes[0].Attributes["LocalDir"].InnerText;

            ftp.ServerAddress = fc.host;
            ftp.ServerPort = fc.port;
            ftp.UserName = fc.user;
            ftp.Password = fc.pass;
            ftp.TransferType = FTPTransferType.BINARY;  // 指定 BINARY 传输，否则对于压缩包会失败
            ftp.CommandEncoding = Encoding.GetEncoding("gb2312"); // 重要，否则乱码且连接不上

            // 读取 WinRAR 压缩配置，在节点RAR上
            rar = xn.ChildNodes[1].Attributes["Path"].InnerText;
            
            // 读取VSS配置
            // 读取Ssh连接配置
            xn = root.SelectSingleNode("SCMS");
            xnl = xn.ChildNodes;
            foreach (XmlNode x in xnl)
            {
                SAWV sv = new SAWV(x.Attributes["name"].Value,
                    x.Attributes["server"].Value,
                    int.Parse(x.Attributes["port"].Value),
                    x.Attributes["database"].Value,
                    x.Attributes["user"].Value,
                    x.Attributes["pass"].Value);

                // 读取FBASE节点，以决定是否进行编译和重启AS
                sv.Workspace = x.Attributes["WorkSpace"].Value;
                sv.Amend = x.Attributes["Amend"].Value;

                // 添加此连接到配置组
                SAWs.Add(sv);
            }
        }

        // 单例化 MAConf
        public static readonly MAConf instance = new MAConf();

        // 刷新详细设计说明书
        public void RefreshDetailList()
        {
            // 判断详细设计说明书是否存在，不存在要求设置，退出程序
            if (!File.Exists(MAConf.instance.DetailFile))
            {
                MessageBox.Show("详细设计说明书不存在，请检查配置文件 DetailFile 节点配置。");
                return;
            }

            DateTime d = File.GetCreationTime(MAConf.instance.DetailFile);
            // 重组时间比较，否则直接用时间比较，因为带了毫秒级数据，会有问题
            DateTime ExcelTime = new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second);

            // 判断 xml 文件是否存在，不存在则创建，存在则判断是否需要更新
            bool bRefresh = false;
            if (File.Exists(MAConf.instance.DetailList))
            {
                XmlTextReader reader = null;
                try
                {
                    // 装载 xml 文件
                    reader = new XmlTextReader(MAConf.instance.DetailList);

                    // 读取 detailtime 属性
                    reader.MoveToContent();
                    string XmlTime = reader.GetAttribute("detailtime");
                    if (DateTime.Compare(ExcelTime, DateTime.Parse(XmlTime)) > 0)
                    {
                        bRefresh = true;
                    }

                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            else
            {
                bRefresh = true;
            }

            // 不需要刷新时直接退出
            if (bRefresh == false)
            {
                return;
            }

            // 需要刷新时，重建详细设计说明书文件
            string strCon = " Provider = Microsoft.Jet.OLEDB.4.0; " +
            "Data Source =" + DetailFile + ";" +
            "Extended Properties = 'Excel 8.0;HDR=No;IMEX=1' ";

            OleDbConnection conn = new OleDbConnection(strCon);
            try
            {
                conn.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            //返回Excel的架构，包括各个sheet表的名称,类型，创建时间和修改时间等 
            DataTable dtSheetName = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables,
                new object[] { null, null, null, "Table" });

            //包含excel中表名的字符串数组
            string[] sTables = new string[dtSheetName.Rows.Count];
            int k = 0;
            for (; k < dtSheetName.Rows.Count; k++)
            {
                sTables[k] = dtSheetName.Rows[k]["TABLE_NAME"].ToString();

                if (sTables[k] == "模块定义$")  // 数起来是 5， 其实是 23
                    break;
            }

            string sql = "select * from [" + sTables[k] + "C10:F200]";
            OleDbDataAdapter myCommand = new OleDbDataAdapter(sql, strCon);
            DataSet ds = new DataSet();
            try
            {
                myCommand.Fill(ds, "[" + sTables[k] + "]");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            // 完毕后关闭连接
            conn.Close();


            #region 保存模块信息到xml文件
            XmlTextWriter writer = new XmlTextWriter(MAConf.instance.DetailList, Encoding.Default);
            writer.Formatting = Formatting.Indented;

            // 写xml文件
            writer.WriteStartDocument();
            // 创建注释节点
            writer.WriteComment("excel详细设计说明书");

            // 创建根元素
            writer.WriteStartElement("detail");
            // 创建本地时间元素属性
            writer.WriteAttributeString("gentime", System.DateTime.Now.ToString());
            // 创建excel文件时间元素属性
            writer.WriteAttributeString("detailtime", File.GetCreationTime(MAConf.instance.DetailFile).ToString());

            // 获取数据到xml文件中，遍历 DataSet 的数据表，保存行列值
            foreach (DataTable table in ds.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    // 处理掉最后一行的空白和中间的空白行
                    if (row[0].ToString().Trim() == String.Empty)
                    {
                        continue;
                    }

                    // 创建节点
                    writer.WriteStartElement(row[0].ToString());
                    writer.WriteAttributeString("file", row[1].ToString());
                    writer.WriteAttributeString("sql", row[2].ToString().Trim());
                    writer.WriteAttributeString("pas", row[3].ToString().Trim());
                    writer.WriteEndElement();
                }
            }

            // 关闭根节点元素和文件
            writer.WriteEndElement();
            writer.WriteEndDocument();

            // 保存文件
            writer.Flush();
            writer.Close();
            #endregion
        }

        // 加载 xml 文件到内存，构建要编译的模块文件
        public void LoadDetailList()
        {
            // 清空列表
            Dls.Clear();
            #region 读取xml文件，重建 alModule
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(MAConf.instance.DetailList);
            XmlNode xn = xmldoc.SelectSingleNode("detail");
            XmlNodeList xnl = xn.ChildNodes;
            foreach (XmlNode x in xnl)
            {
                // 过滤掉期货和融资融券的数据
                if (!ShowFutu && x.Name.IndexOf("期货") >= 0)
                {
                    continue;
                }

                if (!ShowCrdt && x.Name.IndexOf("融资融券") >= 0)
                {
                    continue;
                }

                // 构建 Excel 列表文件
                Detail dl = new Detail(x.Name, x.Attributes["file"].InnerText,
                    x.Attributes["sql"].InnerText, x.Attributes["pas"].InnerText);
                Dls.Add(dl);

            }
            #endregion

            // 设置 Excel 的一些属性
            //eh.ExcelFilePath = MAConf.instance.DetailFile;
            // 显示 Excel
            //eh.IsShowExcel = true;

            //eh.SrcDir = MAConf.instance.SrcDir;
        }

        // 日志事件处理
        public void WriteLog(string info, InfoType type = InfoType.Info)
        {
            LogInfo(info, type);
        }

        // 写文本日志
        private void WriteLogFile(string info, InfoType type = InfoType.Info)
        {
            string prefix;
            if (type == InfoType.Error)
                prefix = "[ERROR]";
            else prefix = "";

            writer.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + prefix + info);
        }

        public void InitLog()
        {
            if (!System.IO.Directory.Exists(LogDir))
            {
                System.IO.Directory.CreateDirectory(LogDir);
            }
            
            filestream = new FileStream(LogFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            writer = new StreamWriter(filestream, System.Text.Encoding.Default);
            writer.BaseStream.Seek(0, SeekOrigin.End);
        }

        public void FlushLog()
        {
            writer.Flush();
            writer.Close();
            filestream.Close();
        }
    }
}
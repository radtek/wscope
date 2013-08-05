using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Data.OleDb;
using System.IO;
using System.Windows.Forms;
using System.Data;
using EnterpriseDT.Net.Ftp;
using System.Diagnostics;
using System.Security;
using System.Collections;
using System.Collections.Generic;

namespace MakeAuto
{
    enum ConfType
    {
        FebsConf = 1,
        CresConf = 2,
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

    public class ComInfo
    {
        public ComInfo(string lang, int version, string coms)
        {
            Lang = lang;
            Version = version;
            Coms = coms;
        }

        public string Lang { get; set; }
        public int Version { get; set; }
        public string Coms { get; set; }
    }

    class BaseConf
    {
        public BaseConf()
        {
            log = OperLog.instance;
            detail = null;
        }

        public void LoadConf(XmlNode root)
        {
            XmlNode xn = null;
            try
            {
                // 读取显示属性
                log.WriteFileLog("读取显示属性");

                type = root.Attributes["type"].InnerText;
                product_id = root.Attributes["product_id"].InnerText;
                enable = bool.Parse(root.Attributes["enable"].InnerText);
                name = root.Attributes["name"].InnerText;
                funclist = root.Attributes["funclist"].InnerText;

                // 读取节点配置明细
                log.WriteFileLog("读取配置库信息");
                xn = root.SelectSingleNode("Repository");
                Repository = xn.Attributes["repo"].InnerText;
                WorkSpace = xn.Attributes["workspace"].InnerText;
                SvnRepo = new SvnPort(name, Repository);
                SvnRepo.Workspace = WorkSpace;

                // 读取修改单配置明细
                log.WriteFileLog("读取开发工具信息");
                xn = root.SelectSingleNode("Develop");
                DevTool = xn.Attributes["devtool"].InnerText;
                Rar = xn.Attributes["rar"].InnerText;
                OutDir = xn.Attributes["outdir"].InnerText;
                // OutDir 如果以 \ 结尾，会导致编译前台Drp时，批处理里会出现 "C:\src\"， \"会被认为是转义，就报错了，
                // 这里如果，结尾是\,去掉
                if (OutDir[OutDir.Length - 1] == '\\')
                {
                    OutDir = OutDir.Substring(0, OutDir.Length - 1);
                }

                // 读取Ssh连接配置
                log.WriteFileLog("读取Ssh连接配置");
                xn = root.SelectSingleNode("SSHConn");
                Conn = new ReSSH(xn.Attributes["name"].InnerText,
                        xn.Attributes["host"].InnerText,
                        int.Parse(xn.Attributes["port"].InnerText),
                        xn.Attributes["user"].InnerText,
                        xn.Attributes["pass"].InnerText,
                        bool.Parse(xn.Attributes["restartas"].InnerText));
                Conn.localdir = OutDir;

                // 读取小球FTP路径递交配置
                log.WriteFileLog("读取小球FTP路径递交配置");
                xn = root.SelectSingleNode("CommitFtp");
                fc = new FtpConf();
                fc.host = xn.Attributes["host"].InnerText;
                fc.port = int.Parse(xn.Attributes["port"].InnerText);
                fc.user = xn.Attributes["user"].InnerText;
                fc.pass = xn.Attributes["pass"].InnerText;
                fc.ServerDir = xn.Attributes["remotedir"].Value;
                fc.LocalDir = xn.Attributes["localdir"].Value;

                // 初始化 ftp配置
                ftp = new FTPConnection();
                ftp.ServerAddress = fc.host;
                ftp.ServerPort = fc.port;
                ftp.UserName = fc.user;
                ftp.Password = fc.pass;
                ftp.TransferType = FTPTransferType.BINARY;  // 指定 BINARY 传输，否则对于压缩包会失败
                ftp.CommandEncoding = Encoding.GetEncoding("gb2312"); // 重要，否则乱码且连接不

                log.WriteFileLog("读取Delphi编译版本配置");
                xn = root.SelectSingleNode("SpecialCom");
                SpeComs = new ArrayList();
                XmlNodeList xnl = xn.ChildNodes;
                foreach (XmlNode x in xnl)
                {
                    if (x.NodeType == XmlNodeType.Comment)
                        continue;
                    ComInfo com = new ComInfo(x.Attributes["lang"].Value,
                        int.Parse(x.Attributes["ver"].Value),
                        x.Attributes["coms"].Value);
                    SpeComs.Add(com);
                }
            }
            catch (Exception e)
            {
                log.WriteLog("加载配置失败，活动节点：" + xn.Name + e.Message, LogLevel.Error);
            }
        }

        public void ResetConf()
        {
            detail = null;
        }
        
        public virtual Detail GetDetail(CommitCom c)
        {
            return null;
        }

        public void GetLangVer(string Name, out string Lang, out int Ver)
        {
            // 检查是否在配置中
            Lang = "delphi";
            Ver = 6;
            foreach (ComInfo c in SpeComs)
            {
                if (c.Coms.IndexOf(Name) >= 0)
                {
                    Lang = c.Lang;
                    Ver = c.Version;
                }
            }
        }

        public virtual bool CompileFront(CommitCom c)
        {
            string Lang;
            int Ver;
            GetLangVer(c.cname, out Lang, out Ver);
            
            // 确定工程名称
            string dPro = Path.Combine(c.sawfile.LocalPath, Path.GetFileNameWithoutExtension(c.cname) + ".dpr");
            bool Result = CompileDpr(Ver, dPro, OutDir);

            return Result;
        }

        public virtual bool CompileBackEnd(CommitCom c)
        {
            return true;
        }

        protected virtual string GetDprName()
        {
            return string.Empty;
        }

        private bool CompileDpr(int dVer, string dPro, string outdir)
        {
            bool Result = true;

            Process p = new Process();
            p.StartInfo.FileName = GetDprName();
            p.StartInfo.Arguments = " " + dVer.ToString() + " " + dPro + " " + outdir + " ";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;
            p.Start();

            string strOutput = p.StandardOutput.ReadToEnd();
            log.WriteLog("[编译命令] " + p.StartInfo.FileName + p.StartInfo.Arguments);
            log.WriteLog("[编译输出]");
            log.WriteLog(strOutput);
            if (strOutput.IndexOf("Complile Failed") >= 0)
            {
                Result = false;
            }

            log.WriteFileLog("[编译结束]");
            p.WaitForExit();
            p.Close();

            return Result;
        }


        public string type {get; set;}
        public string product_id { get; set; }
        public bool enable { get; set; }
        public string name { get; set; }
        public string funclist { get; set; }

        public string Repository { get; private set; }
        public string WorkSpace { get; private set; }
        public string DevTool { get; private set; }
        public string Rar { get; private set; }
        public ReSSH Conn { get; private set; }
        public FtpConf fc {get; private set;}
        public FTPConnection ftp {get; private set;}
        public SvnPort SvnRepo { get; private set; }
        public ArrayList SpeComs;
        public string OutDir { get; set; }
        protected Detail detail ;

        public OperLog log;
    }

    class FebsConf : BaseConf
    {
        public FebsConf(XmlNode root):base()
        {
            LoadConf(root);
            Dls = new Details();
            Exh = new ExcelMacroHelper(DevTool);
            alModule = new ArrayList();
            RefreshDetailList();
            LoadDetailList();
        }

        // 刷新详细设计说明书
        public void RefreshDetailList()
        {
            // 判断详细设计说明书是否存在，不存在要求设置，退出程序
            if (!File.Exists(DevTool))
            {
                MessageBox.Show("详细设计说明书不存在，请检查配置文件 DetailFile 节点配置。");
                return;
            }

            DateTime d = File.GetLastWriteTime(DevTool);
            // 重组时间比较，否则直接用时间比较，因为带了毫秒级数据，会有问题
            DateTime ExcelTime = new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second);

            // 判断 xml 文件是否存在，不存在则创建，存在则判断是否需要更新
            bool bRefresh = false;
            if (File.Exists(detaillist))
            {
                XmlTextReader reader = null;
                try
                {
                    // 装载 xml 文件
                    reader = new XmlTextReader(detaillist);

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
            "Data Source =" + DevTool + ";" +
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
            XmlTextWriter writer = new XmlTextWriter(detaillist, Encoding.Default);
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
            writer.WriteAttributeString("detailtime", ExcelTime.ToString());

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
            xmldoc.Load(detaillist);
            XmlNode xn = xmldoc.SelectSingleNode("detail");
            XmlNodeList xnl = xn.ChildNodes;
            foreach (XmlNode x in xnl)
            {
                // 构建 Excel 列表文件
                Detail dl = new Detail(x.Name, x.Attributes["file"].InnerText,
                    x.Attributes["sql"].InnerText, x.Attributes["pas"].InnerText);
                Dls.Add(dl);

            }
            #endregion
        }

        public override Detail GetDetail(CommitCom c)
        {
            return Dls.FindByName(c);
        }

        public override bool CompileFront(CommitCom c)
        {
            return base.CompileFront(c);
        }

        protected override string GetDprName()
        {
            return "cm.bat";
        }

        public override bool CompileBackEnd(CommitCom c)
        {
            return CompileExcel(c);
        }

        // 编译Excel文件或者后台Sql
        private bool CompileExcel(CommitCom c)
        {
            // 确定详细设计说明书文件
            MacroType m;
            Detail d = Dls.FindByName(c);
            if (d == null)
            {
                log.WriteErrorLog("查找不到对应的详细设计说明书模块！");
                return false;
            }

            // 标定index
            bool Result = true;
            int index = Dls.IndexOf(d) + 1;

            // 先把存在的CError删除，以检测是否发生编译错误
            if (File.Exists(Path.Combine(OutDir, "CError.txt")))
            {
                File.Delete(Path.Combine(OutDir, "CError.txt"));
            }

            // 编译Excel 最耗时，对Excel检查是否需要编译，比较PC文件
            bool bNew = false;
            string tpath = Path.Combine(
                WorkSpace, @"Documents\D2.Designs\详细设计\后端", d.File);
            DateTime t2 = File.GetLastWriteTime(tpath);
            DateTime t1 = t2.AddSeconds(-1);
            if (c.ctype == ComType.SO)
            {
                foreach (string s in d.ProcFiles)
                {
                    t1 = File.GetLastWriteTime(Path.Combine(OutDir, s));
                    if (DateTime.Compare(t1, t2) > 0)
                    {
                        bNew = true;
                        break;
                    }
                }
            }
            else if (c.ctype == ComType.Sql)
            {
                t1 = File.GetLastWriteTime(Path.Combine(OutDir, d.SqlFile));
                if (DateTime.Compare(t1, t2) > 0)
                {
                    bNew = true;
                }
            }

            if (bNew)
            {
                log.WriteLog("本地源代码时间晚于Excel文件时间，不需集成处理！" + c.cname + " " + c.ctype);
                return true;
            }

            if (c.ctype == ComType.SO)
            {
                m = MacroType.ProC;
            }
            else if (c.ctype == ComType.Sql)
            {
                m = MacroType.SQL;
            }
            else if (c.ctype == ComType.Xml)
            {
                m = MacroType.FuncXml;
            }
            else
            {
                return true;
            }

            Result = Exh.ScmRunExcelMacro(m, index, OutDir);

            if (!Result)
            {
                return false;
            }
            else if (File.Exists(Path.Combine(OutDir, "CError.txt")))
            {
                Result = false;
                log.WriteErrorLog("检测到编译错误文件，请确认！");
            }

            return Result;
        }

        private const string detaillist = "detail.xml";
        private ArrayList alModule;
        public Details Dls { get; private set; }
        public ExcelMacroHelper Exh;
    }

    class CresConf : BaseConf
    {
        public CresConf(XmlNode x):base()
        {
            LoadConf(x);
        }

        public override Detail GetDetail(CommitCom c)
        {
            Detail d = new Detail(c);
            return d;
        }

        public override bool CompileFront(CommitCom c)
        {
            return base.CompileFront(c);
        }

        public override bool CompileBackEnd(CommitCom c)
        {
            return CompileHDT(c);
        }

        protected override string GetDprName()
        {
            return "cm_08.bat";
        }

        private bool CompileHDT(CommitCom c)
        {
            bool Result = true;

            //分解Project
            string t = c.sawfile.LocalPath;
            string Pdata = string.Empty;
            string Pproject = string.Empty;
            string Pbiz = string.Empty;
            string FlagStr = string.Empty;

            if (c.ctype == ComType.FuncXml && c.cname.IndexOf("functionlist") >= 0)
            {
                Pdata = Path.Combine(WorkSpace, @"Sources\DevCodes");
                Pproject = name;  // 用项目配置
                Pbiz = c.cname;
                FlagStr = "functionlist文件生成完成";
               
            }
            else
            {
                int i = t.IndexOf("DevCodes");
                Pdata = t.Substring(0, i + 8); // 
                t = t.Substring(i + 9);
                i = t.IndexOf("\\");
                Pproject = t.Substring(0, i);
                Pbiz = t.Substring(i + 1);
                FlagStr = "生成模块代码总耗时";
            }


            // 开发工具限制，一定要在HDT下调用程序，否则总是会报下JAVA的错误，因此通过批处理编译，批处理中先将目录进行切换。
            Process p = new Process();
            p.StartInfo.FileName = "cm_back.bat";
            p.StartInfo.Arguments = " " + Pdata +" " + Pproject + " " + Pbiz + " " + OutDir;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;
            p.Start();

            string strOutput = p.StandardOutput.ReadToEnd();
            // 输出最后几行
            string[] strArr = Regex.Split(strOutput, "\r");
            log.WriteFileLog("[编译命令] " + p.StartInfo.FileName + p.StartInfo.Arguments);
            log.WriteFileLog("[编译日志]");
            log.WriteFileLog("编译输出：");
            if (strOutput.IndexOf(FlagStr) < 0)
            {
                log.WriteLog(strOutput, LogLevel.Error);
                Result = false;
            }
            else
            {
                log.WriteLog(strOutput);
            }

            log.WriteFileLog("[编译结束]");
            p.WaitForExit();
            p.Close();

            return Result;
        }
    }


    sealed class MAConf
    {
        private MAConf()
        {
            log = OperLog.instance;
            Configs = new MAConfigs();
            LoadConf();
        }

        // 单例化 MAConf
        public static readonly MAConf instance = new MAConf();

        private void LoadConf()
        {
            log.WriteFileLog("加载配置文件");
            XmlDocument xmldoc = new XmlDocument();
            try
            {
                xmldoc.Load(conf);
            }
            catch(System.IO.FileNotFoundException)
            {
                MessageBox.Show("配置文件不存在，程序退出！");
                Application.Exit();
            }

            XmlElement root = xmldoc.DocumentElement;

            XmlNode xn = root.SelectSingleNode("Products");
            XmlNodeList xnl = xn.ChildNodes;
            foreach (XmlNode x in xnl)
            {
                if (x.Attributes["enable"].Value == "false")
                    continue;

                // 跳过注释，否则格式不对，会报错
                if (x.NodeType == XmlNodeType.Comment)
                    continue;

                BaseConf c = null;
                if (x.Attributes["type"].Value == "cres")
                {
                    c = new CresConf(x);
                }
                else if (x.Attributes["type"].Value == "febs")
                {
                    c = new FebsConf(x);
                }

                if (c != null)
                    Configs.Add(c);
            }

            log.WriteFileLog("配置初始化完成");
        }

        public void WriteLog(string info, LogLevel level = LogLevel.Info)
        {
            log.WriteLog(info, level);
        }

        // 先初始化日志
        private OperLog log;
        // 取配置文件名称
        private readonly string conf = "MAConf.xml";
        public MAConfigs Configs;
    }


    class MAConfigs : ArrayList
    {
        public BaseConf this[string product_id]
        {
            get
            {
                foreach (BaseConf a in this)
                {
                    if (a.product_id == product_id)
                    {
                        return a;
                    }
                }

                return null;
            }
        }
    }
}
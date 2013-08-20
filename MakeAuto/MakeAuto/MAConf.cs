using System;
using System.Text;
using System.Xml;
using System.Data.OleDb;
using System.IO;
using System.Windows.Forms;
using System.Data;
using EnterpriseDT.Net.Ftp;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;

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

    public class DBUser
    {
        public DBUser(string name, string pass, string note)
        {
            this.name = name;
            this.pass = pass;
            this.note = note;
        }

        public static bool test(string pass)
        {
            return true;

        }

        private static byte[] key;
        private static byte[] iv;

        private static void InitKey()
        {
            if (key == null || iv == null)
            {
                byte[] key_t = Encoding.UTF8.GetBytes(G_KEY);
                // 做一下MD5哈希，作为 key，实际可以直接使用，HASH的值太小了
                SHA512Managed provider_SHA = new SHA512Managed();
                byte[] byte_pwdSHA = provider_SHA.ComputeHash(key_t);

                key = new byte[32];
                Array.Copy(byte_pwdSHA, key, 32);
                iv = new byte[16];
                Array.Copy(Encoding.UTF8.GetBytes(G_IV), key, 16);
                myAes.Key = key;
                myAes.IV = iv;
            }
        }

        public static string DecPass(string encpass)
        {
            InitKey();
            string plainText = DecryptStringFromBytes_Aes(Convert.FromBase64String(encpass), myAes.Key, myAes.IV);
            return plainText;

        }

        public static string EncPass(string original)
        {
            InitKey();
            byte[] cipherText = EncryptStringToBytes_Aes(original, myAes.Key, myAes.IV);
            return Convert.ToBase64String(cipherText);
        }

        static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");
            byte[] encrypted;
            // Create an AesManaged object
            // with the specified key and IV.
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {

                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }


            // Return the encrypted bytes from the memory stream.
            return encrypted;

        }

        static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an AesManaged object
            // with the specified key and IV.
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }

            return plaintext;

        }

        public string name { get; private set; }
        public string pass { get; private set; }
        public string note { get; private set; }
        public string file;

        public static AesManaged myAes = new AesManaged();
        const string G_KEY = "12345678xyze133ttyyfahdfajyeafjaldjzdvjldjfdlajafljdfnvladnnvnswdehhe329789oweuw";
        const string G_IV = "87654321xywer123488ewfjaldjf9u238r2fsjfalsfj;al;fjaoewurwhvfadlfhadfjalkfweourw";
    }


    class BaseConf
    {
        public BaseConf()
        {
            log = OperLog.instance;
            Users = new List<DBUser>();
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
                // OutDir 如果以 \ 结尾，会导致编译前台Drp时，批处理里会出现 "C:\src\"， \"会被认为是转义，就报错了，
                // 这里如果，结尾是\,去掉
                if (WorkSpace[WorkSpace.Length - 1] == '\\')
                {
                    WorkSpace = WorkSpace.Substring(0, WorkSpace.Length - 1);
                }
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
                if (fc.LocalDir[fc.LocalDir.Length - 1] == '\\')
                {
                    fc.LocalDir = fc.LocalDir.Substring(0, fc.LocalDir.Length - 1);
                }

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

                // 读取数据库连接属性
                xn = root.SelectSingleNode("DB");
                dbtns = xn.Attributes["dbtns"].Value;
                xnl = xn.ChildNodes;
                foreach (XmlNode x in xnl)
                {
                    DBUser u = new DBUser(x.Attributes["name"].InnerText,
                        x.Attributes["pass"].InnerText,
                        x.Attributes["note"].InnerText);
                    Users.Add(u);
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

        private string GetConnDesc(string user)
        {
            foreach (DBUser u in Users)
            {
                if (u.name.Equals(user))
                    return u.name + "/" + DBUser.DecPass(u.pass) + "@" + dbtns;
            }

            return string.Empty;
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

        public virtual Boolean CompileSql(CommitCom c)
        {
            bool Result = true;
            string path = string.Empty;
            string u = string.Empty;
            foreach (string user in c.users)
            {
                if (c.ctype == ComType.Sql)
                    path = Path.Combine(OutDir, c.cname);
                else
                    path = c.sawfile.LocalPath;

                u = GetConnDesc(user);
                if (u == string.Empty)
                {
                    Result = false;
                    log.WriteErrorLog("无法确认用户连接。" + c.cname);
                    break;
                }

                Result = CompileSqlOne(u, path);
                if (!Result)
                    break;
            }
            return Result;
        }
        
        private StringBuilder sBuilder = new StringBuilder();
        private StringBuilder sBuilderErr = new StringBuilder();

        // todo 实现异步执行的功能
        protected Boolean CompileSqlOne(string ConnStr, string File)
        {
            bool result = true;
            sBuilder.Clear();
            sBuilderErr.Clear();

            Process p = new Process();
            try
            {
                p.StartInfo.FileName = @"cmd.exe";
                // 同步模式下，使用 @ 导入会有问题，sqlplus错误输出流会报引擎错误，用 < 没问题。异步好像 < 和 @ 都可以
                p.StartInfo.Arguments = @"/C sqlplus.exe -S " + ConnStr + " < " + File;
                log.WriteLog("[执行Sql文件] " + File);
                
                p.StartInfo.UseShellExecute = false;        // 关闭Shell的使用  
                p.StartInfo.RedirectStandardInput = false; // 不重定向标准输入，因为文件要输入
                p.StartInfo.RedirectStandardOutput = true;  //重定向标准出  
                p.StartInfo.RedirectStandardError = true; //重定向错误输出  
                p.StartInfo.CreateNoWindow = true;             // 不显示窗口
                
                p.OutputDataReceived += new DataReceivedEventHandler(p_OutputDataReceived);
                p.ErrorDataReceived += new DataReceivedEventHandler(p_ErrorDataReceived);

                p.Start();    // 启动

                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                p.WaitForExit();
                p.Close();

                if (!string.IsNullOrEmpty(sBuilderErr.ToString()))
                {
                    log.WriteLog("[错误流输出]");
                    log.WriteLog(sBuilderErr.ToString(), LogLevel.Error);
                    result = false;
                }
                else if (sBuilder.ToString().IndexOf("错误") >= 0 || sBuilder.ToString().IndexOf("errors") >= 0)
                {
                    log.WriteLog("编译过程可能有错误，请检查日志文件确认！" + File, LogLevel.Error);
                    result = false;
                }

                log.WriteLog("[执行Sql文件结束]" + File);
                return result;
            }
            catch (Exception ex)
            {
                log.WriteLog("执行Sql脚本失败，文件：" + File + "错误信息，" + ex.Message, LogLevel.Error);
                result = false;
            }

            return result;
        }

        private void p_OutputDataReceived(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                sBuilder.AppendLine(outLine.Data);
                OperLog.instance.WriteLog(outLine.Data, LogLevel.SqlExe);
            }
        }

        private void p_ErrorDataReceived(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                sBuilderErr.AppendLine(outLine.Data);
            }
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
        protected Detail detail;
        public List<DBUser> Users { get; private set; }
        public string dbtns { get; private set; }

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
            DateTime t2 = File.GetLastWriteTime(c.sawfile.LocalPath);
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
            else if (c.ctype == ComType.Sql || c.ctype == ComType.FuncXml)
            {
                if (c.ctype == ComType.Sql)
                {
                    t1 = File.GetLastWriteTime(Path.Combine(OutDir, d.SqlFile));
                }
                else
                {
                    t1 = File.GetLastWriteTime(Path.Combine(OutDir, d.XmlFile));
                }

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
            else if (c.ctype == ComType.FuncXml)
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
                log.WriteErrorLog("检测到编译错误文件 CError.txt，请确认！");
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
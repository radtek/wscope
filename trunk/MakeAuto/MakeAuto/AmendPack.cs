using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using EnterpriseDT.Net.Ftp;

namespace MakeAuto
{
    enum ComType
    {
        Nothing = 0,
        SO = 1,
        Sql,
        Exe,
        Dll,
        Patch,
        // 小包SQL
        Ssql,
        Ini,
        Xml,
    }

    enum ComStatus
    {
        NoChange = 0,
        Add = 1,
        Modify,
        Delete,
        Normal, // 处理完成后变成正常状态
    }

    enum ScmType
    {
        Nothing = 0,
        NewScm = 1, // 全新递交，仅对于V1和V1重复集成
        ReScm = 2, // 重新集成
        BugScm = 3, // 补丁修复递交，如V2, V3等
    }

    // 集成处理的状态，其实就是处理的流程和函数的调用顺序
    enum ScmStatus
    {
        Nothing = 0,
        ReadInfo = 1,
        DownLoadPack,
        ProcessPack,
        ReNewReadMe,
        ProcessComs,
        ProcessMods,
        PostComs,
        ProcessReadMe,
        ProcessSAWPath,
        GetFile,
        Compile,
        DiffFile,
        ReNewFile,
        TarPack,
        UpLoad,
        Over,
        Error,
    }

    //string[] D5Pro = {"HsTools.exe", "HsCentrTrans.exe", "HsCbpTrans.exe", ""}

    // 递交程序项
    class CommitCom
    {
        // 程序项名称
        public string cname;
        // 版本
        public string cver;
        // 类型
        public ComType ctype {get; private set;}
        
        // 对应 源代码路径，这个就是小球里的源代码路径
        public string path;

        // SAW文件信息
        public SAWFile sawfile;

        // 组件状态
        public ComStatus cstatus { get; set; }

        public CommitCom(string name, string version, ComStatus status = ComStatus.NoChange)
        {
            cname = name;
            cver = version;
            cstatus = status;
            path = "";

            if (cname.IndexOf("libs") > -1)
                ctype = ComType.SO;
            else if (cname.IndexOf("sql") > -1)
            {
                if (cname.IndexOf("Patch") > -1)
                    ctype = ComType.Patch;
                else if (cname.IndexOf("小包") > -1 || cname.IndexOf("临时") > -1) // 希望能识别出临时修改单，有时会失效
                    ctype = ComType.Ssql;
                else ctype = ComType.Sql;
            }
            else if (cname.IndexOf("exe") > -1)
                ctype = ComType.Exe;
            else if (cname.IndexOf("dll") > -1)
                ctype = ComType.Dll;
            else if (cname.IndexOf("ini") > -1)
                ctype = ComType.Ini;
            else if (cname.IndexOf("xml") > -1)
                ctype = ComType.Xml;
        }
    }

    // 递交项列表
    class ComList: ArrayList
    {
        public CommitCom this[string name]
        {
            get
            {
                foreach(CommitCom c in this)
                {
                    if(c.cname == name)
                        return c;
                }
                return null;
            }
        }
    }

    // 递交修改包
    class AmendPack
    {
        public AmendPack(string AmendNo)
        {
            this.AmendNo = AmendNo;

            ComComms = new ComList();
            // 创建连接
            sqlconn = new SqlConnection(ConnString);
            //为上面的连接指定Command对象
            sqlcomm = sqlconn.CreateCommand();

            SAWFiles = new SAWFileList();

            log = OperLog.instance;

            DiffDir = MAConf.instance.OutDir;

            if (DiffDir[DiffDir.Length - 1] != '\\')
                DiffDir += "\\";

            if (!Directory.Exists(DiffDir))
            {
                try
                {
                    Directory.CreateDirectory(DiffDir);
                }
                catch (Exception e)
                {
                    log.WriteErrorLog("创建编译目录失败，请调整配置文件 OutDir 节点位置，或者手工创建。" + e.Message);
                    //return;
                }
            }

            // 查询修改单信息
            if (QueryAmendInfo() == true)
            {
                // 生成修改单组件包信息
                SetComs();
            }
            else
            {
                scmstatus = ScmStatus.Error;
                log.WriteErrorLog("查询修改单信息失败！");
                return;
            }

            if (QueryFTP() == false)
            {
                scmstatus = ScmStatus.Error;
                log.WriteErrorLog("查询FTP目录信息错误。");
                return;
            }
        }

        // 根据提供的修改单查询主单号
        private Boolean QueryAmendInfo()
        {
            Boolean result = false;
            // 打开连接
            try
            {
                if (sqlconn.State == ConnectionState.Closed)
                {
                    sqlconn.Open();
                }

                // 指定查询项 a.reference_stuff as 递交程序项, a.program_path_a as 递交路径
                sqlcomm.CommandText = ""
                  + " select a.reference_stuff, a.program_path_a "
                  + " from manage.dbo.programreworking2 a "
                  + " where reworking_id = '" + AmendNo + "' ";
                //为指定的command对象执行DataReader;
                sqldr = sqlcomm.ExecuteReader();

                // 如果有数据，读取数据
                while (sqldr.Read())
                {
                    // 获取数据
                    ComString = sqldr["reference_stuff"].ToString().Trim();
                    CommitPath = sqldr["program_path_a"].ToString().Trim();
                }
                  
                // 从CommitPath中分解递交项和主单号 /融资融券/20111123054-国金短信，分解得到 20111123054
                MainNo = CommitPath.Substring(CommitPath.LastIndexOf("/") + 1, 11);
                // 得到递交模块，融资融券 广发版技术支持测试
                int i = CommitPath.LastIndexOf("/");
                AmendSubject = CommitPath.Substring(1,  i - 1);  // 融资融券
                CommitDir = CommitPath.Substring(i + 1);  // 20111123054-国金短短信

                // Readme 文件名称
                Readme = "Readme-" + CommitDir + ".txt";

                // 标定路径
                RemoteDir = MAConf.instance.fc.ServerDir + CommitPath;

                string val;
                if (MAConf.instance.fc.PathCorr.TryGetValue(AmendSubject, out val))
                {
                    LocalDir = val;
                }
                else
                {
                    LocalDir = MAConf.instance.fc.LocalDir;
                }

                result = true;
            }
            catch (Exception e)
            {
                result = false;
                log.WriteLog("查询修改单递交路径失败，" + e.Message, LogLevel.Error);
            }
            finally
            {
                if (sqldr != null)
                {
                    sqldr.Close();
                }

                if (sqlconn != null)
                {
                    sqlconn.Close();
                }
            }

            return result;
        }

        // 设置递交组件
        private void SetComs()
        {
            ComComms.Clear();
            // 查询出的组件是如下的一段
            // config.ini  [V6.1.4.7]  GJShortMessage.dll  [V6.1.4.1]  HsNoticeSvr.exe  [V6.1.4.6] 
            // 需要进行分解，操作如下
            string name, version, cs = ComString;

            int s = 0, e = 0;
            while (cs.Length > 0)
            {
                s = cs.IndexOf("["); // 取第一个版本分隔符号
                e = cs.IndexOf("]"); // 取版本分隔符号
                name = cs.Substring(0, s - 1).Trim(); // 程序名称 
                version = cs.Substring(s + 1, e - s - 1);  // 程序版本
                CommitCom c = new CommitCom(name, version);
                // 添加组件
                ComComms.Add(c);

                // 取剩余递交项
                if (e < cs.Length - 1)
                {
                    cs = cs.Substring(e + 1).Trim();
                }
                else
                {
                    cs = "";
                }
            }
        }

        #region 递交组件版本比较
        public bool ValidateVersion()
        {
            bool ret = true, ret1 = true;
            foreach (CommitCom c in ComComms)
            {
                // 这里的校验主要校验版本，集成编译之后，对于SO，还要校验大小
                // 根据文件类型调用不同方法校验版本
                switch (c.ctype)
                {
                    case ComType.Dll:
                    case ComType.Exe:
                        ret1 = ValidateDll(c);
                        break;
                    case ComType.SO:  // SO 文件只有一个版本信息
                    case ComType.Ini: // Ini 文件有多个版本信息
                    case ComType.Patch: // Patch 文件有多行版本信息
                    case ComType.Sql:
                        ret1 = ValidateSO(c);
                        break;
                    case ComType.Ssql:  // 小包无版本
                        break;
                    default:
                        break;
                }

                if (ret1 == false)
                    ret = ret1;
            }

            return ret;

        }

        public bool ValidateSO(CommitCom c)
        {
            // 版本比较 
            // 取压缩包的 SO 测试下正则表达式，同时比较版本信息
            StreamReader sr = new StreamReader(SCMAmendDir + "/" + c.cname);
            string input = sr.ReadToEnd();
            //string pattern = @"V(\d+\.){3}\d+"; // V6.1.31.16
            string pattern = c.cver;
            Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            // 确保版本逆序，只取第一个
            Match m = rgx.Match(input);

            sr.Close();
            return m.Success;
        }

        public bool ValidateDll(CommitCom c)
        {
            // 获取文件版本信息
            FileVersionInfo DllVer = FileVersionInfo.GetVersionInfo(
                Path.Combine(SCMAmendDir + Path.DirectorySeparatorChar + c.cname));

            // cver的 V 去掉，从第一个开始比较
            return DllVer.FileVersion == c.cver.Substring(1);
        }
        #endregion

        // 获取FTP递交信息
        public Boolean QueryFTP()
        {
            log.WriteInfoLog("查询FTP目录信息...");
            // 输出递交包，到本地集成环境处理，需要使用ftp连接
            FTPConnection ftp = MAConf.instance.ftp;
            FtpConf fc = MAConf.instance.fc;
            string s;
            ArrayList SubmitL, ScmL;

            SubmitL = new ArrayList();
            ScmL = new ArrayList();

            // 强制重新连接，防止时间长了就断掉了
            if (ftp.IsConnected == false)
            {
                try
                {
                    ftp.Connect();
                }
                catch(Exception e)
                {
                    log.WriteErrorLog("连接FTP服务器失败，错误信息：" + e.Message);
                }
            }

            // 取递交版本信息，确认要输出哪个版本的压缩包，确保只刷出最大的版本
            // 这个地方，应该是如果存在集成的文件夹，就刷出集成的文件夹，
            // 对于V1，是全部都需要集成，对于Vn(n>1)，只集成变动的部分就可以了
            if (ftp.DirectoryExists(RemoteDir) == false)
            {
                System.Windows.Forms.MessageBox.Show("FTP路径" + fc.ServerDir + CommitPath + "不存在！");
                return false;
            }
            ftp.ChangeWorkingDirectory(fc.ServerDir);

            // 不使用 true 列举不出目录，只显示文件，很奇怪
            //string[] files = ftp.GetFiles(fc.ServerDir + ap.CommitPath, true); 
            string[] files = ftp.GetFiles(RemoteDir);

            log.WriteInfoLog("查询FTP目录信息...完成");

            #region 确定修改单基本属性
            // 检查是否存在集成*的文件夹
            // 获取当前的版本信息，先标定版本信息
            SubmitL.Clear();
            ScmL.Clear();
            foreach (string f in files) //查找子目录
            {
                // 跳过 src-V*.rar 之类的东东
                if (f.IndexOf(MainNo) < 0)
                    continue;

                if (f.IndexOf("集成") == 0)
                    ScmL.Add(f);
                else
                    SubmitL.Add(f);
            }
            
            string currVerFile = string.Empty;// 20111123054-国金短信-李景杰-20120117-V3.rar
            if (SubmitL.Count > 0)
            {
                SubmitL.Sort();
                s = SubmitL[SubmitL.Count - 1].ToString();
                currVerFile = s;
                // 取递交版本号 
                // 20111207012-委托管理-高虎-20120116-V13.rar --> 20111207012-委托管理-高虎-20120116-V13 -> 13
                s = s.Substring(0, s.LastIndexOf('.'));
                SubmitVer = int.Parse(s.Substring(s.LastIndexOf('V') + 1));
            }
            else
            {
                SubmitVer = 0;
            }

            // 决定是新集成还是修复集成还是重新集成
            if (SCMLastVer == 0)
            {
                scmtype = ScmType.NewScm;
            }
            else if (SCMLastVer == SubmitVer)
            {
                scmtype = ScmType.ReScm;
            }
            else if (SCMLastVer < SubmitVer)
            {
                scmtype = ScmType.BugScm;  // 修复集成
            }

            string dir = Path.GetFileNameWithoutExtension(currVerFile);

            //AmendDir = LocalDir + "\\" + dir;
            SCMAmendDir = LocalDir + "\\" + "集成-" + dir;

            ScmVer = SubmitVer;
            LocalFile = LocalDir + "\\" + currVerFile;
            RemoteFile = RemoteDir + "\\" + currVerFile;

            SCMLocalFile = SCMAmendDir + "\\" + "集成-" + currVerFile;
            SCMRemoteFile = RemoteDir + "\\" + "集成-" +currVerFile;

            SrcRar = SCMAmendDir + "\\" + "src-V" + ScmVer.ToString() + ".rar";
            SCMSrcRar = SCMAmendDir + "\\" + "集成-src-V" + ScmVer.ToString() + ".rar";

            // 生成一些上次集成的变量，需要把上次的覆盖到本次来
            if (ScmL.Count > 0)
            {
                ScmL.Sort();
                s = ScmL[ScmL.Count - 1].ToString();
                dir = Path.GetFileNameWithoutExtension(s);
                // 上次数据
                SCMLastAmendDir = LocalDir + "\\" + dir;
                SCMLastLocalFile = SCMLastAmendDir + "\\" + s;

                // 取递交版本号 
                // 20111207012-委托管理-高虎-20120116-V13.rar --> 20111207012-委托管理-高虎-20120116-V13 -> 13
                s = s.Substring(0, s.LastIndexOf('.'));
                SCMLastVer = int.Parse(s.Substring(s.LastIndexOf('V') + 1));
                SCMLastSrcRar = SCMLastAmendDir + "\\" + "集成-src-V" + SCMLastVer.ToString() + ".rar";
            }
            else
            {
                SCMLastVer = 0;
            }
            #endregion

            return true;
        }
        
        // 查询单号
        public string AmendNo {get; set;}
        
        // 主单号
        public string MainNo {get; private set;}

        // 修改单列表
        public string AmendList;

        // 修改单列表,可以递交N多修改单
        //public int[] Amends {get; set;}

        // 存放路径
        public string CommitPath {get; private set; } // /广发版技术支持测试/20111223029-深圳大宗

        // 递交文件夹 
        public string CommitDir { get; private set; } // 20111223029-深圳大宗

        public string AmendSubject { get; private set; } // 融资融券 广发版技术支持测试

        // 本地修改单路径，不带最后的 /
        public string LocalDir {get; private set;} // E:\xgd\融资融券\20111123054-国金短信

        // 远程递交文件夹
        public string RemoteDir {get; private set;} //

        // 修改单压缩包本地存放文件
        public string LocalFile { get; private set; }

        public string SCMLocalFile {get; private set; }  // 集成输出文件，可能查询的时候还不存在

        public string RemoteFile {get; private set;}

        public string SCMRemoteFile {get; private set;} // 远程集成输出文件
 
        // 本地修改单V*文件夹路径，不带最后的 /
        //public string AmendDir { get; private set; } // E:\xgd\融资融券\20111123054-国金短信\20111123054-国金短信-李景杰-20120117-V1

        // 集成文件夹路径
        public string SCMAmendDir { get; private set; }  // E:\xgd\融资融券\20111123054-国金短信\集成-20111123054-国金短信-李景杰-20120117-V1

        // 需求单号，暂时不用
        //private string ReqNo {get; set;}

        public string SrcRar { get; private set; }
        public string SCMSrcRar { get; private set; }

        // 上次集成版本
        public int SCMLastVer { get; private set; }
        public string SCMLastAmendDir { get; private set; }
        public string SCMLastLocalFile { get; private set; }
        public string SCMLastSrcRar { get; private set; }

        // 修改单递交组件，以字符串对象和对象两种形态体现，调整字符串对象为私有（主要是使用不方便）
        public string ComString {get; private set;}
        public ComList ComComms {get; private set;}
        public SAWFileList SAWFiles { get; set; }

        // sql server 连接串，定义为私有，对外不可见
        private readonly string ConnString = "server=192.168.60.60;Initial Catalog=manage;Integrated Security=false;"
            + "uid=jiangshen;pwd=jiangshen;Connection Timeout=5";

        // 建立连接对象
        private SqlConnection sqlconn;
        private SqlCommand sqlcomm;
        private SqlDataReader sqldr;

        public int SubmitVer {get; set;}
        public int ScmVer { get; set; }

        public string Readme { get; private set; }  // readme文件名称

        public ScmType scmtype;

        public ScmStatus laststatus;
        public ScmStatus scmstatus;

        public SvnVersion svn;
        //public SAWV sv;

        public string DiffDir { get; private set; }
        private OperLog log;
    }
}

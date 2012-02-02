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
using SAWVSDKLib;
using EnterpriseDT.Net.Ftp;

namespace MakeAuto
{
    enum ComType
    {
        Nothing = 0,
        SO = 1,
        Sql = 2,
        Exe = 3,
        Dll = 4,
        Patch = 5,
        // 小包SQL
        Ssql =6,
        Ini = 7,
    }

    enum ComStatus
    {
        NoChange = 0,
        Add = 1,
        Modify = 2,
        Delete = 3,
    }

    // 递交程序项
    class CommitCom
    {
        // 程序项名称
        public string cname;
        // 版本
        public string cver;
        // 类型
        public ComType ctype {get; private set;}
        // 对应 源代码名称
        //public string srcName;
        // 对应VSS路径（暂时不用）
        //public string SAWPath;

        //
        public ComStatus cstatus { get; set; }

        public CommitCom(string name, string version, ComStatus status = ComStatus.NoChange)
        {
            cname = name;
            cver = version;
            cstatus = status;

            if (cname.IndexOf("libs") > -1)
                ctype = ComType.SO;
            else if (cname.IndexOf("sql") > -1)
            {
                ctype = ComType.Sql;
                if (cname.IndexOf("Patch") > -1)
                    ctype = ComType.Patch;
                else if (cname.IndexOf("小包") > -1)
                    ctype = ComType.Ssql;
            }
            else if (cname.IndexOf("exe") > -1)
                ctype = ComType.Exe;
            else if (cname.IndexOf("dll") > -1)
                ctype = ComType.Dll;
            else if (cname.IndexOf("ini") > -1)
                ctype = ComType.Ini;

        }

        // 取需要刷出的 文件版本 信息
        public int GetAmendVer(string User, string AmendNo, string FileVersion)
        {
            SAWVFileHistorySet hisset = SAWV.instance.GetFileHistory(cname, "");
            string s = string.Empty;
            foreach (SAWVFileHistory his in hisset)
            {
                s = his.Comment;
                // 判断找到的条件，修改单符合，文件版本符合，返回递交的文件的版本
                if (s.IndexOf(AmendNo) > 0 && s.IndexOf(FileVersion) > 0)
                {
                    return his.Version;
                }
            }
            return 0;
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
        public static readonly AmendPack instance = new AmendPack();

        private AmendPack()
        {
            ComComms = new ComList();
            // 创建连接
            sqlconn = new SqlConnection(ConnString);
            //为上面的连接指定Command对象
            sqlcomm = sqlconn.CreateCommand();

            MyAL = new ArrayList();
            ScmAL = new ArrayList();
        }

        // 类初始化
        public void QueryAmend(string AmendNo)
        {
            this.AmendNo = AmendNo;
            // 查询修改单信息
            QueryAmendInfo();
            // 生成修改单组件包信息
            SetComs();
        }

        // 根据提供的修改单查询主单号
        private int QueryAmendInfo()
        {
            // 打开连接
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
            // Readme 文件名称
            Readme = "Readme-" + CommitDir + ".txt";

            sqldr.Close();

            return 0;
        }

        // 设置递交组件
        private void SetComs()
        {
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

        public void DownloadPack()
        {
            // 输出递交包，到本地集成环境处理，需要使用ftp连接
            FTPConnection ftp = MAConf.instance.ftp;
            FtpConf fc = MAConf.instance.fc;
            string s;

            if (ftp.IsConnected == false)
            {
                ftp.Connect();
            }

            // 取递交版本信息，确认要输出哪个版本的压缩包，确保只刷出最大的版本
            // 这个地方，应该是如果存在集成的文件夹，就刷出集成的文件夹，
            // 对于V1，是全部都需要集成，对于Vn(n>1)，只集成变动的部分就可以了
            if (ftp.DirectoryExists(RemoteDir) == false)
            {
                System.Windows.Forms.MessageBox.Show("FTP路径" + fc.ServerDir + CommitPath + "不存在！");
                return;
            }
            ftp.ChangeWorkingDirectory(fc.ServerDir);

            // 不使用 true 列举不出目录，只显示文件，很奇怪
            //string[] files = ftp.GetFiles(fc.ServerDir + ap.CommitPath, true); 
            string[] files = ftp.GetFiles(RemoteDir);

            // 检查是否存在集成*的文件夹
            // 获取当前的版本信息，先标定版本信息
            MyAL.Clear();
            ScmAL.Clear();
            foreach (string f in files) //查找子目录
            {
                // 跳过 src 之类的东东
                if (f.IndexOf(MainNo) < 0)
                    continue;

                if (f.IndexOf("集成") == 0)
                    ScmAL.Add(f);
                else
                    MyAL.Add(f);
            }

            if (MyAL.Count > 0)
            {
                MyAL.Sort();


                s = MyAL[MyAL.Count - 1].ToString();
                currVerFile = s;

                // 取递交版本号 
                // 20111207012-委托管理-高虎-20120116-V13.rar --> 20111207012-委托管理-高虎-20120116-V13 -> 13
                s = s.Substring(0, s.LastIndexOf('.'));
                myver = int.Parse(s.Substring(s.LastIndexOf('V') + 1));
            }
            else myver = 0;

            if (ScmAL.Count > 0)
            {
                ScmAL.Sort();

                s = ScmAL[ScmAL.Count - 1].ToString();

                SCMcurrVerFile = s;

                // 取递交版本号 
                // 20111207012-委托管理-高虎-20120116-V13.rar --> 20111207012-委托管理-高虎-20120116-V13 -> 13
                s = s.Substring(0, s.LastIndexOf('.'));
                scmver = int.Parse(s.Substring(s.LastIndexOf('V') + 1));
            }
            else scmver = 0;

            // 下载递交包
            if (!Directory.Exists(LocalDir))
            {
                Directory.CreateDirectory(LocalDir);
            }
            ftp.DownloadFile(LocalFile, RemoteFile);
        }

        public void ProcessPack()
        {
            string s, lastscm;
            // 检查文件夹是否存在，如果存在，就先执行删除 
            // E:\xgd\融资融券\20111123054-国金短信\20111123054-国金短信-李景杰-20120117-V1
            if (Directory.Exists(AmendDir))
            {
                Directory.Delete(AmendDir, true);
            }

            // 检查集成文件夹是否存在，存在则执行删除
            // E:\xgd\融资融券\20111123054-国金短信\集成-20111123054-国金短信-李景杰-20120117-V1
            if (Directory.Exists(SCMAmendDir))
            {
                Directory.Delete(SCMAmendDir, true);
            }
            Directory.CreateDirectory(SCMAmendDir);

            // 如果还没有集成过，那么执行集成
            if (scmver == 0)
            {
                // 下载压缩包之后，解压缩包，重命名文件夹为集成-*，
                UnRar(LocalFile, SCMAmendDir);

                // 所有递交组件标记为新增
                foreach (CommitCom c in ComComms)
                {
                    c.cstatus = ComStatus.Add;
                }
            }
            else if (scmver == myver) // 如果相等，则是重新集成，不处理
            {

            }
            else if (scmver < myver) // 否则，递交了新的版本，刷新递交包，重新命名集成包
            {
                // 复制上一次集成文件夹为本次集成文件夹
                s = SCMcurrVerFile;
                s = s.Substring(0, s.LastIndexOf('.'));

                lastscm = LocalDir + Path.DirectorySeparatorChar + s;
                if (Directory.Exists(lastscm))
                {
                    DirectoryCopy(lastscm, SCMAmendDir, false);
                }

                // 如果存在 src 压缩文件夹，解压缩 src 文件夹
                string srcrar = SCMAmendDir + Path.DirectorySeparatorChar + "src-V" + scmver.ToString() + ".rar";
                if (File.Exists(srcrar))
                {
                    UnRar(srcrar, SCMAmendDir);
                    File.Delete(srcrar);
                }

                // 对于下载的递交文件，解压缩readme到集成文件夹，以便根据本次变动取出需要重新集成的文件
                UnRar(LocalFile, SCMAmendDir, Readme);
            }
        }
            
        // 根据 Readme 置重新集成状态
        public void ReSCM()
        {
            // 读取readme，重新集成
            try
            {
                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader.
                using (StreamReader sr = new StreamReader(SCMAmendDir + "/" + Readme))
                {
                    string line, name, version, des;
                    int index, index1;
                    CommitCom com;
                    // 读取本次修改
                    while ((line = sr.ReadLine()) != null)
                    {
                        // 对于V1递交，不需要处理
                        if(line.IndexOf("本次修改(V1)") >=0)
                            return;

                        // 跳过前导行
                        if (line.IndexOf("本次修改") < 0)
                        {
                            continue;
                        }

                        if (line.Trim() == string.Empty)
                            continue;

                        // 到集成注意，退出
                        if (line.IndexOf("集成注意") >= 0)
                        {
                            break;
                        }

                        // 此时读取到了本次修改，可以接着向下读取变
                        index = line.IndexOf("[");
                        index1 = line.IndexOf("]");
                        name = line.Substring(0, index).Trim();
                        version = line.Substring(index + 1, index1 - index -1 );      
                        des = line.Substring(index1 + 1).Trim();

                        if (ComComms[name] == null)
                        {
                            CommitCom c = new CommitCom(name, version, ComStatus.Add);
                            ComComms.Add(c);
                            
                        }
                        else
                        {
                            com = ComComms[name];
                            if (des.IndexOf("本次取消") >= 0)
                            {
                                com.cstatus = ComStatus.Delete;
                            }
                            else
                            {
                                com.cstatus = ComStatus.Modify;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Let the user know what went wrong.
                WriteLog(InfoType.Error, "ReSCM异常" + Readme + ex.Message);
            }
        }

        // 集成所有递交组件
        public void DoWork()
        {
            foreach (CommitCom c in ComComms)
            {
                // 生成，包含如下
                // 1.刷出代码 2.编译 3.拷贝文件到集成目录
                // 对于 小包，如果有变动，则直接拷贝递交的就可以了，其他的需要重新集成

            }
        }

        private static void DirectoryCopy(
        string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the source directory does not exist, throw an exception.
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory does not exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }


            // Get the file contents of the directory to copy.
            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files)
            {
                // Create the path to the new copy of the file.
                string temppath = Path.Combine(destDirName, file.Name);

                // Copy the file.
                file.CopyTo(temppath, false);
            }

            // If copySubDirs is true, copy the subdirectories.
            if (copySubDirs)
            {

                foreach (DirectoryInfo subdir in dirs)
                {
                    // Create the subdirectory.
                    string temppath = Path.Combine(destDirName, subdir.Name);

                    // Copy the subdirectories.
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        // 写日志
        public void WriteLog(InfoType type, string LogContent, string Title = "")
        {
            MAConf.instance.WriteLog(type, LogContent, Title);
        }

        // 打包压缩
        private void Rar(string path, string dir)
        {
            // 暂无实现需要

        }

        // 解压缩
        private void UnRar(string rarfile, string dir, string file = "")
        {
            // 开启进程执行 rar解压缩
            // 获取 Winrar 的路径（通过注册表或者是配置，这里直接根据xml来处理）
            // 实例化 Process 类，启动执行进程  
            Process p = new Process();
            try
            {
                p.StartInfo.FileName = MAConf.instance.rar;           // rar程序名  
                // 解压缩的参数

                // 如果没有指定解压某一个文件，则解压缩全部文件
                if (file == "")
                {
                    file = " ";
                }

                p.StartInfo.Arguments = " E -y -ep " + rarfile + " " + file + " " + dir;   // 设置执行参数  
                p.StartInfo.UseShellExecute = false;        // 关闭Shell的使用  
                p.StartInfo.RedirectStandardInput = true; // 重定向标准输入
                p.StartInfo.RedirectStandardOutput = true;  //重定向标准出  
                p.StartInfo.RedirectStandardError = true; //重定向错误输出  
                p.StartInfo.CreateNoWindow = true;             // 不显示窗口 
                p.Start();    // 启动
                //return p.StandardOutput.ReadToEnd();        //從输出流取得命令执行结果
            }
            catch (Exception ex)
            {
                WriteLog(InfoType.Error, "执行rar失败" + ex.Message);
            }
        }

        // 查询单号
        public string AmendNo {get; set;}
        
        // 主单号
        public string MainNo {get; private set;}

        // 修改单列表,可以递交N多修改单
        //public int[] Amends {get; set;}

        // 存放路径
        public string CommitPath {get; private set;} // /广发版技术支持测试/20111223029-深圳大宗

        // 递交文件夹 
        public string CommitDir // 20111223029-深圳大宗
        { 
            get
            {
                return CommitPath.Substring(CommitPath.LastIndexOf("/") + 1);
            } 
        }

        public string CommitModule // 融资融券 广发版技术支持测试
        {
            get
            {
                return CommitPath.Substring(0, CommitPath.IndexOf("/"));
            }
        }

        // 本次V*包
        public string currVerFile { get; set; } // 20111123054-国金短信-李景杰-20120117-V3.rar
        public string SCMcurrVerFile { get; set; } // 集成-20111123054-国金短信-李景杰-20120117-V2.rar

        // 本地修改单路径，不带最后的 /
        public string LocalDir // E:\xgd\融资融券\20111123054-国金短信
        {
            get
            {
                return MAConf.instance.fc.LocalDir + CommitPath;
            }
        }

        // 远程递交文件夹
        public string RemoteDir
        {
            get 
            {
                return MAConf.instance.fc.ServerDir + CommitPath;
            }
        }

        // 修改单压缩包本地存放文件
        public string LocalFile
        {
            get
            {
                return LocalDir + "\\" + currVerFile;
            }
        }

        public string RemoteFile
        {
            get 
            {
                return RemoteDir + "\\" + currVerFile;
            }
        }
 
        // 本地修改单V*文件夹路径，不带最后的 /
        public string AmendDir
        {
            get
            {
                return LocalDir + "\\" + Path.GetFileNameWithoutExtension(LocalFile);
            }
        }  // E:\xgd\融资融券\20111123054-国金短信\20111123054-国金短信-李景杰-20120117-V1

        // 集成文件夹路径
        public string SCMAmendDir 
        {
            get
            {
                return LocalDir + "\\" + "集成-" + Path.GetFileNameWithoutExtension(LocalFile);
            }
        }  // E:\xgd\融资融券\20111123054-国金短信\集成-20111123054-国金短信-李景杰-20120117-V1

        // 需求单号，暂时不用
        //private string ReqNo {get; set;}

        // 修改单递交组件，以字符串对象和对象两种形态体现，调整字符串对象为私有（主要是使用不方便）
        public string ComString {get; private set;}
        public ComList ComComms {get; private set;}

        // sql server 连接串，定义为私有，对外不可见
        private readonly string ConnString = "server=192.168.60.60;database =manage;uid =jiangshen;pwd=jiangshen";

        // 建立连接对象
        private SqlConnection sqlconn;
        private SqlCommand sqlcomm;
        private SqlDataReader sqldr;

        private int myver;
        private int scmver;
        private ArrayList MyAL;
        private ArrayList ScmAL;

        private string Readme;  // readme文件名称
    }
}

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

    // 集成处理的状态，其实就是处理的流程和函数的调用顺序，暂时还没有用到
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
        ReNewFile,
        TarPack,
        UpLoad,
        Over,
        Error,
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

            SubmitL = new ArrayList();
            ScmL = new ArrayList();
            SAWFiles = new SAWFileList();
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
        public bool QueryFTP()
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
                return false;
            }
            ftp.ChangeWorkingDirectory(fc.ServerDir);

            // 不使用 true 列举不出目录，只显示文件，很奇怪
            //string[] files = ftp.GetFiles(fc.ServerDir + ap.CommitPath, true); 
            string[] files = ftp.GetFiles(RemoteDir);

            // 检查是否存在集成*的文件夹
            // 获取当前的版本信息，先标定版本信息
            SubmitL.Clear();
            ScmL.Clear();
            foreach (string f in files) //查找子目录
            {
                // 跳过 src 之类的东东
                if (f.IndexOf(MainNo) < 0)
                    continue;

                if (f.IndexOf("集成") == 0)
                    ScmL.Add(f);
                else
                    SubmitL.Add(f);
            }

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
            else SubmitVer = 0;

            if (ScmL.Count > 0)
            {
                ScmL.Sort();

                s = ScmL[ScmL.Count - 1].ToString();

                SCMcurrVerFile = s;

                // 取递交版本号 
                // 20111207012-委托管理-高虎-20120116-V13.rar --> 20111207012-委托管理-高虎-20120116-V13 -> 13
                s = s.Substring(0, s.LastIndexOf('.'));
                ScmVer = int.Parse(s.Substring(s.LastIndexOf('V') + 1));
            }
            else ScmVer = 0;

            return true;
        }

        public bool DownloadPack()
        {
            // 输出递交包，到本地集成环境处理，需要使用ftp连接
            FTPConnection ftp = MAConf.instance.ftp;
            FtpConf fc = MAConf.instance.fc;
            string scmfile, scmremote, scmdir;

            if (ftp.IsConnected == false)
            {
                ftp.Connect();
            }

            if (ftp.DirectoryExists(RemoteDir) == false)
            {
                System.Windows.Forms.MessageBox.Show("FTP路径" + fc.ServerDir + CommitPath + "不存在！");
                return false;
            }
            ftp.ChangeWorkingDirectory(fc.ServerDir);

            // 下载递交包
            if (!Directory.Exists(LocalDir))
            {
                Directory.CreateDirectory(LocalDir);
            }

            // 递交包只需要同时下载集成包
            ftp.DownloadFile(LocalFile, RemoteFile);

            // 集成包需要全部下载下来
            foreach (string s in ScmL)
            {
                scmfile = LocalDir + "\\" + s;
                scmremote = RemoteDir + "\\" + s;
                if (!File.Exists(scmfile))
                {
                    ftp.DownloadFile(scmfile, scmremote);
                }

                // 如果没有对应的文件夹，那么需要解压缩文件夹
                scmdir = LocalDir + "\\" + Path.GetFileNameWithoutExtension(scmfile);
                if (!Directory.Exists(scmdir))
                {
                    UnRar(scmfile, scmdir);
                }
            }
            return true;
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

            // 决定是新集成还是修复集成还是重新集成
            if (ScmVer == 0)
            {
                scmtype = ScmType.NewScm;
            }
            else if (ScmVer == SubmitVer)
            {
                scmtype = ScmType.ReScm;
            }
            else if (ScmVer < SubmitVer)
            {
                scmtype = ScmType.BugScm;  // 修复集成
            }

            // 对于重新集成，先删除掉上一次集成的软件包，然后按照新集成处理
            if (scmtype == ScmType.ReScm)
            {
                // 删除ftp软件包，删除本地软件包
                FTPConnection ftp = MAConf.instance.ftp;
                if (ftp.IsConnected == false)
                {
                    ftp.Connect();
                }
                Debug.WriteLine("删除本地集成包" + SCMLocalFile);
                File.Delete(SCMLocalFile);
                Debug.WriteLine("删除服务器集成包" + SCMRemoteFile);
                //ftp.DeleteFile(SCMRemoteFile);

                // 重新标定 scmver
                ScmL.RemoveAt(ScmL.Count -1);
                if(ScmL.Count > 0)
                {
                    s = ScmL[ScmL.Count - 1].ToString();
                    SCMcurrVerFile = s;
                    // 取递交版本号
                    // 20111207012-委托管理-高虎-20120116-V13.rar --> 20111207012-委托管理-高虎-20120116-V13 -> 13
                    s = s.Substring(0, s.LastIndexOf('.'));
                    ScmVer = int.Parse(s.Substring(s.LastIndexOf('V') + 1));

                    scmtype = ScmType.BugScm;
                }
                else 
                {
                    ScmVer = 0;
                    scmtype = ScmType.NewScm;
                }
            }

            // 根据集成类型执行集成操作
            if (scmtype == ScmType.NewScm)
            {
                // 下载压缩包之后，解压缩包，重命名文件夹为集成-*，
                UnRar(LocalFile, SCMAmendDir);

                // 所有递交组件标记为新增
                foreach (CommitCom c in ComComms)
                {
                    c.cstatus = ComStatus.Add;
                }
            }
            else // 重新集成和bug集成处理相同
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
                string srcrar = SCMAmendDir + Path.DirectorySeparatorChar + "src-V" + ScmVer.ToString() + ".rar";
                if (File.Exists(srcrar))
                {
                    UnRar(srcrar, SCMAmendDir);
                    File.Delete(srcrar);
                }

                // 对于下载的递交文件，解压缩readme到集成文件夹，以便根据本次变动取出需要重新集成的文件
                UnRar(LocalFile, SCMAmendDir, Readme);
            }
        }
            
        // 根据 Readme 置重新集成状态，这里分成两步，因为数据库里读出来的递交组件不可靠
        // 如果一张修改单不递交，只是生成下 readme，数据库就会更新掉递交的组件，
        // 所以直接使用数据库的stuff字段有问题，调整为使用readme处理
        public void ProcessReadMe()
        {
            // 读取readme分成两步，先重新生成递交组件，然后检测修改
            ProcessComs();
            ProcessMods();
            PostComs();
            ProcessSAWPath();
            // 输出下处理结果
            Debug.WriteLine("ProcessReadMe:Coms...");
            foreach (CommitCom c in ComComms)
            {
                Debug.WriteLine("名称：" + c.cname);
                Debug.Indent();
                Debug.WriteLine("状态：" + Enum.GetName(typeof(ComStatus), c.cstatus));
                Debug.WriteLine("版本：" + c.cver);
                Debug.WriteLine("路径：" + c.path);
                if (c.sawfile == null)
                {
                    Debug.WriteLine("本地路径：test__");
                    Debug.WriteLine("SAW路径：test__");
                }
                else
                {
                    Debug.WriteLine("本地路径：" + c.sawfile.LocalPath);
                    Debug.WriteLine("SAW路径：" + c.sawfile.SAWPath);
                }
                Debug.Unindent();
            }
            Debug.WriteLine("ProcessReadMe:SAWFiles...");
            foreach (SAWFile s in SAWFiles)
            {
                Debug.WriteLine("路径：" + s.Path);
                Debug.Indent();
                Debug.WriteLine("本地路径：" + s.LocalPath);
                Debug.WriteLine("SAW路径：" + s.SAWPath);
                Debug.WriteLine("文件状态：" + Enum.GetName(typeof(FileStatus), s.fstatus));
                Debug.Unindent();
            }
        }

        private void ProcessComs()
        {
            // 清除掉组件列表，重新添加
            ComComms.Clear();
            // 读取readme，重新集成，由于小球上查询的数据库记录的信息冗余，
            try
            {
                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader.
                using (StreamReader sr = new StreamReader(SCMAmendDir + "/" + Readme, Encoding.GetEncoding("gb2312")))
                {
                    string line, name, version;
                    int index, index1;

                    // 读到时停止
                    while ((line = sr.ReadLine()) != null && line.IndexOf("涉及的程序及文件: ") < 0)
                        ;

                    sr.ReadLine(); // 跳过"涉及的程序及文件 ..."这行字

                    // 处理递交的组件
                    while ((line = sr.ReadLine()) != null && line.IndexOf("存放路径: ") < 0)
                    {
                        // 跳过空行
                        if (line.Trim() == string.Empty)
                            continue;

                        // 连续读取两行，一行文件说明和版本，一行存放路径
                        // 读取源代码路径，小包下面没有路径，不用读取
                        index = line.IndexOf("[");
                        name = line.Substring(0, index).Trim();
                        index = line.LastIndexOf("["); // 希望读第一个 "[" 和最后一个 "]" 能够处理掉
                        index1 = line.LastIndexOf("]");
                        version = line.Substring(index + 1, index1 - index - 1);
                       
                        // 生成组件信息
                        CommitCom c = new CommitCom(name, version);

                        if (line.IndexOf("小包-") < 0)
                        {
                            line = sr.ReadLine().Trim();
                            c.path = line.Substring(1, line.Length - 2); // 去除头部和尾部的 []
                        }
                        else 
                        {
                            c.path = "";
                        }
                        ComComms.Add(c);
                    }
                }
            }
            catch (Exception ex)
            {
                // Let the user know what went wrong.
                WriteLog(InfoType.Error, "ProcessComs异常: " + Readme + " " + ex.Message);
            }

            // 输出下处理结果
            Debug.WriteLine("ProcessComs...");
            foreach (CommitCom c in ComComms)
            {
                Debug.WriteLine(c.cname);
                Debug.Indent();
                Debug.WriteLine(Enum.GetName(typeof(ComStatus), c.cstatus));
                Debug.WriteLine(c.cver);
                Debug.WriteLine(c.path);
                Debug.Unindent();
            }
        }

        private void ProcessMods()
        {
            SAWFiles.Clear();
            // 如果是第一次递交，那么不管修改是什么，都需要重新集成
            if (scmtype == ScmType.NewScm)
            {
                // 标记组件为新增
                foreach (CommitCom c in ComComms)
                {
                    c.cstatus = ComStatus.Add;
                }

                // 标记文件为未刷新
                foreach (SAWFile s in SAWFiles)
                {
                    s.fstatus = FileStatus.Old;
                }

                return;
            }

            // 读取readme，重新集成，由于小球上查询的数据库记录的信息可能不对，需要根据readme的作准
            try
            {
                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader.
                using (StreamReader sr = new StreamReader(SCMAmendDir + "/" + Readme, Encoding.GetEncoding("gb2312")))
                {
                    string line, name, version, des;
                    int index, index1;
                    // step 1-本次修改 2-集成注意 3-涉及程序 4-存放路径 5-修改说明
                    CommitCom c;

                    // 读到时停止
                    while ((line = sr.ReadLine()) != null && line.IndexOf("本次修改") < 0)
                        ;

                    sr.ReadLine(); // 跳过"本次修改 ..."这行字

                    // 处理递交的组件
                    while ((line = sr.ReadLine()) != null && line.IndexOf("集成注意：") < 0)
                    {
                        // 跳过空行
                        if (line.Trim() == string.Empty)
                            continue;

                        // 跳过可能的文件名称行，不知道readme里为啥有这种数据
                        if (line.Trim().LastIndexOf("[V") < 0)
                            continue;

                        // 连续读取两行，一行文件说明和版本，一行存放路径
                        // 读取源代码路径，小包下面没有路径，不用读取
                        index = line.IndexOf("[");
                        name = line.Substring(0, index).Trim();
                        index = line.LastIndexOf("["); // 希望读第一个 "[" 和最后一个 "]" 能够处理掉
                        index1 = line.LastIndexOf("]");
                        version = line.Substring(index + 1, index1 - index - 1);
                        des = line.Substring(index1 + 1).Trim();

                        // 这里如果返回了两个或者没找到，那就是有异常
                        c = ComComms[name];
                        if (c == null)
                        {
                            WriteLog(InfoType.Error, "ProcessMods未能找到组件");
                            scmstatus = ScmStatus.Error;
                            return;
                        }

                        if (des.IndexOf("本次取消") >= 0)
                        {
                            c.cstatus = ComStatus.Delete;
                        }
                        else
                        {
                            c.cstatus = ComStatus.Modify;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Let the user know what went wrong.
                WriteLog(InfoType.Error, "ProcessMods异常: " + Readme + " " + ex.Message);
            }
        }
        
        // 后处理组件，删除不需要的内容
        public void PostComs()
        {
            if (scmstatus == ScmStatus.Error)
            {
                return;
            }
            else
            {
                scmstatus = ScmStatus.PostComs;
            }

            // 如果是删除的，那么删除文件（对于so，需要删除src文件）
            foreach (CommitCom c in ComComms)
            {
                if (c.cstatus == ComStatus.Delete || c.cstatus == ComStatus.Modify)
                {
                    // SO删除了，源文件也删除掉
                    File.Delete(SCMAmendDir + "\\" + c.cname);
                    if(c.ctype == ComType.SO)
                    {  
                        foreach(Detail d in MAConf.instance.Dls)
                        {
                            if(d.SO == c.cname)
                            {
                                foreach(string s in d.ProcFiles)
                                {
                                    File.Delete(SCMAmendDir + "\\" + s);
                                }
                            }

                            break;
                        }
                    }  
                }

                // 对于标记为删除的，不需要在列表中维护了
                if (c.cstatus == ComStatus.Delete)
                    ComComms.Remove(c);
            }

            // 处理 SAWFile
            foreach (CommitCom c in ComComms)
            {
                if (c.cstatus == ComStatus.NoChange)
                    continue;

                // 标记文件需要刷新，添加到文件状态列表中
                if (SAWFiles[c.path] == null)
                {
                    SAWFile f = new SAWFile(c.path, FileStatus.Old);
                    f.Version = c.cver;
                    SAWFiles.Add(f);                    
                    c.sawfile = f;
                }
                else
                {
                    // 如果第一个文件不是更新，那就改成需要更新，但是不会有这种情况吧，所以直接改掉好了。
                    SAWFile f = SAWFiles[c.path];
                    f.fstatus = FileStatus.Old;
                    f.Version = c.cver;
                    c.sawfile = f;
                }
            }
        }

        // 处理 SAWPath，设置检出代码的路径
        public void ProcessSAWPath()
        {
            if(scmstatus == ScmStatus.Error)
            {
                return;
            }
            else
            {
                scmstatus = ScmStatus.ProcessSAWPath;
            }

            // 确认修改单的配置库
            sv = MAConf.instance.SAWs.GetByAmend(AmendSubject);
            if(sv == null)
            {
                Debug.WriteLine("获取对应配置库失败");
                scmstatus = ScmStatus.Error;
                return;
            }

            // 处理SAW代码的路径，暂时只对06版有效，因为目录是固定的，需要写死
            foreach (SAWFile s in SAWFiles)
            {
                if (s.Path.IndexOf("小包-") >= 0)
                    continue;

                // 根据 s 的名称来处理，这是一段很纠结的代码
                if (s.Path.IndexOf("金融产品销售系统_详细设计说明书") >= 0) // 
                {
                    string temp = @"HSTRADES11\Documents\D2.Designs\详细设计\后端\";
                    s.LocalPath = sv.Workspace + temp + s.Path;
                    s.SAWPath = @"$/" + temp.Replace('\\','/') + s.Path;
                    
                }
                else
                {
                    s.LocalPath = sv.Workspace + @"HSTRADES11\" + s.Path;
                    s.SAWPath = @"$/" + @"HSTRADES11/" + s.Path.Replace('\\', '/');
                }
            }
        }

        // 刷出VSS代码
        public void GetCode()
        {
            // 这个地方由于没有历史性刷出的办法，对于DLL，可能需要两遍代码刷出，第一遍，先刷出最新版，第二遍，根据递交的版本历史，
            // 把在这个版本之后修改的刷回去
            // 需要注意的是ReadMe中的路径是不全的，这个很恶心
            foreach (SAWFile s in SAWFiles)
            {
                sv.GetAmendCode(AmendNo, s);
                s.fstatus = FileStatus.New;
            }

        }

        public void Compile()
        {
            foreach (CommitCom c in ComComms)
            {
                // 小包直接跳过
                if (c.ctype == ComType.Ssql)
                {
                    c.cstatus = ComStatus.Normal;
                    continue;
                }

                if (c.cstatus == ComStatus.NoChange)
                    continue;

                // Patch 和 Ini 刷下来，拷贝过去就可以
                else if (c.ctype == ComType.Patch || c.ctype == ComType.Ini)
                {
                    File.Copy(c.sawfile.LocalPath, SCMAmendDir+ "//" + c.cname, true);
                }
                else if (c.ctype == ComType.Dll || c.ctype == ComType.Exe)
                {
                    CompileDpr(c);
                }
                else if (c.ctype == ComType.SO || c.ctype == ComType.Sql)
                {
                    // 这里不能并发执行，有可能锁住Excel，可能只能等待执行
                    CompileExcel(c);
                }

                c.cstatus = ComStatus.Normal;
            }
        }

        // 集成所有递交组件
        public void DoPacker()
        {
            // 此时，文件夹的组件已完成，对文件夹压包处理
            // 需要把递交组件和src分别压包，这个与开发递交的组件不同


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

        public void WriteLog(string info)
        {
            MAConf.instance.WriteLog(info);
        }

        // 打包压缩
        private void Rar(string path, string dir)
        {
            // 暂无实现需要

        }

        // 解压缩
        private void UnRar(string rarfile, string dir, string file = "")
        {
            if (!File.Exists(rarfile))
            {
                return;
            }

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            
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

        // 编译Dll或者Exe
        public void CompileDpr(CommitCom c)
        {
            int DVer = 6;
            if(c.cname == "HsTools.exe" || c.cname == "HsCentrTrans.exe" || c.cname == "HsCbpTrans.exe")
            {
                DVer = 5;
            }

            Process p = new Process();
            p.StartInfo.FileName = "cm.bat";
            p.StartInfo.Arguments = " " + DVer.ToString() + " " + c.sawfile.LocalPath + " " + SCMAmendDir;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = false;
            p.Start();

            string strOutput = null;
            strOutput = p.StandardOutput.ReadToEnd();
            Debug.WriteLine(strOutput);
            p.WaitForExit();
            p.Close();
        }

        // 编译Dll或者Exe
        public void CompileExcel(CommitCom c)
        {
            // 确定详细设计说明书文件
            Detail d;
            MacroType m;
            if (c.ctype == ComType.SO)
            {
                m = MacroType.ProC;
                d = MAConf.instance.Dls.FindBySo(c.cname);
            }
            else
            {
                m = MacroType.SQL;
                d = MAConf.instance.Dls.FindBySql(c.cname);
            }

            if (d == null)
            {
                MAConf.instance.WriteLog("查找不对对应的详细设计说明书模块！");
                return;
            }

            // 标定index
            int index = MAConf.instance.Dls.IndexOf(d) + 1;
            ExcelMacroHelper.instance.ScmRunExcelMacro(m, index, SCMAmendDir);

            if (c.ctype == ComType.SO)
            {
                // 编译完成后，需要上传到 ssh 服务器上得到 SO
                SshConn s = MAConf.instance.Conns["scm"];  // 一定要配置这个
                if (s == null)
                {
                    MAConf.instance.WriteLog("集成ssh配置不存在！");
                    return;
                }

                s.localdir = SCMAmendDir + "\\";
                s.UploadModule(d);
                s.Compile(d);
                s.DownloadModule(d);
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

        public string AmendSubject // 融资融券 广发版技术支持测试
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

        public string SCMLocalFile
        {
            get { return LocalDir + "\\" + SCMcurrVerFile; }
        }

        public string RemoteFile
        {
            get 
            {
                return RemoteDir + "\\" + currVerFile;
            }
        }

        public string SCMRemoteFile
        {
            get { return RemoteDir + "\\" + SCMcurrVerFile; }
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
        public SAWFileList SAWFiles { get; set; }

        // sql server 连接串，定义为私有，对外不可见
        private readonly string ConnString = "server=192.168.60.60;database =manage;uid =jiangshen;pwd=jiangshen";

        // 建立连接对象
        private SqlConnection sqlconn;
        private SqlCommand sqlcomm;
        private SqlDataReader sqldr;

        public int SubmitVer {get; private set;}
        public int ScmVer { get; private set; }
        private ArrayList SubmitL;
        private ArrayList ScmL;

        private string Readme;  // readme文件名称

        public ScmType scmtype;

        public ScmStatus scmstatus;

        public SAWV sv;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnterpriseDT.Net.Ftp;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Web;

namespace MakeAuto
{
    /// <summary>
    /// 状态模式实现
    /// </summary>
    abstract class State
    {
        // 状态模式基础状态
        public abstract bool DoWork(AmendPack ap);
        
        public AmendPack Amend
        {
            get { return _amend; }
            set { _amend = value; }
        }

        protected static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
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

        // 解压缩
        protected Boolean UnRar(string rarfile, string dir, string file = "")
        {
            if (!File.Exists(rarfile))
            {
                return false;
            }

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            bool result = true;

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
                    file = "*.*";
                }

                p.StartInfo.Arguments = " E -o+ -- " + rarfile + " " + file + " " + dir;   // 设置执行参数 
                p.StartInfo.UseShellExecute = false;        // 关闭Shell的使用  
                p.StartInfo.RedirectStandardInput = false; // 不重定向标准输入，不知道为啥，重定向好像会一直等待
                p.StartInfo.RedirectStandardOutput = false;  //重定向标准出  
                p.StartInfo.RedirectStandardError = false; //重定向错误输出  
                p.StartInfo.CreateNoWindow = true;             // 不显示窗口

                MAConf.instance.WriteLog(p.StartInfo.FileName + " " + p.StartInfo.Arguments, LogLevel.FileLog);

                p.Start();    // 启动
                p.WaitForExit();

                if (p.HasExited && p.ExitCode != 0)
                {
                    result = false;
                    log.WriteErrorLog("解压缩失败，退出码：" + p.ExitCode.ToString());
                }

                p.Close();
            }
            catch (Exception ex)
            {
                MAConf.instance.WriteLog("执行rar失败" + ex.Message, LogLevel.Error);
            }

            return result;
        }

        // 日志组件
        protected OperLog log = OperLog.instance;

        // 修改单记录
        protected AmendPack _amend {get; set;}

        //
        public virtual string StateName
        { get { return "test";  } }

        public virtual bool Tip
        {
            get { return _tip; }
        }

        private bool _tip = true;
    }

    /// <summary>
    /// 下载压缩包
    /// </summary>
    class PackerDownload : State
    {
        public override bool DoWork(AmendPack ap)
        {
            // 输出递交包，到本地集成环境处理，需要使用ftp连接
            FTPConnection ftp = MAConf.instance.ftp;
            FtpConf fc = MAConf.instance.fc;

            if (ftp.IsConnected == false)
            {
                ftp.Connect();
            }

            if (ftp.DirectoryExists(ap.RemoteDir) == false)
            {
                System.Windows.Forms.MessageBox.Show("FTP路径" + fc.ServerDir + ap.CommitPath + "不存在！");
                return false;
            }
            //ftp.ChangeWorkingDirectory(fc.ServerDir);

            // 下载递交包
            if (!Directory.Exists(ap.LocalDir))
            {
                Directory.CreateDirectory(ap.LocalDir);
            }

            // 递交包只需要下载最新包
            if (ftp.Exists(ap.RemoteFile) == false)
            {
                System.Windows.Forms.MessageBox.Show("FTP文件 " + ap.RemoteFile + " 不存在！");
                return false;
            }
            try
            {
                ftp.DownloadFile(ap.LocalFile, ap.RemoteFile);
            }
            catch (IOException)
            {
                log.WriteErrorLog("下载文件失败，请检查压缩文件是否被锁定！");
                return false;
            }

            // 集成包应该不需要下载，本地文件夹中应该已经存在；这里为了我自己模拟，下载上次的集成包
            if (ap.scmtype == ScmType.BugScm && ftp.Exists(ap.ScmLastRemoteFile))
            {
                if (!File.Exists(ap.SCMLastLocalFile)) // 如果不存在，下载集成包
                {
                    // 下载递交包
                    if (!Directory.Exists(ap.SCMLastAmendDir))
                    {
                        Directory.CreateDirectory(ap.SCMLastAmendDir);
                    }

                    ftp.DownloadFile(ap.SCMLastLocalFile, ap.ScmLastRemoteFile); 
                }

                UnRar(ap.SCMLastLocalFile, ap.SCMLastAmendDir);

                if (ap.scmtype == ScmType.BugScm && ftp.Exists(ap.ScmLastRemoteSrcRar))
                {
                    if (!File.Exists(ap.SCMLastLocalSrcRar))
                    {
                        ftp.DownloadFile(ap.SCMLastLocalSrcRar, ap.ScmLastRemoteSrcRar);
                    }

                    UnRar(ap.SCMLastLocalSrcRar, ap.SCMLastAmendDir);
                }
            }

            return true;
        }
        
        public override string StateName
        {
            get { return "下载压缩包"; }
        }

        public override bool Tip
        {
            get {return _tip;}
        }

        public bool _tip = false;
    }

    /// <summary>
    /// 处理ReadMe
    /// </summary>
    class PackerReadMe : State
    {
        public override string StateName
        {
            get { return "处理ReadMe"; }
        }

        // 根据 Readme 置重新集成状态，这里分成两步，因为数据库里读出来的递交组件不可靠
        // 如果一张修改单不递交，只是生成下 readme，数据库就会更新掉递交的组件，
        // 所以直接使用数据库的stuff字段有问题，调整为使用readme处理
        public override bool DoWork(AmendPack ap)
        {
            bool result = true;

            // 检查集成本地文件夹是否存在，不存在则创建，存在则先删除后创建
            // 对于本地文件夹存在只读文件的情况，直接删除会报错，这里要去只读属性后来删除
            if (Directory.Exists(ap.SCMAmendDir))
            {
                DeleteFolder(ap.SCMAmendDir);
            }
            Directory.CreateDirectory(ap.SCMAmendDir);

            // 如果本地集成文件已存在，删除本地集成文件
            if (File.Exists(ap.SCMLocalFile))
            {
                File.Delete(ap.SCMLocalFile);
            }

            if (!File.Exists(ap.LocalFile))
            {
                log.WriteErrorLog("本地递交包丢失，无法解压缩！");
                return false;
            }

            // 下载压缩包之后，解压缩包，得到 集成-[修改单号]-[模块]-[修改人]-[日期]-V* 的一个文件夹
            UnRar(ap.LocalFile, ap.SCMAmendDir);

            // 如果存在 src 压缩文件夹，解压缩 src 文件夹
            if (File.Exists(ap.SrcRar))
            {
                UnRar(ap.SrcRar, ap.SCMAmendDir);  // 解压处理，供对比用
                File.Delete(ap.SrcRar);  // 删除掉递交的src，不需要留着了
            }

            // 如果Readme不存在，之后的的操作不用执行
            if(!File.Exists(Path.Combine(ap.SCMAmendDir, ap.Readme)))
            {
                log.WriteErrorLog("Readme文件不存在，" + Path.Combine(ap.SCMAmendDir, ap.Readme));
                return false;
            }

            // 优先处理集成注意
            result = ProcessScmNotice(ap);
            if (!result)
                return false;

            // 处理 ReadMe，以获取变动
            // 读取readme分成两步，先重新生成递交组件，然后检测修改
            result = ProcessComs(ap);
            if(!result)
                return false;

            result = ProcessMods(ap);
            if(!result)
                return false;

            result = ProcessSAWPath(ap);
            if(!result)
                return false;

            // 显示处理结果，打开ReadMe文件
            ProcessReadMe(ap);

            return true;
        }

        /// <summary>
        /// 对于有集成注意的，提示集成注意，集成注意现在还不能自动处理
        /// </summary>
        /// <param name="ap"></param>
        /// <returns></returns>
        private bool ProcessScmNotice(AmendPack ap)
        {
            List<string> notice = new List<string>();
            // 读取readme，生成集成注意
            try
            {
                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader.
                using (StreamReader sr = new StreamReader(ap.SCMAmendDir + "/" + ap.Readme,
                    Encoding.GetEncoding("gb2312")))
                {
                    string line;

                    // 读取修改单列表
                    while ((line = sr.ReadLine()) != null && line.IndexOf("修 改 单： ") < 0)
                        ;
                    ap.AmendList = line.Replace("修 改 单： ", string.Empty).Trim();

                    // 读到时停止
                    while ((line = sr.ReadLine()) != null && line.IndexOf("集成注意：") < 0)
                        ;

                    sr.ReadLine(); // 跳过"涉及的程序及文件 ..."这行字

                    // 处理递交的组件
                    while ((line = sr.ReadLine()) != null && line.IndexOf("涉及的程序及文件: ") < 0)
                    {
                        // 跳过空行
                        if (line.Trim() == string.Empty)
                            continue;

                        if (line.Trim() == "暂无!")   // 没有集成注意，会填写一行暂无表示，这个也跳过去
                            continue;

                        // 把集成注意读进来
                        notice.Add(line);
                    }
                }
            }
            catch (Exception ex)
            {
                log.WriteErrorLog("ProcessScmNotice异常 ReadMe：" + ap.Readme + " " + ex.Message);
                return false;
            }

            // 如果集成注意不为空，那么提示用户先处理集成注意
            if (notice.Count > 0)
            {
                string s = "Readme中有如下集成注意，请在处理完成后点击确定继续\n【注意：如果刷新了详细设计说明书文件，请退出程序重启！】：";
                foreach(string t in notice)
                {
                    s += "\r\n" + t;
                }
                System.Windows.Forms.MessageBox.Show(s,
                    "集成注意",
                    System.Windows.Forms.MessageBoxButtons.OK);
            }

            return true;
        }

        private bool ProcessComs(AmendPack ap)
        {
            // 清除掉组件列表，重新添加
            ap.ComComms.Clear();
            // 读取readme，重新集成，由于小球上查询的数据库记录的信息冗余，
            try
            {
                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader.
                using (StreamReader sr = new StreamReader(ap.SCMAmendDir + "/" + ap.Readme,
                    Encoding.GetEncoding("gb2312")))
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
                            if (line == string.Empty)
                            {
                                log.WriteErrorLog("无法取到文件路径信息，请检查！组件名：" + name);
                                return false;
                            }
                            c.path = line.Substring(1, line.Length - 2); // 去除头部和尾部的 []
                        }
                        else
                        {
                            c.path = "";
                        }

                        if ( c.ctype == ComType.Xml && (Path.GetExtension(c.path) == ".xls"))
                        {
                            c.ctype = ComType.FuncXml;
                        }

                        if (c.ctype == ComType.SO)
                        {
                            AddToComs(ap, c);
                        }
                        else
                        {
                            ap.ComComms.Add(c);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.WriteErrorLog("ProcessComs异常 ReadMe：" + ap.Readme + " " + ex.Message);
                return false;
            }

            // 输出下处理结果
            /*
            log.WriteInfoLog("ProcessComs...");
            log.WriteInfoLog(System.Reflection.MethodBase.GetCurrentMethod().Name);
            foreach (CommitCom c in ap.ComComms)
            {
                log.WriteInfoLog(c.cname);
                Debug.Indent();
                log.WriteInfoLog(Enum.GetName(typeof(ComStatus), c.cstatus));
                log.WriteInfoLog(c.cver);
                log.WriteInfoLog(c.path);
                Debug.Unindent();
            }
             * */

            return true;
        }

        private bool ProcessMods(AmendPack ap)
        {
            ap.SAWFiles.Clear();
            // 如果是第一次递交，那么不管修改是什么，都需要重新集成
            if (ap.scmtype == ScmType.NewScm)
            {
                // 标记组件为新增
                foreach (CommitCom c in ap.ComComms)
                {
                    c.cstatus = ComStatus.Add;
                }

                // 标记文件为未刷新
                foreach (SAWFile s in ap.SAWFiles)
                {
                    s.fstatus = FileStatus.Old;
                }

                return true;
            }

            bool deleteCom = false, havechange = false;
            // 读取readme，重新集成，由于小球上查询的数据库记录的信息可能不对，需要根据readme的作准
            try
            {
                using (StreamReader sr = new StreamReader(ap.SCMAmendDir + "/" + ap.Readme,
                    Encoding.GetEncoding("gb2312")))
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
                        c = null;

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

                        // ProcessComms 生成的组件不包含已删除的组件，这里需要跳过此类文件
                        if (des.IndexOf("本次取消") >= 0)
                        {
                            deleteCom = true;
                            continue;
                        }
                        else
                        {
                            c = ap.ComComms[name];
                            if (c == null)
                            {
                                log.WriteErrorLog("ProcessMods未能找到组件 " + name);
                                ap.scmstatus = ScmStatus.Error;
                                return false;
                            }
                            c.cstatus = ComStatus.Modify;  // 新增和修改都作为修改处理
                            havechange = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.WriteErrorLog("ProcessMods异常 ReadMe:" + ap.Readme + " " + ex.Message);
                return false;
            }

            // 检查是否有变更，对于Bug集成，如果没有任何修改，无法集成
            if (ap.scmtype == ScmType.BugScm && havechange == false && deleteCom == false)
            {
                log.WriteErrorLog("木有修改，也木有删除，还是个 V" + ap.ScmVer.ToString() + "你让人家咋集成 ?");
                ap.scmstatus = ScmStatus.Error;
                return false;
            }

            return true;
        }

        // 处理 SAWPath，设置检出代码的路径
        private bool ProcessSAWPath(AmendPack ap)
        {
            // 确认修改单的配置库
            ap.svn = MAConf.instance.Svns.GetByAmend(ap.AmendSubject);
            if (ap.svn == null)
            {
                log.WriteInfoLog("获取对应配置库失败，在配置文件节点无法找到该递交主题的配置库路径");
                ap.scmstatus = ScmStatus.Error;
                return false;
            }
 
            // 处理 SAWFile
            foreach (CommitCom c in ap.ComComms)
            {
                // 对于无变动和要删除的，不需要再生成SAW库信息；对于小包，不需要生成 SAW库信息
                if (c.cstatus == ComStatus.NoChange || c.cstatus == ComStatus.Delete || c.ctype == ComType.Ssql)
                    continue;

                // 标记文件需要刷新，添加到文件状态列表中
                SAWFile f = ap.SAWFiles[c.path];
                if (f == null)
                {
                    f = new SAWFile(c.path, FileStatus.Old);
                    f.Version = c.cver;
                    f.fstatus = FileStatus.Old;
                    ap.SAWFiles.Add(f);
                }
                c.sawfile = f;
                
                #region 根据组件类别，统一路径处理
                if (c.ctype == ComType.Ssql)
                    continue;

                if (c.cname == "HsSettle.exe" || c.cname == "HsBkSettle.exe")
                {
                    string temp = @"HsSettle\trunk\Sources\ClientCom\" + Path.GetFileNameWithoutExtension(c.cname) + "\\";
                    f.LocalPath = ap.svn.Workspace + "\\" + temp;
                    f.UriPath = ap.svn.Server + "/" + temp.Replace('\\', '/');
                }
                else if (c.ctype == ComType.SO || c.ctype == ComType.Sql || c.ctype == ComType.Xml || c.ctype == ComType.FuncXml) // 
                {
                    string temp = @"HSTRADES11\trunk\Documents\D2.Designs\详细设计\后端\";
                    f.LocalPath = ap.svn.Workspace + "\\" + temp + c.path;
                    f.UriPath = ap.svn.Server + "/"+ temp.Replace('\\', '/') + c.path.Replace('\\', '/');
                }
                else if (c.ctype == ComType.Patch || c.ctype == ComType.Ini || c.ctype == ComType.MenuPatch)
                {
                    if (c.path[0] != '\\')
                        c.path = "\\" + c.path;
                    if (c.path[c.path.Length - 1] != '\\')
                        c.path = c.path + "\\";
                    
                    // 对于PATCH，INI 文件，制定路径名称为主键，否则出现两个递交在 \\证券下的PATCH，就认为是同一个了
                    c.path = c.path + c.cname;
                    f.Path = c.path;
                    
                    f.LocalPath = ap.svn.Workspace + @"\HSTRADES11\trunk" + c.path;
                    f.UriPath = ap.svn.Server + @"/HSTRADES11/trunk" + c.path.Replace('\\', '/');

                }
                else
                {
                    // 如果第一个不是路径分隔符号，那么补路径分隔符号
                    if (c.path[0] != '\\')
                        c.path = "\\" + c.path;
                    if (c.path[c.path.Length - 1] != '\\')
                        c.path = c.path + "\\";
                    f.LocalPath = ap.svn.Workspace + @"\HSTRADES11\trunk" + c.path;
                    f.UriPath = ap.svn.Server + @"/HSTRADES11/trunk" + c.path.Replace('\\', '/');
                }
                #endregion
            }
            return true;
        }

        private void ProcessReadMe(AmendPack ap)
        {
            // 输出下处理结果
            log.WriteInfoLog("[递交组件]");
            foreach (CommitCom c in ap.ComComms)
            {
                log.WriteInfoLog("名称：" + c.cname + " "
                    + "状态：" + Enum.GetName(typeof(ComStatus), c.cstatus) + " "
                    + "版本：" + c.cver + " "
                    + "路径：" + c.path);
            }

            log.WriteInfoLog("[配置库文件]");
            foreach (SAWFile s in ap.SAWFiles)
            {
                log.WriteInfoLog("路径：" + s.Path + " "
                    + "本地路径：" + s.LocalPath + " "
                    + "SvnUri：" + s.UriPath + " "
                    + "文件状态：" + Enum.GetName(typeof(FileStatus), s.fstatus));
            }
        }

        private bool AddToComs(AmendPack ap, CommitCom c)
        {
            
            // 标定该组件在 Order中的顺序
            //int t = MAConf.instance.Order.IndexOf(c.cname);
            //int t1, t2;
             int t = -1;
            if (t == -1)
            {
                ap.ComComms.Add(c);
                return true;
            }

            /*
            // 检索已经存在的组件库中，第一个当前组件的索引靠后的，处理掉
            foreach (CommitCom m in ap.ComComms)
            {
                if (m.ctype != ComType.SO)
                    continue;

                // 标定当前组件的索引值
                t2 = ap.ComComms.IndexOf(m);

                // 标定当前组件的编译顺序
                t1 = MAConf.instance.Order.IndexOf(m.cname);

                // 如果没有指定编译该操作的顺序
                if (t1 == -1 || t1 > t)
                {
                    ap.ComComms.Insert(t2, c);
                    break;
                }
                else
                {
                    ap.ComComms.Add(c);
                    break;
                }
            }
             * */

            return true;
        }

        /// <summary>
        /// 递归删除子文件夹以及文件(包括只读文件)
        /// </summary>
        /// <param name="TARGET_PATH">文件路径</param>
        public void DeleteFolder(string TARGET_PATH)
        {
            //如果存在目录文件，就将其目录文件删除
            if (Directory.Exists(TARGET_PATH))
            {
                foreach (string filenamestr in Directory.GetFileSystemEntries(TARGET_PATH))
                {
                    if (File.Exists(filenamestr))
                    {
                        FileInfo file = new FileInfo(filenamestr);
                        if (file.Attributes.ToString().IndexOf("ReadOnly") != -1)
                        {
                            file.Attributes = FileAttributes.Normal;//去掉文件属性
                        }
                        File.Delete(filenamestr);//直接删除其中的文件
                    }
                    else {
                        DeleteFolder(filenamestr);//递归删除
                    }

                }
                System.IO.DirectoryInfo DirInfo = new DirectoryInfo(TARGET_PATH);
                DirInfo.Attributes = FileAttributes.Normal & FileAttributes.Directory;    //去掉文件夹属性     
                Directory.Delete(TARGET_PATH, true);
            }
        }

        public override bool Tip
        {
            get { return _tip; }
        }

        public bool _tip = false;
    }

    class PackerProcess : State
    {
        public override string StateName
        {
            get { return "处理压缩包"; }
        }
        
        public override bool DoWork(AmendPack ap)
        {
            log.WriteInfoLog("集成类型:" + Enum.GetName(typeof(ScmType), ap.scmtype));

            // 根据集成类型执行集成操作
            if (ap.scmtype == ScmType.NewScm)
            {
                // 所有递交组件标记为新增
                foreach (CommitCom c in ap.ComComms)
                {
                    c.cstatus = ComStatus.Add;
                }
            }
            else if (ap.scmtype == ScmType.BugScm) // bug集成处理
            {
                foreach (CommitCom c in ap.ComComms)
                {
                    // 对于没有变动的组件，复制老的集成到新的集成包里
                    if (c.cstatus == ComStatus.NoChange)  // 无变动的可以复制了
                    {
                        log.WriteInfoLog("复制文件，源地址：" + Path.Combine(ap.SCMLastAmendDir, c.cname) + "，"
                            + "目标文件：" + Path.Combine(ap.SCMAmendDir, c.cname));

                        File.Copy(Path.Combine(ap.SCMLastAmendDir, c.cname), 
                            Path.Combine(ap.SCMAmendDir, c.cname), 
                            true);

                        // 对于 SO，需要把压缩文件从上次递交中解压缩，覆盖掉本次包里的文件
                        if (c.ctype == ComType.SO)
                        {
                            Detail d= MAConf.instance.Dls.FindBySo(c.cname);
                            if(d == null)
                            {
                                log.WriteErrorLog("无法定位" + c.cname + "的详细设计组件！");
                                return false;
                            }

                            string st = d.GetProcStr(true);
                            log.WriteInfoLog("解压缩源文件，源文件夹：" + ap.SCMLastLocalSrcRar + "， "
                                + "文件：" + st + "， "
                                + "目标文件夹：" + ap.SCMAmendDir);
                            UnRar(ap.SCMLastLocalSrcRar, ap.SCMAmendDir, st);
                        }
                    }
                    // 如果是删除的，那么删除文件（对于so，需要删除src文件），
                    // 由于是以递交包为基础的，所以对于删除的，应该不会再存在在文件夹里
                    else if (c.cstatus == ComStatus.Delete) 
                    {
                        log.WriteInfoLog("删除文件：" + Path.Combine(ap.SCMAmendDir, c.cname));
                        File.Delete(Path.Combine(ap.SCMAmendDir, c.cname));
                        // SO删除了，源文件也删除掉
                        if (c.ctype == ComType.SO)
                        {
                            Detail d = MAConf.instance.Dls.FindBySo(c.cname);
                            if(d == null)
                            {
                                log.WriteErrorLog("无法定位" + c.cname + "的详细设计组件！");
                                return false;
                            }

                            foreach (string s in d.ProcFiles)
                            {
                                log.WriteInfoLog("删除文件：" + Path.Combine(ap.SCMAmendDir, s));
                                File.Delete(Path.Combine(ap.SCMAmendDir, s));
                            }
                        }
                        
                        // 对于标记为删除的，不需要在递交组件列表中维护了
                        ap.ComComms.Remove(c);
                    }                        
                }
            }
            return true;
        }

        public override bool Tip
        {
            get { return _tip; }
        }

        public bool _tip = false;
    }

    class PackerCheck : State
    {
        public override string StateName
        {
            get { return "递交检查"; }
        }

        public override bool DoWork(AmendPack ap)
        {
            // 如果递交包里没有任何变动记录，而且不是第一次集成，修改单退回

            return true;
        }

        public bool _tip = true;
    }

    /// <summary>
    /// 检出Svn代码
    /// </summary>
    class PackerSvnCode : State
    {
        public override string StateName
        {
            get { return "检出Svn代码"; }
        }

        public override bool DoWork(AmendPack ap)
        {
            bool Result = true;
            // 这个地方由于没有历史性刷出的办法，对于DLL，可能需要两遍代码刷出，第一遍，先刷出最新版，第二遍，根据递交的版本历史，
            // 把在这个版本之后修改的刷回去
            // 需要注意的是ReadMe中的路径是不全的，这个很恶心
            SvnVersion svn = new SvnVersion("0", "1");
            foreach (SAWFile s in ap.SAWFiles)
            {
                if (s.fstatus == FileStatus.New)
                {
                    continue;
                }

                svn.Path = s.LocalPath;
                svn.Uri = s.UriPath;
                svn.AmendNo = ap.AmendNo;
                svn.AmendList = ap.AmendList;
                svn.Version = s.Version;
                // 检出失败，退出循环
                Result = svn.GetAmendCode();
                if (false == Result)
                {
                    break;
                }
                s.fstatus = FileStatus.New;
                s.LastModTime = svn.uriinfo.LastChangeTime;
            }

            return Result;
        }

        public override bool Tip
        {
            get { return _tip; }
        }

        public bool _tip = false;
    }

    /// <summary>
    /// 本地编译，dpr生成输出物，so生成pc，送集成服务器编译
    /// </summary>
    class PackerCompile : State
    {
        public override string StateName
        {
            get { return "编译输出物"; }
        }

        public override bool DoWork(AmendPack ap)
        {
            ap.laststatus = ap.scmstatus;
            ap.scmstatus = ScmStatus.Compile;
            bool Result = true;
            foreach (CommitCom c in ap.ComComms)
            {
                // 小包直接跳过
                if (c.ctype == ComType.Ssql)
                {
                    c.cstatus = ComStatus.Normal;
                    continue;
                }

                if (c.cstatus == ComStatus.NoChange)
                    continue;

                // 重复编译时，对于上次已经编译过的，不需要再进行编译了
                if (c.cstatus == ComStatus.Normal)
                    continue;

                log.WriteLog("处理：" + c.cname);

                // Patch 和 Ini 刷下来，拷贝过去就可以
                if (c.ctype == ComType.Patch || c.ctype == ComType.Ini || c.ctype == ComType.Xml 
                    || c.ctype == ComType.MenuPatch)
                {
                    // 去除文件只读属性，否则复制时不能覆盖
                    FileAttributes attributes = File.GetAttributes(Path.Combine(ap.SCMAmendDir, c.cname));
                    FileAttributes attr1 = attributes;
                    if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        // Show the file.
                        attributes = attributes & ~FileAttributes.ReadOnly;
                        File.SetAttributes(Path.Combine(ap.SCMAmendDir, c.cname), attributes);
                    }

                    File.Copy(c.sawfile.LocalPath, Path.Combine(ap.SCMAmendDir, c.cname), true);

                    // 还原只读属性
                    attributes = attr1;
                    if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        File.SetAttributes(Path.Combine(ap.SCMAmendDir, c.cname), attributes);
                    }
                }
                else if (c.ctype == ComType.Dll || c.ctype == ComType.Exe)
                {
                    // 确认Delphi版本
                    int dVer = GetDelphiVer(c.cname);
                    // 确定工程名称
                    string dPro = Path.Combine(c.sawfile.LocalPath, Path.GetFileNameWithoutExtension(c.cname) + ".dpr");
                    Result = CompileDpr(dVer, dPro, ap.SCMAmendDir);
                    if (Result == false)
                        break;
                    
                    // 对于delphi 5 编译输出，可能会生成MAP文件，删除掉生成的MAP文件
                    if (File.Exists(Path.Combine(ap.SCMAmendDir, Path.GetFileNameWithoutExtension(c.cname) + ".map")))
                    {
                        File.Delete(Path.Combine(ap.SCMAmendDir, Path.GetFileNameWithoutExtension(c.cname) + ".map"));
                    }
                }
                else if (c.ctype == ComType.SO || c.ctype == ComType.Sql)
                {
                    // 这里不能并发执行，有可能锁住Excel，可能只能等待执行
                    Result = CompileExcel(c.ctype, c.cname, ap.DiffDir);
                    if (Result == false)
                        break;
                }
                else if (c.ctype == ComType.FuncXml)
                {
                    // 这里不能并发执行，有可能锁住Excel，可能只能等待执行
                    Result = CompileExcel(c.ctype, c.cname, ap.SCMAmendDir);
                    // 删除掉生成的Files.txt文件
                    if (File.Exists(Path.Combine(ap.SCMAmendDir, "Files.txt")))
                    {
                        File.Delete(Path.Combine(ap.SCMAmendDir, "Files.txt"));
                    }

                    if (Result == false)
                        break;
                }

                c.cstatus = ComStatus.Normal;
            }

            return Result; 
        }

        // 编译Dll或者Exe
        private bool CompileDpr(int dVer, string dPro, string outdir)
        {
            bool Result = true;

            Process p = new Process();
            p.StartInfo.FileName = "cm.bat";
            p.StartInfo.Arguments = " " + dVer.ToString() + " " + dPro + " " + outdir;
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
            if (strOutput.IndexOf("Complile Failed") >= 0)
            {
                Result = false;
                log.WriteErrorLog(strArr[strArr.Length - 3].Replace('\n', ' '));  // 输出最后一行报错信息
                log.WriteErrorLog(strArr[strArr.Length - 2].Replace('\n', ' '));
                log.WriteFileLog("编译输出：");
                log.WriteFileLog(strOutput);
            }
            else
            {
                log.WriteFileLog(strArr[strArr.Length - 4].Replace('\n', ' '));
                log.WriteFileLog(strArr[strArr.Length - 3].Replace('\n', ' '));
                log.WriteFileLog(strArr[strArr.Length - 2].Replace('\n', ' '));
            }
          
            log.WriteFileLog("[编译结束]");
            p.WaitForExit();
            p.Close();

            return Result;
        }

        // 要重写
        private int GetDelphiVer(string Name)
        {
            // 检查是否在配置中
            int ver;
            if (MAConf.instance.DelCom.TryGetValue(Name, out ver))
            {
            }
            else  // 否则，默认当做 D6
            {
                ver = 6;
            }
            return ver;
        }

        // 编译Excel文件或者后台Sql
        private bool CompileExcel(ComType ctype, string cname, string dir)
        {
            // 确定详细设计说明书文件
            Detail d;
            MacroType m;
            if (ctype == ComType.SO)
            {
                m = MacroType.ProC;
                d = MAConf.instance.Dls.FindBySo(cname);
            }
            else if (ctype == ComType.Sql)
            {
                m = MacroType.SQL;
                d = MAConf.instance.Dls.FindBySql(cname);
            }
            else if (ctype == ComType.FuncXml)
            {
                m = MacroType.FuncXml;
                d = MAConf.instance.Dls.FindByXml(cname);
            }
            else
            {
                return true;
            }

            if (d == null)
            {
                log.WriteErrorLog("查找不到对应的详细设计说明书模块！");
                return false;
            }

            // 标定index
            bool Result = true;
            int index = MAConf.instance.Dls.IndexOf(d) + 1;

            // 先把存在的CError删除，以检测是否发生编译错误
            if (File.Exists(Path.Combine(dir, "CError.txt")))
            {
                File.Delete(Path.Combine(dir, "CError.txt"));
            }

            // 编译Excel 最耗时，对Excel检查是否需要编译，比较PC文件
            bool bNew = false;
            string tpath = Path.Combine(
                Path.GetDirectoryName(MAConf.instance.DetailFile), "后端",
                d.File);
            DateTime t2 = File.GetLastWriteTime(tpath);
            DateTime t1 = t2.AddSeconds(-1);
            if (ctype == ComType.SO)
            {
                foreach (string s in d.ProcFiles)
                {
                    t1 = File.GetLastWriteTime(Path.Combine(dir, s));
                    if (DateTime.Compare(t1, t2) > 0)
                    {
                        bNew = true;
                        break;
                    }
                }
            }
            else if (ctype == ComType.Sql)
            {
                t1 = File.GetLastWriteTime(Path.Combine(dir, d.SqlFile));
                if (DateTime.Compare(t1, t2) > 0)
                {
                    bNew = true;
                }
            }

            if (bNew)
            {
                log.WriteLog("本地源代码时间晚于Excel文件时间，不需集成处理！"+ cname);
                return true;
            }

            Result = ExcelMacroHelper.instance.ScmRunExcelMacro(m, index, dir);

            if (!Result)
            {
                return false;
            }
            else if (File.Exists(Path.Combine(dir, "CError.txt")))
            {
                Result = false;
                log.WriteErrorLog("检测到编译错误文件，请确认！");
            }

            return Result;
        }

        public override bool Tip
        {
            get { return _tip; }
        }

        public bool _tip = true;
    }

    /// <summary>
    /// 主要是进行源代码对比工作，包括 so 源文件和 sql 源文件的检查，应该是调用外部工具如 Beyond Compare 之类
    /// </summary>
    class PackerDiffer : State
    {
        public override string StateName
        {
            get { return "文件对比"; }
        }

        public override bool DoWork(AmendPack ap)
        {
            // 对于相同的，可以不提示用户，对于不同的，需要集成的同学确认，这个还需要想办法来写
            bool Result = true;
            foreach (CommitCom c in ap.ComComms)
            {
                // 小包直接跳过
                if (c.ctype == ComType.Ssql)
                {
                    c.cstatus = ComStatus.Normal;
                    continue;
                }

                if (c.cstatus == ComStatus.NoChange)
                    continue;

                if (c.ctype == ComType.SO)
                {
                    // 按照Hash对比，希望是可行的，
                    Detail d = MAConf.instance.Dls.FindBySo(c.cname);
                    if (d == null)
                    {
                        log.WriteErrorLog("无法确认组件详细设计说明书信息，" + c.cname);
                        return false;
                    }

                    foreach(string s in d.ProcFiles)
                    {
                        string file1 = Path.Combine(ap.SCMAmendDir, s);
                        string file2 =  Path.Combine(ap.DiffDir, s);
                        
                        if (!File.Exists(file1) || !File.Exists(file2))
                        {
                            log.WriteErrorLog("对比源文件不存在，请检查是否进行了编译输出。");
                        }

                        if (!HashCom(file1, file2))
                        //if (!CompareSrc(file1, file2))
                        {
                            log.WriteErrorLog("文件对比不同，请手工处理一致后继续。" + s);
                            if (Result)
                            {
                                Result = false;
                            }

                            continue;
                        }
                    }
                }
                else if(c.ctype == ComType.Sql)
                {
                    // 按照Hash对比，希望是可行的，
                    Detail d = MAConf.instance.Dls.FindBySql(c.cname);
                    if (d == null)
                    {
                        log.WriteErrorLog("无法确认组件详细设计说明书信息，" + c.cname);
                        return false;
                    }

                    string file1 = Path.Combine(ap.SCMAmendDir, c.cname);
                    string file2 = Path.Combine(ap.DiffDir, c.cname);

                    if (!File.Exists(file1) || !File.Exists(file2))
                    {
                        log.WriteErrorLog("对比源文件不存在，请检查是否进行了编译输出。");
                    }

                    if (!HashCom(file1, file2))
                    //if(!CompareSrc(file1, file2))
                    {
                        log.WriteErrorLog("文件对比不同，请手工处理一致后继续。" + c.cname);
                        if (Result)
                        {
                            Result = false;
                        }
                        continue;
                    }
                }

                c.cstatus = ComStatus.Normal;
            }
            log.WriteInfoLog("源代码对比完成。");
            return Result;
        }

        private bool HashCom(string file1, string file2)
        {
            string hash1 = getMd5Hash(file1);
            return verifyMd5Hash(file2, hash1);
        }

        // Hash an input string and return the hash as
        // a 32 character hexadecimal string.
        private string getMd5Hash(string file)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            MD5 md5Hasher = MD5.Create();

            FileStream fileStream = File.Open(file, FileMode.Open);
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hasher.ComputeHash(fileStream);

            fileStream.Close();
            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        // Verify a hash against a string.
        private bool verifyMd5Hash(string file, string hash)
        {
            string hashOfInput = getMd5Hash(file);

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, hash))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool CompareSrc(string filescm, string filesub)
        { 
            // 使用 diff 来作对比
            string strOutput = string.Empty;
            Process p = new Process();
            try
            {
                p.StartInfo.FileName = MAConf.instance.Diff;           // diff 程序，现在先使用 diff

                p.StartInfo.UseShellExecute = false;        // 关闭Shell的使用  
                p.StartInfo.RedirectStandardInput = true; // 重定向标准输入
                p.StartInfo.RedirectStandardOutput = true;  //重定向标准出  
                p.StartInfo.RedirectStandardError = true; //重定向错误输出  
                p.StartInfo.CreateNoWindow = true;             // 不显示窗口

                // 打包Pack
                p.StartInfo.Arguments = MAConf.instance.DiffArg + " " + filescm + " " + filesub;   // 设置执行参数
                p.Start();    // 启动
                strOutput = p.StandardOutput.ReadToEnd();        // 从输出流取得命令执行结果
                p.WaitForExit();

                p.Close();
            }
            catch (Exception ex)
            {
                log.WriteErrorLog("执行diff失败" + ex.Message);
            }

            // 处理执行结果
            if (strOutput.Trim().Equals(string.Empty, StringComparison.Ordinal))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool Tip
        {
            get { return _tip; }
        }

        public bool _tip = false;
    }

    /// <summary>
    /// 对比完成后，生成SO
    /// </summary>
    class PackerSO : State
    {
        public override string StateName
        {
            get { return "转移源文件，生成中间件"; }
        }

        public override bool DoWork(AmendPack ap)
        {
            bool Result = true;
            foreach (CommitCom c in ap.ComComms)
            {
                // 小包直接跳过
                if (c.ctype == ComType.Ssql)
                {
                    c.cstatus = ComStatus.Normal;
                    continue;
                }

                if (c.cstatus == ComStatus.NoChange)
                    continue;

                // 编译完成后，需要上传到 ssh 服务器上得到 SO，sql不需要处理
                if (c.ctype == ComType.SO)
                {
                    ReSSH s = MAConf.instance.ReConns["scm"];  // 一定要配置这个
                    if (s == null)
                    {
                        log.WriteErrorLog("集成ssh配置不存在！");
                        return false;
                    }
                    s.localdir = ap.SCMAmendDir + "\\";

                    // 把OutDir下的文件移动过来
                    Detail d = MAConf.instance.Dls.FindBySo(c.cname);
                    if (d == null)
                    {
                        log.WriteErrorLog("无法定位组件" + c.cname + "详细设计说明书位置，编译失败");
                        return false;
                    }

                    foreach (string f in d.ProcFiles)
                    {
                        File.Copy(Path.Combine(MAConf.instance.OutDir, f),
                            Path.Combine(ap.SCMAmendDir, f),
                            true);
                    }

                    // 上传、编译、下载
                    Result = s.UploadModule(d) && s.Compile(d) && s.DownloadModule(d);
                }
                else if (c.ctype == ComType.Sql)
                {
                    // 送到 Oracle 上执行，暂时没有
                    //Detail d = MAConf.instance.Dls.FindBySql(c.cname);
                    
                    File.Copy(Path.Combine(MAConf.instance.OutDir, c.cname),
                        Path.Combine(ap.SCMAmendDir, c.cname),
                        true);

                    Result = true;
                }

                c.cstatus = ComStatus.Normal;
            }

            return Result;
        }

        public override bool Tip
        {
            get { return _tip; }
        }

        public bool _tip = false;
    }

    /// <summary>
    /// 重新集成打包
    /// </summary>
    class PackerRePack : State
    {
        public override string StateName
        {
            get { return "重新打包处理"; }
        }

        public override bool DoWork(AmendPack ap)
        {
            // 此时，文件夹的组件已完成，对文件夹压包处理
            // 需要把递交组件和src分别压包，这个与开发递交的组件不同
            // 检查是否存在源代码
            bool SrcFlag = false, result = true;
            foreach (CommitCom c in ap.ComComms)
            {
                if (c.ctype == ComType.SO)  // 交了SO，一定要存在源代码 
                {
                    SrcFlag = true;
                    break;
                }
            }

            // 暂无实现需要
            // 开启进程执行 rar解压缩
            // 获取 Winrar 的路径（通过注册表或者是配置，这里直接根据xml来处理）
            // 实例化 Process 类，启动执行进程
            Process p = new Process();
            try
            {
                p.StartInfo.FileName = MAConf.instance.rar;           // rar程序名

                p.StartInfo.UseShellExecute = false;        // 关闭Shell的使用  
                p.StartInfo.RedirectStandardInput = false; // 重定向标准输入
                p.StartInfo.RedirectStandardOutput = false;  //重定向标准出  
                p.StartInfo.RedirectStandardError = false; //重定向错误输出  
                p.StartInfo.CreateNoWindow = true;             // 不显示窗口

                if (SrcFlag == true)
                {
                    // 打包 Src
                    p.StartInfo.Arguments = " m -ep " + ap.SCMSrcRar + " "
                        + ap.SCMAmendDir + "\\" + "*.h " + ap.SCMAmendDir + "\\" + "*.pc "
                        + ap.SCMAmendDir + "\\" + "*.cpp " + ap.SCMAmendDir + "\\" + "*.gcc ";   // 设置执行参数  

                    log.WriteFileLog(p.StartInfo.FileName + " " + p.StartInfo.Arguments);

                    p.Start();    // 启动
                    p.WaitForExit();

                    if (p.HasExited && p.ExitCode != 0)
                    {
                        result = false;
                        log.WriteErrorLog("压缩失败，退出码：" + p.ExitCode.ToString());
                    }
                    p.Close();
                }

                if (result == false)
                {
                    return result;
                }

                // 打包Pack，替换 压缩.bat 的 a -ep 为 m -ep
                p.StartInfo.Arguments = " a -ep " + ap.SCMLocalFile + " "
                    + ap.SCMAmendDir + " " + "-x" +ap.SCMSrcRar;   // 设置执行参数
                
                log.WriteFileLog(p.StartInfo.FileName + " " + p.StartInfo.Arguments);
                
                p.Start();    // 启动
                p.WaitForExit();

                if (p.HasExited && p.ExitCode != 0)
                {
                    result = false;
                    log.WriteErrorLog("压缩失败，退出码：" + p.ExitCode.ToString());
                }

                p.Close();
            }
            catch (Exception ex)
            {
                log.WriteErrorLog("执行rar失败 " + ex.Message);
            }

            return result;
        }

        public override bool Tip
        {
            get { return _tip; }
        }

        public bool _tip = false;
    }

    /// <summary>
    /// 上传到ftp目录，集成完成
    /// </summary>
    class PackerUpload : State
    {
        public override string StateName
        {
            get { return "压缩包上传"; }
        }

        public override bool DoWork(AmendPack ap)
        {
            // 对于重新集成，先删除掉上一次集成的软件包，然后按照新集成处理
            if (ap.scmtype == ScmType.ReScm)
            {
                // 删除ftp软件包，删除本地软件包
                FTPConnection ftp = MAConf.instance.ftp;
                if (ftp.IsConnected == false)
                {
                    ftp.Connect();
                }

                log.WriteInfoLog("重新集成，删除服务器集成包" + ap.SCMRemoteFile);
                //ftp.DeleteFile(SCMRemoteFile);

                // 重新标定，暂时不考虑重复集成
            }

            return true;
        }

        public override bool Tip
        {
            get { return _tip; }
        }

        public bool _tip = true;
    }

    /// <summary>
    /// 本地的一些清理工作，暂时没有用
    /// </summary>
    class PackCleanUp : State
    {
        public override string StateName
        {
            get { return "输出清理"; }
        }

        public override bool DoWork(AmendPack ap)
        {
            // 删除本地下载的递交压缩包
            log.WriteInfoLog("删除下载的递交包");
            if (File.Exists(ap.LocalFile))
            {
                File.Delete(ap.LocalFile);
            }

            // 删除 src 下的源文件和 SO
            //log.WriteInfoLog("删除临时源文件和SO");

            return true;
        }

        public override bool Tip
        {
            get { return _tip; }
        }

        public bool _tip = false;
    }
}
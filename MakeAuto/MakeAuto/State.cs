using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnterpriseDT.Net.Ftp;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

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
        protected void UnRar(string rarfile, string dir, string file = "")
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

                string strOutput = p.StandardOutput.ReadToEnd();        //從输出流取得命令执行结果
                MAConf.instance.WriteLog(strOutput, LogLevel.FileLog);

                p.WaitForExit();
                p.Close();
            }
            catch (Exception ex)
            {
                MAConf.instance.WriteLog("执行rar失败" + ex.Message, LogLevel.Error);
            }
        }

        // 日志组件
        protected OperLog log = OperLog.instance;

        // 修改单记录
        protected AmendPack _amend {get; set;}

        //
        public virtual string StateName
        { get { return "test";  } }
    }

    /// <summary>
    /// 下载压缩包
    /// </summary>
    class PackerDownload : State
    {
        public override string StateName
        {
            get { return "下载压缩包"; }
        }

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
            ftp.DownloadFile(ap.LocalFile, ap.RemoteFile);

            // 集成包不需要下载，使用本地文件夹处理
            /*
            string scmfile, scmremote, scmdir;
            foreach (string s in ap.ScmL)
            {
                scmfile = ap.LocalDir + "\\" + s;
                scmremote = ap.RemoteDir + "\\" + s;
                if (!File.Exists(scmfile))
                {
                    ftp.DownloadFile(scmfile, scmremote);
                }

                // 如果没有对应的文件夹，那么需要解压缩文件夹
                scmdir = ap.LocalDir + "\\" + Path.GetFileNameWithoutExtension(scmfile);
                if (!Directory.Exists(scmdir))
                {
                    UnRar(scmfile, scmdir);
                }
            }
             * */
            return true;
        }
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
            // 检查集成本地文件夹是否存在，不存在则创建
            if (!Directory.Exists(ap.SCMAmendDir))
            {
                Directory.CreateDirectory(ap.SCMAmendDir);
            }

            // 对于下载的递交文件，解压缩readme到集成文件夹，以便根据本次变动取出需要重新集成的文件
            UnRar(ap.LocalFile, ap.SCMAmendDir, ap.Readme);

            // 处理 ReadMe，以获取变动
            // 读取readme分成两步，先重新生成递交组件，然后检测修改
            ProcessComs(ap);
            ProcessMods(ap);
            ProcessSAWPath(ap);

            // 显示处理结果，打开ReadMe文件
            ProcessReadMe(ap);

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
                /*
                if (c.sawfile == null)
                {
                    log.WriteInfoLog("本地路径：test__");
                    log.WriteInfoLog("SAW路径：test__");
                }
                else
                {
                    log.WriteInfoLog("本地路径：" + c.sawfile.LocalPath);
                    log.WriteInfoLog("SAW路径：" + c.sawfile.SAWPath);
                }
                 * */
            }
            log.WriteInfoLog("[配置库文件]");
            foreach (SAWFile s in ap.SAWFiles)
            {
                log.WriteInfoLog("路径：" + s.Path + " "
                    + "本地路径：" + s.LocalPath + " "
                    + "SAW路径：" + s.SAWPath + " "
                    + "文件状态：" + Enum.GetName(typeof(FileStatus), s.fstatus));
            }
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
                            c.path = line.Substring(1, line.Length - 2); // 去除头部和尾部的 []
                        }
                        else
                        {
                            c.path = "";
                        }
                        ap.ComComms.Add(c);
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
                        c = ap.ComComms[name];
                        if (c == null)
                        {
                            log.WriteErrorLog("ProcessMods未能找到组件");
                            ap.scmstatus = ScmStatus.Error;
                            return false;
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
                log.WriteErrorLog("ProcessMods异常 ReadMe:" + ap.Readme + " " + ex.Message);
                return false;
            }

            return true;
        }

        // 处理 SAWPath，设置检出代码的路径
        private bool ProcessSAWPath(AmendPack ap)
        {
            #region 第一步，生成基本的SAWFile
            // 处理 SAWFile
            foreach (CommitCom c in ap.ComComms)
            {
                // 对于无变动和要删除的，不需要再生成SAW库信息；对于小包，不需要生成 SAW库信息
                if (c.cstatus == ComStatus.NoChange || c.cstatus == ComStatus.Delete || c.ctype == ComType.Ssql)
                    continue;

                // 标记文件需要刷新，添加到文件状态列表中
                if (ap.SAWFiles[c.path] == null)
                {
                    SAWFile f = new SAWFile(c.path, FileStatus.Old);
                    f.Version = c.cver;
                    ap.SAWFiles.Add(f);
                    c.sawfile = f;
                }
                else
                {
                    // 如果第一个文件不是更新，那就改成需要更新，但是不会有这种情况吧，所以直接改掉好了。
                    SAWFile f = ap.SAWFiles[c.path];
                    f.fstatus = FileStatus.Old;
                    f.Version = c.cver;
                    c.sawfile = f;
                }
            }
            #endregion

            #region 第二步，生出每个文件的配置库路径
            // 确认修改单的配置库
            ap.sv = MAConf.instance.SAWs.GetByAmend(ap.AmendSubject);
            if (ap.sv == null)
            {
                log.WriteInfoLog("获取对应配置库失败，在配置文件节点无法找到该递交主题的配置库路径");
                ap.scmstatus = ScmStatus.Error;
                return false;
            }

            // 处理SAW代码的路径，暂时只对06版有效，因为目录是固定的，需要写死，这段代码可能需要很多变动
            foreach (SAWFile s in ap.SAWFiles)
            {
                if (s.Path.IndexOf("小包-") >= 0)
                    continue;

                // 根据 s 的名称来处理，这是一段很纠结的代码
                if (s.Path.IndexOf("金融产品销售系统_详细设计说明书") >= 0) // 
                {
                    string temp = @"HSTRADES11\Documents\D2.Designs\详细设计\后端\";
                    s.LocalPath = ap.sv.Workspace + "\\" + temp + s.Path;
                    s.SAWPath = @"$/" + temp.Replace('\\', '/') + s.Path;

                }
                else
                {
                    // 如果第一个不是路径分隔符号，那么补路径分隔符号
                    if (s.Path[0] != '\\')
                        s.Path = "\\" + s.Path;
                    s.LocalPath = ap.sv.Workspace + "\\" + @"HSTRADES11" + s.Path;
                    s.SAWPath = @"$/" + @"HSTRADES11" + s.Path.Replace('\\', '/');
                }
            }
            #endregion

            return true;
        }
    }

    class PackerProcess : State
    {
        public override string StateName
        {
            get { return "处理压缩包"; }
        }
        
        public override bool DoWork(AmendPack ap)
        {
            // 如果本地集成文件已存在，删除本地集成文件
            if (File.Exists(ap.SCMLocalFile))
            {
                File.Delete(ap.SCMLocalFile);
            }

            // 下载压缩包之后，解压缩包，得到 集成-[修改单号]-[模块]-[修改人]-[日期]-V* 的一个文件夹
            UnRar(ap.LocalFile, ap.SCMAmendDir);

            // 如果存在 src 压缩文件夹，解压缩 src 文件夹
            if (File.Exists(ap.SrcRar))
            {
                UnRar(ap.SrcRar, ap.SCMAmendDir);  // 解压处理，供对比用
                File.Delete(ap.SrcRar);  // 删除掉，不需要留着了
            }

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
                        File.Copy(Path.Combine(ap.SCMLastAmendDir, c.cname), 
                            Path.Combine(ap.SCMAmendDir, c.cname), 
                            true);
                    }
                    else if (c.cstatus == ComStatus.Delete) // 如果是删除的，那么删除文件（对于so，需要删除src文件）
                    {
                        // SO删除了，源文件也删除掉
                        File.Delete(ap.SCMAmendDir + "\\" + c.cname);
                        if (c.ctype == ComType.SO)
                        {
                            foreach (Detail d in MAConf.instance.Dls)
                            {
                                if (d.SO == c.cname)
                                {
                                    foreach (string s in d.ProcFiles)
                                    {
                                        File.Delete(ap.SCMAmendDir + "\\" + s);
                                    }

                                    break;
                                }
                            }
                        }
                        
                        // 对于标记为删除的，不需要在递交组件列表中维护了
                        ap.ComComms.Remove(c);
                    }                        
                }
            }
            return true;
        }
    }

    class PackerCheck : State
    {
        public override string StateName
        {
            get { return "递交检查"; }
        }

        public override bool DoWork(AmendPack ap)
        {
            return true;
        }
    }

    /// <summary>
    /// 检出VSS代码
    /// </summary>
    class PackerVSSCode : State
    {
        public override string StateName
        {
            get { return "检出VSS代码"; }
        }

        public override bool DoWork(AmendPack ap)
        {
            // 这个地方由于没有历史性刷出的办法，对于DLL，可能需要两遍代码刷出，第一遍，先刷出最新版，第二遍，根据递交的版本历史，
            // 把在这个版本之后修改的刷回去
            // 需要注意的是ReadMe中的路径是不全的，这个很恶心
            foreach (SAWFile s in ap.SAWFiles)
            {
                ap.sv.GetAmendCode(ap.AmendNo, s);
                s.fstatus = FileStatus.New;
            }

            return true;
        }
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

                // Patch 和 Ini 刷下来，拷贝过去就可以
                else if (c.ctype == ComType.Patch || c.ctype == ComType.Ini)
                {
                    File.Copy(c.sawfile.LocalPath, ap.SCMAmendDir + "//" + c.cname, true);
                }
                else if (c.ctype == ComType.Dll || c.ctype == ComType.Exe)
                {
                    // 确认Delphi版本
                    int dVer = GetDelphiVer(c.cname);
                    // 确定工程名称
                    string dPro = c.sawfile.LocalPath + Path.GetFileNameWithoutExtension(c.cname) + ".dpr";
                    Result = CompileDpr(dVer, dPro, ap.SCMAmendDir);
                    if (Result == false)
                        break;
                }
                else if (c.ctype == ComType.SO || c.ctype == ComType.Sql)
                {
                    // 这里不能并发执行，有可能锁住Excel，可能只能等待执行
                    Result = CompileExcel(c.ctype, c.cname, ap.SCMAmendDir);
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
            
            if (strOutput.IndexOf("Complile Failed") >= 0)
            {
                Result = false;
                // 输出最后几行
                string[] strArr = Regex.Split(strOutput, "\r");
                log.WriteErrorLog(strArr[strArr.Length - 3]); // 输出最后一行报错信息
            }
            log.WriteFileLog("[编译日志]");
            log.WriteFileLog("[Delphi版本] " + dVer.ToString());
            log.WriteFileLog("[工程] " + dPro);
            log.WriteFileLog("[输出目录] " + outdir);
            log.WriteFileLog(strOutput);
            log.WriteFileLog("[编译日志结束]");
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
            else
            {
                return true;
            }

            if (d == null)
            {
                log.WriteErrorLog("查找不对对应的详细设计说明书模块！");
                return false;
            }

            // 标定index
            bool Result = true;
            int index = MAConf.instance.Dls.IndexOf(d) + 1;
            string LocalSrc = MAConf.instance.OutDir + "\\";

            // 在 src 文件不完整时才执行编译
            //if ((ctype == ComType.SO && (!File.Exists(LocalSrc + d.Gcc) || !File.Exists(LocalSrc + d.Cpp)
            //      || !File.Exists(LocalSrc + d.Header) || !File.Exists(LocalSrc + d.Pc))) 
            //    || (ctype == ComType.Sql && (!File.Exists(LocalSrc + d.Sql))) )
            //{
                Result = ExcelMacroHelper.instance.ScmRunExcelMacro(m, index, LocalSrc);
            //}

            if (!Result)
            {
                return false;
            }

            return Result;
        }
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

                if (c.ctype == ComType.SO || c.ctype == ComType.Sql)
                {
                    // 对比实现
                        break;
                }

                c.cstatus = ComStatus.Normal;
            }
            return true;
        }
    }

    /// <summary>
    /// 对比完成后，生成SO
    /// </summary>
    class PackerSO : State
    {
        public override string StateName
        {
            get { return "转移源文件，生成So"; }
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
            return true;
        }
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
            bool SrcFlag = false;
            foreach (CommitCom c in ap.ComComms)
            {
                if (c.ctype == ComType.SO)  // 交了SO，一定要存在源代码 
                {
                    SrcFlag = true;
                    break;
                }
            }

            string strOutput = string.Empty;
            // 暂无实现需要
            // 开启进程执行 rar解压缩
            // 获取 Winrar 的路径（通过注册表或者是配置，这里直接根据xml来处理）
            // 实例化 Process 类，启动执行进程
            Process p = new Process();
            try
            {
                p.StartInfo.FileName = MAConf.instance.rar;           // rar程序名

                p.StartInfo.UseShellExecute = false;        // 关闭Shell的使用  
                p.StartInfo.RedirectStandardInput = true; // 重定向标准输入
                p.StartInfo.RedirectStandardOutput = true;  //重定向标准出  
                p.StartInfo.RedirectStandardError = true; //重定向错误输出  
                p.StartInfo.CreateNoWindow = true;             // 不显示窗口

                if (SrcFlag == true)
                {
                    // 打包 Src
                    p.StartInfo.Arguments = " m -ep " + ap.SCMSrcRar + " "
                        + ap.SCMAmendDir + "\\" + "*.h " + ap.SCMAmendDir + "\\" + "*.pc "
                        + ap.SCMAmendDir + "\\" + "*.cpp " + ap.SCMAmendDir + "\\" + "*.gcc ";   // 设置执行参数  

                    p.Start();    // 启动

                    strOutput = p.StandardOutput.ReadToEnd();        // 从输出流取得命令执行结果
                    log.WriteFileLog(strOutput);

                    p.WaitForExit();
                }

                // 打包Pack
                p.StartInfo.Arguments = " a -ep " + ap.SCMLocalFile + " "
                    + ap.SCMAmendDir + " " + "-x" +ap.SCMSrcRar;   // 设置执行参数

                p.Start();    // 启动

                strOutput = p.StandardOutput.ReadToEnd();        // 从输出流取得命令执行结果
                log.WriteFileLog(strOutput);

                p.WaitForExit();

                p.Close();
            }
            catch (Exception ex)
            {
                log.WriteErrorLog("执行rar失败" + ex.Message);
            }

            return true;
        }
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
            if (File.Exists(ap.SCMLocalFile))
            {
                File.Delete(ap.SCMLocalFile);
            }
            return true;
        }
    }


}
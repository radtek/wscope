using System;
using System.Collections.Generic;
using System.Text;
using EnterpriseDT.Net.Ftp;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace MakeAuto
{
    /// <summary>
    /// 状态模式实现
    /// </summary>
    abstract class State
    {
        public State()
        {

        }

        // 状态模式基础状态
        public abstract bool DoWork(AmendPack ap);

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
        protected Boolean UnRar(AmendPack ap, string rarfile, string dir, string file = "")
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
                p.StartInfo.FileName = MAConf.instance.Configs[ap.ProductId].Rar;           // rar程序名  
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
        public PackerDownload()
            : base()
        {
        }

        public override bool DoWork(AmendPack ap)
        {
            // 输出递交包，到本地集成环境处理，需要使用ftp连接
            FTPConnection ftp = MAConf.instance.Configs[ap.ProductId].ftp;
            FtpConf fc = MAConf.instance.Configs[ap.ProductId].fc;

            if (ftp.IsConnected == false)
            {
                try
                {
                    ftp.Connect();
                }
                catch (Exception e)
                {
                    log.WriteErrorLog("连接FTP服务器失败，错误信息：" + e.Message);
                }
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
                // 如果本地已经存在，提示是否覆盖
                bool Res = true;
                if (File.Exists(ap.LocalFile))
                {
                    log.WriteLog("本地文件已存在！" + ap.LocalFile);
                    string message = "本地已存在 " + ap.LocalFile + " 是否覆盖？";
                    string caption = "测试";

                    System.Windows.Forms.DialogResult result = MessageBox.Show(message, caption, MessageBoxButtons.YesNo);
                    if (result == System.Windows.Forms.DialogResult.No)
                    {
                        log.WriteLog("选择保留本地文件！" + ap.LocalFile);
                        Res = false;
                    }
                }

                if (Res)
                {
                    ftp.DownloadFile(ap.LocalFile, ap.RemoteFile);
                }
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

                UnRar(ap, ap.SCMLastLocalFile, ap.SCMLastAmendDir);

                if (ap.scmtype == ScmType.BugScm && ftp.Exists(ap.ScmLastRemoteSrcRar))
                {
                    if (!File.Exists(ap.SCMLastLocalSrcRar))
                    {
                        ftp.DownloadFile(ap.SCMLastLocalSrcRar, ap.ScmLastRemoteSrcRar);
                    }

                    UnRar(ap, ap.SCMLastLocalSrcRar, ap.SCMLastAmendDir);
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
        public PackerReadMe()
            : base()
        {
        }

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
            UnRar(ap, ap.LocalFile, ap.SCMAmendDir);

            // 如果存在 src 压缩文件夹，解压缩 src 文件夹
            if (File.Exists(ap.SrcRar))
            {
                UnRar(ap, ap.SrcRar, ap.SCMAmendDir);  // 解压处理，供对比用
                File.Delete(ap.SrcRar);  // 删除掉递交的src，不需要留着了
            }

            // 20130802 
            // 如果Readme不存在，之后的的操作不用执行
            if(!File.Exists(Path.Combine(ap.SCMAmendDir, ap.Readme)))
            {
                log.WriteErrorLog("Readme文件不存在，" + Path.Combine(ap.SCMAmendDir, ap.Readme));
                return false;
            }

            // 处理 ReadMe，以获取变动
            // 读取readme分成两步，先重新生成递交组件，然后检测修改
            result = ProcessComs(ap);
            if (!result)
                return false;

            // 优先处理集成注意
            result = ProcessScmNotice(ap);
            if (!result)
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

        private bool ProcessComs(AmendPack ap)
        {
            bool Result = true;
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

                        // 跳过 src 行
                        if (line.Trim().IndexOf("src-V") >= 0)
                            continue;

                        // 连续读取两行，一行文件说明和版本，一行存放路径
                        // 读取源代码路径，小包下面没有路径，不用读取
                        index = line.IndexOf("[");
                        name = line.Substring(0, index).Trim();
                        index = line.LastIndexOf("["); // 希望读第一个 "[" 和最后一个 "]" 能够处理掉
                        index1 = line.LastIndexOf("]");
                        version = line.Substring(index + 1, index1 - index - 1);

                        // 判断是否是临时包
                        if (int.Parse(version.Substring(version.LastIndexOf(".") + 1)) > 500)
                        {
                            ap.TempModiFlag = true;
                            break;
                        }

                        // 生成组件信息
                        CommitCom c = new CommitCom(name, version);

                        if (c.ctype != ComType.Ssql)
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

                        if (c.ctype == ComType.Xml && (c.path.IndexOf(".xls") >= 0    // Path.GetExtension 对于箭头字符会判断非法，换成 stirng.index
                            || c.cname.IndexOf("s_ls_") >= 0 || c.cname.IndexOf("s_as_") >= 0  // CRESxml定义
                            || c.cname.IndexOf("functionlist") >= 0))  // cres 有 funclist.xml 需要特别处理
                        {
                            c.ctype = ComType.FuncXml;
                        }

                        if (c.ctype == ComType.Nothing)
                        {
                            log.WriteErrorLog("无法确定的递交组件类型！" + c.cname);
                            return false;
                        }

                        InsertCom(ap, c);
                    }
                }
            }
            catch (Exception ex)
            {
                log.WriteErrorLog("ProcessComs异常 ReadMe：" + ap.Readme + " " + ex.Message);
                return false;
            }

            if (ap.TempModiFlag)
            {
                string message = "该修改单中存在递交项>500，应该是临时包，确认继续处理？";
                string caption = "测试";

                DialogResult dRes = MessageBox.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (dRes == System.Windows.Forms.DialogResult.No)
                {
                    Result = false;
                }
            }

            // 输出下处理结果
            //log.WriteInfoLog("ProcessComs...");
            /*
            log.WriteInfoLog(System.Reflection.MethodBase.GetCurrentMethod().Name);
            foreach (CommitCom c in ap.ComComms)
            {
                log.WriteInfoLog(c.cname + " " + Enum.GetName(typeof(ComStatus), c.cstatus) 
                    + " " + Enum.GetName(typeof(ComType), c.ctype) + " " + c.cver + " " + c.path);
            }
             * */
            return Result;
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
                string s = "Readme中有如下集成注意，请在处理完成后点击确定继续\n【注意：06版集成注意如果详细设计说明书文件新增了模块，请更新文件后退出程序重启！】：";
                foreach (string t in notice)
                {
                    s += "\r\n" + t;
                }
                System.Windows.Forms.MessageBox.Show(s,
                    "集成注意",
                    System.Windows.Forms.MessageBoxButtons.OK);
            }
            else if(ap.scmtype == ScmType.NewScm) // 检查下是否存在 TablePatch之类
            {
                foreach (CommitCom c in ap.ComComms)
                {
                    if (c.ctype == ComType.TablePatch)
                    {
                        string message = "Readme中没有集成注意，但是检测到TablePatch脚本，请在处理完成后继续。";
                        string caption = "测试";

                        DialogResult dRes = MessageBox.Show(message, caption, MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                        if (dRes == System.Windows.Forms.DialogResult.No)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 编译顺序为：表结构、存储过程、公用原子、其他原子、周边原子、外围原子、公用业务逻辑、同步业务逻辑、其他业务逻辑、周边业务逻辑、外围业务逻辑、
        /// 适配器业务逻辑（有些产品因为模块依赖关系，顺序需要调整，如消费支付的消费支付通知要在公用之后，其他之前编译）、DLL；
        /// 此处，编译SO前要比对SRC
        /// </summary>
        /// <param name="ap"></param>
        /// <returns></returns>
        private bool InsertCom(AmendPack ap, CommitCom c)
        {
            // 只要TablePatch在Patch和过程之前就可以了 根据 ComType的枚举顺序确定
            int i = 0;
            while (i < ap.ComComms.Count && (ap.ComComms[i] as CommitCom).ctype <= c.ctype)
            {
                i++;
            }
            ap.ComComms.Insert(i, c);

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
                string message = "递交版本为 V" + ap.SubmitVer.ToString() + "，未检测到本次修改，确认继续？";
                string caption = "测试";

                DialogResult dRes = MessageBox.Show(message, caption, MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (dRes == System.Windows.Forms.DialogResult.No)
                {
                    return false;
                }
            }

            return true;
        }

        // 处理 SAWPath，设置检出代码的路径
        private bool ProcessSAWPath(AmendPack ap)
        {
            // 确认修改单的配置库
            ap.svn = MAConf.instance.Configs[ap.ProductId].SvnRepo;
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
                    // todo 修改判断
                    if (c.ctype == ComType.FuncXml && c.cname.IndexOf("functionlist") >= 0)
                    {
                        f.fstatus = FileStatus.New;
                    }
                    else
                    {
                        f.fstatus = FileStatus.Old;
                    }
                    ap.SAWFiles.Add(f);
                }
                c.sawfile = f;
                
                #region 根据组件类别，统一路径处理
                if (c.ctype == ComType.Ssql)
                    continue;

                if (c.cname == "HsSettle.exe" || c.cname == "HsBkSettle.exe")
                {
                    string temp = @"HsSettle\trunk\Sources\ClientCom\" + Path.GetFileNameWithoutExtension(c.cname);
                    f.LocalPath = Path.Combine(ap.svn.Workspace, temp).Replace('/', '\\');
                    f.UriPath = Path.Combine(ap.svn.Server, temp).Replace('\\', '/');
                }
                else if (c.ctype == ComType.SO || c.ctype == ComType.Sql || c.ctype == ComType.FuncXml) // 
                {
                    if (c.path[0] == '\\' || c.path[0] == '/')
                        c.path = c.path.Substring(1);

                    string temp = string.Empty;
                    if ((ap.ProductId == "00052" || ap.ProductId == "00053") 
                        && (c.ctype == ComType.SO || c.ctype == ComType.Sql || c.ctype == ComType.FuncXml))
                    {
                        temp = @"Documents\D2.Designs\详细设计\后端";
                    }
                    //todo 如何确定FuncXml的路径？ 最好还是为FuncListXml维护一个产品信息，像转融通，多个project，没办法区分的
                    else if (c.ctype == ComType.FuncXml && c.cname.IndexOf("functionlist") >= 0)
                    {
                        c.path = string.Empty;
                    }

                    f.LocalPath = Path.Combine(ap.svn.Workspace, temp, c.path).Replace('/', '\\');
                    f.UriPath = Path.Combine(ap.svn.Server, temp, c.path).Replace('\\', '/');
                }
                else if (c.ctype == ComType.Patch || c.ctype == ComType.TablePatch || c.ctype == ComType.Ini 
                    || c.ctype == ComType.MenuPatch || c.ctype == ComType.Xml || c.ctype == ComType.Excel)
                {
                    if (c.path[0] == '\\' || c.path[0] == '/')
                        c.path = c.path.Substring(1);

                    // 对于PATCH，INI 文件，指定路径名称为主键，否则出现两个递交在 \\证券下的PATCH，就认为是同一个了
                    if (c.path[c.path.Length - 1] != '\\' && c.path[c.path.Length - 1] != '/')
                        c.path = c.path + '\\';
                    c.path = c.path + c.cname; 
                    f.Path = c.path;
                    
                    f.LocalPath = Path.Combine(ap.svn.Workspace, c.path).Replace('/', '\\');
                    f.UriPath = Path.Combine(ap.svn.Server, c.path).Replace('\\', '/');
                }
                else
                {
                    // 如果第一个是路径分隔符号，那么去路径分隔符号，防止被Path.Combine认为绝对路径
                    if (c.path[0] == '\\' || c.path[0] == '/')
                        c.path = c.path.Substring(1);
                    
                    f.LocalPath = Path.Combine(ap.svn.Workspace, c.path).Replace('/', '\\');
                    f.UriPath = Path.Combine(ap.svn.Server, c.path).Replace('\\', '/');
                }
                #endregion
            }
            return true;
        }
        
        private void ProcessReadMe(AmendPack ap)
        {
            // 输出下处理结果
            log.WriteFileLog("[递交组件]");
            foreach (CommitCom c in ap.ComComms)
            {
                log.WriteFileLog("名称：" + c.cname + " "
                    + "状态：" + Enum.GetName(typeof(ComStatus), c.cstatus) + " "
                    + "版本：" + c.cver + " "
                    + "路径：" + c.path);
            }

            log.WriteFileLog("[配置库文件]");
            foreach (SAWFile s in ap.SAWFiles)
            {
                log.WriteFileLog("路径：" + s.Path + " "
                    + "本地路径：" + s.LocalPath + " "
                    + "SvnUri：" + s.UriPath + " "
                    + "文件状态：" + Enum.GetName(typeof(FileStatus), s.fstatus));
            }
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

    class CopyUnChange : State
    {
        public CopyUnChange()
            : base()
        {
        }

        public override string StateName
        {
            get { return "处理压缩包"; }
        }
        
        public override bool DoWork(AmendPack ap)
        {
            log.WriteInfoLog("集成类型:" + Enum.GetName(typeof(ScmType), ap.scmtype));

            BaseConf pconf = MAConf.instance.Configs[ap.ProductId];
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
                        log.WriteLog("复制文件，源地址：" + Path.Combine(ap.SCMLastAmendDir, c.cname) + "，"
                            + "目标文件：" + Path.Combine(ap.SCMAmendDir, c.cname), LogLevel.FileLog);

                        if (!File.Exists(Path.Combine(ap.SCMLastAmendDir, c.cname)))
                        {
                            log.WriteLog(Path.Combine(ap.SCMLastAmendDir, c.cname) + " 不存在，请检查是否集成出现异常", LogLevel.Error);
                            continue;
                        }

                        File.Copy(Path.Combine(ap.SCMLastAmendDir, c.cname), 
                            Path.Combine(ap.SCMAmendDir, c.cname), 
                            true);

                        // 对于 SO，需要把压缩文件从上次递交中解压缩，覆盖掉本次包里的文件
                        if (c.ctype == ComType.SO)
                        {
                            string st = pconf.GetDetail(c).GetProcStr();
                            log.WriteInfoLog("解压缩源文件，源文件夹：" + ap.SCMLastLocalSrcRar + "， "
                                + "文件：" + st + "， "
                                + "目标文件夹：" + ap.SCMAmendDir);
                            UnRar(ap, ap.SCMLastLocalSrcRar, ap.SCMAmendDir, st);
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
                            foreach (string s in pconf.GetDetail(c).ProcFiles)
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
        public PackerCheck()
            : base()
        {
        }


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
        public PackerSvnCode()
            : base()
        {
        }

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
            SvnPort svn = new SvnPort("0", "1");
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
                if (!Result)
                {
                    string message = " 获取版本库信息错误，确认继续？" + "\r\n" + s.LocalPath;
                    string caption = "测试";

                    DialogResult dRes = MessageBox.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                    if (dRes == System.Windows.Forms.DialogResult.No)
                    {
                        break;
                    }
                    else
                    {
                        Result = true;  // 防止最后一个确认的svn点了Yes，但是仍然返回失败
                    }
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
        public PackerCompile()
            : base()
        {
        }

        public override string StateName
        {
            get { return "编译输出物"; }
        }

        public override bool DoWork(AmendPack ap)
        {
            BaseConf pconf = MAConf.instance.Configs[ap.ProductId];
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

                // Patch 和 Ini 刷下来，拷贝过去就可以，在后面的Repacker里面拷贝
                if (c.ctype == ComType.Patch || c.ctype == ComType.TablePatch || c.ctype == ComType.Ini ||
                   c.ctype == ComType.Xml|| c.ctype == ComType.Excel || c.ctype == ComType.MenuPatch)
                {
                }
                else if (c.ctype == ComType.Dll || c.ctype == ComType.Exe)
                {
                    Result = pconf.CompileFront(c);
                    if (Result == false)
                        break;

                    c.cstatus = ComStatus.Normal;
                    // 对于delphi 5 编译输出，可能会生成MAP文件，删除掉生成的MAP文件
                    if (File.Exists(Path.Combine(ap.SCMAmendDir, Path.GetFileNameWithoutExtension(c.cname) + ".map")))
                    {
                        File.Delete(Path.Combine(ap.SCMAmendDir, Path.GetFileNameWithoutExtension(c.cname) + ".map"));
                    }
                }
                else if (c.ctype == ComType.SO || c.ctype == ComType.Sql || c.ctype == ComType.FuncXml)
                {
                    // 这里不能并发执行，有可能锁住Excel，可能只能等待执行
                    Result = pconf.CompileBackEnd(c);
                    if (Result == false)
                        break;

                    // 对于原子AS，生成会同时生成过程，对于这种，编译时可以检查下是否存在，存在同样路径的递交，sql和so可以同时改状态
                    if (pconf is CresConf)
                    {
                        foreach (CommitCom c1 in ap.ComComms)
                        {
                            if (c1.path == c.path && c1.ctype != c.ctype && (c1.cstatus == ComStatus.Add || c1.cstatus == ComStatus.Modify))
                                c1.cstatus = ComStatus.Normal;
                        }
                    }
                    c.cstatus = ComStatus.Normal;
                }

                
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
    /// 主要是进行源代码对比工作，包括 so 源文件和 sql 源文件的检查，应该是调用外部工具如 Beyond Compare 之类
    /// </summary>
    class PackerDiffer : State
    {
        public PackerDiffer()
            : base()
        {
        }

        private string DiffBin;
        private string DiffArgs;

        public override string StateName
        {
            get { return "文件对比"; }
        }

        public override bool DoWork(AmendPack ap)
        {
            // 对于相同的，可以不提示用户，对于不同的，需要集成的同学确认，这个还需要想办法来写
            bool Result = true, CompareSame = true;
            BaseConf pconf = MAConf.instance.Configs[ap.ProductId];

            // 支持UF不做源代码对比
            if (!pconf.DiffEnable)
            {
                log.WriteLog("源代码对比未启用，跳过步骤。");
                return true;
            }

            if (!File.Exists(pconf.DiffBin))
            {
                log.WriteLog("源代码对比程序不存在，将跳过源代码对比", LogLevel.Warning);
                return true;
            }

            DiffBin = pconf.DiffBin;
            DiffArgs = pconf.DiffArgs;

            foreach (CommitCom c in ap.ComComms)
            {
                CompareSame = true;

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
                    foreach (string s in pconf.GetDetail(c).ProcFiles)
                    {
                        string file1 = Path.Combine(ap.SCMAmendDir, s);
                        string file2 = Path.Combine(pconf.OutDir, s);

                        if (!File.Exists(file1))
                        {
                            log.WriteErrorLog("对比源文件不存在，请检查是否递交了源代码。" + file1);
                            CompareSame = false;
                        }
                        else if (!File.Exists(file2))
                        {
                            log.WriteErrorLog("对比源文件不存在，请检查是否存在编译输出。" + file2);
                            CompareSame = false;
                        }

                        if (!CompareSame)
                            continue;

                        if (!HashCom(file1, file2))
                        {
                            log.WriteLog("文件对比不同，等待用户处理。" + file1 + " " + file2, LogLevel.Warning);
                            CompareSame = false;
                            CompareSrc(file1, file2);
                        }
                    }
                }
                else if(c.ctype == ComType.Sql)
                {
                    string file1 = Path.Combine(ap.SCMAmendDir, c.cname);
                    string file2 = Path.Combine(pconf.OutDir, c.cname);

                    if (!File.Exists(file1))
                    {
                        log.WriteErrorLog("对比源文件不存在，请检查是否递交了源代码。" + file1);
                        CompareSame = false;
                    }
                    else if (!File.Exists(file2))
                    {
                        log.WriteErrorLog("对比源文件不存在，请检查是否存在编译输出。" + file2);
                        CompareSame = false;
                    }

                    if (!CompareSame)
                        continue;

                    // 减少对比时的弹出框框
                    if (!HashCom(file1, file2))
                    {
                        log.WriteLog("文件对比不同，等待用户处理。" + file1 + " " + file2, LogLevel.Warning);
                        CompareSame = false;
                        CompareSrc(file1, file2);
                    }
                }

                if (Result && !CompareSame)
                {
                    Result = false;
                }
                else if (CompareSame)
                {
                    c.cstatus = ComStatus.Normal;
                }
            }

            if (!Result)
            {
                string message = "源代码比对不同，是否继续处理？";
                string caption = "测试";

                DialogResult dRes = MessageBox.Show(message, caption, MessageBoxButtons.YesNo);
                if (dRes == System.Windows.Forms.DialogResult.Yes)
                {
                    Result = true;
                }
            }

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
                p.StartInfo.FileName = DiffBin;
                p.StartInfo.Arguments = DiffArgs.Replace("%filescm%", "\"" + filescm + "\"").Replace("%filesub%", "\"" + filesub + "\"");
                log.WriteLog("对比命令：" + p.StartInfo.FileName + " " + p.StartInfo.Arguments, LogLevel.FileLog);
                p.StartInfo.UseShellExecute = true;        // 关闭Shell的使用  
                p.StartInfo.RedirectStandardInput = false; // 重定向标准输入
                p.StartInfo.RedirectStandardOutput = false;  //重定向标准出  
                p.StartInfo.RedirectStandardError = false; //重定向错误输出  
                p.StartInfo.CreateNoWindow = false;             // 不显示窗口
                p.Start();    // 启动
                //strOutput = p.StandardOutput.ReadToEnd();        // 从输出流取得命令执行结果
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
    /// 送到后台编译
    /// </summary>
    class PackerSO : State
    {
        public PackerSO()
            : base()
        {
        }

        public override string StateName
        {
            get { return "转移源文件，生成中间件"; }
        }

        public override bool DoWork(AmendPack ap)
        {
            bool Result = true;
            BaseConf pconf = MAConf.instance.Configs[ap.ProductId];
            if (pconf == null)
            {
                log.WriteErrorLog("产品配置失败不存在！" + ap.ProductId);
                return false;
            }

            ReSSH s = pconf.Conn;  // 一定要配置这个
            if (s == null)
            {
                log.WriteErrorLog("集成ssh配置不存在！");
                return false;
            }

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
                    Detail dl = pconf.GetDetail(c);
                    // 上传、编译、下载；先把所有的代码上传，防止有新增的函数依赖，导致编译报错
                    Result = s.UploadModule(dl);
                    if (!Result)
                    {
                        log.WriteErrorLog("上传源代码失败了！ " + c.cname);
                        break;
                    }
                }
                else if (c.ctype == ComType.Sql || c.ctype == ComType.TablePatch)
                {
                    Result = pconf.CompileSql(c);
                    if (!Result)
                    {
                        log.WriteErrorLog("编译Sql脚本失败了！" + c.cname);
                        break;
                    }
                    c.cstatus = ComStatus.Normal;
                }
            }

            // 上传成功，就编译、下载
            if (Result)
            {
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
                        Detail dl = pconf.GetDetail(c);

                        // 编译、下载
                        Result = s.Compile(dl) && s.DownloadModule(dl);
                        if (!Result)
                        {
                            log.WriteErrorLog("编译SO失败了！" + dl.Name);
                            break;
                        }
                        c.cstatus = ComStatus.Normal;
                    }
                }
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
        public PackerRePack()
            : base()
        {
        }

        public override string StateName
        {
            get { return "重新打包处理"; }
        }

        #region 递交组件版本比较
        public bool ValidateVersion(AmendPack ap)
        {
            BaseConf pconf = MAConf.instance.Configs[ap.ProductId];
            bool Result = true, ret = true;
            string VFile = string.Empty;
            foreach (CommitCom c in ap.ComComms)
            {
                ret = true;
                
                // 这里的校验主要校验版本，集成编译之后，对于SO，还要校验大小
                // 根据文件类型调用不同方法校验版本
                switch (c.ctype)
                {
                    case ComType.Dll:
                    case ComType.Exe:
                        ret = ValidateDll(Path.Combine(pconf.OutDir, c.cname), c.cver, out VFile);
                        break;
                    case ComType.Ini: // Ini 文件有多个版本信息
                    case ComType.TablePatch: // Patch 文件有多行版本信息
                    case ComType.MenuPatch:
                    case ComType.Patch: // Patch 文件有多行版本信息
                        ret = ValidateSO(c.sawfile.LocalPath, c.cver, out VFile);
                        break;
                    case ComType.FuncXml: //todo 确认有无版本？
                    case ComType.SO:  // SO 文件只有一个版本信息
                    case ComType.Sql:
                        ret = ValidateSO(Path.Combine(pconf.OutDir, c.cname), c.cver, out VFile);
                        break;
                    case ComType.Excel: // 暂时没法检查版本
                    case ComType.Ssql:  // 小包无版本
                        break;
                    default:
                        break;
                }

                if (!ret) // 先校验完所有的，如果有一个不同，则返回版本不对
                {
                    log.WriteErrorLog(c.cname + "版本信息不同!" + "[集成]" + VFile + "<->[递交]" + c.cver);

                    if (Result)
                        Result = false;
                }
            }

            if (!Result)
            {
                string message = " 版本对比不同，确认继续？";
                string caption = "测试";

                DialogResult dRes = MessageBox.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (dRes == System.Windows.Forms.DialogResult.Yes)
                {
                    Result = true;
                }
            }

            return Result;
        }
        
        public bool ValidateSO(string path, string version, out string VFile)
        {
            if (!File.Exists(path))
            {
                log.WriteErrorLog(path + " 不存在");
                VFile = "V0.0.0.0";
                return false;
            }

            // 版本比较 
            // 取压缩包的 SO 测试下正则表达式，同时比较版本信息
            StreamReader sr = new StreamReader(path);
            string input = sr.ReadToEnd();
            sr.Close();

            bool Result = true;
            VFile = string.Empty;

            string pattern = @"V(\d+\.){3}\d+"; // V6.1.31.16
            Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            // 确保版本逆序，只取第一个
            Match m = rgx.Match(input);
            if (!m.Success||(VFile =m.Groups[0].Captures[0].Value) != version)
            {
                Result = false;
            }
            
            return Result;
        }
        
        public bool ValidateDll(string path, string version, out string VFile)
        {
            if (!File.Exists(path))
            {
                log.WriteErrorLog(path + " 不存在");
                VFile = "V0.0.0.0";
                return false;
            }
            // 获取文件版本信息
            FileVersionInfo DllVer = FileVersionInfo.GetVersionInfo(
                path);

            VFile = "V" + DllVer.FileVersion;
            // cver的 V 去掉，从第一个开始比较
            return VFile == version;
        }
        #endregion

        public bool CopyCommit(AmendPack ap)
        {
            bool Result = true;
            BaseConf pconf = MAConf.instance.Configs[ap.ProductId];
            foreach (CommitCom c in ap.ComComms)
            {
                // 小包直接跳过
                if (c.ctype == ComType.Ssql || c.cstatus == ComStatus.NoChange)
                {
                    continue;
                }

                log.WriteLog("复制：" + c.cname, LogLevel.FileLog);

                // 去除文件只读属性，否则复制时不能覆盖
                FileAttributes attributes = File.GetAttributes(Path.Combine(ap.SCMAmendDir, c.cname));
                FileAttributes attr1 = attributes;
                if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    // Show the file.
                    attributes = attributes & ~FileAttributes.ReadOnly;
                    File.SetAttributes(Path.Combine(ap.SCMAmendDir, c.cname), attributes);
                }

                if (c.ctype == ComType.Patch || c.ctype == ComType.TablePatch || c.ctype == ComType.Ini || 
                   c.ctype == ComType.Xml || c.ctype == ComType.Excel || c.ctype == ComType.MenuPatch)
                {
                    File.Copy(c.sawfile.LocalPath, Path.Combine(ap.SCMAmendDir, c.cname), true);
                }
                else
                {
                    File.Copy(Path.Combine(pconf.OutDir, c.cname), Path.Combine(ap.SCMAmendDir, c.cname), true);

                    if (c.ctype == ComType.SO) // so 多复制源代码
                    {
                        Detail d = pconf.GetDetail(c);
                        foreach (string s in d.ProcFiles)
                        {
                            File.Copy(Path.Combine(pconf.OutDir, s), Path.Combine(ap.SCMAmendDir, s), true);
                        }
                    }
                }

                // 还原只读属性，似乎不需要还原，没啥用
                //attributes = attr1;
                //if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                //{
                //    File.SetAttributes(Path.Combine(ap.SCMAmendDir, c.cname), attributes);
                //}
            }
            return Result;
        }

        public override bool DoWork(AmendPack ap)
        {
            // 检查版本一致性
            bool Result = ValidateVersion(ap);
            if (!Result)
                return false;

            // 复制递交组件到递交包 
            Result = CopyCommit(ap);
            if (!Result)
                return false;

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
                p.StartInfo.FileName = MAConf.instance.Configs[ap.ProductId].Rar;           // rar程序名

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
        public PackerUpload()
            : base()
        {
        }

        public override string StateName
        {
            get { return "压缩包上传"; }
        }

        public override bool DoWork(AmendPack ap)
        {
            BaseConf pconf = MAConf.instance.Configs[ap.ProductId];
            
            FTPConnection ftp = pconf.ftp;
            if (ftp.IsConnected == false)
            {
                try
                {
                    ftp.Connect();
                }
                catch (Exception e)
                {
                    log.WriteErrorLog("连接FTP服务器失败，错误信息：" + e.Message);
                    return false;
                }
            }

            try
            {
                if (ftp.Exists(ap.SCMRemoteFile))
                {
                    string message = " Ftp文件已存在，确认继续？" + "\r\n" + ap.SCMRemoteFile;
                    string caption = "测试";

                    DialogResult dRes = MessageBox.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                    if (dRes == System.Windows.Forms.DialogResult.Yes)
                    {
                        ftp.DeleteFile(ap.SCMRemoteFile);
                    }
                    else
                    {
                        log.WriteErrorLog("选择保留ftp文件！" + ap.SCMRemoteFile);
                        return true;
                    }
                }

                ftp.UploadFile(ap.SCMLocalFile, ap.SCMRemoteFile);
            }
            catch (Exception e)
            {
                log.WriteErrorLog("上传文件失败，错误信息：" + e.Message);
                return false;
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
        public PackCleanUp()
            : base()
        {
        }

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
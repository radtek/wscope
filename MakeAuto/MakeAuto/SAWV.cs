using System;
using SAWVSDKLib;
using System.Collections;
using System.Diagnostics;

namespace MakeAuto
{
    /// <summary>
    /// 对应VSS的一些操作，包括刷新文件，获取版本，检入检出等，使用
    /// </summary>
    class SAWV
    {
        public SAWV(string name, string server, int port, string database, string user, string password)
        {
            sv = new SAWVSDK();
            ConnectedToServer = false;
            LoggedIn = false;

            Name = name;
            ServerIP = server;
            ServerPort = port;
            DatabaseName = database;
            UserName = user;
            Password = password;

            // 初始化日志
            log = OperLog.instance;
        }

        /// <summary>
        /// 连接到服务器
        /// </summary>
        public Boolean ConnectToServer()
        {
            log.WriteLog("连接配置库 IP: " + ServerIP + ", Port: " + ServerPort);
            // 测试 SAW 功能
            Boolean bConn = false;
            int Result = sv.ConnectToServer(ServerIP, ServerPort, out bConn,
                out EncryptType, out OnlyTrial, out LeftTrialDays,
                out Canceled, out ResultDescription, Enum_ProxyType.Enum_NOPROXY,
                "", 0, "", "");
            if (Result == 0)
            {
                this.ConnectedToServer = bConn;
                //log.WriteLog("配置库连接成功");
            }
            else
            {
                log.WriteErrorLog("配置库连接失败。");
            }

            return this.ConnectedToServer;
        }

        /// <summary>
        /// 登录到数据库
        /// </summary>
        public Boolean Login()
        {
            bool result = true;
            // 连接配置库
            if (!ConnectedToServer)
                result = ConnectToServer();

            if (!result)
                return false;

            log.WriteLog("登录配置库 DataBaseName:" + DatabaseName);
            // 登录
            SAWVKeyInfoSet sk = new SAWVKeyInfoSet();
            int Result = sv.Login(UserName, Password, DatabaseName,
                sk, out MustChangePassword, out ExpireDays, out Canceled, 
                out ResultDescription);

            if (Result == 0)
            {
                this.LoggedIn = true;
                //log.WriteLog("用户登录成功");
            }
            else
            {
                log.WriteErrorLog("用户登录失败！");
            }

            return this.LoggedIn;
        }

        public Boolean GetAmendCode(string AmendNo, SAWFile sf)
        {
            bool result = true;
            // 登录配置库
            if (!LoggedIn)
                result = Login();

            if (!result)
                return false;

            log.WriteLog("获取修改单代码文件,修改单号:" + AmendNo + 
                " 递交类型：" + Enum.GetName(typeof(SAWType), sf.Type) + 
                " 文件：" + sf.SAWPath + 
                " 本地路径：" + sf.LocalPath);

            int FileVersion = 0;
            string FileName;
            bool Found = false;
            if (sf.Type == SAWType.File)
            {
                // 获取文件历史
                SAWVFileHistorySet hisset;
                Boolean Pinned;
                string commituser = string.Empty;
                // 获取文件历史，可以预期，检入文件的时间和集成的时间之差应该在一个月之内，据此定义时间
                int Result = sv.GetFileHistory(sf.SAWPath, out Pinned, out hisset, commituser,
                    DateTime.Now.AddMonths(-1), DateTime.Now, out Canceled, out ResultDescription);

                if (Result != 0)
                {
                    log.WriteErrorLog("获取文件历史历史信息失败！ 文件：" +sf.SAWPath + " 返回信息：" + ResultDescription);
                    return false;
                }

                foreach (SAWVFileHistory his in hisset)
                {
                    // 20111215020-V6.1.4.10-V1
                    if (his.Comment.IndexOf(AmendNo + "-" + sf.Version) >= 0)
                    {
                        FileVersion = his.Version;  // 得到要Get文件的版本
                        sf.filehis = his;
                        break;
                    }
                }

                if (FileVersion == 0)
                {
                    log.WriteErrorLog("未能找到该修改单对应文件版本。" + AmendNo + "-" + sf.Version);
                    return false;
                }

                // 获取历史文件
                result = GetOldVersionFile(sf.SAWPath, FileVersion, sf.LocalPath);

                if (!result)
                    return false;
            }
            else  // 对于 Dpr 数据 
            {
                // 工程的处理方法，由于不能确定本地的数据是否规范，所以先把所有数据刷到最新版
                // 然后把本历史之后的数据处理到前一个版本，应该可以解决这个问题
                int ResultValue = sv.GetLatestProject(sf.SAWPath, sf.LocalPath, false, false,
                    Enum_WritableFileHandling.Enum_WritableFileHandlingReplace,
                    Enum_EOL.Enum_CRLF,
                    Enum_SetLocalFileTime.Enum_SetLocalFileTimeCurrent,
                    "", "", out Canceled, out ResultDescription, out OperationResultSet);

                if (ResultValue == 0)
                {
                    foreach (SAWVOperationResult o in OperationResultSet)
                    {
                        if (o.OperationResult != 0)
                        {
                            log.WriteErrorLog("检出文件" + o.ItemFullName + "失败，" + o.Description);
                            return false;
                        }
                    }
                }
                else
                {
                    log.WriteErrorLog("GetLatestProject 失败，" + ResultDescription);
                    return false;
                }

                // 这时工程的代码是新的，然后要获取工程的历史，以确定要刷哪个版本
                SAWVProjectHistorySet ProjectHistorySet;
                // 第一个 true 表示包含文件历史
                ResultValue = sv.GetProjectHistory(sf.SAWPath, true, false,
                    out ProjectHistorySet, "",
                    DateTime.Now.AddMonths(-1), DateTime.Now, out Canceled, out ResultDescription);

                if (ResultValue != 0)
                {
                    Debug.WriteLine("获取工程历史信息失败！工程：" + sf.SAWPath + " 返回信息：" + ResultDescription);
                    return false;
                }

                // 这里先过一遍，确保能够找到，找不到就不用刷新历史版本了
                foreach (SAWVProjectHistory his in ProjectHistorySet)
                {
                    if (his.Comment.IndexOf(AmendNo + "-" + sf.Version) >= 0)
                    {
                        Found = true;
                        break;
                    }
                }

                if (Found == false)
                {
                    log.WriteErrorLog("无法找到工程信息。");
                    return false;
                }

                // 重新检出代码
                log.WriteInfoLog("开始回滚工程代码...");
                foreach (SAWVProjectHistory his in ProjectHistorySet)
                {
                    if (his.Comment.IndexOf(AmendNo + "-" + sf.Version) >= 0)
                    {
                        break;
                    }

                    FileVersion = his.Version - 1; // 回滚到上一个版本，应该可以吧
                    FileName = his.FileName;
                    string LocalPath = FileName.Replace("$/", Workspace);  // 本地路径，替换$/ 为 E:\VSS\
                    
                    result = GetOldVersionFile(FileName, FileVersion, LocalPath);

                    if (!result)
                        return false;
                }

            }
            return true;
        }

        // 获取指定版本文件，检出详细设计说明书
        private Boolean GetOldVersionFile(string FileName, int Version, string LocalPath)
        {
            // 获取历史代码
            SAWVOperationResult OperationResult = new SAWVOperationResult();

            // 获取历史文件
            string Project = FileName.Substring(0, FileName.LastIndexOf("/"));
            string File = FileName.Substring(FileName.LastIndexOf("/") + 1);

            int Result = sv.GetOldVersionFile(Project, File, Version,
                LocalPath, false, 
                Enum_WritableFileHandling.Enum_WritableFileHandlingReplace,
                Enum_EOL.Enum_CRLF, 
                Enum_SetLocalFileTime.Enum_SetLocalFileTimeCurrent,
                "", "", out Canceled,
                out ResultDescription, OperationResult);

            log.WriteInfoLog(OperationResult.ItemFullName + "，版本：" + Version 
                + "，本地路径：" + LocalPath
                + "..." + OperationResult.Description);

            if (Result != 0 || OperationResult.OperationResult != 0)
                return false;
            else
                return true;
        }

        // SAWVSDK 对象
        private SAWVSDK sv;

        // ConnectToServer 的参数
        public String ServerIP {get; private set; }
        public int ServerPort { get; private set; }
        public String UserName { get; private set; }
        public String Password { get; private set; }
        public String DatabaseName { get; private set; }
        public Boolean ConnectedToServer {get; private set;}
        public Boolean LoggedIn { get; private set; }

        private Enum_EncryptType EncryptType;
        private Boolean OnlyTrial;
        private int LeftTrialDays;

        private Boolean Canceled;
        private String ResultDescription;
        private Boolean MustChangePassword; 
        private int ExpireDays;

        private SAWVOperationResultSet OperationResultSet;

        public string Name;  // 类的主键
        public string Workspace {get; set;}  // 工作目录
        public string Amend {get; set;}  // 根据修改单中的这个特性来瞄定数据库

        private OperLog log;
    }

    class SAWVList : ArrayList
    {
        public SAWV this[string name]
        {
            get
            {
                foreach (SAWV s in this)
                {
                    if (s.Name == name)
                        return s;
                }
                return null;
            }
        }

        // 根据递交特征来决定连接到哪个库，比如小球的目录， 顶级的是 广发版技术支持测试，
        // 那就匹配到了 “广发版技术支持测试|融资融券” 上
        public SAWV GetByAmend(string AmendSub)
        {
            foreach (SAWV s in this)
            {
                if (s.Amend.IndexOf(AmendSub) >= 0)
                    return s;
            }
            return null;
        }
    }

    enum FileStatus
    {
        NoChange = 0,
        Old = 1,
        New = 2,
        Unkown = 3,
    }

    enum SAWType
    {
        Nothing = 0,
        Project = 1,
        File = 2,
    }

    // 这里保存需要从 SAW 刷代码的文件列表
    class SAWFile
    {
        public SAWFile(string path, FileStatus status = FileStatus.NoChange)
        {
            Path = path;
            fstatus = status;

            // 根据最后带不带后缀分析
            if (System.IO.Path.GetExtension(Path) == string.Empty)
            {
                Type = SAWType.Project;
            }
            else Type = SAWType.File;
        }

        public string Path;  // ReadMe 中的路径，可能是文件，也可能是目录，这个类的主键
        public SAWType Type;   // Project or File ?? 1 - Project 2-File
        public string SAWPath;
        public string LocalPath;
        public string Version;
        //public string LocalVersion;
        public FileStatus fstatus;
        public SAWVFileHistory filehis;
    }

    class SAWFileList : ArrayList
    {
        public SAWFile this[string path]
        {
            get
            {
                foreach (SAWFile s in this)
                {
                    if (s.Path == path)
                        return s;
                }
                return null;
            }
        }
    }
}

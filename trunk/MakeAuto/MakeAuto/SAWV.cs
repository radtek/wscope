using System;
using SAWVSDKLib;
using System.Collections;

namespace MakeAuto
{
    /// <summary>
    /// 对应VSS的一些操作，包括刷新文件，获取版本，检入检出等，使用
    /// </summary>
    class SAWV
    {
        // 占位
        // 单例化 SAWV
        public static readonly SAWV instance = new SAWV();

        private SAWV()
        {
            sv = new SAWVSDK();
            ConnectedToServer = false;
            LoggedIn = false;
        }

        /// <summary>
        /// 连接到服务器
        /// </summary>
        /// <param name="ServerIP">服务器地址或名称</param>
        /// <param name="ServerPort">服务器端口</param>
        /// <returns>是否连接成功</returns>
        public Boolean ConnectToServer(String ServerIP, int ServerPort)
        {
            // 测试 SAW 功能
            Boolean bConn = false;
            int Result = sv.ConnectToServer(ServerIP, ServerPort, out bConn,
                out EncryptType, out OnlyTrial, out LeftTrialDays,
                out Canceled, out ResultDescription, Enum_ProxyType.Enum_NOPROXY,
                "", 0, "", "");
            if (Result == 0)
            {
                this.ServerIP = ServerIP;
                this.ServerPort = ServerPort;
                this.ConnectedToServer = bConn;
            }
            return this.ConnectedToServer;
        }

        /// <summary>
        /// 登录到数据库
        /// </summary>
        /// <param name="UserName">用户名</param>
        /// <param name="Password">密码</param>
        /// <param name="DatabaseName">数据库名称</param>
        /// <returns></returns>
        public Boolean Login(String UserName, String Password, String DatabaseName)
        {
            // 登录
            SAWVKeyInfoSet sk = new SAWVKeyInfoSet();
            int Result = sv.Login(UserName, Password, DatabaseName,
                sk, out MustChangePassword, out ExpireDays, out Canceled, 
                out ResultDescription);
            
            if (Result == 0)
            {
                this.UserName = UserName;
                this.Password = Password;
                this.DatabaseName = DatabaseName;
                this.LoggedIn = true;
            }
            return this.LoggedIn;
        }

        // 获取修改单详细设计说明书
        public Boolean GetAmendDetail(string AmendNo, string FileName, string AmendVersion, string UserNmae = "")
        {
            SAWVFileHistorySet hisset = GetFileHistory(FileName, UserName);
            int FileVersion = 0;
            foreach (SAWVFileHistory his in hisset)
            {
                // 20111215020-V6.1.4.10-V1
                if (his.Comment.IndexOf(AmendNo + "-" + AmendVersion) >= 0)
                {
                    FileVersion = his.Version;  // 得到要Get文件的版本
                    break;
                }
            }

            if (FileVersion == 0)
                return false;

            // 获取历史文件
            string Project = FileName.Substring(0, FileName.LastIndexOf("/"));
            string File = FileName.Substring(FileName.LastIndexOf("/"));
            GetOldVersionFile(Project, File, FileVersion);
            return true;
        }

        public SAWVFileHistorySet GetFileHistory(String FileName, string UserName = "")
        {
            // 获取文件历史，可以预期，检入文件的时间和集成的时间之差应该在一个月之内，据此定义时间
            SAWVFileHistorySet hisset;
            Boolean Pinned;
            int Result = sv.GetFileHistory(FileName, out Pinned, out hisset, UserName,
                DateTime.Now.AddMonths(-1), DateTime.Now, out Canceled, out ResultDescription);

            return hisset;
        }

        // 获取指定版本文件，检出详细设计说明书
        private Boolean GetOldVersionFile(String ProjectName, String FileName, int Version)
        {
            // 暂时只对06版有效，因为目录是固定的，需要写死
            string detail = MAConf.instance.DetailFile;
            string LocalDir = detail.Substring(0, detail.LastIndexOf(@"\") + 1);

            // 获取历史代码
            int Result = sv.GetOldVersionFile(ProjectName, FileName, Version,
                LocalDir + "后端\\" + FileName, false, 
                Enum_WritableFileHandling.Enum_WritableFileHandlingCanceled,
                Enum_EOL.Enum_CRLF, 
                Enum_SetLocalFileTime.Enum_SetLocalFileTimeCurrent,
                "", "", out Canceled, 
                out ResultDescription, OpertationResult);

            if (Result != 0 || OpertationResult.OperationResult != 0)
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

        private SAWVOperationResult OpertationResult;
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
        public string LocalVersion;
        public FileStatus fstatus;
        private SAWVFileHistory filehis;
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

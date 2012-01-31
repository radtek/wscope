using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SAWVSDKLib;

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


        public SAWVFileHistorySet GetFileHistory(String FileName, string UserName = "")
        {
            // 获取文件历史，可以预期，检入文件的时间和集成的时间之差应该在一个月之内，据此定义时间
            SAWVFileHistorySet hisset;
            Boolean Pinned;
            int Result = sv.GetFileHistory(FileName, out Pinned, out hisset, UserName,
                DateTime.Now.AddMonths(-1), DateTime.Now, out Canceled, out ResultDescription);

            return hisset;
        }


        // SAWVSDK 对象
        private SAWVSDK sv;

        // ConnectToServer 的参数
        public String ServerIP {get; private set; }
        public int ServerPort { get; private set; }
        public String UserName { get; private set; }
        public String Password { get; private set; }
        public String DatabaseName { get; private set; }

        private Enum_EncryptType EncryptType;
        private Boolean OnlyTrial;
        private int LeftTrialDays;

        public Boolean ConnectedToServer {get; private set;}
        public Boolean LoggedIn { get; private set; }
        private Boolean Canceled;
        private String ResultDescription;
        private Boolean MustChangePassword; 
        private int ExpireDays;
    }
}

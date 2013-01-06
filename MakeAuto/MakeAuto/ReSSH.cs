using System.Collections;
using Renci.SshNet;
using System.IO;

namespace MakeAuto
{
    class ReSSH
    {
        /// <summary>
        /// 配置一个ssh连接
        /// </summary>
        /// <param name="name">连接名称，主键</param>
        /// <param name="host">主机地址</param>
        /// <param name="port">端口</param>
        /// <param name="user">用户名</param>
        /// <param name="pass">密码</param>
        /// <param name="compile">是否对此配置执行编译</param>
        /// <param name="restartAs">是否重启AS</param>
        public ReSSH(string name, string host, int port, string user, string pass, bool compile = false, bool restartAs = false)
        {
            ConnectionInfo info = new PasswordConnectionInfo(host, port, user, pass);
            ssh = new SshClient(info);
            sftp = new SftpClient(info);

            this.name = name;
            this.host = host;
            this.port = port;
            this.user = user;
            this.pass = pass;
            this.compile = compile;
            this.restartAs = restartAs;
            localdir = @"C:\src\";
            remotedir = @"/home/" + user + "/src/";

            // 初始化日志
            log = OperLog.instance;
        }

        /// <summary>
        /// 对于没有连接的环境执行初始化，并打开一个channel和shell
        /// </summary>
        /// <param name="reconnect">是否强制重连，强制重连会先断开现有的环境</param>
        /// <returns>是否成功</returns>
        public bool InitSsh(bool reconnect = false)
        {
            // 如果连接已经存在，则直退出
            if (ssh.IsConnected)
            {
                return true;
            }

            ssh.Connect();

            return true;
        }

        public bool CloseSsh()
        {
            if (ssh.IsConnected)
            {
                ssh.Disconnect();
            }

            return true;
        }

        public bool InitSftp()
        {
            if (sftp.IsConnected)
            {
                return true;
            }

            sftp.Connect();

            return true;
        }

        public bool CloseSftp()
        {
            if (sftp.IsConnected)
            {
                sftp.Disconnect();
            }

            return true;
        }

        /// <summary>
        /// 重启AS
        /// </summary>
        /// <returns>是否重启成功</returns>
        public bool RestartAS()
        {
            // 初始化连接，打开shell
            if (InitSsh() == false)
            {
                log.WriteErrorLog("初始化ssh连接失败");
                return false;
            }

            // 执行命令
            // 开启命令，发送重启AS指令
            // 这里 gfas 总是应答不能返回，可能是它启动 haas的进程是后台执行的，所以只能处理到超时这个异常，可能会导致真正的异常被
            // 隐藏掉
            var cmd = ssh.CreateCommand("source ~/.bash_profile; gfas");
            cmd.CommandTimeout = new System.TimeSpan(0, 0, 5);
            try
            {
                cmd.Execute();
            }
            catch (Renci.SshNet.Common.SshOperationTimeoutException e)
            {
                if (cmd.Result.Trim() == string.Empty)
                {
                    log.WriteErrorLog("重启AS异常，" + e.Message);
                }
            }

            if (cmd.ExitStatus != 0)
            {
                log.WriteLog(cmd.Result, LogLevel.Error);
                return false;
            }
            else
            {
                log.WriteLog(cmd.Result, LogLevel.Info);
            }

            return true;
        }

        public bool UploadModule(Detail dl)
        {
            if (sftp.IsConnected == false)
            {
                if (InitSftp() == false)
                {
                    log.WriteLog("初始化sftp连接失败", LogLevel.Error);
                    return false;
                }
            }

            FileStream fs;
            string path;

            // 处理 .pc, .h, .cpp, .gc 文件
            foreach (string f in dl.ProcFiles)
            {
                log.WriteLog("上传文件 " + f, LogLevel.Info);
                path = Path.Combine(localdir, f);
                fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                sftp.UploadFile(fs, remotedir + f);
                fs.Close();
            }

            log.WriteLog("文件上传成功！");
            return true;
        }

        public bool DownloadModule(Detail dl)
        {
            if (sftp.IsConnected == false)
            {
                InitSftp();
            }

            FileStream fs = new FileStream(Path.Combine(localdir, dl.SO), FileMode.Create);
            MAConf.instance.WriteLog("下载文件 " + dl.SO, LogLevel.Info);
            sftp.DownloadFile("/home/" + user + "/appcom/" + dl.SO, fs);
            fs.Close();

            return true;
        }

        private string MakeCmd(Detail dl)
        {
            // bug? 这里如果不读下 bash_profile，就读不出bash_profile的环境变量 
            string s = " cd ~/src; source ~/.bash_profile; " +
                "rm " + dl.OFlow + " " + dl.OFunc + " "+ dl.FCPP + " " + "../appcom/" + dl.SO + " 2 &> 1;";
            string make = " make -f ";
            string m_g;
            string m_p = " ORA_VER=10";

            switch(dl.SO)
            {
                case "libs_publicfunc.10.so":
                    m_g = "s_publicfunc.gcc";
                    break;
                default:
                    m_g = dl.Gcc;
                    break;
            }

            return s + make + m_g + m_p;
        }

        public bool Compile(Detail dl)
        {
            if (ssh.IsConnected == false)
            {
                InitSsh();
            }

            //  开启命令，发送编译指令
            string Make = MakeCmd(dl);
            log.WriteLog("发送编译命令： " + Make, LogLevel.Info);

            var cmd = ssh.RunCommand(Make);
            //  获取输出
            log.WriteLog(cmd.Result);
            if (cmd.ExitStatus != 0)
            {
                log.WriteLog(cmd.Error, LogLevel.Error);
                log.WriteLog("编译so报错，请参考输出日志！", LogLevel.Error);
                return false; 
            }
            else
            {
                log.WriteLog("编译so完成！", LogLevel.Info);
            }

            return true;
        }

        public string name { get; private set; }
        private string host;
        private int port;
        private string user;
        private string pass;

        public bool compile { get; set; }
        public bool restartAs { get; set; }
        public string localdir { get; set; }
        public string remotedir { get; set; }

        private SshClient ssh;  // ssh组件
        private SftpClient sftp; // sftp组件

        private OperLog log;
    }

    class ReSSHConns : ArrayList
    {
        public ReSSH this[string name]
        {
            get
            {
                foreach (ReSSH a in this)
                {
                    if (a.name == name)
                    {
                        return a;
                    }
                }

                return null;
            }
        }
    }

}

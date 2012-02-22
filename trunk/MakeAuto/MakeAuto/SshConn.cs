using System.Windows.Forms;
using System.Collections;

namespace MakeAuto
{
    class SshConn
    {
        public string name {get; set;}
        public string host { get; set; }
        public int port { get; set; }
        public string user { get; set; }
        public string pass { get; set; }

        public bool compile { get; set; }
        public bool restartAs { get; set; }
        public string localdir { get; set; }
        public string remotedir { get; set; }

        // 日志输出的文本框
        public TextBox rbLog { get; set; } // 日志输出

        public Chilkat.Ssh ssh;  // ssh组件
        private int sshchannel;  // ssh 频道号
        public Chilkat.SFtp sftp; // sftp组件
        //private int sftpchannel; // sftp 频道号

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
        public SshConn(string name, string host, int port, string user, string pass, bool compile = false, bool restartAs = false)
        {
            this.name = name;
            this.host = host;
            this.port = port;
            this.user = user;
            this.pass = pass;
            this.compile = compile;
            this.restartAs = restartAs;
            localdir = @"C:\src\";
            remotedir = @"/home/"+ user +"/src/";
            
            // 初始化 ssh 
            ssh = new Chilkat.Ssh();
            //  Wait a max of 5 seconds when reading responses..
            ssh.ConnectTimeoutMs = 10000;
            ssh.IdleTimeoutMs = 30000;
            ssh.EnableEvents = false;
            ssh.KeepSessionLog = false;

            // 初始化 sftp 传输
            sftp = new Chilkat.SFtp();
            // 启用回调事件
            sftp.EnableEvents = true;
            //  Set some timeouts, in milliseconds:
            sftp.ConnectTimeoutMs = 10000;
            sftp.IdleTimeoutMs = 30000;
            sftp.KeepSessionLog = false;
        }

        /// <summary>
        /// 对于没有连接的环境执行初始化，并打开一个channel和shell
        /// </summary>
        /// <param name="reconnect">是否强制重连，强制重连会先断开现有的环境</param>
        /// <returns>是否成功</returns>
        private bool InitSsh(bool reconnect=false)
        {
            // 如果连接已经存在，则直退出
            if (ssh.IsConnected)
            {
                return true;
            }

            //  Any string automatically begins a fully-functional 30-day trial.
            bool success;
            success = ssh.UnlockComponent("30-day trial");
            if (success != true)
            {
                MAConf.instance.WriteLog(ssh.LastErrorText, InfoType.FileLog);
                return false;
            }

            success = ssh.Connect(host, port);
            if (success != true)
            {
                MAConf.instance.WriteLog(ssh.LastErrorText, InfoType.FileLog);
                return false;
            }

            //  Authenticate using login/password:
            success = ssh.AuthenticatePw(user, pass);
            if (success != true)
            {
                MAConf.instance.WriteLog(ssh.LastErrorText, InfoType.FileLog);
                return false;
            }

            //  Open a session channel.  (It is possible to have multiple
            //  session channels open simultaneously.)
            int channelNum;
            channelNum = ssh.OpenSessionChannel();
            if (channelNum < 0)
            {
                MAConf.instance.WriteLog(ssh.LastErrorText, InfoType.FileLog);
                return false;
            }

            //  Some SSH servers require a pseudo-terminal
            //  If so, include the call to SendReqPty.  If not, then
            //  comment out the call to SendReqPty.
            //  Note: The 2nd argument of SendReqPty is the terminal type,
            //  which should be something like "xterm", "vt100", "dumb", etc.
            //  A "dumb" terminal is one that cannot process escape sequences.
            //  Smart terminals, such as "xterm", "vt100", etc. process
            //  escape sequences.  If you select a type of smart terminal,
            //  your application will receive these escape sequences
            //  included in the command's output.  Use "dumb" if you do not
            //  want to receive escape sequences.  (Assuming your SSH
            //  server recognizes "dumb" as a standard dumb terminal.)
            string termType;
            termType = "linux";
            int widthInChars;
            widthInChars = 120;
            int heightInChars;
            heightInChars = 40;
            //  Use 0 for pixWidth and pixHeight when the dimensions
            //  are set in number-of-chars.
            int pixWidth;
            pixWidth = 0;
            int pixHeight;
            pixHeight = 0;
            success = ssh.SendReqPty(channelNum, termType, widthInChars, heightInChars, pixWidth, pixHeight);
            if (success != true)
            {
                MAConf.instance.WriteLog(ssh.LastErrorText, InfoType.FileLog);
                return false;
            }

            //  Start a shell on the channel:
            success = ssh.SendReqShell(channelNum);
            if (success != true)
            {
                MAConf.instance.WriteLog(ssh.LastErrorText, InfoType.FileLog);
                return false;
            }

            // 此时，通道打开成功，保留ssh通道号
            sshchannel = channelNum;

            return true;
        }

        public bool CloseSsh()
        {
            bool success;
            int channelNum = sshchannel;
            //  Read whatever output may already be available on the
            //  SSH connection.  ChannelReadAndPoll returns the number of bytes
            //  that are available in the channel's internal buffer that
            //  are ready to be "picked up" by calling GetReceivedText
            //  or GetReceivedData.
            //  A return value of -1 indicates failure.
            //  A return value of -2 indicates a failure via timeout.

            //  The ChannelReadAndPoll method waits
            //  for data to arrive on the connection usingi the IdleTimeoutMs
            //  property setting.  Once the first data arrives, it continues
            //  reading but instead uses the pollTimeoutMs passed in the 2nd argument:
            //  A return value of -2 indicates a timeout where no data is received.
            int n;
            int pollTimeoutMs;
            pollTimeoutMs = 1000;
            n = ssh.ChannelReadAndPoll(channelNum, pollTimeoutMs);
            if (n < 0)
            {
                MAConf.instance.WriteLog(ssh.LastErrorText, InfoType.FileLog);
                return false;
            }

            //  Close the channel:
            success = ssh.ChannelSendClose(channelNum);
            if (success != true)
            {
                MAConf.instance.WriteLog(ssh.LastErrorText, InfoType.FileLog);
                return false;
            }

            //  Perhaps we did not receive all of the commands output.
            //  To make sure,  call ChannelReceiveToClose to accumulate any remaining
            //  output until the server's corresponding "channel close" is received.
            success = ssh.ChannelReceiveToClose(channelNum);
            if (success != true)
            {
                MAConf.instance.WriteLog(ssh.LastErrorText, InfoType.FileLog);
                return false;
            }

            //  获取输出
            string cmdOutput;
            cmdOutput = ssh.GetReceivedText(channelNum, "ansi");
            if (cmdOutput == null)
            {
                MAConf.instance.WriteLog(ssh.LastErrorText, InfoType.FileLog);
                return false;
            }

            //  Display the remote shell's command output:
            MAConf.instance.WriteLog(cmdOutput, InfoType.FileLog);

            //  Disconnect
            ssh.Disconnect();

            return true;
        }

        public bool InitSftp()
        {
            if (sftp.IsConnected)
            {
                return true;
            }
            
            //sftp = new Chilkat.SFtp();

            //  Any string automatically begins a fully-functional 30-day trial.
            bool success;
            success = sftp.UnlockComponent("Anything for 30-day trial");
            if (success != true)
            {
                MAConf.instance.WriteLog(sftp.LastErrorText, InfoType.FileLog);
                return false;
            }

            // 连接 SSH 服务器
            success = sftp.Connect(host, port);
            if (success != true)
            {
                MAConf.instance.WriteLog(sftp.LastErrorText, InfoType.FileLog);
                return false ;
            }

            //  授权验证，使用密码方式
            success = sftp.AuthenticatePw(user, pass);
            if (success != true)
            {
                MAConf.instance.WriteLog(sftp.LastErrorText, InfoType.FileLog);
                return false;
            }

            //  授权成功，初始化 sftp 连接
            success = sftp.InitializeSftp();
            if (success != true)
            {
                MAConf.instance.WriteLog(sftp.LastErrorText, InfoType.FileLog);
                return false;
            }

            //sftp.HeartbeatMs = 5000;
            //sftp.OnAbortCheck += new Chilkat.SFtp.AbortCheckEventHandler(sftp_OnAbortCheck);
            //sftp.OnPercentDone += new Chilkat.SFtp.PercentDoneEventHandler(sftp_OnPercentDone);

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
                MAConf.instance.WriteLog("初始化ssh连接失败", InfoType.Error);
                return false;
            }

            // 执行命令
            // 开启命令，发送重启AS指令
            int channelNum = sshchannel;
            bool success;
            success = ssh.ChannelSendString(channelNum, " gfas \r\n ", "ansi");
            if (success != true)
            {
                MAConf.instance.WriteLog(ssh.LastErrorText, InfoType.FileLog);
                return false;
            }

            //  Send an EOF.  This tells the server that no more data will
            //  be sent on this channel.  The channel remains open, and
            //  the SSH client may still receive output on this channel.
            success = ssh.ChannelSendEof(channelNum);
            if (success != true)
            {
                MAConf.instance.WriteLog(ssh.LastErrorText, InfoType.FileLog);
                return false;
            }

            int pollTimeoutMs = 1000;
            int n = ssh.ChannelReadAndPoll(channelNum, pollTimeoutMs);
            if (n < 0)
            {
                MAConf.instance.WriteLog(ssh.LastErrorText, InfoType.FileLog);
                return false;
            }

            //  获取输出
            string cmdOutput = ssh.GetReceivedText(channelNum, "ansi");
            if (cmdOutput == null)
            {
                MAConf.instance.WriteLog(ssh.LastErrorText, InfoType.FileLog);
                return false;
            }

            //  Display the remote shell's command output:
            MAConf.instance.WriteLog(cmdOutput);
            
            return true;
        }

        public bool UploadModule(Detail dl)
        {
            if (sftp.IsConnected == false)
            {
                if (InitSftp() == false)
                {
                    MAConf.instance.WriteLog("初始化sftp连接失败", InfoType.Error);
                    return false;
                }
            }
            
            bool success;

            //  使用文件名上传文件
            //string remoteFilePath;
            //remoteFilePath = "src/s_cbpoutsideflow.gcc";
            //string localFilePath;
            //localFilePath = "c:/src/s_cbpoutsideflow.gcc";

            #region 处理 .pc, .h, .cpp, .gc 文件
            MAConf.instance.WriteLog("上传文件 " + dl.Header, InfoType.Info);
            success = sftp.UploadFileByName(remotedir + dl.Header, localdir + dl.Header);
            if (success != true)
            {
                MAConf.instance.WriteLog(sftp.LastErrorText, InfoType.FileLog);
                return false;
            }

            MAConf.instance.WriteLog("上传文件 " + dl.Pc, InfoType.Info);
            success = sftp.UploadFileByName(remotedir + dl.Pc, localdir + dl.Pc);
            if (success != true)
            {
                MAConf.instance.WriteLog(sftp.LastErrorText, InfoType.FileLog);
                return false;
            }

            MAConf.instance.WriteLog("上传文件 " + dl.Cpp, InfoType.Info);
            success = sftp.UploadFileByName(remotedir + dl.Cpp, localdir + dl.Cpp);
            if (success != true)
            {
                MAConf.instance.WriteLog(sftp.LastErrorText, InfoType.FileLog);
                return false;
            }

            MAConf.instance.WriteLog("上传文件 " + dl.Gcc, InfoType.Info);
            success = sftp.UploadFileByName(remotedir + dl.Gcc, localdir + dl.Gcc);
            if (success != true)
            {
                MAConf.instance.WriteLog(sftp.LastErrorText, InfoType.FileLog);
                return false;
            }
            #endregion

            MAConf.instance.WriteLog("文件上传成功！");
            return true;
        }

        public bool DownloadModule(Detail dl)
        {
            if (sftp.IsConnected == false)
            {
                InitSftp();
            }

            MAConf.instance.WriteLog("下载文件 " + dl.SO, InfoType.Info);
            bool success = sftp.DownloadFileByName("/home/" + user + "/appcom/" + dl.SO,
                localdir + dl.SO);
            if (success != true)
            {
                MAConf.instance.WriteLog(sftp.LastErrorText, InfoType.FileLog);
                MAConf.instance.WriteLog("下载文件 " + dl.SO + "失败" , InfoType.Error);
            }
            return success;
        }

        public bool Compile(Detail dl)
        {
            if (ssh.IsConnected == false)
            {
                InitSsh();
            }

            int channelNum = sshchannel;

            //  开启命令，发送编译指令
            MAConf.instance.WriteLog("发送编译命令： make -f " + dl.Gcc , InfoType.Info);
            bool success = ssh.ChannelSendString(channelNum, " cd ~/src; " +
                "rm " + dl.OFlow + " " + dl.OFunc + "; " +
                "make -f " + dl.Gcc + "; echo \"Result:$?, OVER, $USER \" \n", "ansi");
            if (success != true)
            {
                MAConf.instance.WriteLog(ssh.LastErrorText, InfoType.FileLog);
                return false;
            }

            #region 这段代码会导致界面假死，使用下面这段，希望有所改善
            //s 这段代码会导致界面假死，使用下面这段，希望有所改善
            //  Read the response until we get the shell prompt (assuming it's successful)
            //  In my case, the shell prompt is: "root@ubuntu:/home/chilkat# "
            //  It will be different in your case.
            success = ssh.ChannelReceiveUntilMatch(channelNum, "OVER, " + user, "ansi", true);
            if (success != true)
            {
                //  Check the last-error information and the session log...
                MAConf.instance.WriteLog(ssh.LastErrorText);
                MAConf.instance.WriteLog(ssh.SessionLog, InfoType.FileLog);
                //  Check to see what was received.
                MAConf.instance.WriteLog(ssh.GetReceivedText(channelNum, "ansi"));
                return false;
            }

            //  获取输出
            string cmdOutput;
            cmdOutput = ssh.GetReceivedText(channelNum, "ansi");
            if (cmdOutput == null)
            {
                MAConf.instance.WriteLog(ssh.LastErrorText, InfoType.FileLog);
                return false;
            }
            
            // 编译失败，提示用户，编译成功，提示是否重启AS
            int Begin = cmdOutput.LastIndexOf("Result:");
            int End = cmdOutput.LastIndexOf(", OVER");
            int Result = int.Parse(cmdOutput.Substring(Begin + 7, End - Begin - 7));
            if (Result != 0)
            {
                //  Display the remote shell's command output:
                MAConf.instance.WriteLog(cmdOutput);
                MAConf.instance.WriteLog("编译so报错，请参考输出日志！", InfoType.Error);
                return false;
            }
            else
            {
                MAConf.instance.WriteLog(cmdOutput, InfoType.FileLog);
                MAConf.instance.WriteLog("编译so完成！", InfoType.Info);
                return true;
            }
            #endregion

            /*
            // 其实木有改善
            // Now check for incoming data from the SSH channel.
            int retval;
            string cmdOutput = string.Empty;
            string cmdLog = string.Empty;
            do
            {
                retval = ssh.ChannelPoll(channelNum, 10000);
                if (retval == -1)
                {
                    MAConf.instance.WriteLog(ssh.LastErrorText, InfoType.Error);
                    // Read so we can see the error before the console closes.
                    return false;
                }
                if (retval > 0)
                {
                    cmdOutput = ssh.GetReceivedText(channelNum, "ansi");
                    //MAConf.instance.WriteLog(cmdOutput, InfoType.Info);
                    cmdLog += cmdOutput;
                    System.Threading.Thread.Sleep(5000);  // 重绘界面
                }
                else
                {
                    // If data arrived, loop around and get more immediately.
                    // Otherwise wait 20ms.
                    System.Threading.Thread.Sleep(20);
                }
            } while (retval > 0);

            // 判断编译是否成功
            

            if (cmdOutput.IndexOf("0") >= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
             * */
        }        
    }

    class SshConns:  ArrayList
    {
        public SshConn this[string name]
        {
            get 
            {
                foreach (SshConn a in this)
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

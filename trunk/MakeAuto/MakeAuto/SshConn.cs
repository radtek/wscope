﻿using System.Windows.Forms;
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
        public TextBox txtLog { get; set; } // 日志输出

        private Chilkat.Ssh ssh;  // ssh组件
        private int sshchannel;  // ssh 频道号
        private Chilkat.SFtp sftp; // sftp组件
        private int sftpchannel; // sftp 频道号

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

            // 无连接时建立连接
            ssh = new Chilkat.Ssh();

            //  Any string automatically begins a fully-functional 30-day trial.
            bool success;
            success = ssh.UnlockComponent("30-day trial");
            if (success != true)
            {
                MessageBox.Show(ssh.LastErrorText);
                return false;
            }

            success = ssh.Connect(host, port);
            if (success != true)
            {
                MessageBox.Show(ssh.LastErrorText);
                return false;
            }

            //  Wait a max of 5 seconds when reading responses..
            ssh.IdleTimeoutMs = 5000;

            //  Authenticate using login/password:
            success = ssh.AuthenticatePw(user, pass);
            if (success != true)
            {
                MessageBox.Show(ssh.LastErrorText);
                return false;
            }

            //  Open a session channel.  (It is possible to have multiple
            //  session channels open simultaneously.)
            int channelNum;
            channelNum = ssh.OpenSessionChannel();
            if (channelNum < 0)
            {
                MessageBox.Show(ssh.LastErrorText);
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
                MessageBox.Show(ssh.LastErrorText);
                return false;
            }

            //  Start a shell on the channel:
            success = ssh.SendReqShell(channelNum);
            if (success != true)
            {
                MessageBox.Show(ssh.LastErrorText);
                return false;
            }

            // 此时，频道打开成功，保留ssh频道号
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
                MessageBox.Show(ssh.LastErrorText);
                return false;
            }

            //  Close the channel:
            success = ssh.ChannelSendClose(channelNum);
            if (success != true)
            {
                MessageBox.Show(ssh.LastErrorText);
                return false;
            }

            //  Perhaps we did not receive all of the commands output.
            //  To make sure,  call ChannelReceiveToClose to accumulate any remaining
            //  output until the server's corresponding "channel close" is received.
            success = ssh.ChannelReceiveToClose(channelNum);
            if (success != true)
            {
                MessageBox.Show(ssh.LastErrorText);
                return false;
            }

            //  获取输出
            string cmdOutput;
            cmdOutput = ssh.GetReceivedText(channelNum, "ansi");
            if (cmdOutput == null)
            {
                MessageBox.Show(ssh.LastErrorText);
                return false;
            }


            //  Display the remote shell's command output:
            // txtLog.Text += cmdOutput + "\r\n";

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
            
            sftp = new Chilkat.SFtp();

            //  Any string automatically begins a fully-functional 30-day trial.
            bool success;
            success = sftp.UnlockComponent("Anything for 30-day trial");
            if (success != true)
            {
                MessageBox.Show(sftp.LastErrorText);
                return false;
            }

            //  Set some timeouts, in milliseconds:
            sftp.ConnectTimeoutMs = 5000;
            sftp.IdleTimeoutMs = 10000;

            // 连接 SSH 服务器
            success = sftp.Connect(host, port);
            if (success != true)
            {
                MessageBox.Show(sftp.LastErrorText);
                return false ;
            }

            //  授权验证，使用密码方式
            success = sftp.AuthenticatePw(user, pass);
            if (success != true)
            {
                MessageBox.Show(sftp.LastErrorText);
                return false;
            }

            //  授权成功，初始化 sftp 连接
            success = sftp.InitializeSftp();
            if (success != true)
            {
                MessageBox.Show(sftp.LastErrorText);
                return false;
            }

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
            InitSsh();

            // 执行命令
            // 开启命令，发送重启AS指令
            int channelNum = sshchannel;
            bool success;
            success = ssh.ChannelSendString(channelNum, " gfas \r\n ", "ansi");
            if (success != true)
            {
                MessageBox.Show(ssh.LastErrorText);
                return false;
            }

            //  Send an EOF.  This tells the server that no more data will
            //  be sent on this channel.  The channel remains open, and
            //  the SSH client may still receive output on this channel.
            success = ssh.ChannelSendEof(channelNum);
            if (success != true)
            {
                MessageBox.Show(ssh.LastErrorText);
                return false;
            }

            return true;
        }

        public void UploadModule(Detail dl)
        {
            if (sftp.IsConnected == false)
            {
                InitSftp();
            }
            
            bool success;

            //  使用文件名上传文件
            //string remoteFilePath;
            //remoteFilePath = "src/s_cbpoutsideflow.gcc";
            //string localFilePath;
            //localFilePath = "c:/src/s_cbpoutsideflow.gcc";

            #region 处理 .pc, .h, .cpp, .gc 文件
            success = sftp.UploadFileByName(remotedir + dl.Header, localdir + dl.Header);
            if (success != true)
            {
                MessageBox.Show(sftp.LastErrorText);
                return;
            }

            success = sftp.UploadFileByName(remotedir + dl.Pc, localdir + dl.Pc);
            if (success != true)
            {
                MessageBox.Show(sftp.LastErrorText);
                return;
            }

            success = sftp.UploadFileByName(remotedir + dl.Cpp, localdir + dl.Cpp);
            if (success != true)
            {
                MessageBox.Show(sftp.LastErrorText);
                return;
            }

            success = sftp.UploadFileByName(remotedir + dl.Gcc, localdir + dl.Gcc);
            if (success != true)
            {
                MessageBox.Show(sftp.LastErrorText);
                return;
            }
            #endregion

            //MessageBox.Show("Success.");

        }

        public bool DownloadModule(Detail dl)
        {
            if (sftp.IsConnected == false)
            {
                InitSftp();
            }


            bool success = sftp.DownloadFileByName("/home/" + user + "/appcom/" + dl.SO,
                localdir + dl.SO);
            if (success != true)
            {
                MessageBox.Show(sftp.LastErrorText);
            }
            return success;
        }

        public void Compile(Detail dl)
        {
            if (ssh.IsConnected == false)
            {
                InitSsh();
            }

            int channelNum = sshchannel;

            //  开启命令，发送编译指令
            bool success = ssh.ChannelSendString(channelNum, " cd ~/src; " +
                "rm " + dl.OFlow + " " + dl.OFunc + "; " +
                "m1 " + dl.Gcc + " \r\n ", "ansi");
            if (success != true)
            {
                MessageBox.Show(ssh.LastErrorText);
                return;
            }

            //  Read the response until we get the shell prompt (assuming it's successful)
            //  In my case, the shell prompt is: "root@ubuntu:/home/chilkat# "
            //  It will be different in your case.
            success = ssh.ChannelReceiveUntilMatch(channelNum, "Compile so over!", "ansi", true);
            if (success != true)
            {
                //  Check the last-error information and the session log...
                txtLog.Text += ssh.LastErrorText + "\r\n";
                txtLog.Text += ssh.SessionLog + "\r\n";
                //  Check to see what was received.
                txtLog.Text += ssh.GetReceivedText(channelNum, "ansi") + "\r\n";
                return;
            }

            //  获取输出
            string cmdOutput;
            cmdOutput = ssh.GetReceivedText(channelNum, "ansi");
            if (cmdOutput == null)
            {
                MessageBox.Show(ssh.LastErrorText);
                return;
            }
            txtLog.Text += cmdOutput + "\r\n";

            // 编译失败，提示用户，编译成功，提示是否重启AS
            int iSucceed = cmdOutput.IndexOf("Succeed");
            if (iSucceed == -1)
            {
                MessageBox.Show("编译so报错，请参考输出日志！");
                //  Display the remote shell's command output:
                txtLog.Text += cmdOutput + "\r\n";
                return;
            }
   
            DialogResult result = MessageBox.Show("编译成功，重启 AS ?", "重启AS",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
            {
                
                //  开启命令，发送重启指令
                success = ssh.ChannelSendString(channelNum, " gfas \r\n ", "ansi");
                if (success != true)
                {
                    MessageBox.Show(ssh.LastErrorText);
                    return;
                }

                success = ssh.ChannelSendEof(channelNum);
                if (success != true)
                {
                    MessageBox.Show(ssh.LastErrorText);
                    return;
                }

                int n;
                int pollTimeoutMs;
                pollTimeoutMs = 1000;
                n = ssh.ChannelReadAndPoll(channelNum, pollTimeoutMs);
                if (n < 0)
                {
                    MessageBox.Show(ssh.LastErrorText);
                    return;
                }

                //  获取输出
                cmdOutput = ssh.GetReceivedText(channelNum, "ansi");
                if (cmdOutput == null)
                {
                    MessageBox.Show(ssh.LastErrorText);
                    return;
                }

                //  Display the remote shell's command output:
                txtLog.Text += cmdOutput + "\r\n";
            }


            // 日志通用记法
            //txtLog.Text += DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + "修改单递交准备完成，生成目录：" + CurrVer + "\r\n";
        
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
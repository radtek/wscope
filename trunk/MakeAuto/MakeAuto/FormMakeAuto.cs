using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Collections;
using System.Xml;
using System.Text;
using System.IO;

namespace MakeAuto
{
    public partial class frmMakeAuto : Form
    {
        // 获取系统连接配置
        public readonly string SSH_SERVER = ConfigHelper.GetConfig("SSHServer", "192.168.87.244");
        public readonly int SSH_PORT = int.Parse(ConfigHelper.GetConfig("SSHPort", "22"));
        public readonly string SSH_USER = ConfigHelper.GetConfig("SSHUser", "gftrade");
        public readonly string SSH_PASSWD = ConfigHelper.GetConfig("SSHPasswd", "handsome");
        // 定义本地目录和远程目录
        public readonly string EXCEL_DETAIL = ConfigHelper.GetConfig("ExcelDetail");
        public readonly string LOCAL_DIR = ConfigHelper.GetConfig("LocalDir");
        public readonly string REMOTE_DIR = ConfigHelper.GetConfig("RemoteDir");

        // 取xml 文件名称
        private readonly string detailfile = "detail.xml";


        // 定义保存 Excel 列表的东东
        ArrayList alModule = new ArrayList();

        // 宏助手
        private ExcelMacroHelper eh = new ExcelMacroHelper();

        // 当前活动ExcelFile;
        private ExcelFile currExcel;

        public frmMakeAuto()
        {
            InitializeComponent();
        }

        private void UploadFile(int FileNo)
        {
            #region 初始化文件类，获取当前编号文件的属性
            ExcelFile ef = (ExcelFile)alModule[FileNo - 1]; // 取当前文件
            #endregion

            #region 上传文件到服务器,调用Chilkat的sftp组件，这个需要调
            Chilkat.SFtp sftp = new Chilkat.SFtp();

            //  Any string automatically begins a fully-functional 30-day trial.
            bool success;
            success = sftp.UnlockComponent("Anything for 30-day trial");
            if (success != true)
            {
                MessageBox.Show(sftp.LastErrorText);
                return;
            }

            //  Set some timeouts, in milliseconds:
            sftp.ConnectTimeoutMs = 5000;
            sftp.IdleTimeoutMs = 10000;

            // 连接 SSH 服务器
            success = sftp.Connect(SSH_SERVER, SSH_PORT);
            if (success != true)
            {
                MessageBox.Show(sftp.LastErrorText);
                return;
            }

            //  授权验证，使用密码方式
            success = sftp.AuthenticatePw(SSH_USER, SSH_PASSWD);
            if (success != true)
            {
                MessageBox.Show(sftp.LastErrorText);
                return;
            }

            //  授权成功，初始化 sftp 连接
            success = sftp.InitializeSftp();
            if (success != true)
            {
                MessageBox.Show(sftp.LastErrorText);
                return;
            }

            //  使用文件名上传文件
            //string remoteFilePath;
            //remoteFilePath = "src/s_cbpoutsideflow.gcc";
            //string localFilePath;
            //localFilePath = "c:/src/s_cbpoutsideflow.gcc";

            #region 处理 .pc, .h, .cpp, .gc 文件
            success = sftp.UploadFileByName(REMOTE_DIR + ef.HeaderFile, LOCAL_DIR + ef.HeaderFile);
            if (success != true)
            {
                MessageBox.Show(sftp.LastErrorText);
                return;
            }

            success = sftp.UploadFileByName(REMOTE_DIR + ef.PcFile, LOCAL_DIR + ef.PcFile);
            if (success != true)
            {
                MessageBox.Show(sftp.LastErrorText);
                return;
            }

            success = sftp.UploadFileByName(REMOTE_DIR + ef.CppFile, LOCAL_DIR + ef.CppFile);
            if (success != true)
            {
                MessageBox.Show(sftp.LastErrorText);
                return;
            }

            success = sftp.UploadFileByName(REMOTE_DIR + ef.GccFile, LOCAL_DIR + ef.GccFile);
            if (success != true)
            {
                MessageBox.Show(sftp.LastErrorText);
                return;
            }
            #endregion

            //MessageBox.Show("Success.");

            #endregion
        }

        private void DownloadFile(ExcelFile currExcel, int Flag, string LocalDir)
        {
            #region 上传文件到服务器,调用Chilkat的sftp组件，这个需要调
            Chilkat.SFtp sftp = new Chilkat.SFtp();

            //  Any string automatically begins a fully-functional 30-day trial.
            bool success;
            success = sftp.UnlockComponent("Anything for 30-day trial");
            if (success != true)
            {
                MessageBox.Show(sftp.LastErrorText);
                return;
            }

            //  Set some timeouts, in milliseconds:
            sftp.ConnectTimeoutMs = 5000;
            sftp.IdleTimeoutMs = 10000;

            // 连接 SSH 服务器
            success = sftp.Connect(SSH_SERVER, SSH_PORT);
            if (success != true)
            {
                MessageBox.Show(sftp.LastErrorText);
                return;
            }

            //  授权验证，使用密码方式
            success = sftp.AuthenticatePw(SSH_USER, SSH_PASSWD);
            if (success != true)
            {
                MessageBox.Show(sftp.LastErrorText);
                return;
            }

            //  授权成功，初始化 sftp 连接
            success = sftp.InitializeSftp();
            if (success != true)
            {
                MessageBox.Show(sftp.LastErrorText);
                return;
            }

            //  使用文件名上传文件
            //string remoteFilePath;
            //remoteFilePath = "src/s_cbpoutsideflow.gcc";
            //string localFilePath;
            //localFilePath = "c:/src/s_cbpoutsideflow.gcc";

            #region 下载目标so文件
            if (Flag == 1)
            {
                success = sftp.DownloadFileByName("/home/" + SSH_USER + "/appcom/" + currExcel.SOFile, 
                    LocalDir + currExcel.SOFile);
                if (success != true)
                {
                    MessageBox.Show(sftp.LastErrorText);
                    return;
                }
            }
            #endregion

            //MessageBox.Show("Success.");

            #endregion
        }

        private void btnSO_Click(object sender, EventArgs e)
        {
            #region 初始化文件类，获取当前编号文件的属性
            int iFileNo = cmbBegin.SelectedIndex;
            ExcelFile ef = (ExcelFile)alModule[iFileNo]; // 取当前文件
            #endregion

            // 如果选择的序号pas文件字段为空，那么不需要编译SO
            if (ef.PasFile == " ")
            {
                MessageBox.Show("不是函数模块，不需要编译SO！");
                return;
            }

            #region 上传文件到服务器
            UploadFile(iFileNo);
            #endregion

            #region 编译文件，发送编译指令
            Chilkat.Ssh ssh = new Chilkat.Ssh();

            //  Any string automatically begins a fully-functional 30-day trial.
            bool success;
            success = ssh.UnlockComponent("30-day trial");
            if (success != true) {
                MessageBox.Show(ssh.LastErrorText);
                return;
            }

            success = ssh.Connect(SSH_SERVER, SSH_PORT);
            if (success != true) {
                MessageBox.Show(ssh.LastErrorText);
                return;
            }

            // 最多等待 10 秒钟，有些编译较慢，多等一会
            ssh.IdleTimeoutMs = 10000;

            //  Authenticate using login/password:
            success = ssh.AuthenticatePw(SSH_USER, SSH_PASSWD);
            if (success != true) {
                MessageBox.Show(ssh.LastErrorText);
                return;
            }

            //  Open a session channel.  (It is possible to have multiple
            //  session channels open simultaneously.)
            int channelNum;
            channelNum = ssh.OpenSessionChannel();
            if (channelNum < 0) {
                MessageBox.Show(ssh.LastErrorText);
                return;
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
            success = ssh.SendReqPty(channelNum,termType,widthInChars,heightInChars,pixWidth,pixHeight);
            if (success != true) {
                MessageBox.Show(ssh.LastErrorText);
                return;
            }

            //  Start a shell on the channel:
            success = ssh.SendReqShell(channelNum);
            if (success != true) {
                MessageBox.Show(ssh.LastErrorText);
                return;
            }

            //  开启命令，发送编译指令
            success = ssh.ChannelSendString(channelNum," cd ~/src; " + 
                "rm " + ef.OFlowFile + " " + ef.OFuncFile + "; " +
                "m1 " + ef.GccFile + " \r\n ", "ansi");
            if (success != true) {
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
            if (cmdOutput == null ) {
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
            }
            else
            {
                DialogResult result = MessageBox.Show("编译成功，重启 AS ?", "重启AS",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
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
            }

            //  We're done, so shut it down..

            //  Send an EOF.  This tells the server that no more data will
            //  be sent on this channel.  The channel remains open, and
            //  the SSH client may still receive output on this channel.
            success = ssh.ChannelSendEof(channelNum);
            if (success != true)
            {
                txtLog.Text += ssh.LastErrorText + "\r\n";
                txtLog.Text += ssh.SessionLog + "\r\n";
                return;
            }

            //  Close the channel:
            success = ssh.ChannelSendClose(channelNum);
            if (success != true)
            {
                txtLog.Text += ssh.LastErrorText + "\r\n";
                txtLog.Text += ssh.SessionLog + "\r\n";
                return;
            }

            //  Disconnect
            ssh.Disconnect();
            #endregion
        }

        private void btnAS_Click(object sender, EventArgs e)
        {
            RestartAS();
        }

        /// <summary>
        /// 重启AS
        /// </summary>
        /// <returns>是否重启成功</returns>
        public bool RestartAS()
        {
            #region 编译完成后，重启 AS
            
            // 无连接时建立连接
            Chilkat.Ssh ssh = new Chilkat.Ssh();
           
            //  Any string automatically begins a fully-functional 30-day trial.
            bool success;
            success = ssh.UnlockComponent("30-day trial");
            if (success != true)
            {
                MessageBox.Show(ssh.LastErrorText);
                return false;
            }

            success = ssh.Connect(SSH_SERVER, SSH_PORT);
            if (success != true)
            {
                MessageBox.Show(ssh.LastErrorText);
                return false;
            }

            //  Wait a max of 5 seconds when reading responses..
            ssh.IdleTimeoutMs = 5000;

            //  Authenticate using login/password:
            success = ssh.AuthenticatePw(SSH_USER, SSH_PASSWD);
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

            //  开启命令，发送重启AS指令
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
            txtLog.Text += cmdOutput + "\r\n";

            //  Disconnect
            ssh.Disconnect();
            #endregion

            return true;
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            // 判断详细设计说明书是否存在，不存在要求设置，退出程序
            if (!File.Exists(EXCEL_DETAIL))
            {
                //throw new FileNotFoundException("详细设计说明书不存在，请检查！", EXCEL_DETAIL);
                string sError = "无法加载详细设计说明书！";
                MessageBox.Show(sError);
                txtLog.Text += sError + "\r\n";
                //Application.Exit();
            }

            DateTime d = File.GetCreationTime(EXCEL_DETAIL);
            // 直接用时间比较，因为带了毫秒级数据，会有问题
            DateTime ExcelTime = new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second);

            // 判断 xml 文件是否存在，不存在则创建，存在则判断是否需要更新
            bool bRefresh = false;
            if (File.Exists(detailfile))
            {
                XmlTextReader reader = null;
                try
                {
                    // 装载 xml 文件
                    reader = new XmlTextReader(detailfile);

                    // 读取 detailtime 属性
                    reader.MoveToContent();
                    string XmlTime = reader.GetAttribute("detailtime");
                    if (DateTime.Compare(ExcelTime, DateTime.Parse(XmlTime)) > 0)
                    {
                        bRefresh = true;
                    }

                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            else
            {
                bRefresh = true;
            }

            // 需要刷新时，重建详细设计说明书文件
            if (bRefresh)
            {
                #region 读取读取 Excel,并加载到DataGridView 和 ExcelFile类中
                string strCon = " Provider = Microsoft.Jet.OLEDB.4.0; " +
                "Data Source =" + ConfigHelper.GetConfig("ExcelDetail") + ";" +
                "Extended Properties = 'Excel 8.0;HDR=No;IMEX=1' ";

                //textBox1.Text = strCon;

                OleDbConnection conn = new OleDbConnection(strCon);
                try
                {
                    conn.Open();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                //返回Excel的架构，包括各个sheet表的名称,类型，创建时间和修改时间等 
                DataTable dtSheetName = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "Table" });

                //包含excel中表名的字符串数组
                string[] sTables = new string[dtSheetName.Rows.Count];
                int k = 0;
                for (; k < dtSheetName.Rows.Count; k++)
                {
                    sTables[k] = dtSheetName.Rows[k]["TABLE_NAME"].ToString();
                    //textBox1.Text += k.ToString() + ";;;;" + sTables[k] + "\r\n";

                    if (sTables[k] == "模块定义$")  // 数起来是 5， 其实是 23
                        break;
                }

                string sql = "select * from [" + sTables[k] + "C10:F200]";
                OleDbDataAdapter myCommand = new OleDbDataAdapter(sql, strCon);
                DataSet ds = new DataSet();
                try
                {
                    myCommand.Fill(ds, "[" + sTables[k] + "]");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                // 完毕后关闭连接
                conn.Close();
                #endregion

                #region 保存模块信息到xml文件
                XmlTextWriter writer = new XmlTextWriter(detailfile, Encoding.Default);
                writer.Formatting = Formatting.Indented;

                // 写xml文件
                writer.WriteStartDocument();
                // 创建注释节点
                writer.WriteComment("excel详细设计说明书");

                // 创建根元素
                writer.WriteStartElement("detail");
                // 创建本地时间元素属性
                writer.WriteAttributeString("gentime", System.DateTime.Now.ToString());
                // 创建excel文件时间元素属性
                writer.WriteAttributeString("detailtime", File.GetCreationTime(EXCEL_DETAIL).ToString());

                // 获取数据到xml文件中，遍历 DataSet 的数据表，保存行列值
                foreach (DataTable table in ds.Tables)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        // 处理掉最后一行的空白和中间的空白行
                        if (row[0].ToString().Trim() == String.Empty)
                        {
                            continue;
                        }

                        // 创建节点
                        writer.WriteStartElement(row[0].ToString());
                        writer.WriteAttributeString("file", row[1].ToString());
                        //writer.WriteAttributeString("show", "false");
                        writer.WriteElementString("sqlfile", row[2].ToString().Trim());
                        writer.WriteElementString("pasfile", row[3].ToString().Trim());
                        writer.WriteEndElement();
                    }
                }

                // 关闭根节点元素和文件
                writer.WriteEndElement();
                writer.WriteEndDocument();

                // 保存文件
                writer.Flush();
                writer.Close();
                #endregion
            }

            #region 读取xml文件，重建 alModule
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(detailfile);
            XmlNode xn = xmldoc.SelectSingleNode("detail");
            XmlNodeList xnl = xn.ChildNodes;
            int iNo = 0;
            alModule.Clear(); // 清理下
            cmbBegin.Items.Clear();
            cmbEnd.Items.Clear();
            foreach (XmlNode x in xnl)
            {
                // 构建 Excel 列表文件
                ExcelFile ef = new ExcelFile(++iNo, x.Name, x.Attributes["file"].InnerText,
                    x.ChildNodes[0].InnerText, x.ChildNodes[1].InnerText);
                alModule.Add(ef);

                try
                {
                    if (x.Attributes["show"].InnerText == "true")
                    {
                        cmbBegin.Items.Add(ef.ModuleNo.ToString() + '-' + ef.ModuleName);
                        cmbEnd.Items.Add(ef.ModuleNo.ToString() + '-' + ef.ModuleName);
                    }
                }
                catch (Exception ex)
                {
                    // 没有就没有了，不怎么样
                    txtLog.Text += ef.ModuleName + "\r\n";
                }

            }
            #endregion

            // 设置 Excel 的一些属性
            eh.ExcelFilePath = EXCEL_DETAIL;
            // 显示 Excel
            eh.IsShowExcel = true;

            eh.SrcDir = LOCAL_DIR;
        }

        private void bgwProc_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            // This event handler is called when the background thread finishes.
            // This method runs on the main thread.
            if (e.Error != null)
                MessageBox.Show("Error: " + e.Error.Message);
            else if (e.Cancelled)
                txtLog.Text += "取消任务 \r\n";
            else
                txtLog.Text += "编译完成" + "\r\n";
        }

        private void bgwProc_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            // This method runs on the main thread.
            txtLog.Text += "进度：" + e.ProgressPercentage.ToString() +"%, "+ e.UserState.ToString() + " \r\n";
            if (e.ProgressPercentage == 0)
            {
                // 启动后处理到托盘
                if (this.WindowState == FormWindowState.Normal)
                {
                    this.WindowState = FormWindowState.Minimized;
                    this.Hide();
                    nfnMake.ShowBalloonTip(1, "提示", "后台处理已启动", ToolTipIcon.Info);
                }

            }
            else if (e.ProgressPercentage == 100)
            {
                // 编译完成恢复到桌面区
                if (this.WindowState == FormWindowState.Minimized)
                {
                    this.Show();
                    this.WindowState = FormWindowState.Normal;
                    this.Activate();
                    nfnMake.ShowBalloonTip(1, "提示", "后台处理已完成", ToolTipIcon.Info);
                }
 
            }
        }

        private void bgwProc_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            // This event handler is where the actual work is done.
            // This method runs on the background thread.

            // Get the BackgroundWorker object that raised this event.
            System.ComponentModel.BackgroundWorker worker;
            worker = (System.ComponentModel.BackgroundWorker)sender;

            // Get the Words object and call the main method.
            ExcelMacroHelper eh = (ExcelMacroHelper)e.Argument;
            eh.RunExcelMacro(worker, e);
        }

        // Load 时判断详细设计说明书是否有更新，有更新则重建
        private void frmMakeAuto_Load(object sender, EventArgs e)
        {
            btnRefresh_Click(sender, e);
            // 加载初始编译模块
            try
            {
                cmbBegin.SelectedIndex = int.Parse(ConfigHelper.GetConfig("InitBegin", "1")) - 1;
                cmbEnd.SelectedIndex = int.Parse(ConfigHelper.GetConfig("InitEnd", "1")) - 1;
            }
            catch (Exception ex)
            {
                cmbBegin.SelectedIndex = -1;
                cmbEnd.SelectedIndex = -1;
            }
            
            // 加载模块表 modlist.xml
            //LoadModList();
        }

        private void btnProc_Click(object sender, EventArgs e)
        {
            // 先校验模块中需要忽略的部分
            for (int iFileNo = cmbBegin.SelectedIndex; iFileNo <= cmbEnd.SelectedIndex; ++iFileNo)
            {
                #region 初始化文件类，获取当前编号文件的属性
                ExcelFile ef = (ExcelFile)alModule[iFileNo]; // 取当前文件
                #endregion

                // 如果选择的序号pas文件字段为空，那么不需要编译SO
                if (ef.PasFile.Trim() == String.Empty)
                {
                    txtLog.Text += ef.ModuleNo.ToString() + "-" + ef.ModuleName + "不是函数模块，不会为其编译Proc文件！ \r\n";
                    continue;
                }
            }

            try
            {
                // 启动异步后台工作
                // 获得一个ExcelMacroHelper对象
                eh.MacroName = "CreateAs3CodePub";
                eh.BeginNo = (alModule[cmbBegin.SelectedIndex] as ExcelFile).ModuleNo;
                eh.EndNo = (alModule[cmbEnd.SelectedIndex] as ExcelFile).ModuleNo;
                bgwProc.RunWorkerAsync(eh);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnHyper_Click(object sender, EventArgs e)
        {
            // This method runs on the main thread.
            // Initialize the object that the background worker calls.

            try
            {
                // 启动异步后台工作
                // 获得一个ExcelMacroHelper对象
                eh.MacroName = "DocHyberLinkPub";
                eh.BeginNo = (alModule[cmbBegin.SelectedIndex] as ExcelFile).ModuleNo;
                eh.EndNo = (alModule[cmbEnd.SelectedIndex] as ExcelFile).ModuleNo;
                bgwProc.RunWorkerAsync(eh);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cmbBegin_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 如果结束索引比开始的小，那么重置结束的索引值
            if (cmbEnd.SelectedIndex < cmbBegin.SelectedIndex)
            {
                cmbEnd.SelectedIndex = cmbBegin.SelectedIndex;
            }
        }

        private void cmbEnd_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 如果结束索引比开始的小，那么重置开始的索引值
            if (cmbEnd.SelectedIndex < cmbBegin.SelectedIndex)
            {
                cmbBegin.SelectedIndex = cmbEnd.SelectedIndex;
            }
        }

        private void txtLog_TextChanged(object sender, EventArgs e)
        {
            // 自动滚动到底部
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret(); 
        }

        private void frmMakeAuto_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                nfnMake.Visible = true;
            }
        }

        private void nfnMake_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // 鼠标双击恢复窗体
            if (this.WindowState == FormWindowState.Normal)
            {
                this.WindowState = FormWindowState.Minimized;
                this.Hide();
            }
            else if (this.WindowState == FormWindowState.Minimized)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.Activate();
            }
        }

        private void frmMakeAuto_FormClosed(object sender, FormClosedEventArgs e)
        {
            // 释放掉托盘的资源，不然关闭了还是会在通知区域显示
            this.nfnMake.Dispose();
        }

        private void mniAbout_Click(object sender, EventArgs e)
        {
            AboutBoxMakeAuto mk = new AboutBoxMakeAuto();
            mk.Show(this);
        }

        private void btnSql_Click(object sender, EventArgs e)
        {
            // 先校验模块中需要忽略的部分
            for (int iFileNo = cmbBegin.SelectedIndex; iFileNo <= cmbEnd.SelectedIndex; ++iFileNo)
            {
                #region 初始化文件类，获取当前编号文件的属性
                ExcelFile ef = (ExcelFile)alModule[iFileNo]; // 取当前文件
                #endregion

                // 如果选择的序号pas文件字段为空，那么不需要编译SO
                if (ef.SqlFile.Trim() == String.Empty)
                {
                    txtLog.Text += ef.ModuleNo.ToString() + "-" + ef.ModuleName + "不是过程模块，不会为其编译Sql文件！ \r\n";
                    continue;
                }
            }

            try
            {
                // 启动异步后台工作
                // 获得一个ExcelMacroHelper对象
                eh.MacroName = "CreateSQLCodePub";
                eh.BeginNo = (alModule[cmbBegin.SelectedIndex] as ExcelFile).ModuleNo;
                eh.EndNo = (alModule[cmbEnd.SelectedIndex] as ExcelFile).ModuleNo;
                bgwProc.RunWorkerAsync(eh);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnModPre_Click(object sender, EventArgs e)
        {
            if (txtAmendList.Text.Trim() == String.Empty)
            {
                MessageBox.Show("请输入修改单号！");
                txtAmendList.Focus();
            }

            // 基目录不存在，则创建基目录
            string ModBaseDir = ConfigHelper.GetConfig("ModBaseDir");
            if (!Directory.Exists(ModBaseDir))
            {
                Directory.CreateDirectory(ModBaseDir);
            }

            // 分解出递交模块的名称，创建递交目录 [修改单号-模块名]
            int start= cmbModule.Text.IndexOf('-');
            string ModDir = txtAmendList.Text + "-" + cmbModule.Text.Substring(start + 1);
            if (!Directory.Exists(ModBaseDir+ModDir))
            {
                Directory.CreateDirectory(ModBaseDir+ModDir);
            }

            // 创建递交子目录，遍历当前子目录，在当前已有的最大值上加1 [修改单号-模块名-作者-日期-Vn]
            // 检测一下是否存在第一次递交的信息
            int vr = 0;
            string dt = DateTime.Today.ToString("yyyyMMdd");
            DirectoryInfo Dir = new DirectoryInfo(ModBaseDir + ModDir);
            try
            {
                // 获取当前的版本信息
                foreach (DirectoryInfo d in Dir.GetDirectories()) //查找子目录
                {
                    int k = d.Name.IndexOf('V') > 0 ? d.Name.IndexOf('V') : d.Name.IndexOf('v') ;
                    if( k > 0)
                    {
                        // 取递交版本
                        vr = int.Parse(d.Name.Substring(k + 1)) > vr ? int.Parse(d.Name.Substring(k + 1)) : vr;
                        // 取修改日期
                        dt = d.Name.Substring(k - 9, 8);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            // 构建新版本目录
            string ModAuthor = ConfigHelper.GetConfig("ModAuthor", "test");
            string ModSubDir = ModDir + "-" + ModAuthor + "-" + dt + "-" + "V" +(vr +1).ToString();
            Dir.CreateSubdirectory(ModSubDir);

            // 定义当前目录
            string CurrVer = ModBaseDir + ModDir + "/" + ModSubDir + "/";

            // 复制文件
            for (int iFileNo = cmbBegin.SelectedIndex; iFileNo <= cmbEnd.SelectedIndex; ++iFileNo)
            {
                currExcel = (ExcelFile)alModule[iFileNo]; // 取当前文件
                
                // 复制编译源文件
                foreach(string s in currExcel.ProcFiles)
                {
                    if (File.Exists(LOCAL_DIR + s))
                    {
                        File.Copy(LOCAL_DIR + s, CurrVer + s, true);
                    }
                    else
                    {
                        txtLog.Text += "文件：" + LOCAL_DIR + s + "不存在，请确认！" + "\r\n";
                    }
                }

                // 复制 sql 文件
                if (currExcel.SqlFile != String.Empty)
                {
                    if (File.Exists(LOCAL_DIR + currExcel.SqlFile))
                    {
                        File.Copy(LOCAL_DIR + currExcel.SqlFile, CurrVer + currExcel.SqlFile, true);
                    }
                    else
                    {
                        txtLog.Text += "文件：" + LOCAL_DIR + currExcel.SqlFile + "不存在，请确认！" +"\r\n";
                    }
                }

                // 获取远程编译 so
                if (currExcel.SOFile != string.Empty)
                {
                    DownloadFile(currExcel, 1, CurrVer);
                }
            }

            // 日志通用记法
            txtLog.Text += DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + "修改单递交准备完成，生成目录：" + CurrVer + "\r\n";
        }

        private void txtAmendList_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 修改单不可过长，20110906036 
            if (txtAmendList.Text.Length > 11)
            {
                MessageBox.Show("修改单编号过长，请输入 11 位修改单号！");
                txtAmendList.Focus();
            }
        }
        
        private void cmbBegin_TextUpdate(object sender, EventArgs e)
        {
            /* 内存会有写问题，暂时不实现了
            txtLog.Text += "updae" + "\r\n";
            cmbBegin.AutoCompleteCustomSource.Clear();
            string ts = cmbBegin.Text.Trim();
            foreach (object s in cmbBegin.Items)
            {
                if ((s as string).IndexOf(ts) > -1)
                {
                    txtLog.Text += (s as string) + "\r\n";
                    cmbBegin.AutoCompleteCustomSource.Add(s as string);
                }
            }
             * */
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DateTime dtFile = File.GetCreationTime(EXCEL_DETAIL);
            DateTime dtFile2 = File.GetCreationTime(EXCEL_DETAIL);

            txtLog.Text  += DateTime.Compare(dtFile, dtFile2).ToString();
        }
    }
}

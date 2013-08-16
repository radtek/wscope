using System;
using System.Windows.Forms;
using System.Collections;
using System.Text;
using System.IO;
using System.Diagnostics;
using Renci.SshNet;
using SharpSvn;
using System.Collections.Generic;
using AutoUpdaterDotNET;
using System.Threading;

namespace MakeAuto
{
    public partial class frmMakeAuto : Form
    {
        // 定义保存 Excel 列表的东东
        

        // 日志实例
        private OperLog log = OperLog.instance;

        //Spell sp;

        AmendFlow secuflow;
        AmendPack ap;

        public frmMakeAuto()
        {
            //System.Windows.Forms.MessageBox.Show("窗体入口");
            InitializeComponent();

            //System.Windows.Forms.MessageBox.Show("注册日志");
            // 注册一个事件处理
            log.OnLogInfo += new LogInfoEventHandler(WriteLog);
        }

        public delegate void AppendTextCallback(object sender, LogInfoArgs e);

        private void WriteLog(object sender, LogInfoArgs e)
        {
            if (e.level < LogLevel.FormLog)
                return;
            
            if (rbLog.InvokeRequired)
            {
                AppendTextCallback d = new AppendTextCallback(WriteLog);
                rbLog.Invoke(d, new object[]{sender, e});
            }
            else
            {
                if (e.level == LogLevel.Error)
                {
                    rbLog.SelectionColor = System.Drawing.Color.Red;
                }
                else if (e.level == LogLevel.Warning)
                {
                    rbLog.SelectionColor = System.Drawing.Color.IndianRed;
                }

                if (e.level == LogLevel.SqlExe)
                {
                    rbLog.AppendText(e.info + Environment.NewLine);
                }
                else
                {
                    rbLog.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + e.title + e.info + Environment.NewLine);
                }
             }
        }

        private void btnSO_Click(object sender, EventArgs e)
        {
            /*
            foreach (Detail dl in MAConf.instance.Dls)
            {
                if (!dl.Compile)
                    continue;
                
                // 如果选择的序号pas文件字段为空，那么不需要编译SO
                if (dl.Pas == " ")
                {
                    MessageBox.Show("不是函数模块，不需要编译SO！");
                    return;
                }

                // 上传文件到服务器
                currSsh.UploadModule(dl);

                // 执行编译
                currSsh.Compile(dl);
            }
             * */
        }

        private void btnAS_Click(object sender, EventArgs e)
        {
            //currSsh.RestartAS();
        }
        
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            //MAConf.instance.RefreshDetailList();
            //MAConf.instance.LoadDetailList();
        }

        private void bgwProc_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            // This event handler is called when the background thread finishes.
            // This method runs on the main thread.
            if (e.Error != null)
                MessageBox.Show("Error: " + e.Error.Message);
            else if (e.Cancelled)
                log.WriteLog("取消任务", LogLevel.Info);
            else
                log.WriteLog("编译完成", LogLevel.Info);
        }

        private void bgwProc_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            // This method runs on the main thread.
            VBAState c = (VBAState)e.UserState;
            log.WriteLog("编译进度：" + e.ProgressPercentage.ToString() + "%, "
                + ",编译信息：" + c.info, LogLevel.Info);
            
            // 此处以为托盘化是便利的，但是调试期间不见得，所以先不要这样子处理
            if (e.ProgressPercentage == 0)
            {
                // 启动后处理到托盘
                if (this.WindowState == FormWindowState.Normal)
                {
                    this.WindowState = FormWindowState.Minimized;
                    this.Hide();
                    nfnMake.ShowBalloonTip(1000, "提示", "后台处理已启动", ToolTipIcon.Info);
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
                    nfnMake.ShowBalloonTip(1000, "提示", "后台处理已完成", ToolTipIcon.Info);
                }
 
            }
        }

        private void bgwProc_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            System.ComponentModel.BackgroundWorker worker;
            worker = (System.ComponentModel.BackgroundWorker)sender;

            //eh.RunExcelMacro(worker, e);
        }

        private void btnProc_Click(object sender, EventArgs e)
        {
            // 对于选中的模块，如果需要编译，则执行编译过程
            bgwProc.RunWorkerAsync(MacroType.ProC);
        }

        private void btnHyper_Click(object sender, EventArgs e)
        {
            bgwProc.RunWorkerAsync(MacroType.Hyper);
        }

        private void btnSql_Click(object sender, EventArgs e)
        {
            bgwProc.RunWorkerAsync(MacroType.SQL);
        }

        private void txtLog_TextChanged(object sender, EventArgs e)
        {
            // 自动滚动到底部
            rbLog.SelectionStart = rbLog.Text.Length;
            rbLog.ScrollToCaret(); 
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

            // 关闭远程连接
            foreach (BaseConf b in MAConf.instance.Configs)
            {
                b.Conn.CloseSsh();
                b.Conn.CloseSftp();
            }

            // 执行下文件
        }

        private void btnModPre_Click(object sender, EventArgs e)
        {
            /*
            if (txtAmendList.Text.Trim() == String.Empty)
            {
                MessageBox.Show("请输入修改单号！");
                txtAmendList.Focus();
            }

            // 基目录不存在，则创建基目录
            string ModBaseDir = MAConf.instance.BaseDir; ;
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
            string ModAuthor = MAConf.instance.Author;
            string ModSubDir = ModDir + "-" + ModAuthor + "-" + dt + "-" + "V" +(vr +1).ToString();
            Dir.CreateSubdirectory(ModSubDir);

            // 定义当前目录
            string CurrVer = ModBaseDir + ModDir + "/" + ModSubDir + "/";

            // 复制文件
            for (int iFileNo = 0; iFileNo <= 10; ++iFileNo)
            {
                currDetail = (Detail)alModule[iFileNo]; // 取当前文件

                // 复制编译源文件
                foreach (string s in currDetail.ProcFiles)
                {
                    if (File.Exists(MAConf.instance.SrcDir + s))
                    {
                        File.Copy(MAConf.instance.SrcDir + s, CurrVer + s, true);
                    }
                    else
                    {
                        log.WriteLog("文件：" + MAConf.instance.SrcDir + s + "不存在，请确认！", LogLevel.Info);
                    }
                }

                // 复制 sql 文件
                if (currDetail.Sql != String.Empty)
                {
                    if (File.Exists(MAConf.instance.SrcDir + currDetail.Sql))
                    {
                        File.Copy(MAConf.instance.SrcDir + currDetail.Sql, CurrVer + currDetail.Sql, true);
                    }
                    else
                    {
                        log.WriteLog("文件：" + MAConf.instance.SrcDir + currDetail.Sql + "不存在，请确认！", LogLevel.Error);
                    }
                }

                // 获取远程编译 so
                if (currDetail.SO != string.Empty)
                {
                    currSsh.DownloadModule(currDetail);
                }
            }

            // 日志通用记法
            log.WriteLog("修改单递交准备完成，生成目录：" + CurrVer, LogLevel.Info);
             * */
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

        private void frmMakeAuto_Load(object sender, EventArgs e)
        {
            //MAConf.instance.RefreshDetailList();
            //MAConf.instance.LoadDetailList();
            //foreach(Detail dl in MAConf.instance.Dls)
            //{
            //    clbModule.Items.Add(dl.Name);
            //}

            //sp = new Spell(MAConf.instance.Dls);

            //foreach(sp.Detaildic.key)
            //tbModule.AutoCompleteCustomSource += sp.Detaildic.Keys;

            /* AutoUpdater.Start function takes following Arguments
             * 1. url of the appcast xml file that specifies download url, changelog url, application Version and title
             * 2. If you want user to select remind later interval then set lateUserSelectRemindLater as true. If you select true third and fourth arguments will be ignored.
             * 3. reminderLaterTime is a remind later timespan value if user choose Remind Later.
             * 4. reminderLaterTimeFormat is a time format enum that specifies if you want to take remind later time span value as minutes, hours or days.
             * AutoUpdater.Start(string appcastURL, bool lateUserSelectRemindLater, int reminderLaterTime, int reminderLaterTimeFormat)
            */
            AutoUpdater.Start("http://192.168.185.89/makeauto/MA_AppCast.xml");
        }

        private void tbModule_TextChanged(object sender, EventArgs e)
        {
            /*
            clbModule.Items.Clear();

            int index;

            foreach (Detail dl in MAConf.instance.Dls)
            {
                // 如果是空，则显示所有，否则显示匹配项
                if (tbModule.Text == "" || dl.Name.IndexOf(tbModule.Text) >= 0)
                {
                    index = clbModule.Items.Add(dl.Name);
                    // 如果是选中，则默认显示选中
                    if (dl.Compile)
                    {
                        clbModule.SetItemChecked(index, true);
                    }
                }
            }
             * */
        }

        private void clbModule_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            /*
            // 根据点击事件更新模块选中状态
            string name = clbModule.Items[e.Index].ToString();
            Detail dl = MAConf.instance.Dls[name];
            if (dl != null)
            {
                dl.Compile = e.NewValue == CheckState.Checked ? true : false;
            }
             */
        }

        private void btnReadInfo_Click(object sender, EventArgs e)
        {
            log.WriteInfoLog("查询递交包路径，修改单编号：" + txbAmenNo.Text + "...");

            ap = new AmendPack(txbAmenNo.Text);
            if (ap.scmstatus == ScmStatus.Error)
                return;

            txbCommitPath.Text = ap.CommitPath;
            txtSubmitVer.Text = ap.SubmitVer.ToString();
            txtScmVer.Text = ap.ScmVer.ToString();

            secuflow = new AmendFlow(ap);
            btnFlow.Enabled = true;
            btnDel.Enabled = true;
        }

        private void rbLog_TextChanged(object sender, EventArgs e)
        {
            rbLog.ScrollToCaret();
        }

        // edtFTP的心跳包
        private void timer1_Tick(object sender, EventArgs e)
        {
            foreach (BaseConf b in MAConf.instance.Configs)
            {
                if (b.ftp.IsConnected == true)
                {
                    b.ftp.GetFileInfos(b.fc.ServerDir);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            btnFlow.Enabled = false;
            if (secuflow.Work() == false)
            {
                btnFlow.Enabled = true;
                MessageBox.Show("集成失败。");
            }
            else
            {
                MessageBox.Show("处理完成。");
            }
        }

        private void txbAmenNo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnReadInfo_Click(sender, e);

                if (btnFlow.Enabled == true)
                {
                    btnFlow.Focus();
                }
            }
        }

        private void btnDel_Click(object sender, EventArgs e)
        {
            //
            if (ap.SubmitVer == 1)
            {
                ap.DeletePack(0);
                log.WriteLog("删除成功！ " + ap.RemoteFile);
            }
            else
            {
                log.WriteErrorLog("非首次，手工吧！");
            }
        }

        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox ab = new AboutBox();
            ab.Show(this);
        }

        private void button9_Click(object sender, EventArgs e)
        {

        }
    }
}
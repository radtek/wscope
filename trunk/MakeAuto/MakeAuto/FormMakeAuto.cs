﻿using System;
using System.Windows.Forms;
using System.Collections;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace MakeAuto
{
    public partial class frmMakeAuto : Form
    {

        // 定义保存 Excel 列表的东东
        ArrayList alModule = new ArrayList();

        // 配置实例
        private MAConf mc = MAConf.instance;

        // 宏助手
        private ExcelMacroHelper eh = ExcelMacroHelper.instance;

        AmendPack ap = AmendPack.instance;

        // 当前活动ExcelFile;
        private Detail currDetail;

        // 当前活动编译服务器
        private SshConn currSsh;

        Spell sp;

        public frmMakeAuto()
        {
            InitializeComponent();
            currSsh = (SshConn)mc.Conns[0];

            // 注册一个事件处理
            mc.OnLogInfo += new LogInfoEventHandler(WriteLog);

            // 注册一个事件给 sftp
            foreach(SshConn s in MAConf.instance.Conns)
            {
                s.sftp.OnPercentDone += new Chilkat.SFtp.PercentDoneEventHandler(sftp_OnPercentDone);
            }
        }

        void sftp_OnPercentDone(object sender, Chilkat.PercentDoneEventArgs args)
        {
            rbLog.AppendText("..." + args.PercentDone.ToString() + "%");

            if (args.PercentDone == 100)
                rbLog.AppendText("\r\n");
        }

        private void WriteLog(string info, InfoType type = InfoType.Info)
        {
            if (type == InfoType.FileLog)
                return;

            if (type == InfoType.Error)
            {
                rbLog.SelectionColor = System.Drawing.Color.Red;
            }

            rbLog.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + info + "\r\n");
        }

        private void btnSO_Click(object sender, EventArgs e)
        {
            Detail dl = null; // 取当前文件

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

        private void btnAS_Click(object sender, EventArgs e)
        {
            currSsh.RestartAS();
        }
        
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            MAConf.instance.RefreshDetailList();
            MAConf.instance.LoadDetailList();
        }

        private void bgwProc_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            // This event handler is called when the background thread finishes.
            // This method runs on the main thread.
            if (e.Error != null)
                MessageBox.Show("Error: " + e.Error.Message);
            else if (e.Cancelled)
                WriteLog("取消任务", InfoType.Info);
            else
                WriteLog("编译完成", InfoType.Info);
        }

        private void bgwProc_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            // This method runs on the main thread.
            State c = (State)e.UserState;
            WriteLog("编译进度：" + e.ProgressPercentage.ToString() + "%, 当前模块：" + c.dl.Name
                + ",编译信息：" + c.info, InfoType.Info);
            
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

            eh.RunExcelMacro(worker, e);
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

            // 刷出日志
            MAConf.instance.FlushLog();

            // 关闭远程连接
            foreach (SshConn c in MAConf.instance.Conns)
            {
                c.CloseSsh();
                c.CloseSftp();
            }
        }

        private void mniAbout_Click(object sender, EventArgs e)
        {
            AboutBoxMakeAuto mk = new AboutBoxMakeAuto();
            mk.Show(this);
        }

        private void btnModPre_Click(object sender, EventArgs e)
        {
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
                        WriteLog("文件：" + MAConf.instance.SrcDir + s + "不存在，请确认！", InfoType.Info);
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
                        WriteLog("文件：" + MAConf.instance.SrcDir + currDetail.Sql + "不存在，请确认！", InfoType.Info);
                    }
                }

                // 获取远程编译 so
                if (currDetail.SO != string.Empty)
                {
                    currSsh.DownloadModule(currDetail);
                }
            }

            // 日志通用记法
            WriteLog("修改单递交准备完成，生成目录：" + CurrVer, InfoType.Info);
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

        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBoxMakeAuto mk = new AboutBoxMakeAuto();
            mk.Show(this);
        }

        private void frmMakeAuto_Load(object sender, EventArgs e)
        {
            MAConf.instance.RefreshDetailList();
            MAConf.instance.LoadDetailList();
            foreach(Detail dl in MAConf.instance.Dls)
            {
                clbModule.Items.Add(dl.Name);
            }

            sp = new Spell(MAConf.instance.Dls);
        }

        private void tbModule_TextChanged(object sender, EventArgs e)
        {
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
        }

        private void clbModule_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // 根据点击事件更新模块选中状态
            string name = clbModule.Items[e.Index].ToString();
            Detail dl = MAConf.instance.Dls[name];
            if (dl != null)
            {
                dl.Compile = e.NewValue == CheckState.Checked ? true : false;
            }
        }

        private void btnReadInfo_Click(object sender, EventArgs e)
        {
            mc.WriteLog("查询递交包路径，修改单编号：" + txbAmenNo.Text + "...");

            if (ap.QueryAmend(txbAmenNo.Text) == true)
            {
                txbMainNo.Text = ap.MainNo;
                txbCommitPath.Text = ap.CommitPath;
            }
            else
            {
                mc.WriteLog("查询递交路径错误。", InfoType.Error);
                return;
            }

            mc.WriteLog("查询FTP目录信息...");

            if (ap.QueryFTP() == true)
            {
                txtSubmitVer.Text = ap.SubmitVer.ToString();
                txtScmVer.Text = ap.ScmVer.ToString();
            }
            else 
            {
                mc.WriteLog("查询FTP目录信息错误。", InfoType.Error);
                return;
            }

            mc.WriteLog("查询FTP目录信息...完成");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            mc.WriteLog("下载递交包...");
            ap.DownloadPack();
            mc.WriteLog("解包处理...");
            ap.ProcessPack();
            mc.WriteLog("处理ReadMe...");
            ap.ProcessReadMe();
            mc.WriteLog("下载递交包...完成");
        }

        private void button12_Click(object sender, EventArgs e)
        {
            mc.WriteLog("获取VSS代码...");
            ap.GetCode();
            mc.WriteLog("获取VSS代码...完成");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            mc.WriteLog("执行编译...");
            ap.Compile();
            mc.WriteLog("编译...完成");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            mc.WriteLog("版本对比...");
            ap.ValidateVersion();
            mc.WriteLog("版本对比...完成");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // 打包
            // 先打 src --> src-V*
            mc.WriteLog("打包处理...");
            ap.DoPacker();
            mc.WriteLog("打包处理...完成");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            // test
        }

        private void rbLog_TextChanged(object sender, EventArgs e)
        {
            rbLog.ScrollToCaret();
        }

        // edtFTP的心跳包
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (mc.ftp.IsConnected == true)
            {
                mc.ftp.GetFileInfos(mc.fc.ServerDir);
            }
        }


    }
}

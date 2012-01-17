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

        // 定义保存 Excel 列表的东东
        ArrayList alModule = new ArrayList();

        // 宏助手
        private ExcelMacroHelper eh = new ExcelMacroHelper();

        // 当前活动ExcelFile;
        private Detail currDetail;

        // 当前活动编译服务器
        private SshConn currSsh;

        //

        public frmMakeAuto()
        {
            InitializeComponent();
        }

        private void btnSO_Click(object sender, EventArgs e)
        {
            int iFileNo = cmbBegin.SelectedIndex;
            Detail dl = (Detail)alModule[iFileNo]; // 取当前文件

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

        private void btnProc_Click(object sender, EventArgs e)
        {
            // 先校验模块中需要忽略的部分
            for (int iFileNo = cmbBegin.SelectedIndex; iFileNo <= cmbEnd.SelectedIndex; ++iFileNo)
            {
                Detail dl = (Detail)alModule[iFileNo]; // 取当前文件

                // 如果选择的序号pas文件字段为空，那么不需要编译SO
                if (dl.Pas.Trim() == String.Empty)
                {
                    txtLog.Text += dl.Name + "不是函数模块，不会为其编译Proc文件！ \r\n";
                    continue;
                }
            }

            try
            {
                // 启动异步后台工作
                // 获得一个ExcelMacroHelper对象
                eh.MacroName = "CreateAs3CodePub";
                //eh.BeginNo = (alModule[cmbBegin.SelectedIndex] as Detail).ModuleNo;
                //eh.EndNo = (alModule[cmbEnd.SelectedIndex] as Detail).ModuleNo;
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
                //eh.BeginNo = (alModule[cmbBegin.SelectedIndex] as Detail).ModuleNo;
                //eh.EndNo = (alModule[cmbEnd.SelectedIndex] as Detail).ModuleNo;
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
                Detail dl = (Detail)alModule[iFileNo]; // 取当前文件

                // 如果选择的序号pas文件字段为空，那么不需要编译SO
                if (dl.Sql.Trim() == String.Empty)
                {
                    txtLog.Text += dl.Name + "不是过程模块，不会为其编译Sql文件！ \r\n";
                    continue;
                }
            }

            try
            {
                // 启动异步后台工作
                // 获得一个ExcelMacroHelper对象
                eh.MacroName = "CreateSQLCodePub";
                //eh.BeginNo = (alModule[cmbBegin.SelectedIndex] as Detail).ModuleNo;
                //eh.EndNo = (alModule[cmbEnd.SelectedIndex] as Detail).ModuleNo;
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
            for (int iFileNo = cmbBegin.SelectedIndex; iFileNo <= cmbEnd.SelectedIndex; ++iFileNo)
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
                        txtLog.Text += "文件：" + MAConf.instance.SrcDir + s + "不存在，请确认！" + "\r\n";
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
                        txtLog.Text += "文件：" + MAConf.instance.SrcDir + currDetail.Sql + "不存在，请确认！" + "\r\n";
                    }
                }

                // 获取远程编译 so
                if (currDetail.SO != string.Empty)
                {
                    currSsh.DownloadModule(currDetail);
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

        private void button2_Click(object sender, EventArgs e)
        {
            MAConf m = MAConf.instance;
        }

        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBoxMakeAuto mk = new AboutBoxMakeAuto();
            mk.Show(this);
        }

        private void frmMakeAuto_Load(object sender, EventArgs e)
        {
            MAConf.instance.LoadDetailList();
            foreach(Detail dl in MAConf.instance.Dls)
            {
                clbModule.Items.Add(dl.Name);
            }
        }

    }
}

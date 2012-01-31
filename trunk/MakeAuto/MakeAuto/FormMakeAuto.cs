using System;
using System.Windows.Forms;
using System.Collections;
using System.Text;
using System.IO;
using EnterpriseDT.Net.Ftp;
using System.Diagnostics;
using Microsoft.International.Converters.PinYinConverter;
using SAWVSDKLib;

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

        //

        public frmMakeAuto()
        {
            InitializeComponent();
            mc.ErrorOut = txtLog;
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

        public void WriteLog(InfoType type, string LogContent, string Title = "")
        {
            mc.WriteLog(type, LogContent, Title);
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
                WriteLog(InfoType.Info, "取消任务");
            else
                WriteLog(InfoType.Info, "编译完成");
        }

        private void bgwProc_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            // This method runs on the main thread.
            State c = (State)e.UserState;
            WriteLog(InfoType.Info, "进度：" + e.ProgressPercentage.ToString() + "%, 当前模块：" + c.dl.Name
                + ",编译信息：" + c.info, "编译");
            
            /* 此处以为托盘化是便利的，但是调试期间不见得，所以先不要这样子处理
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
             * */
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
                        WriteLog(InfoType.Info, "文件：" + MAConf.instance.SrcDir + s + "不存在，请确认！", "集成");
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
                        WriteLog(InfoType.Info, "文件：" + MAConf.instance.SrcDir + currDetail.Sql + "不存在，请确认！", "集成");
                    }
                }

                // 获取远程编译 so
                if (currDetail.SO != string.Empty)
                {
                    currSsh.DownloadModule(currDetail);
                }
            }

            // 日志通用记法
            WriteLog(InfoType.Info, "修改单递交准备完成，生成目录：" + CurrVer, "集成");
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
            ArrayList myAL = new ArrayList();
            myAL.Add("集成-123");
            myAL.Add("123");

            myAL.Sort();
            foreach (Object obj in myAL)
               txtLog.Text += obj + "\r\n";
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



        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void btnReadInfo_Click(object sender, EventArgs e)
        {
            ap.QueryAmend(txbAmenNo.Text);
            txbMainNo.Text = ap.MainNo;
            txbCommitPath.Text = ap.CommitPath;

            foreach (CommitCom c in ap.ComComms)
            {
                txtLog.Text += c.cname + "\t" + c.cver + "\t" + Enum.GetName(typeof(ComType), c.ctype) + "\r\n";
            }

        }

        private void button6_Click(object sender, EventArgs e)
        {
            // 输出递交包，到本地集成环境处理，需要使用ftp连接
            FTPConnection ftp = mc.ftp;
            FtpConf fc = mc.fc;
            int myver, scmver;
            string s, lastscm;

            if(ftp.IsConnected == false)
            {
                ftp.Connect();
            }


            #region 取递交版本信息，确认要输出哪个版本的压缩包，确保只刷出最大的版本
            // 这个地方，应该是如果存在集成的文件夹，就刷出集成的文件夹，
            // 对于V1，是全部都需要集成，对于Vn(n>1)，只集成变动的部分就可以了
            if (ftp.DirectoryExists(ap.RemoteDir) == false)
            {
                MessageBox.Show("FTP路径" + fc.ServerDir + ap.CommitPath + "不存在！");
                return;
            }
            ftp.ChangeWorkingDirectory(fc.ServerDir);
            
            // 不使用 true 列举不出目录，只显示文件，很奇怪
            //string[] files = ftp.GetFiles(fc.ServerDir + ap.CommitPath, true); 
            string[] files = ftp.GetFiles(ap.RemoteDir);
            
            // 检查是否存在集成*的文件夹
            // 获取当前的版本信息，先标定版本信息
            ArrayList myAL = new ArrayList();
            ArrayList SCMAL = new ArrayList();
            foreach (string f in files) //查找子目录
            {
                // 跳过 src 之类的东东
                if (f.IndexOf(ap.MainNo) < 0)
                    continue;

                if (f.IndexOf("集成") == 0)
                    SCMAL.Add(f);
                else
                  myAL.Add(f);
            }

            if (myAL.Count > 0)
            {
                myAL.Sort();


                s = myAL[myAL.Count - 1].ToString();
                ap.currVerFile = s;

                // 取递交版本号 20111207012-委托管理-高虎-20120116-V13.rar --> 20111207012-委托管理-高虎-20120116-V13 -> 13
                s = s.Substring(0, s.LastIndexOf('.'));
                myver = int.Parse(s.Substring(s.LastIndexOf('V') + 1));
            }
            else myver = 0;

            if (SCMAL.Count > 0)
            {
                SCMAL.Sort();


                s = SCMAL[SCMAL.Count - 1].ToString();

                ap.SCMcurrVerFile = s;

                // 取递交版本号 20111207012-委托管理-高虎-20120116-V13.rar --> 20111207012-委托管理-高虎-20120116-V13 -> 13
                s = s.Substring(0, s.LastIndexOf('.'));
                scmver = int.Parse(s.Substring(s.LastIndexOf('V') + 1));

                
            }
            else scmver = 0;

            // 下载递交包
            if (!Directory.Exists(ap.LocalDir))
            {
                Directory.CreateDirectory(ap.LocalDir);
            }
            ftp.DownloadFile(ap.LocalFile, ap.RemoteFile);


            // 检查文件夹是否存在，如果存在，就先执行删除 
            // E:\xgd\融资融券\20111123054-国金短信\20111123054-国金短信-李景杰-20120117-V1
            if (Directory.Exists(ap.AmendDir))
            {
                Directory.Delete(ap.AmendDir, true);
            }

            // 检查集成文件夹是否存在，存在则执行删除
            // E:\xgd\融资融券\20111123054-国金短信\集成-20111123054-国金短信-李景杰-20120117-V1
            if (Directory.Exists(ap.SCMAmendDir))
            {
                Directory.Delete(ap.SCMAmendDir, true);
            }
            Directory.CreateDirectory(ap.SCMAmendDir);

            // 如果还没有集成过，那么执行集成
            if (scmver == 0)
            {
                // 下载压缩包之后，解压缩包，重命名文件夹为集成-*，
                ap.UnRar(ap.LocalFile, ap.SCMAmendDir);
            }
            else if (scmver == myver) // 如果相等，则是重新集成，不处理
            {
                
            }
            else if (scmver < myver) // 否则，递交了新的版本，刷新递交包，重新命名集成包
            {
                // 复制上一次集成文件夹为本次集成文件夹
                s = SCMAL[SCMAL.Count - 1].ToString();
                s = s.Substring(0, s.LastIndexOf('.'));
                
                lastscm = ap.LocalDir + Path.DirectorySeparatorChar + s;
                if (Directory.Exists(lastscm))
                {
                    DirectoryCopy(lastscm, ap.SCMAmendDir, false);
                }
                
                // 如果存在 src 压缩文件夹，解压缩 src 文件夹
                string srcrar = ap.SCMAmendDir + Path.DirectorySeparatorChar + "src-V" + scmver.ToString() + ".rar";
                if (File.Exists(srcrar))
                {
                    ap.UnRar(srcrar, ap.SCMAmendDir);
                    File.Delete(srcrar);
                }

                
                // 对于下载的递交文件，解压缩readme到集成文件夹，以便根据本次变动取出需要重新集成的文件
                ap.UnRar(ap.LocalFile, ap.SCMAmendDir, "Readme-" + ap.MainNo.ToString() + "*.txt");

                // 读取readme，重新集成
                try
                {
                    // Create an instance of StreamReader to read from a file.
                    // The using statement also closes the StreamReader.
                    using (StreamReader sr = new StreamReader("TestFile.txt"))
                    {
                        string line;
                        // Read and display lines from the file until the end of 
                        // the file is reached.
                        while ((line = sr.ReadLine()) != null)
                        {
                            Console.WriteLine(line);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Let the user know what went wrong.
                    Console.WriteLine("The file could not be read:");
                    Console.WriteLine(ex.Message);
                }
            }
            #endregion
        }

        private void button7_Click(object sender, EventArgs e)
        {
            // 解压缩代码没有问题，就可以执行编译过程
            // 此处跳过，先不执行

        }

        private void button8_Click(object sender, EventArgs e)
        {
            ap.ValidateVersion();
        }

        private static void DirectoryCopy(
        string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the source directory does not exist, throw an exception.
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory does not exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }


            // Get the file contents of the directory to copy.
            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files)
            {
                // Create the path to the new copy of the file.
                string temppath = Path.Combine(destDirName, file.Name);

                // Copy the file.
                file.CopyTo(temppath, false);
            }

            // If copySubDirs is true, copy the subdirectories.
            if (copySubDirs)
            {

                foreach (DirectoryInfo subdir in dirs)
                {
                    // Create the subdirectory.
                    string temppath = Path.Combine(destDirName, subdir.Name);

                    // Copy the subdirectories.
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            // 测试 SAW 功能
            SAWVSDK sv = new SAWVSDK();
            bool bc, btrial, bcanceled;
            int days, CResu;
            string Result;
            Enum_EncryptType eEncry;
            CResu = sv.ConnectToServer("cm02.hundsun.com", 8884, out bc,
                out eEncry, out btrial, out days,
                out bcanceled, out Result, Enum_ProxyType.Enum_NOPROXY,
                "", 0, "", "");
            if (CResu == 0)
            {
                txtLog.Text += "OK" + "\r\n";
                // 登录
                SAWVKeyInfoSet sk = new SAWVKeyInfoSet();
                CResu = sv.Login("gaohu", "Hundsun_2", "10106-证券一部-2006版", 
                    sk, out bc, out days, out bcanceled, out Result);

                if (CResu == 0)
                {
                    txtLog.Text += "OK1" + "\r\n";
                }
                else
                {
                    txtLog.Text += "False1" + Result + "\r\n";
                }

                // 获取文件历史
                SAWVProjectHistorySet hisset;
                CResu = sv.GetProjectHistory("$/HSTRADES11/Documents/D2.Designs/详细设计/后端/", true, false,
                    out hisset, "",
                    DateTime.Now.AddDays(-7), DateTime.Now, 
                    out bcanceled, out Result);

                txtLog.Text += hisset.Count.ToString() + "\r\n";
                foreach (SAWVProjectHistory his in hisset)
                {
                    txtLog.Text += his.Version.ToString() + "\t" + his.Action + "\t" + his.Comment + "\t" + his.DateAction.ToString()
                        + "\t" + his.UserName + "\t" + his.FileName + "\r\n"; 
                }

            }
            else
            {
                txtLog.Text += "False" + Result + "\r\n";
            }
        }
    }
}

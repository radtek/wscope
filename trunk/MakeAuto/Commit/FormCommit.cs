using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Diagnostics;

namespace MakeAuto
{
    public partial class FormCommit : Form
    {
        public FormCommit()
        {
            InitializeComponent();
        }

        public FormCommit(string[] args)
        {
            InitializeComponent();
            log.OnLogInfo += new LogInfoEventHandler(WriteLog);
            ch = new CommitHelper(args[0]);
            ch.GetStatus();
            RefreshStatus();
        }

        public delegate void AppendTextCallback(object sender, LogInfoArgs e);
        private void WriteLog(object sender, LogInfoArgs e)
        {
            if (e.level < LogLevel.FormLog)
                return;

            if (rbLog.InvokeRequired)
            {
                AppendTextCallback d = new AppendTextCallback(WriteLog);
                rbLog.Invoke(d, new object[] { sender, e });
            }
            else
            {
                if (e.level == LogLevel.Error)
                {
                    rbLog.SelectionColor = System.Drawing.Color.Red;
                }
                else if (e.level == LogLevel.Warning)
                {
                    rbLog.SelectionColor = System.Drawing.Color.DarkViolet;
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

        private void RefreshStatus()
        {
            string sname, tname;
            ListViewGroup lvg;
            ListViewItem lvItem;
            BaseConf pf = ch.pf;

            listView1.Items.Clear();

            foreach (KeyValuePair<String, Dictionary<string, Boolean>> element in ch.lpath)
            {

                sname = System.IO.Path.GetDirectoryName(element.Key);
                tname = sname.Replace(pf.WorkSpace, "");

                lvg = new System.Windows.Forms.ListViewGroup(tname + "  [" + ch.lver[element.Key] + "]",
                    System.Windows.Forms.HorizontalAlignment.Left);
                lvg.Tag = element.Key;
                listView1.Groups.Add(lvg);

                foreach (KeyValuePair<string, Boolean> le in element.Value)
                {
                    if (le.Value == false)
                        continue;

                    lvItem = new ListViewItem(le.Key.Replace(sname, ""));
                    lvItem.SubItems.Add(Enum.GetName(typeof(SharpSvn.SvnStatus), (ch.lstatus[element.Key])[le.Key]));
                    lvItem.Tag = le.Key;
                    lvItem.Checked = true;
                    lvItem.Group = lvg;
                    listView1.Items.Add(lvItem);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // 置选中状态
            foreach (ListViewGroup g in listView1.Groups)
            {
                foreach (ListViewItem l in g.Items)
                {
                    (ch.lpath[g.Tag as string])[l.Tag as string] = l.Checked;
                }
            }

            if (!ch.DoCommit())
            {
                MessageBox.Show("检入异常，请手工检查！");
            }
        }

        private void rbLog_TextChanged(object sender, EventArgs e)
        {
            rbLog.ScrollToCaret();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ch.GetStatus();
            RefreshStatus();
        }


        CommitHelper ch;
        OperLog log = OperLog.instance;

        private void 对比ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RegistryKey svnkey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\TortoiseSVN");
            string s = svnkey.GetValue("ProcPath").ToString();
            if (s == null)
            {
                log.WriteErrorLog("无svn安装记录");
                return;
            }

            string file = listView1.FocusedItem.Tag.ToString();
            Process p = new Process();
            try
            {
                p.StartInfo.FileName = s;           //  

                p.StartInfo.Arguments = " /command:diff /path:" + file;   // 设置执行参数 
                p.StartInfo.UseShellExecute = false;        // 关闭Shell的使用  
                p.StartInfo.RedirectStandardInput = false; // 不重定向标准输入，不知道为啥，重定向好像会一直等待
                p.StartInfo.RedirectStandardOutput = false;  //重定向标准出  
                p.StartInfo.RedirectStandardError = false; //重定向错误输出  
                p.StartInfo.CreateNoWindow = false;             // 不显示窗口

                MAConf.instance.WriteLog(p.StartInfo.FileName + " " + p.StartInfo.Arguments, LogLevel.FileLog);

                p.Start();    // 启动
                p.WaitForExit();
            }
            catch (Exception ex)
            {
                MAConf.instance.WriteLog("执行比较异常" + ex.Message, LogLevel.Error);
            }
        }
    }
    
}

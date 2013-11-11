using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            this.args = args;
            
            // 根据路径先分解出修改单
            //E:\xgd\20121030017-综业周边\20121030017-综业周边-高虎-20130705-V1
            CommitPath = args[0];
            //CommitPath = @"E:\xgd\20121030017-综业周边\20121030017-综业周边-高虎-20130705-V1";
            if (CommitPath[CommitPath.Length -1] == '\\')
                CommitPath = CommitPath.Substring(0, CommitPath.Length -1);

            int i = CommitPath.LastIndexOf("\\");
            AmendDir = CommitPath.Substring(i + 1);  // 20121030017-综业周边-高虎-20130705-V1
            i = AmendDir.IndexOf("-");
            AmendNo = AmendDir.Substring(0, i); // 20121030017
            i = AmendDir.IndexOf('-', i + 1);
            CommitDir = AmendDir.Substring(0, i);  // 20121030017-综业周边

            i = AmendDir.LastIndexOf("-V");
            Version = int.Parse(AmendDir.Substring(i + 2)); //1

            // Readme 文件名称
            Readme = "Readme-" + CommitDir + ".txt";
            ap = new AmendPack(AmendNo);
            ap.LocalDir = CommitPath;
            ap.SCMAmendDir = ap.LocalDir;
            if (Version == 1)
                ap.scmtype = ScmType.NewScm;
            else
                ap.scmtype = ScmType.BugScm;

            pr = new PackerReadMe();
            pr.ProcessComs(ap);
            pr.ProcessMods(ap);
            pr.ProcessSAWPath(ap);
            
            // 全部修改


            #region
            // 输出下处理结果
            foreach (CommitCom c in ap.ComComms)
            {
                rbLog.AppendText("名称：" + c.cname + " "
                    + "状态：" + Enum.GetName(typeof(ComStatus), c.cstatus) + " "
                    + "版本：" + c.cver + " "
                    + "路径：" + c.path);
                rbLog.AppendText(Environment.NewLine);
            }

            //log.WriteFileLog("[配置库文件]");
            foreach (SAWFile s in ap.SAWFiles)
            {
                rbLog.AppendText("路径：" + s.Path + " "
                    + "本地路径：" + s.LocalPath + " "
                    + "SvnUri：" + s.UriPath + " "
                    + "文件状态：" + Enum.GetName(typeof(FileStatus), s.fstatus));
                rbLog.AppendText(Environment.NewLine);
            }
            #endregion
        }

        private string[] args;
        AmendPack ap;
        PackerReadMe pr;
        BaseConf pf;
        string CommitPath;
        string AmendDir;
        string AmendNo;
        string CommitDir;
        int Version;
        string Readme;

        private void button1_Click(object sender, EventArgs e)
        {
            
            //BaseConf pf = MAConf.instance.Configs[ap.ProductId];

            System.Collections.ObjectModel.Collection<SharpSvn.SvnStatusEventArgs> ss;
            SharpSvn.SvnClient sclient = new SharpSvn.SvnClient();
            bool bFlag = false;
            string Name;
            foreach (CommitCom c in ap.ComComms)
            {
                // 对于无变动和要删除的，不需要再生成SAW库信息；对于小包，不需要生成 SAW库信息
                if (c.cstatus == ComStatus.NoChange || c.cstatus == ComStatus.Delete || c.ctype == ComType.Ssql)
                    continue;

                bFlag = false;
                Name = "[" + c.cname + "]";
                // 相同路径放到同一组
                foreach (ListViewItem l in listView1.Items)
                {
                    if (l.Tag.ToString() == c.sawfile.LocalPath)
                    {
                        l.Group.Header = l.Group.Header + " " + Name;
                        bFlag = true;
                        break;
                    }
                    
                }

                if (bFlag)
                    continue;

                ListViewGroup lvg = new System.Windows.Forms.ListViewGroup(Name, System.Windows.Forms.HorizontalAlignment.Left);

                try
                {
                    // RetrieveAllEntries 以显示所有，包括正常的版本
                    sclient.GetStatus(c.sawfile.LocalPath, out ss);
                    foreach (SharpSvn.SvnStatusEventArgs s in ss)
                    {
                        if (s.LocalContentStatus == SharpSvn.SvnStatus.NotVersioned)
                            continue;
                        if (s.LocalContentStatus == SharpSvn.SvnStatus.Normal)
                            continue;
                        

                        ListViewItem lvItem = new ListViewItem(System.IO.Path.GetFileName(s.Path));
                        lvItem.SubItems.Add(Enum.GetName(typeof(SharpSvn.SvnStatus), s.LocalContentStatus));
                        lvItem.Tag = c.sawfile.LocalPath;
                        lvItem.Checked = true;
                        lvItem.Group = lvg;
                        listView1.Items.Add(lvItem);

                    }
                }
                catch (Exception ex)
                {
                    MAConf.instance.WriteLog("获取状态失败" + ex.Message, LogLevel.Error);
                }

                listView1.Groups.Add(lvg);
           }
            
    }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
    
}

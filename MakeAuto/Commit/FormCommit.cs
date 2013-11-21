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
        List<string> ld;  // 递交文件（或文件夹）列表
        Dictionary<string, string> lver;  // 递交文件或者文件夹版本号
        Dictionary<string, Dictionary<string, Boolean>> lpath; // 每一个递交项对应的递交文件列表
        SharpSvn.SvnClient sclient;
        Dictionary<string, Boolean> tpath; // 临时变量

        private void button1_Click(object sender, EventArgs e)
        {
            
            pf = MAConf.instance.Configs[ap.ProductId];
            
            System.Collections.ObjectModel.Collection<SharpSvn.SvnStatusEventArgs> ss;
            sclient = new SharpSvn.SvnClient();
            ld = new List<string>();
            lver = new Dictionary<string,string>();
            lpath = new Dictionary<string, Dictionary<string, Boolean>>();

            foreach (CommitCom c in ap.ComComms)
            {
                // 对于无变动和要删除的，不需要再生成SAW库信息；对于小包，不需要生成 SAW库信息
                if (c.cstatus == ComStatus.NoChange || c.cstatus == ComStatus.Delete || c.ctype == ComType.Ssql)
                    continue;

                if (!ld.Contains(c.sawfile.LocalPath))
                {
                    ld.Add(c.sawfile.LocalPath);
                    lver.Add(c.sawfile.LocalPath, c.cver);
                }
            }

            foreach (string s in pf.CommitPublic)
            {
                if (!ld.Contains(s))
                {
                    ld.Add(System.IO.Path.Combine(pf.WorkSpace, s));
                    lver.Add(System.IO.Path.Combine(pf.WorkSpace, s), pf.logmessage);
                }
            }

            string sname;
            listView1.Items.Clear();
            ListViewGroup lvg;
            ListViewItem lvItem;
            foreach (string k in ld)
            {
                try
                {
                    // RetrieveAllEntries 参数可以显示所有，包括正常的版本
                    sclient.GetStatus(k, out ss);


                    if (ss.Count > 0)
                    {
                        if (System.IO.Path.HasExtension(k))
                        {
                            sname = System.IO.Path.GetFileName(k);
                        }
                        else
                        {
                            sname = k.Replace(pf.WorkSpace, string.Empty);
                        }

                        lvg = new System.Windows.Forms.ListViewGroup(sname,
                          System.Windows.Forms.HorizontalAlignment.Left);
                        lvg.Tag = k;

                        tpath = new Dictionary<string, Boolean>();
                        foreach (SharpSvn.SvnStatusEventArgs s in ss)
                        {
                            if (s.LocalContentStatus == SharpSvn.SvnStatus.NotVersioned)
                            {
                                rbLog.AppendText("[NotVersioned] " + s.Path + Environment.NewLine);
                                continue;
                            }
                            if (s.LocalContentStatus == SharpSvn.SvnStatus.Normal)
                                continue;

                            tpath.Add(s.Path, true);
                            lvItem = new ListViewItem(System.IO.Path.GetFileName(s.Path));
                            lvItem.SubItems.Add(Enum.GetName(typeof(SharpSvn.SvnStatus), s.LocalContentStatus));
                            lvItem.Tag = s.Path;
                            lvItem.Checked = true;
                            lvItem.Group = lvg;
                            listView1.Items.Add(lvItem);
                        }

                        // 都是 Not Versioned 不需处理
                        if (tpath.Count > 0)
                        {
                            lpath.Add(k, tpath);
                            listView1.Groups.Add(lvg);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MAConf.instance.WriteLog("获取状态失败" + ex.Message, LogLevel.Error);
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
                    (lpath[g.Tag as string])[l.Tag as string] = l.Checked;
                }
            }

            bool bRes;
            SharpSvn.SvnCommitArgs cargs = new SharpSvn.SvnCommitArgs();
            cargs.KeepLocks = true;
            SharpSvn.SvnCommitResult cres;
            List<string> paths = new List<string>();
            foreach (KeyValuePair<String, Dictionary<string, Boolean>> element in lpath)
            {                
                paths.Clear();
                cargs.LogMessage = AmendNo.ToString() +"-" + lver[element.Key] + "-V" + Version;
                rbLog.AppendText(cargs.LogMessage + " " + element.Key + Environment.NewLine);
                foreach (KeyValuePair<string, Boolean> le in element.Value)
                {
                    if (le.Value == false)
                        continue;

                    paths.Add(le.Key);
                    rbLog.AppendText(le.Key + Environment.NewLine);
                }
                try
                {
                    bRes = sclient.Commit(paths, cargs, out cres);
                    if (!bRes)
                    {
                        rbLog.AppendText("递交失败! " + cres.PostCommitError + Environment.NewLine);
                    }
                    else
                    {
                        rbLog.AppendText("递交成功! " + cres.Revision.ToString() + Environment.NewLine);
                    }
                }
                catch (Exception ex)
                {
                    MAConf.instance.WriteLog("获取状态失败" + ex.Message, LogLevel.Error);
                }
            }
        }
    }
    
}

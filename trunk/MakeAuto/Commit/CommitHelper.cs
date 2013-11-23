using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MakeAuto
{
    class CommitHelper
    {
        public CommitHelper(string LocalCommitPath)
        {
            string CommitPath = LocalCommitPath;
            log = OperLog.instance;

            // 根据路径先分解出修改单
            //E:\xgd\20121030017-综业周边\20121030017-综业周边-高虎-20130705-V1\
            log.WriteLog("递交路径：" + CommitPath);

            if (CommitPath[CommitPath.Length -1] == '\\')
                CommitPath = CommitPath.Substring(0, CommitPath.Length -1);

            int i = CommitPath.LastIndexOf("\\");
            string AmendDir = CommitPath.Substring(i + 1);  // 20121030017-综业周边-高虎-20130705-V1
            i = AmendDir.IndexOf("-");
            string AmendNo = AmendDir.Substring(0, i); // 20121030017
            i = AmendDir.IndexOf('-', i + 1);
            string CommitDir = AmendDir.Substring(0, i);  // 20121030017-综业周边

            i = AmendDir.LastIndexOf("-V");
            Version = int.Parse(AmendDir.Substring(i + 2)); //1

            // Readme 文件名称
            string Readme = "Readme-" + CommitDir + ".txt";
            
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
                log.WriteLog("名称：" + c.cname + " "
                    + "状态：" + Enum.GetName(typeof(ComStatus), c.cstatus) + " "
                    + "版本：" + c.cver + " "
                    + "路径：" + c.path);
            }

            //log.WriteFileLog("[配置库文件]");
            foreach (SAWFile s in ap.SAWFiles)
            {
                log.WriteLog("路径：" + s.Path + " "
                    + "本地路径：" + s.LocalPath + " "
                    + "SvnUri：" + s.UriPath + " "
                    + "文件状态：" + Enum.GetName(typeof(FileStatus), s.fstatus));
            }
            #endregion
        }

        public void GetStatus()
        {
            
            pf = MAConf.instance.Configs[ap.ProductId];

            System.Collections.ObjectModel.Collection<SharpSvn.SvnStatusEventArgs> ss;
            SharpSvn.SvnStatusArgs sarg = new SharpSvn.SvnStatusArgs();
            sclient = new SharpSvn.SvnClient();
            ld = new List<string>();
            lver = new Dictionary<string, string>();
            lpath = new Dictionary<string, Dictionary<string, bool>>();
            lstatus = new Dictionary<string, Dictionary<string, SharpSvn.SvnStatus>>();
            Dictionary<string, bool> tpath; // 临时变量
            Dictionary<string, SharpSvn.SvnStatus> tstatus; // 临时变量

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

            foreach (string k in ld)
            {
                try
                {
                    sarg.Depth = SharpSvn.SvnDepth.Infinity;
                    sclient.GetStatus(k, sarg, out ss);

                    if (ss.Count > 0)
                    {
                        tpath = new Dictionary<string, bool>();
                        tstatus = new Dictionary<string, SharpSvn.SvnStatus>();
                        foreach (SharpSvn.SvnStatusEventArgs s in ss)
                        {
                            if (s.LocalContentStatus == SharpSvn.SvnStatus.NotVersioned)
                            {
                                log.WriteLog("[NotVersioned] " + s.Path);
                                continue;
                            }
                            if (s.LocalContentStatus == SharpSvn.SvnStatus.Normal)
                                continue;

                            tpath.Add(s.Path, true);
                            tstatus.Add(s.Path, s.LocalContentStatus);
                        }

                        // 都是 Not Versioned 不需处理
                        if (tpath.Count > 0)
                        {
                            lpath.Add(k, tpath);
                            lstatus.Add(k, tstatus);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MAConf.instance.WriteLog("获取状态失败" + ex.Message, LogLevel.Error);
                }
            }
        }

        public bool DoCommit()
        {
            bool bRes = true;
            SharpSvn.SvnCommitArgs cargs = new SharpSvn.SvnCommitArgs();
            cargs.KeepLocks = true;
            SharpSvn.SvnCommitResult cres;
            List<string> paths = new List<string>();
            foreach (KeyValuePair<String, Dictionary<string, Boolean>> element in lpath)
            {
                paths.Clear();
                cargs.LogMessage = ap.AmendNo.ToString() + "-" + lver[element.Key] + "-V" + Version;
                log.WriteLog(cargs.LogMessage + " " + element.Key);
                foreach (KeyValuePair<string, Boolean> le in element.Value)
                {
                    if (le.Value == false)
                        continue;

                    paths.Add(le.Key);
                    log.WriteLog(le.Key);
                }

                if (paths.Count == 0)
                {
                    log.WriteErrorLog("无选中递交项！");
                    return true;
                }

                try
                {
                    bRes = sclient.Commit(paths, cargs, out cres);
                    if (!bRes)
                    {
                        log.WriteLog("递交失败! " + cres.PostCommitError);
                    }
                    else
                    {
                        log.WriteLog("递交成功! " + cres.Revision.ToString());
                    }
                }
                catch (Exception ex)
                {
                    MAConf.instance.WriteLog("获取状态失败" + ex.Message, LogLevel.Error);
                }
            }

            return bRes;
        }

        AmendPack ap;
        PackerReadMe pr;
        public BaseConf pf {get; private set; }

        int Version;
        List<string> ld;  // 递交文件（或文件夹）列表
        public Dictionary<string, string> lver {get; private set;}  // 递交文件或者文件夹版本号
        public Dictionary<string, Dictionary<string, bool>> lpath {get; private set;} // 每一个递交项对应的递交文件列表
        public Dictionary<string, Dictionary<string, SharpSvn.SvnStatus>> lstatus { get; private set;} // 每一个递交项对应的svn状态
        SharpSvn.SvnClient sclient;
        OperLog log;
    }
}

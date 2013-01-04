using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpSvn;
using System.Collections;

namespace MakeAuto
{
    /// <summary>
    /// 对应 SVN 的一些接口操作，参照 SAWV 完成
    /// </summary>
    class SvnVersion
    {
        public SvnVersion(string name, string server)
        {
            Name = name;
            Server = server;
            client = new SvnClient();
            log = OperLog.instance;
        }

        public Boolean GetAmendCode()
        {
            Boolean Result = false;
            log.WriteLog("获取修改单代码文件,修改单号:" + AmendNo + 
                " SVNURL：" + Uri + " 本地路径：" + Path);

            uritarget = new SvnUriTarget(Uri);
            pathtarget = new SvnPathTarget(Path);
            
            // Get Info
            log.WriteLog("取服务端信息......");
            SvnInfoEventArgs uriinfo;
            client.GetInfo(uritarget, out uriinfo);
            long endRevision = uriinfo.LastChangeRevision; // Revision代表整个版本库的当前版本，LastChangeRevision 表示这个文件最后的版本

            log.WriteLog("取本地文件信息......");
            long startRevision = 0;
            try
            {

                SvnInfoEventArgs pathinfo;
                client.GetInfo(pathtarget, out pathinfo);
                startRevision = pathinfo.LastChangeRevision;
            }
            catch (Exception)
            {
                startRevision = 0;
            }

            // Get Log
            System.Collections.ObjectModel.Collection<SvnLogEventArgs> logs;
            List<string> changelog = new List<string>();
            SvnLogArgs arg = new SvnLogArgs();
            // 时间正序，版本历史从小到大；时间反向，版本历史从大到小
            //arg.Range = new SvnRevisionRange(new SvnRevision(DateTime.Now.AddDays(-10)), new SvnRevision(DateTime.Now.AddDays(-20)));
            arg.Range = new SvnRevisionRange(endRevision, startRevision);

            log.WriteLog("取历史......");
            client.GetLog(uritarget.Uri, arg, out logs);

            SvnLogEventArgs l = null;
            foreach (SvnLogEventArgs g in logs)
            {
                // 20111215020-V6.1.4.10-V1
                if (g.LogMessage.Trim().IndexOf(AmendNo + "-" + Version) >= 0)
                {
                    l = g;
                    log.WriteLog("[版本信息]" + g.LogMessage.Trim() + "，时间：" + g.Time.ToString() 
                        + "，版本：" + g.Revision + "\r\n");
                    break;
                }
                
            }

            if (l == null)
                return false;

            // Svn Update
            log.WriteLog("更新文件......");
            SvnUpdateArgs uarg = new SvnUpdateArgs();
            uarg.UpdateParents = true;
            uarg.Revision = l.Revision;

            Result = client.Update(pathtarget.FullPath, uarg);

            return Result;
        }

        public Boolean GetAmendCode(string AmendNo, string Version)
        {
            return GetAmendCode();
        }

        public string Path { get; set; }
        public string Uri { get; set; }
        public string AmendNo { get; set; }
        public string Version { get; set; }
        public string Amend { get; set; }
        public string Name { get; set; }
        public string Server { get; set; }
        public string Workspace { get; set; }
        public OperLog log;

        private SvnClient client;
        private SvnUriTarget uritarget;
        private SvnPathTarget pathtarget;
    }

    class SvnList : ArrayList
    {
        public SvnVersion this[string name]
        {
            get
            {
                foreach (SvnVersion s in this)
                {
                    if (s.Name == name)
                        return s;
                }
                return null;
            }
        }

        // 根据递交特征来决定连接到哪个库，比如小球的目录， 顶级的是 广发版技术支持测试，
        // 那就匹配到了 “广发版技术支持测试|融资融券” 上
        public SvnVersion GetByAmend(string AmendSub)
        {
            foreach (SvnVersion s in this)
            {
                if (s.Amend.IndexOf(AmendSub) >= 0)
                    return s;
            }
            return null;
        }
    }
}
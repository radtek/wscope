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
    class SvnPort
    {
        public SvnPort(string name, string server)
        {
            Name = name;
            Server = server;
            // 要发布app.config，否则不能启用混合程序集
            client = new SvnClient();
            log = OperLog.instance;
        }

        public Boolean GetAmendCode()
        {
            Boolean Result = false;
            log.WriteFileLog("文件版本：" + Version + " SvnUri：" + Uri + " 本地路径：" + Path);

            uritarget = new SvnUriTarget(Uri);
            pathtarget = new SvnPathTarget(Path);

            // Get Info
            //log.WriteLog("取服务端信息......");
            long endRevision = 0;
            try
            {
                client.GetInfo(uritarget, out uriinfo);
                endRevision = uriinfo.LastChangeRevision; // Revision代表整个版本库的当前版本，LastChangeRevision 表示这个文件最后的版本
            }
            catch (Exception e)
            {
                log.WriteLog("取版本库信息异常: " + uritarget.Uri + " 错误信息：" + e.Message, LogLevel.Error);
                return false;
            }

            //log.WriteLog("取本地文件信息......");
            long startRevision = 0;
            try
            {
                client.GetInfo(pathtarget, out pathinfo);
                startRevision = pathinfo.LastChangeRevision;
            }
            catch (Exception e)
            {
                log.WriteLog("，取本地版本库信息异常:" + pathtarget.FileName + " 错误信息：" + e.Message, LogLevel.Error);
                //return false;
            }

            // 本地文件版本已经最新，不重新获取服务器版本
            if (startRevision >= endRevision)
            {
                log.WriteLog(pathtarget.FileName +
                    "，本地文件与服务器版本一致，不检查Svn服务器版本。" +
                    "Revision = " + startRevision.ToString());
                return true;
            }

            // Get Log
            System.Collections.ObjectModel.Collection<SvnLogEventArgs> logs;
            List<string> changelog = new List<string>();
            SvnLogArgs arg = new SvnLogArgs();
            // 时间正序，版本历史从小到大；时间反向，版本历史从大到小
            //arg.Range = new SvnRevisionRange(new SvnRevision(DateTime.Now.AddDays(-10)), new SvnRevision(DateTime.Now.AddDays(-20)));
            arg.Range = new SvnRevisionRange(endRevision, startRevision);

            //log.WriteLog("取历史......");
            client.GetLog(uritarget.Uri, arg, out logs);

            SvnLogEventArgs l = null;

            foreach (SvnLogEventArgs g in logs)
            {
                // 20111215020-V6.1.4.10-V1，考虑多个修改单递交，文件的修改单号可能不是主修改单号，从修改单列表中检索
                // 考虑融资融券和证券公用的修改单资源，按修改单号检查可能会有问题，按照版本直接检查
                if (g.LogMessage.IndexOf(Version) >= 0)
                {
                    l = g;
                    break;
                }
            }

            if (l == null)
            {
                log.WriteLog("[无法确认Svn版本信息]，" + pathtarget.FileName + "，endRevision = " + endRevision.ToString()
                    + "，startRevision " + startRevision.ToString());

                return false;
            }
            else if (l.Revision == startRevision)
            {
                log.WriteLog("本地文件版本满足，不再检出。Revision = " + l.Revision.ToString());
                return true;
            }
            else
            {
                log.WriteLog("[版本信息] " + pathtarget.FileName + "，" + l.LogMessage.Trim() + "，时间：" + l.Time.ToString()
                   + "，版本：" + l.Revision);
            }

            // Svn Update
            //log.WriteLog("更新文件......");
            SvnUpdateArgs uarg = new SvnUpdateArgs();
            // svn 1.7 使用 uarg.UpdateParents = true;
            uarg.Depth = SvnDepth.Infinity;
            uarg.Revision = l.Revision;

            Result = client.Update(pathtarget.FullPath, uarg);
            
            /* Result 更新不到也是true
            if (Result)
            {
                log.WriteLog("更新文件成功！");
            }
            else
            {
                log.WriteLog("更新文件失败！");
            }
             * */

            return Result;
        }

        public string Path { get; set; }
        public string Uri { get; set; }
        public string AmendNo { get; set; }
        public string AmendList;
        public string Version { get; set; }
        public string Amend { get; set; }
        public string Name { get; set; }
        public string Server { get; set; }
        public string Workspace { get; set; }
        public OperLog log;

        public SvnInfoEventArgs uriinfo;
        public SvnInfoEventArgs pathinfo;

        private SvnClient client;
        private SvnUriTarget uritarget;
        private SvnPathTarget pathtarget;
    }
}
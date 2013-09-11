using EnterpriseDT.Net.Ftp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace MakeAuto
{
    enum ComType
    {
        Nothing = 0,
        TablePatch = 1,
        Patch,
        Ssql, // 小包SQL
        MenuPatch, // 增值Menu
        Sql,
        FuncXml,
        SO,
        Exe,
        Dll,
        Ini,
        Xml,
        Excel,
    }

    enum ComStatus
    {
        NoChange = 0,
        Add = 1,
        Modify,
        Delete,
        Normal, // 处理完成后变成正常状态
    }

    enum ScmType
    {
        Nothing = 0,
        NewScm = 1, // 全新递交，仅对于V1和V1重复集成
        ReScm = 2, // 重新集成
        BugScm = 3, // 补丁修复递交，如V2, V3等
    }

    // 集成处理的状态，其实就是处理的流程和函数的调用顺序
    enum ScmStatus
    {
        Nothing = 0,
        ReadInfo = 1,
        DownLoadPack,
        ProcessPack,
        ReNewReadMe,
        ProcessComs,
        ProcessMods,
        PostComs,
        ProcessReadMe,
        ProcessSAWPath,
        GetFile,
        Compile,
        DiffFile,
        ReNewFile,
        TarPack,
        UpLoad,
        Over,
        Error,
    }

    // 递交程序项
    class CommitCom
    {
        // 程序项名称
        public string cname;
        // 版本
        public string cver;
        // 类型
        public ComType ctype {get; set;}
        
        // 对应 源代码路径，这个就是小球里的源代码路径
        public string path;

        // SAW文件信息
        public SAWFile sawfile;

        // 组件状态
        public ComStatus cstatus { get; set; }

        // Sql文件执行用户
        private const string U_PREFIX = "hs_";
        public List<string> users { get; private set; }

        public CommitCom(string name, string version, ComStatus status = ComStatus.NoChange)
        {
            cname = name;
            cver = version;
            cstatus = status;
            path = string.Empty;

            if (cname.IndexOf("libs") >= 0)
                ctype = ComType.SO;
            else if (cname.IndexOf("sql") >= 0)
            {
                users = new List<string>();

                if (cname.IndexOf("TablePatch") >= 0)
                {
                    ctype = ComType.TablePatch;
                    users.Add(U_PREFIX + cname.Substring(0, cname.IndexOf('_')));
                }
                else if (cname.IndexOf("Patch") >= 0)
                {
                    ctype = ComType.Patch;
                    users.Add(U_PREFIX + cname.Substring(0, cname.IndexOf('_'))); 
                }
                else if (cname.IndexOf("user_") >= 0 && cname.IndexOf("菜单功能") >= 0)
                {
                    ctype = ComType.MenuPatch;
                    users.Add(U_PREFIX + cname.Substring(0, cname.IndexOf('_')));
                }
                else
                {
                    // 用于识别临时脚本
                    Regex reg = new Regex(@"[\u4e00-\u9fa5]"); // 希望能识别出临时修改单，有时会失效；带有中文的则认为是临时单
                    if (reg.IsMatch(cname))
                    {
                        ctype = ComType.Ssql;
                    }
                    else
                    {
                        ctype = ComType.Sql;
                    }

                    if (ctype == ComType.Sql)
                    {
                        try
                        {
                            // 处理sql脚本用户 secu secusz busin or.sql 去除最后两项
                            int i = 0;
                            string u = cname.Substring(0, cname.LastIndexOf('_'));
                            u = u.Substring(0, u.LastIndexOf('_'));
                            while ((i = u.IndexOf('_')) >= 0)
                            {
                                users.Add(U_PREFIX + u.Substring(i + 1));
                                u = u.Substring(0, i);
                            }
                            users.Add(U_PREFIX + u);
                        }
                        catch (Exception ex)
                        {
                            OperLog.instance.WriteErrorLog("无法确认文件的用户：" + cname + ", Ex_Msg:" + ex.Message);
                        }
                    }
                }
            }
            else if (cname.IndexOf("exe") >= 0)
                ctype = ComType.Exe;
            else if (cname.IndexOf("dll") >= 0)
                ctype = ComType.Dll;
            else if (cname.IndexOf("ini") >= 0)
                ctype = ComType.Ini;
            else if (cname.IndexOf("xml") >= 0)
                ctype = ComType.Xml;
            else if (cname.IndexOf("xls") >= 0)
                ctype = ComType.Excel;
            else ctype = ComType.Nothing;
        }
    }

    // 递交项列表
    class ComList: ArrayList
    {
        public CommitCom this[string name]
        {
            get
            {
                foreach(CommitCom c in this)
                {
                    if(c.cname == name)
                        return c;
                }
                return null;
            }
        }
    }

    // 递交修改包
    class AmendPack
    {
        public AmendPack(string AmendNo)
        {
            this.AmendNo = AmendNo;
            TempModiFlag = false;

            log = OperLog.instance;

            ComComms = new ComList();
            // 创建连接
            sqlconn = new SqlConnection(ConnString);
            //为上面的连接指定Command对象
            sqlcomm = sqlconn.CreateCommand();

            SAWFiles = new SAWFileList();

            SubmitL = new ArrayList();
            ScmL = new ArrayList();
            ScmSrc = new ArrayList();

            StatusDict = new Dictionary<string, string>();
            StatusDict.Add("512", "递交集成");
            StatusDict.Add("504", "改完交审核"); 
            StatusDict.Add("505", "递交测试"); 
            StatusDict.Add("506", "分配测试");
            StatusDict.Add("507", "测试开始");
            StatusDict.Add("508", "测试通过");
            StatusDict.Add("509", "测试不通过");
            StatusDict.Add("510", "验收通过");

            ReworkList = new Dictionary<string, string>();
            ReworkStatus = new List<string>();
            ReworkStatus.Add("512"); // 递交集成
            //todo 发布时去除504
            ReworkStatus.Add("504"); // 递交测试
            ReworkStatus.Add("505"); // 递交测试
            ReworkStatus.Add("506"); // 分配测试
            ReworkStatus.Add("507"); // 测试开始
            ReworkStatus.Add("508"); // 测试通过
            ReworkStatus.Add("509"); // 测试不通过
            ReworkStatus.Add("510"); // 

            // 查询修改单信息
            if (QueryAmendInfo() == true)
            {
                // 生成修改单组件包信息
                //SetComs();
            }
            else
            {
                scmstatus = ScmStatus.Error;
                log.WriteErrorLog("查询修改单信息失败！");
                return;
            }

            if (QueryFTP() == false)
            {
                scmstatus = ScmStatus.Error;
                log.WriteErrorLog("查询FTP目录信息错误。");
                return;
            }

            if (CheckAmendStatus() == false)
            {
                scmstatus = ScmStatus.Error;
                foreach (KeyValuePair<string, string> kvp in ReworkList)
                {
                    if (!ReworkStatus.Contains(kvp.Value))
                    {
                        string sta;
                        if (!StatusDict.TryGetValue(kvp.Value, out sta))
                        {
                            sta = "未知状态";
                        }

                        log.WriteLog("修改单号：" + kvp.Key + "，修改单状态：" + 
                            kvp.Value + "-"  + sta, 
                            LogLevel.Warning);
                        break;
                    }
                }
                return;
            }
        }

        // 根据提供的修改单查询主单号
        private Boolean QueryAmendInfo()
        {
            Boolean result = false;
            // 打开连接
            try
            {
                if (sqlconn.State == ConnectionState.Closed)
                {
                    sqlconn.Open();
                }

                // 指定查询项 a.reference_stuff as 递交程序项, a.program_path_a as 递交路径, product_id as 产品
                sqlcomm.CommandText = ""
                  + " select a.reference_stuff, a.program_path_a, a.product_id "
                  + " from manage.dbo.programreworking2 a "
                  + " where reworking_id = '" + AmendNo + "' ";
                //为指定的command对象执行DataReader;
                sqldr = sqlcomm.ExecuteReader();

                // 如果有数据，读取数据
                while (sqldr.Read())
                {
                    // 获取数据
                    ComString = sqldr["reference_stuff"].ToString();  // 不 Trim() 以保留最后一个换行给 SetComs 判断用
                    CommitPath = sqldr["program_path_a"].ToString();
                    ProductId = sqldr["product_id"].ToString();
                }
                  
                // 从CommitPath中分解递交项和主单号 /融资融券/20111123054-国金短信，分解得到 20111123054
                MainNo = CommitPath.Substring(CommitPath.LastIndexOf("/") + 1, 11);
                // 得到递交模块，融资融券 广发版技术支持测试
                int i = CommitPath.LastIndexOf("/");
                AmendSubject = CommitPath.Substring(1,  i - 1);  // 融资融券
                CommitDir = CommitPath.Substring(i + 1);  // 20111123054-国金短短信

                // Readme 文件名称
                Readme = "Readme-" + CommitDir + ".txt";

                // 标定路径
                RemoteDir = MAConf.instance.Configs[ProductId].fc.ServerDir + CommitPath;
                LocalDir = MAConf.instance.Configs[ProductId].fc.LocalDir;

                result = true;
            }
            catch (Exception e)
            {
                result = false;
                log.WriteLog("查询修改单递交路径失败，" + e.Message, LogLevel.Error);
            }
            finally
            {
                if (sqldr != null)
                {
                    sqldr.Close();
                }

                if (sqlconn != null)
                {
                    sqlconn.Close();
                }
            }

            return result;
        }

        private Boolean CheckAmendStatus()
        {
            Boolean result = true;
            // 打开连接
            try
            {
                if (sqlconn.State == ConnectionState.Closed)
                {
                    sqlconn.Open();
                }

                // 指定查询项 a.reference_stuff as 递交程序项, a.program_path_a as 递交路径, product_id as 产品
                sqlcomm.CommandText = ""
                  + " select a.reworking_id, a.reworking_status "
                  + " from manage.dbo.programreworking2 a "
                  + " where a.program_path_a = '" + CommitPath + "' ";
                //为指定的command对象执行DataReader;
                sqldr = sqlcomm.ExecuteReader();

                // 如果有数据，读取数据
                while (sqldr.Read())
                {
                    // 获取数据  修改单号， 修改单状态
                    ReworkList.Add(sqldr["reworking_id"].ToString(), sqldr["reworking_status"].ToString());
                }

                foreach (KeyValuePair<string, string> kvp in ReworkList)
                {
                    if (!ReworkStatus.Contains(kvp.Value)) 
                    {
                        result = false;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                result = false;
                log.WriteLog("查询修改单递交路径失败，" + e.Message, LogLevel.Error);
            }
            finally
            {
                if (sqldr != null)
                {
                    sqldr.Close();
                }

                if (sqlconn != null)
                {
                    sqlconn.Close();
                }
            }

            return result;
        }

        /// <summary>
        /// 设置递交组件，作废，由处理ReadMe时读取Coms操作，数据库记录不可靠
        /// </summary>
        private void SetComs()
        {
            ComComms.Clear();
            // 查询出的组件是如下的一段
            // config.ini  [V6.1.4.7]  GJShortMessage.dll  [V6.1.4.1]  HsNoticeSvr.exe  [V6.1.4.6] 
            // 需要进行分解，操作如下
            string name, version, line, cs = ComString;

            int i = -1, s = 0, e = 0;
            while ((i = cs.IndexOf("\r\n")) > 0)
            {
                line = cs.Substring(0, i);
                s = line.IndexOf("["); // 取第一个版本分隔符号
                name = line.Substring(0, s).Trim(); // 程序名称

                s = line.LastIndexOf("[");
                e = line.LastIndexOf("]"); // 取版本分隔符号
                version = line.Substring(s + 1, e - s - 1);  // 程序版本
                CommitCom c = new CommitCom(name, version);
                // 添加组件
                ComComms.Add(c);

                // 取剩余递交项
                cs = cs.Substring(i + 2);
            }
        }

        // 获取FTP递交信息
        public Boolean QueryFTP()
        {
            log.WriteInfoLog("查询FTP目录信息...");
            // 输出递交包，到本地集成环境处理，需要使用ftp连接
            FTPConnection ftp = MAConf.instance.Configs[ProductId].ftp;
            FtpConf fc = MAConf.instance.Configs[ProductId].fc;
            string s;

            // 强制重新连接，防止时间长了就断掉了
            if (ftp.IsConnected == false)
            {
                try
                {
                    ftp.Connect();
                }
                catch(Exception e)
                {
                    log.WriteErrorLog("连接FTP服务器失败，错误信息：" + e.Message);
                }
            }

            // 取递交版本信息，确认要输出哪个版本的压缩包，确保只刷出最大的版本
            // 这个地方，应该是如果存在集成的文件夹，就刷出集成的文件夹，
            // 对于V1，是全部都需要集成，对于Vn(n>1)，只集成变动的部分就可以了
            if (ftp.DirectoryExists(RemoteDir) == false)
            {
                System.Windows.Forms.MessageBox.Show("FTP路径" + fc.ServerDir + CommitPath + "不存在！");
                return false;
            }
            ftp.ChangeWorkingDirectory(fc.ServerDir);

            // 不使用 true 列举不出目录，只显示文件，很奇怪
            //string[] files = ftp.GetFiles(fc.ServerDir + ap.CommitPath, true); 
            string[] files = ftp.GetFiles(RemoteDir);

            log.WriteInfoLog("查询FTP目录信息...完成");

            #region 确定修改单基本属性
            // 检查是否存在集成*的文件夹
            // 获取当前的版本信息，先标定版本信息
            SubmitL.Clear();
            ScmL.Clear();
            ScmSrc.Clear();
            foreach (string f in files) //查找子目录
            {
                // 跳过 src-V*.rar 之类的东东
                if (f.IndexOf("集成-src-V") >= 0 || f.IndexOf("集成-Src-V") >= 0)
                    ScmSrc.Add(f);
                else if (f.IndexOf(MainNo) < 0)
                    continue;
                else if (f.IndexOf("集成") == 0)
                    ScmL.Add(f);
                else
                    SubmitL.Add(f);
            }
            
            string currVerFile = string.Empty;// 20111123054-国金短信-李景杰-20120117-V3.rar
            if (SubmitL.Count > 0)
            {
                SubmitL.Sort();
                s = SubmitL[SubmitL.Count - 1].ToString();
                currVerFile = s;
                // 取递交版本号 
                // 20111207012-委托管理-高虎-20120116-V13.rar --> 20111207012-委托管理-高虎-20120116-V13 -> 13
                s = s.Substring(0, s.LastIndexOf('.'));
                SubmitVer = int.Parse(s.Substring(s.LastIndexOf('V') + 1));
            }
            else
            {
                SubmitVer = 0;
            }

            // 生成一些上次集成的变量，需要把上次的覆盖到本次来
            if (ScmL.Count > 1)  // 重复集成
            {
                ScmL.Sort();
                s = ScmL[ScmL.Count - 1].ToString();
                // 取递交版本号 
                // 20111207012-委托管理-高虎-20120116-V13.rar --> 20111207012-委托管理-高虎-20120116-V13 -> 13
                s = s.Substring(0, s.LastIndexOf('.'));
                ScmVer = int.Parse(s.Substring(s.LastIndexOf('V') + 1));
                ScmedVer = ScmVer;
                // 如果存在对应的src文件夹，一并删除掉
                if (ScmVer == SubmitVer)
                {
                    ScmL.RemoveAt(ScmL.Count -1 );
                    
                    if (ScmSrc.Count > 1)
                    {
                        ScmSrc.Sort();
                        if (ScmSrc.IndexOf("集成-src-V" + ScmVer+".rar") >= 0 || 
                            ScmSrc.IndexOf("集成-Src-V" + ScmVer+".rar") >= 0)
                        {
                            ScmSrc.RemoveAt(ScmSrc.Count - 1);
                        }
                    }
                }
            }


            string dir = Path.GetFileNameWithoutExtension(currVerFile);

            //AmendDir = LocalDir + "\\" + dir;
            SCMAmendDir = Path.Combine(LocalDir, "集成-" + dir);

            ScmVer = SubmitVer;
            LocalFile = Path.Combine(LocalDir, currVerFile);
            RemoteFile = Path.Combine(RemoteDir, currVerFile);

            SCMLocalFile = Path.Combine(SCMAmendDir, "集成-" + currVerFile);
            SCMRemoteFile = Path.Combine(RemoteDir, "集成-" +currVerFile);

            SrcRar = Path.Combine(SCMAmendDir, "src-V" + ScmVer.ToString() + ".rar");
            SCMSrcRar = Path.Combine(SCMAmendDir,"集成-src-V" + ScmVer.ToString() + ".rar");


            if (ScmL.Count > 0)
            {
                ScmL.Sort();
                s = ScmL[ScmL.Count - 1].ToString();
                dir = Path.GetFileNameWithoutExtension(s);
                // 上次数据
                SCMLastAmendDir = Path.Combine(LocalDir, dir);
                SCMLastLocalFile = Path.Combine(SCMLastAmendDir, s);
                ScmLastRemoteFile = Path.Combine(RemoteDir, s);

                // 取递交版本号 
                // 20111207012-委托管理-高虎-20120116-V13.rar --> 20111207012-委托管理-高虎-20120116-V13 -> 13
                s = s.Substring(0, s.LastIndexOf('.'));
                SCMLastVer = int.Parse(s.Substring(s.LastIndexOf('V') + 1));
                SCMLastLocalSrcRar = Path.Combine(SCMLastAmendDir, "集成-src-V" + SCMLastVer.ToString() + ".rar");
                ScmLastRemoteSrcRar = Path.Combine(RemoteDir, "集成-src-V" + SCMLastVer.ToString() + ".rar");
            }
            else
            {
                SCMLastVer = 0;
            }

            if (ScmedVer == 0)
            {
                ScmedVer = SCMLastVer;
            }

            if (ScmSrc.Count > 0)
            {
                ScmSrc.Sort();
                s = ScmSrc[ScmSrc.Count - 1].ToString();
                SCMLastLocalSrcRar = Path.Combine(SCMLastAmendDir, s);
                ScmLastRemoteSrcRar = Path.Combine(RemoteDir, s);
            }

            // 决定是新集成还是修复集成还是重新集成
            if (ScmVer == 0 || (ScmVer == 1 && SubmitVer == 1))  // 第一次集成
            {
                scmtype = ScmType.NewScm;
            }
            else if (SCMLastVer <= SubmitVer) // 重新集成也当成修复集成
            {
                scmtype = ScmType.BugScm;  // 修复集成
            }
            #endregion

            return true;
        }

        public Boolean DeletePack(int v)
        {
            // 输出递交包，到本地集成环境处理，需要使用ftp连接
            FTPConnection ftp = MAConf.instance.Configs[ProductId].ftp;
            FtpConf fc = MAConf.instance.Configs[ProductId].fc;

            // 强制重新连接，防止时间长了就断掉了
            if (ftp.IsConnected == false)
            {
                try
                {
                    ftp.Connect();
                }
                catch (Exception e)
                {
                    log.WriteErrorLog("连接FTP服务器失败，错误信息：" + e.Message);
                }
            }

            if (ftp.DirectoryExists(RemoteDir) == false)
            {
                System.Windows.Forms.MessageBox.Show("FTP路径" + fc.ServerDir + CommitPath + "不存在！");
                return false;
            }

            //ftp.ChangeWorkingDirectory(fc.ServerDir);
            ftp.DeleteFile(RemoteFile);

            return true;
        }
        
        // 查询单号
        public string AmendNo {get; set;}
        
        // 主单号
        public string MainNo {get; private set;}

        // 修改单列表
        public string AmendList;

        // 修改单列表,可以递交N多修改单
        //public int[] Amends {get; set;}

        public string ProductId { get; private set; }  // 00052-确定股东
        
        // 存放路径
        public string CommitPath {get; private set; } // /广发版技术支持测试/20111223029-深圳大宗

        // 递交文件夹 
        public string CommitDir { get; private set; } // 20111223029-深圳大宗

        public string AmendSubject { get; private set; } // 融资融券 广发版技术支持测试

        // 本地修改单路径，不带最后的 /
        public string LocalDir {get; private set;} // E:\xgd\融资融券\20111123054-国金短信

        // 远程递交文件夹
        public string RemoteDir {get; private set;} //

        // 修改单压缩包本地存放文件
        public string LocalFile { get; private set; }

        public string SCMLocalFile {get; private set; }  // 集成输出文件，可能查询的时候还不存在

        public string RemoteFile {get; private set;}

        public string SCMRemoteFile {get; private set;} // 远程集成输出文件
 
        // 本地修改单V*文件夹路径，不带最后的 /
        //public string AmendDir { get; private set; } // E:\xgd\融资融券\20111123054-国金短信\20111123054-国金短信-李景杰-20120117-V1

        // 集成文件夹路径
        public string SCMAmendDir { get; private set; }  // E:\xgd\融资融券\20111123054-国金短信\集成-20111123054-国金短信-李景杰-20120117-V1

        // 需求单号，暂时不用
        //private string ReqNo {get; set;}

        public string SrcRar { get; private set; }
        public string SCMSrcRar { get; private set; }

        // 上次集成版本
        public int SCMLastVer { get; private set; }
        public string SCMLastAmendDir { get; private set; }
        public string SCMLastLocalFile { get; private set; }
        public string SCMLastLocalSrcRar { get; private set; }

        // 修改单递交组件，以字符串对象和对象两种形态体现，调整字符串对象为私有（主要是使用不方便）
        public string ComString {get; private set;}
        public ComList ComComms {get; private set;}
        public SAWFileList SAWFiles { get; set; }

        // sql server 连接串，定义为私有，对外不可见
        private readonly string ConnString = "server=192.168.60.60;Initial Catalog=manage;Integrated Security=false;"
            + "uid=jiangshen;pwd=jiangshen;Connection Timeout=5";

        // 建立连接对象
        private SqlConnection sqlconn;
        private SqlCommand sqlcomm;
        private SqlDataReader sqldr;

        public bool TempModiFlag;
        public int SubmitVer {get; set;}
        public int ScmVer { get; set; }
        public int ScmedVer { get; set; } // 实际已经集成的版本

        public string Readme { get; private set; }  // readme文件名称

        public ScmType scmtype;

        public ScmStatus laststatus;
        public ScmStatus scmstatus;

        public SvnPort svn;

        private OperLog log;

        public ArrayList SubmitL;
        public ArrayList ScmL;
        public ArrayList ScmSrc;

        public string ScmLastRemoteFile;
        public string ScmLastRemoteSrcRar;

        public Dictionary<string, string> ReworkList { get; private set; }
        public Dictionary<string, string> StatusDict { get; private set; }
        public List<string> ReworkStatus { get; private set; }
    }
}

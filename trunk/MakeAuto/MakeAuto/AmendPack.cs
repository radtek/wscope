using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using SAWVSDKLib;

namespace MakeAuto
{
    enum ComType
    {
        Nothing = 0,
        SO = 1,
        Sql = 2,
        Exe = 3,
        Dll = 4,
        Patch = 5,
        // 小包SQL
        Ssql =6,
        Ini = 7,
    }

    // 递交程序项
    class CommitCom
    {
        // 程序项名称
        public string cname;
        // 版本
        public string cver;
        // 类型
        public ComType ctype {get; private set;}
        // 对应 源代码名称
        //public string srcName;
        // 对应VSS路径（暂时不用）
        //public string SAWPath;

        public CommitCom(string name, string version)
        {
            cname = name;
            cver = version;
            if (cname.IndexOf("libs") > -1)
                ctype = ComType.SO;
            else if (cname.IndexOf("sql") > -1)
            {
                ctype = ComType.Sql;
                if (cname.IndexOf("Patch") > -1)
                    ctype = ComType.Patch;
                else if (cname.IndexOf("小包") > -1)
                    ctype = ComType.Ssql;
            }
            else if (cname.IndexOf("exe") > -1)
                ctype = ComType.Exe;
            else if (cname.IndexOf("dll") > -1)
                ctype = ComType.Dll;
            else if (cname.IndexOf("ini") > -1)
                ctype = ComType.Ini;

        }

        // 取需要刷出的 文件版本 信息
        public int GetAmendVer(string User, string AmendNo, string FileVersion)
        {
            SAWVFileHistorySet hisset = SAWV.instance.GetFileHistory(cname, "");
            string s = string.Empty;
            foreach (SAWVFileHistory his in hisset)
            {
                s = his.Comment;
                // 判断找到的条件，修改单符合，文件版本符合，返回递交的文件的版本
                if (s.IndexOf(AmendNo) > 0 && s.IndexOf(FileVersion) > 0)
                {
                    return his.Version;
                }
            }
            return 0;
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
        public static readonly AmendPack instance = new AmendPack();

        private AmendPack()
        {
            ComComms = new ComList();
            // 创建连接
            sqlconn = new SqlConnection(ConnString);
            //为上面的连接指定Command对象
            sqlcomm = sqlconn.CreateCommand();

        }

        // 类初始化
        public void QueryAmend(string AmendNo)
        {
            this.AmendNo = AmendNo;
            // 查询修改单信息
            QueryAmendInfo();
            // 生成修改单组件包信息
            SetComs();
        }

        // 根据提供的修改单查询主单号
        private int QueryAmendInfo()
        {
            // 打开连接
            if (sqlconn.State == ConnectionState.Closed)
            {
                sqlconn.Open();
            }

            // 指定查询项 a.reference_stuff as 递交程序项, a.program_path_a as 递交路径
            sqlcomm.CommandText = ""
              + " select a.reference_stuff, a.program_path_a "
              + " from manage.dbo.programreworking2 a "
              + " where reworking_id = '" + AmendNo + "' ";
            //为指定的command对象执行DataReader;
            sqldr = sqlcomm.ExecuteReader();

            // 如果有数据，读取数据
            while (sqldr.Read())
            {
                // 获取数据
                ComString = sqldr["reference_stuff"].ToString().Trim();
                CommitPath = sqldr["program_path_a"].ToString().Trim();
            }

            // 从CommitPath中分解递交项和主单号 /融资融券/20111123054-国金短信，分解得到 20111123054
            MainNo = CommitPath.Substring(CommitPath.LastIndexOf("/") + 1, 11);

            sqldr.Close();

            return 0;
        }

        // 设置递交组件
        private void SetComs()
        {
            // 查询出的组件是如下的一段
            // config.ini  [V6.1.4.7]  GJShortMessage.dll  [V6.1.4.1]  HsNoticeSvr.exe  [V6.1.4.6] 
            // 需要进行分解，操作如下
            string name, version, cs = ComString;

            int s = 0, e = 0;
            while (cs.Length > 0)
            {

                s = cs.IndexOf("["); // 取第一个版本分隔符号
                e = cs.IndexOf("]"); // 取版本分隔符号
                name = cs.Substring(0, s - 1).Trim(); // 程序名称 
                version = cs.Substring(s + 1, e - s - 1);  // 程序版本
                CommitCom c = new CommitCom(name, version);
                // 添加组件
                ComComms.Add(c);

                // 取剩余递交项
                if (e < cs.Length - 1)
                {
                    cs = cs.Substring(e + 1).Trim();
                }
                else
                {
                    cs = "";
                }
            }
        }

        #region 递交组件版本比较
        public bool ValidateVersion()
        {
            bool ret = true, ret1 = true;
            foreach (CommitCom c in ComComms)
            {
                // 这里的校验主要校验版本，集成编译之后，对于SO，还要校验大小
                // 根据文件类型调用不同方法校验版本
                switch (c.ctype)
                {
                    case ComType.Dll:
                    case ComType.Exe:
                        ret1 = ValidateDll(c);
                        break;
                    case ComType.SO:  // SO 文件只有一个版本信息
                    case ComType.Ini: // Ini 文件有多个版本信息
                    case ComType.Patch: // Patch 文件有多行版本信息
                    case ComType.Sql:
                        ret1 = ValidateSO(c);
                        break;
                    case ComType.Ssql:  // 小包无版本
                        break;
                    default:
                        break;
                }

                if (ret1 == false)
                    ret = ret1;
            }

            return ret;

        }

        public bool ValidateSO(CommitCom c)
        {
            // 版本比较 
            // 取压缩包的 SO 测试下正则表达式，同时比较版本信息
            StreamReader sr = new StreamReader(SCMAmendDir + "/" + c.cname);
            string input = sr.ReadToEnd();
            //string pattern = @"V(\d+\.){3}\d+"; // V6.1.31.16
            string pattern = c.cver;
            Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            // 确保版本逆序，只取第一个
            Match m = rgx.Match(input);

            sr.Close();
            return m.Success;
        }

        public bool ValidateDll(CommitCom c)
        {
            // 获取文件版本信息
            FileVersionInfo DllVer = FileVersionInfo.GetVersionInfo(
                Path.Combine(SCMAmendDir + Path.DirectorySeparatorChar + c.cname));

            // cver的 V 去掉，从第一个开始比较
            return DllVer.FileVersion == c.cver.Substring(1);
        }
        #endregion

        // 写日志
        public void WriteLog(InfoType type, string LogContent, string Title = "")
        {
            MAConf.instance.WriteLog(type, LogContent, Title);
        }

        // 打包压缩
        public void Rar(string path, string dir)
        {

        }

        // 解压缩
        public void UnRar(string rarfile, string dir, string file = "")
        {
            // 开启进程执行 rar解压缩
            // 获取 Winrar 的路径（通过注册表或者是配置，这里直接根据xml来处理）
            // 实例化 Process 类，启动执行进程  
            Process p = new Process();
            try
            {
                p.StartInfo.FileName = MAConf.instance.rar;           // rar程序名  
                // 解压缩的参数

                // 如果没有指定解压某一个文件，则解压缩全部文件
                if (file == "")
                {
                    file = " ";
                }

                p.StartInfo.Arguments = " E -y -ep " + rarfile + " " + file + " " + dir;   // 设置执行参数  
                p.StartInfo.UseShellExecute = false;        // 关闭Shell的使用  
                p.StartInfo.RedirectStandardInput = true; // 重定向标准输入
                p.StartInfo.RedirectStandardOutput = true;  //重定向标准出  
                p.StartInfo.RedirectStandardError = true; //重定向错误输出  
                p.StartInfo.CreateNoWindow = true;             // 不显示窗口 
                p.Start();    // 启动
                //return p.StandardOutput.ReadToEnd();        //從输出流取得命令执行结果
            }
            catch (Exception ex)
            {
                WriteLog(InfoType.Error, "执行rar失败" + ex.Message);
            }
        }

        // 查询单号
        public string AmendNo {get; set;}
        
        // 主单号
        public string MainNo {get; private set;}

        // 修改单列表,可以递交N多修改单
        //public int[] Amends {get; set;}

        // 存放路径
        public string CommitPath {get; private set;} // /广发版技术支持测试/20111223029-深圳大宗

        // 递交文件夹 
        public string CommitDir // 20111223029-深圳大宗
        { 
            get
            {
                return CommitPath.Substring(CommitPath.LastIndexOf("/") + 1);
            } 
        }

        public string CommitModule // 融资融券 广发版技术支持测试
        {
            get
            {
                return CommitPath.Substring(0, CommitPath.IndexOf("/"));
            }
        }

        // 本次V*包
        public string currVerFile { get; set; } // 20111123054-国金短信-李景杰-20120117-V3.rar
        public string SCMcurrVerFile { get; set; } // 集成-20111123054-国金短信-李景杰-20120117-V2.rar

        // 本地修改单路径，不带最后的 /
        public string LocalDir // E:\xgd\融资融券\20111123054-国金短信
        {
            get
            {
                return MAConf.instance.fc.LocalDir + CommitPath;
            }
        }

        // 远程递交文件夹
        public string RemoteDir
        {
            get 
            {
                return MAConf.instance.fc.ServerDir + CommitPath;
            }
        }

        // 修改单压缩包本地存放文件
        public string LocalFile
        {
            get
            {
                return LocalDir + "\\" + currVerFile;
            }
        }

        public string RemoteFile
        {
            get 
            {
                return RemoteDir + "\\" + currVerFile;
            }
        }
 
        // 本地修改单V*文件夹路径，不带最后的 /
        public string AmendDir
        {
            get
            {
                return LocalDir + "\\" + Path.GetFileNameWithoutExtension(LocalFile);
            }
        }  // E:\xgd\融资融券\20111123054-国金短信\20111123054-国金短信-李景杰-20120117-V1

        // 集成文件夹路径
        public string SCMAmendDir 
        {
            get
            {
                return LocalDir + "\\" + "集成-" + Path.GetFileNameWithoutExtension(LocalFile);
            }
        }  // E:\xgd\融资融券\20111123054-国金短信\集成-20111123054-国金短信-李景杰-20120117-V1

        // 需求单号，暂时不用
        //private string ReqNo {get; set;}

        // 修改单递交组件，以字符串对象和对象两种形态体现，调整字符串对象为私有（主要是使用不方便）
        public string ComString {get; private set;}
        public ComList ComComms {get; private set;}

        // sql server 连接串，定义为私有，对外不可见
        private readonly string ConnString = "server=192.168.60.60;database =manage;uid =jiangshen;pwd=jiangshen";

        // 建立连接对象
        private SqlConnection sqlconn;
        private SqlCommand sqlcomm;
        private SqlDataReader sqldr;
    }
}

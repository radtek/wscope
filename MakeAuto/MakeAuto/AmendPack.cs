using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
using System.Data.SqlClient;

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
        // 查询单号
        public string AmendNo {get; set;}
        
        // 主单号
        public string MainNo {get; private set;}

        // 修改单列表,可以递交N多修改单
        //public int[] Amends {get; set;}

        // 存放路径
        public string CommitPath {get; private set;}

        public string CommitDir 
        { 
            get
            {
                return CommitPath.Substring(CommitPath.LastIndexOf("/") + 1);
            } 
        }

        public string CommitModule
        {
            get
            {
                return CommitPath.Substring(0, CommitPath.IndexOf("/"));
            }
        }

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

        // 根据提供的修改单查询主单号
        private int QueryAmendInfo()
        {   
            // 打开连接
            if(sqlconn.State == ConnectionState.Closed)
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
            while(sqldr.Read())
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

        public static readonly AmendPack instance = new AmendPack();

        
        private AmendPack()
        {
            ComComms = new ComList();
            // 创建连接
            sqlconn = new SqlConnection(ConnString);
            //为上面的连接指定Command对象
            sqlcomm = sqlconn.CreateCommand();

        }

        public void QueryAmend(string AmendNo)
        {
            this.AmendNo = AmendNo;
            // 查询修改单信息
            QueryAmendInfo();
            // 生成修改单组件包信息
            SetComs();
        }
    }
}

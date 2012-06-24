using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Ionic.Zip;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using System.Data;

namespace OraZip
{
    /// <summary>
    /// Nhs 是 New HSS 的简写，希望能解决掉HSS的缺陷，并引入新增的需求
    /// </summary>
    class Nhs
    {
        public Nhs(string nhsfile)
        {
            oplog = OperLog.instance;
            sb = new StringBuilder();
            dbconf = OraConf.instance.DBs[0];
            file = nhsfile;
        }

        /* 处理的几步， 1-去注释 2-压缩
         * 
         * 
         * */
        public Boolean ProcessSql(CVSArg cvs, string user, string sqlfile)
        {
            this.cv = cvs;
            this.user = user;
            this.file = Path.Combine(Path.GetDirectoryName(sqlfile),
                Path.GetFileNameWithoutExtension(sqlfile) + ".nhs"); // 确定路径的名字
            sb.Clear();
            return RemoveComment(sqlfile) && Compress(sqlfile);
        }

        /// <summary>
        /// 预处理sql脚本第一步，把脚本的注释全部处理掉，输出为字符串，可以保存为文件
        /// </summary>
        /// <param name="test"></param>
        /// <returns></returns>
        private Boolean RemoveComment(string sqlfile)
        {

            string s, stemp;
            int i, j;
            bool bComment = false;
            // 读取readme，生成集成注意
            try
            {
                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader.
                using (StreamReader sr = new StreamReader(file,
                    Encoding.GetEncoding("gb2312")))
                {
                    // 读取一行，处理掉注释行
                    while ((s = sr.ReadLine()) != null)
                    {
                        s = s.TrimEnd();

                        // 跳过空行，保留，便于过程格式定义处理，原有的空行保留，这里用于处理掉
                        if (s == string.Empty)
                        {
                            sb.AppendLine();
                            //OperLog.instance.WriteInfo(s);
                            continue;
                        }

                        // 跳过提示行
                        if (s.Trim().StartsWith("prompt"))
                            continue;

                        // 如果是注释匹配模式，获取第一个 */
                        if (bComment)
                        {
                            if ((i = s.IndexOf("*/")) >= 0)
                            {
                                bComment = false;
                                // 取注释之后部分的行
                                if (i + 2 < s.Length)
                                {
                                    s = s.Substring(i + 2);
                                }
                                else
                                {
                                    //s = string.Empty;
                                    continue; // 空行跳过
                                }
                            }
                            else  // 否则，跳过中间的注释行
                            {
                                continue;
                            }
                        }

                        // 如果行以包含--，则处理掉"--"后的部分
                        if (s.IndexOf("--") >= 0)
                        {
                            s = s.Substring(0, s.IndexOf("--"));

                            if (s.Trim() == string.Empty)
                            {
                                continue;
                            }
                        }

                        // 处理 /* */ 的注释，但是要略过 /*+ +*/ 和 /*+ */ 这种模式，这样写不知道有没有风险
                        // 不会出现 包含了 /* ... /*+ */ ... */ 这种， pl/sql 就过不去
                        // 这种情况 /*+ .. */ ... /* 换行 */ 咋办 ？
                        // 或者是 /* */ ../*..*/  /*+ */
                        // 仅包含 /*+ 的行不考虑，包含了 /*+ 的行不再处理，可能会导致有些注释存在，概率很低
                        if (s.IndexOf("/*+") >= 0 && s.IndexOf("*/") < 0)
                        {
                            oplog.WriteLog("检测到不在同一行的 /*+, " + s, LogLevel.Error);
                        }

                        // 处理 /* 结尾的注释，循环处理掉
                        while ((i = s.LastIndexOf("/*")) >= 0 && s[i + 2] != '+' && s.LastIndexOf("*/") < i)
                        {
                            bComment = true;

                            s = s.Substring(0, i); // 循环忽略
                        }

                        // 处理一行中间的注释
                        while ((i = s.IndexOf("/*")) >= 0 && s[i + 2] != '+') // 从 /* 开始找，不处理/*+
                        {
                            // 如果第一个注释，不存在配对的 */，则第一个注释开始忽略；从 /*之后开始找起 */出现的比 /*早，肯定不是本行的注释，通过下面来找
                            if ((j = s.IndexOf("*/", i + 2)) < 0)
                            {
                                bComment = true;
                                s = s.Substring(0, i);
                                break;
                            }

                            // 把注释忽略掉，处理行内的配对
                            stemp = string.Empty;

                            if (i > 0)
                            {
                                stemp = s.Substring(0, i);
                            }

                            if (i > 0 && j + 2 < s.Length && s[i - 1] != ' ' && s[j + 2] != ' ')
                            {
                                stemp += " "; // 注释前后无空格，补空格
                            }

                            if (j + 2 < s.Length)
                            {
                                stemp += s.Substring(j + 2);
                            }

                            s = stemp.TrimEnd(); // 原值转回
                        }

                        s = s.TrimEnd();
                        if (s == string.Empty)
                            continue;

                        sb.AppendLine(s);
                        //OperLog.instance.WriteInfo(s);
                    }

                    //OperLog.instance.WriteLog("注释处理：", LogLevel.Info);
                    //OperLog.instance.WriteLog(sb.ToString(), LogLevel.Info);

                    // 处理掉最后两行，包括sql文档分割线和最后一个不可显示字符。以最后一个 /作为标记
                    //i = sb.ToString().LastIndexOf("/\r\n");
                    //if (i > 0 && i < sb.Length - 1)
                    //    sb.Remove(i + 1, sb.Length - i - 1);

                    OperLog.instance.WriteInfo(sb.ToString());

                    return true;
                }
            }
            catch (Exception ex)
            {
                OperLog.instance.WriteLog("脚本注释处理异常" + ex.Message, LogLevel.Error);
                return false;
            }
        }

        private Boolean Extract(string nhsfile)
        {
            // 用输入文件获取要解压缩的文件名
            string sfile = Path.GetFileNameWithoutExtension(nhsfile) + ".sql";

            ReadOptions op = new ReadOptions();
            op.Encoding = Encoding.GetEncoding("gb2312");
            using (ZipFile zip = ZipFile.Read(nhsfile, op))
            {
                ZipEntry e = zip[sfile];
                // comment 的结构 hs_user|06版|1.4.1.4 productid 1, BL4, SP1PACK4
                string s = zip.Comment;
                user = s.Substring(0, s.IndexOf("|")); // 获取脚本用户

                // 确定CVSArg
                s = s.Substring(s.IndexOf("|") + 1);
                string Product = s.Substring(0, s.IndexOf("|"));
                s = s.Substring(s.IndexOf("|") + 1);
                string[] b = s.Split('.'); 
                cv = new CVSArg(Product, int.Parse(b[0]), int.Parse(b[1]), int.Parse(b[2]), int.Parse(b[3])); 

                ms = new MemoryStream();
                e.ExtractWithPassword(ms, G_PASS);
            }

            return true;
        }

        public Boolean RunNhs()  // 执行Nhs文件
        {
            string s, SqlContent = string.Empty;

            // 先解压缩，然后执行
            Extract(file);

            DBUser d = null;

            foreach (DBUser u in dbconf.Users)
            {
                if (u.name.Equals(user))
                {
                    d = u;
                    break;
                }
            }

            if (d == null)
            { 
                // 报错返回
            }

            string oradb = "Data Source=" +  dbconf.tnsname + 
                ";User Id=" + d.name + ";Password=" + DBUser.DecPass(d.pass) + ";";
            conn = new OracleConnection(oradb); // C#
            try
            {
                conn.Open();
            }
            catch (OracleException e)
            {
                string err = "连接Oracle数据库失败，TNSNAME:" + dbconf.tnsname + "，Oracle异常信息：" + e.Message;
                oplog.WriteLog(err, LogLevel.Error);
                System.Windows.Forms.MessageBox.Show(err);
     
                return false;
            }
            catch (Exception e)
            {
                string err = "连接Oracle数据库失败，TNSNAME:" + dbconf.tnsname +  "，异常信息：" + e.Message;
                oplog.WriteLog(err, LogLevel.Error);
                System.Windows.Forms.MessageBox.Show(err);

                return false;
            }

            cmd = new OracleCommand();
            cmd.Connection = conn;

            // 读取文件
            try
            {
                // 重定位流，否则读取从最后开始，无数据了
                ms.Seek(0, SeekOrigin.Begin);  
                using (StreamReader sr = new StreamReader(ms,
                    Encoding.GetEncoding("gb2312")))
                {
                    // 读取到 Sql 分隔符
                    while ((s = sr.ReadLine()) != null)
                    {
                        // 读到了 /，做一次提交
                        if (s.Trim().Equals("/"))
                        {
                            if (!ExSqlBlock(SqlContent))
                                return false;

                            SqlContent = string.Empty;
                        }
                        else
                        {
                            // readline 会去掉最后的换行符，这里把格式补上，否则编译出来的过程都不换行，很难看
                            SqlContent = SqlContent + "\n" + s;
                        }
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                OperLog.instance.WriteLog("脚本注释处理异常" + ex.Message, LogLevel.Error);

                return false;
            }

            finally
            {
                conn.Dispose();
            }

        }

        private Boolean ExSqlBlock(string sql)
        {
            cmd.CommandText = sql;
            cmd.CommandType = System.Data.CommandType.Text;

            //cmd.CommandText = cmd.CommandText.Replace("\r\n", "\n"); // 包含了 \r\n 就是各种报错，如果有，千万要换掉

            // delcar型脚本
            // 处理换行符号
            // 
            try
            {
                int rowsUpdated = cmd.ExecuteNonQuery();
                return true;
            }
            catch(Exception ex)
            {
                string err = "执行异常，[sql语句] " + sql + "， [报错信息] " + ex.Message;
                OperLog.instance.WriteLog(err, LogLevel.Error);
                System.Windows.Forms.MessageBox.Show("执行sql语句失败，请检查日志确认。");
                return false;
            }
        }

        private Boolean Compress(string sqlfile)
        {
            using (ZipFile zip = new ZipFile(Encoding.GetEncoding("gb2312")))
            {
                zip.Password = G_PASS;  // 设置密码
                zip.Encryption = G_ENCRYPT;
                
                // 添加版本受限和用户信息

                ZipEntry e = zip.AddEntry(sqlfile, sb.ToString());
                zip.Comment = user + "|" + cv.Product + "|" + cv.Sversion;
                // 获取输出文件名
                zip.Save(file);
            }

            return true;
        }

        const string G_PASS = "best12deal3"; //默认的加密密码值
        const EncryptionAlgorithm G_ENCRYPT = EncryptionAlgorithm.None;
        const String G_POFIX = ".nhs";


        private OperLog oplog;
        private StringBuilder sb;
        //public string sqlfile {set; get;}

        public string file { get; set; }
        FileStream fs;
        StreamWriter sw;
        StreamReader sr;
        MemoryStream ms;
        public CVSArg cv;
        public string user;


        OracleConnection conn;
        OracleCommand cmd;
        DBConf dbconf;
    }
}

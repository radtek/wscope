using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using System.IO;

namespace OraWin
{
    public partial class OraWin : Form
    {
        public OraWin()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string oradb = "Data Source=rzgh;User Id=hs_secu;Password=handsome;";

            OracleConnection conn = new OracleConnection(oradb); // C#

            conn.Open();

            OracleCommand cmd = new OracleCommand();

            cmd.Connection = conn;

            cmd.CommandText = "select init_date from hs_user.sysarg where rownum = 1";
            cmd.CommandType = CommandType.Text;

            OracleDataReader dr = cmd.ExecuteReader();

            dr.Read();

            label1.Text = dr.GetValue(0).ToString();

            OperLog oplog = OperLog.instance;
            StringBuilder sb = new StringBuilder();
            string file = "zq06-BL2011SP1PACK4_user_20120618.sql";

            RemoveComment(file, sb);

            // 读取文件
            cmd.CommandText = sb.ToString();

            //cmd.CommandType = CommandType.Text;
            // 脚本执行的几个过程
            /*
             1.预处理sql脚本，检查各项内容是否配对
             2.执行脚本，预检查是否存在编译不能通过的过程，这个可以略过
             3.执行用户脚本，提取脚本中的函数名暂存，去除头部的注释，和 prompt 行，以及最后的 '/'，
             4.检查执行情况，提取脚本中的函数名暂存
             （考虑每10个过程提交检查1次）
             */
            // ExecuteNonQuery 执行脚本时 -1 表示执行没有问题，但是编译出来的脚本，可能还是失败的，需要做一下检查
            // 
            cmd.CommandText = cmd.CommandText.Replace("\r\n", "\n"); // 用 \r\n 各种报错

            // delcar型脚本
            // 处理换行符号
            // 
            try
            {
                //int rowsUpdated = cmd.ExecuteNonQuery();

                //label1.Text = rowsUpdated.ToString();
            }
            finally
            {
                conn.Dispose();
            }
        }
        
        /// <summary>
        /// 读取和处理sql脚本
        /// </summary>
        /// <param name="ap"></param>
        /// <returns></returns>
        private string ProcessSql(string test)
        {
            List<string> notice = new List<string>();
            // 读取readme，生成集成注意
            try
            {
                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader.
                using (StreamReader sr = new StreamReader("test.sql",
                    Encoding.GetEncoding("gb2312")))
                {

                    return sr.ReadToEnd().Replace("\r\n", " ");
                }
            }
            catch (Exception ex)
            {
                //log.WriteErrorLog("ProcessScmNotice异常 ReadMe：" + ap.Readme + " " + ex.Message);
                return "F";
            }

            //return "T";
        }

        /// <summary>
        /// 预处理sql脚本第一步，把脚本的注释全部处理掉，输出为字符串，可以保存为文件
        /// </summary>
        /// <param name="test"></param>
        /// <returns></returns>
        private Boolean RemoveComment(string file, StringBuilder sb)
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
                        if(s.IndexOf("/*+") >= 0 && s.IndexOf("*/") < 0)
                        {
                            OperLog.instance.WriteLog("检测到不在同一行的 /*+, " + s, LogLevel.Error);
                        }
                        
                        // 处理 /* 结尾的注释，循环处理掉
                        while((i = s.LastIndexOf("/*")) >= 0 && s[i +2] != '+' && s.LastIndexOf("*/") < i)
                        {
                            bComment = true;

                            s = s.Substring(0, i); // 循环忽略
                        }
                        
                        // 处理一行中间的注释
                        while ((i = s.IndexOf("/*")) >= 0 && s[i+2] != '+') // 从 /* 开始找，不处理/*+
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

                            if (i > 0 && j + 2 < s.Length && s[i-1] != ' ' && s[j+2] != ' ')
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
                    //sb.Remove(i + 1, sb.Length - i - 1);

                    //OperLog.instance.WriteInfo(sb.ToString());

                    return true;
                }
            }
            catch (Exception ex)
            {
                OperLog.instance.WriteLog("脚本注释处理异常" + ex.Message, LogLevel.Error);
                return false;
            }
        }
    }
}

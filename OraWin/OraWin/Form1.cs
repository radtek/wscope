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

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string oradb = "Data Source=devgh;User Id=hs_user;Password=handsome;";

            OracleConnection conn = new OracleConnection(oradb); // C#

            conn.Open();

            OracleCommand cmd = new OracleCommand();

            cmd.Connection = conn;

            cmd.CommandText = "select init_date from sysarg where rownum = 1";
            cmd.CommandType = CommandType.Text;

            OracleDataReader dr = cmd.ExecuteReader();

            dr.Read();

            label1.Text = dr.GetValue(0).ToString();

            // 读取文件
            cmd.CommandText = ProcessSql("test.sql");
            cmd.CommandType = CommandType.Text;
            // 脚本执行的几个过程
            /*
             1.预处理sql脚本，检查各项内容是否配对
             2.执行脚本，预检查是否存在编译不能通过的过程，这个可以略过
             3.执行用户脚本，提取脚本中的函数名暂存，去除头部的注释，和 prompt 行，以及最后的 '/'，
             4.检查执行情况，提取脚本中的函数名暂存
             （考虑每10个过程提交检查1次）
             */
            // ExecuteNonQuery 执行脚本时 -1 表示执行没有问题，但是编译出来的脚本，可能还是失败的
            // 
            int rowsUpdated = cmd.ExecuteNonQuery();

            label1.Text = rowsUpdated.ToString();

            conn.Dispose();
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

                    return sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                //log.WriteErrorLog("ProcessScmNotice异常 ReadMe：" + ap.Readme + " " + ex.Message);
                return "F";
            }

            //return "T";
        }
    }
}

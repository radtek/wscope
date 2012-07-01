using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.Data;
using System.IO;
using System.Diagnostics;


namespace ExMer
{
    struct Para
    {
        string prefix; // 参数类型 D DE I IO
        string name; // 参数名称
        string type; // 参数类型
        string desc; // 参数说明
    }

    struct Func
    {

    }

    class ExFunc
    {
        public ExFunc(string path)
        {
            this.path = path;
            if (!File.Exists(path))
            {
                System.Windows.Forms.MessageBox.Show("文件不存在。");
                return;
            }
            fi = new FileInfo(path);

            strCon = " Provider = Microsoft.Jet.OLEDB.4.0;" +
                "Data Source=" + path + ";" +
                "Extended Properties='Excel 8.0;HDR=No;IMEX=1'";
            conn = new OleDbConnection(strCon);
        }

        public bool ReadExcel()
        {

            // 判断文件是否存在，不存在不需要读取
            if (!File.Exists(path))
            {
                return false;
            }

            try
            {
                conn.Open();
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }

            //返回Excel的架构，包括各个sheet表的名称,类型，创建时间和修改时间等 
            dtSheetName = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables,
                new object[] { null, null, null, "Table" });

            //包含excel中表名的字符串数组
            string[] sTables = new string[dtSheetName.Rows.Count];
            int k = 0;
            for (; k < dtSheetName.Rows.Count; k++)
            {
                sTables[k] = dtSheetName.Rows[k]["TABLE_NAME"].ToString();

                if (sTables[k].IndexOf("函数实现-") >= 0)  //
                    break;
            }


            string etb = "[" + sTables[k] + "]";
            string sql = "select * from " + etb;
            myCommand = new OleDbDataAdapter(sql, strCon);
            ds = new DataSet();
            try
            {
                myCommand.Fill(ds, etb);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }

            System.Diagnostics.Debug.WriteLine(ds.Tables[0].Rows[2].ItemArray[2]);
            //ds.Tables[0].Select

            // 完毕后关闭连接
            conn.Close();

            return true;
        }

        private string strCon;
        private OleDbConnection conn;
        private DataTable dtSheetName;
        public DataSet ds { get; private set; }
        private OleDbDataAdapter myCommand;
        private int[] Flist;
        public string path {get; private set;}
        public FileInfo fi {get; private set; }

    }
}

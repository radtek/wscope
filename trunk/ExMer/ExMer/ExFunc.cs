using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.Data;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel; 

namespace ExMer
{
    class Para
    {
        public Para()
        {}

        public string prefix; // 参数类型 D DE I IO
        public string name; // 参数名称
        public string type; // 参数类型
        public string desc; // 参数说明
    }

    class Func
    {
        public Func(int ObjectNo)
        {
            this.ObjectNo = ObjectNo;
            InputPara = new List<Para>();
            OutPutPara = new List<Para>();
            VarPara = new List<Para>();
            BusinFlow = new StringBuilder();
            ModiRecord = new StringBuilder();
        }

        public int StartLine; // 起始行号
        public int ObjectNo;  // 对象号
        public string Version; // 版本号
        public int UpdateDate; // 更新日期
        public string FuncName; // 服务名称
        public string FuncNo; // 服务编号
        public bool ResultRet; // 结果集返回
        public string InterFlag; // 接口标志
        public string ConnDB; // 数据库
        public bool Audit; // 是否复核
        public string Description; // 业务描述
        public List<Para> InputPara;
        public List<Para> OutPutPara;
        public List<Para> VarPara;
        public StringBuilder BusinFlow;
        public StringBuilder ErrorDesc;
        public StringBuilder ModiRecord;
    }

    class ExFunc
    {
        public ExFunc(string file)
        {
            this.file = file;
            if (!File.Exists(file))
            {
                System.Windows.Forms.MessageBox.Show("文件不存在。");
                return;
            }
            fi = new FileInfo(file);

            strCon = "Provider = Microsoft.Jet.OLEDB.4.0;" +
                "Data Source=" + file + ";" + "Extended Properties=\"Excel 8.0;HDR=No;IMEX=1;\"";
            conn = new OleDbConnection(strCon);

            FuncList = new List<Func>();
            stopwatch = new Stopwatch();
            stopwatch.Stop();
        }

        public bool ReadExcel()
        {

            // 判断文件是否存在，不存在不需要读取
            if (!File.Exists(file))
            {
                return false;
            }

            stopwatch.Reset(); //reset to 0
            stopwatch.Start();
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
            
            string etb = sTables[k].Replace("'", "");
            //string sql = "select * from " + "[" + etb +"B3:G100]";
            string sql = "select * from " + "[" + etb + "]";
            myCommand = new OleDbDataAdapter(sql, strCon);
            ds = new DataSet();
            try
            {
                ds.Tables.Add("Func");
                myCommand.Fill(ds, "Func");
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(sTables[k] + " " +ex.Message);
            }

            //System.Diagnostics.Debug.WriteLine(ds.Tables[0].Rows[2].ItemArray[2]);

            // 完毕后关闭连接
            conn.Close();
            stopwatch.Stop();
            return true;
        }

        public bool ReadExcel2()
        {
            Excel.Application xlApp;
            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet;
            Excel.Range xlRange;
            object misValue = System.Reflection.Missing.Value;

            xlApp = new Excel.Application();
            xlWorkBook = xlApp.Workbooks.Open(@"E:\ExMer\ExMer\bin\Debug\" + file);
            /*
            xlWorkBook = xlApp.Workbooks.Open(@"E:\ExMer\ExMer\bin\Debug\" + file, 
                0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, 
                "\t", false, false, 0, true, 1, 0);*/
            xlWorkSheet = xlWorkBook.Sheets[1];
            xlRange = xlWorkSheet.UsedRange;

            int rowCount = xlRange.Rows.Count;
            int colCount = xlRange.Columns.Count;

            MessageBox.Show(xlWorkSheet.get_Range("A1","A1").Value2.ToString());

            xlWorkBook.Close(true, misValue, misValue);
            xlApp.Quit();

            releaseObject(xlWorkSheet);
            releaseObject(xlWorkBook);
            releaseObject(xlApp);

            return true;
        }

        private void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                MessageBox.Show("Unable to release the Object " + ex.ToString());
            }
            finally
            {
                GC.Collect();
            }
        }

        public bool SetParaTable()
        {
            foreach (Func f in FuncList)
            {
                DataTable tpara = new DataTable("para" + f.ObjectNo.ToString());

                // Declare DataColumn and DataRow variables.
                DataColumn column;
                DataRow row;

                // Create new DataColumn, set DataType, 
                // ColumnName and add to DataTable.    
                column = new DataColumn();
                column.DataType = System.Type.GetType("System.String");
                column.ColumnName = "prefix";
                tpara.Columns.Add(column);

                // Create second column.
                column = new DataColumn();
                column.DataType = Type.GetType("System.String");
                column.ColumnName = "name";
                tpara.Columns.Add(column);

                // Create second column.
                column = new DataColumn();
                column.DataType = Type.GetType("System.String");
                column.ColumnName = "type";
                tpara.Columns.Add(column);

                column = new DataColumn();
                column.DataType = Type.GetType("System.String");
                column.ColumnName = "desc";
                tpara.Columns.Add(column);

                // Create new DataRow objects and add to DataTable. 
                
                Para p;
                row = tpara.NewRow();
                row["prefix"] = "输入参数";
                row["name"] = "字段名";
                row["type"] = "字段类型";
                row["desc"] = "字段说明";
                tpara.Rows.Add(row);

                for (int i = 0; i < f.InputPara.Count; i++)
                {
                    p = f.InputPara[i];
                    row = tpara.NewRow();
                    row["prefix"] = p.prefix;
                    row["name"] = p.name;
                    row["type"] = p.type;
                    row["desc"] = p.desc;
                    tpara.Rows.Add(row);
                }

                row = tpara.NewRow();
                row["prefix"] = "输出参数";
                row["name"] = "字段名";
                row["type"] = "字段类型";
                row["desc"] = "字段说明";
                tpara.Rows.Add(row);

                for (int i = 0; i < f.InputPara.Count; i++)
                {
                    p = f.InputPara[i];
                    row = tpara.NewRow();
                    row["prefix"] = p.prefix;
                    row["name"] = p.name;
                    row["type"] = p.type;
                    row["desc"] = p.desc;
                    tpara.Rows.Add(row);
                }

                row = tpara.NewRow();
                row["prefix"] = "变量";
                row["name"] = "字段名";
                row["type"] = "字段类型";
                row["desc"] = "字段说明";
                tpara.Rows.Add(row);

                for (int i = 0; i < f.InputPara.Count; i++)
                {
                    p = f.InputPara[i];
                    row = tpara.NewRow();
                    row["prefix"] = p.prefix;
                    row["name"] = p.name;
                    row["type"] = p.type;
                    row["desc"] = p.desc;
                    tpara.Rows.Add(row);
                }

                ds.Tables.Add(tpara);
            }

            return true;
        }

        public int ReadFuncList()
        {
            int count = 0;
            int fno, fdate;
            for (int i = 0; i < ds.Tables["Func"].Rows.Count; ++i)
            {
                DataRow r = ds.Tables["Func"].Rows[i];

                if (!r.ItemArray[1].ToString().Equals("对象号"))
                    continue;

                // 读取一个函数信息
                
                // 第一行 对象号	1283017	版本号	6.1.4.20120625	更新日期	20120625
                // 对象号
                if (!int.TryParse(r.ItemArray[2].ToString(), out fno))
                    fno = 0;
                Func f = new Func(fno);
                f.StartLine = i;
                // 版本号
                f.Version = r.ItemArray[4].ToString();
                // 修改日期
                if (!int.TryParse(r.ItemArray[6].ToString(), out fdate))
                    fdate = 0;
                f.UpdateDate = fdate;

                // 第二行 函数名称	FN_MONFUND_	函数说明	函数_货币基金_撤单参数设置	功能号	1283017
                // 函数说明
                r = ds.Tables["Func"].Rows[++i];
                f.FuncName = r.ItemArray[4].ToString();
                // 功能号
                f.FuncNo = r.ItemArray[6].ToString();

                // 第三行 结果集返回	 	接口标志		所连数据库	SECUDB
                r = ds.Tables["Func"].Rows[++i];
                f.ResultRet = r.ItemArray[2].ToString().Equals("Y") ? true : false;
                f.InterFlag = r.ItemArray[4].ToString();
                f.ConnDB = r.ItemArray[6].ToString();

                // 第四行 业务描述					
                r = ds.Tables["Func"].Rows[++i];
                f.Description = r.ItemArray[2].ToString();

                // 第五行 输入参数	字段名	字段类型	字段说明	长度	缺省值
                ++i;
                // 第六行开始 输入参数信息
                r = ds.Tables["Func"].Rows[++i];
                while (!r.ItemArray[1].ToString().Equals("输出参数"))
                {
                    Para p = new Para();
                    p.prefix = r.ItemArray[1].ToString();
                    p.name = r.ItemArray[2].ToString();
                    p.type = r.ItemArray[3].ToString();
                    p.desc = r.ItemArray[4].ToString();

                    f.InputPara.Add(p);

                    r = ds.Tables["Func"].Rows[++i];
                }

                // 这一行是输出参数，跳过，获取输出参数
                r = ds.Tables["Func"].Rows[++i];
                while (!r.ItemArray[1].ToString().Equals("变量"))
                {
                    Para p = new Para();
                    p.prefix = r.ItemArray[1].ToString();
                    p.name = r.ItemArray[2].ToString();
                    p.type = r.ItemArray[3].ToString();
                    p.desc = r.ItemArray[4].ToString();

                    f.OutPutPara.Add(p);

                    r = ds.Tables["Func"].Rows[++i];
                }

                // 这一行是变量，跳过，获取变量
                r = ds.Tables["Func"].Rows[++i];
                while (!r.ItemArray[1].ToString().Equals("业务处理流程"))
                {
                    Para p = new Para();
                    p.prefix = r.ItemArray[1].ToString();
                    p.name = r.ItemArray[2].ToString();
                    p.type = r.ItemArray[3].ToString();
                    p.desc = r.ItemArray[4].ToString();

                    f.VarPara.Add(p);

                    r = ds.Tables["Func"].Rows[++i];
                }

                // 获取业务处理流程
                r = ds.Tables["Func"].Rows[++i];
                while (!r.ItemArray[1].ToString().Equals("出错说明"))
                {
                    string s = string.Empty;
                    string pre = r.ItemArray[1].ToString().Trim();
                    if (!string.IsNullOrEmpty(pre))
                        s = "<" + pre + ">";
                    s += r.ItemArray[2].ToString();

                    f.BusinFlow.AppendLine(s);

                    r = ds.Tables["Func"].Rows[++i];
                }

                // 获取出错说明
                r = ds.Tables["Func"].Rows[++i];
                while (!r.ItemArray[1].ToString().Equals("修改记录"))
                {
                    string s = r.ItemArray[2].ToString();
                    if (string.IsNullOrEmpty(s.Trim()))
                    {
                        r = ds.Tables["Func"].Rows[++i];
                        continue;
                    }
                    
                    f.ErrorDesc.AppendLine(s);

                    r = ds.Tables["Func"].Rows[++i];
                }

                // 获取修改记录
                while (!r.ItemArray[1].ToString().Equals("对象号"))
                {
                    string s = r.ItemArray[2].ToString();
                    if (string.IsNullOrEmpty(s.Trim()))
                    {
                        break;
                    }

                    f.ModiRecord.AppendLine(s);

                    r = ds.Tables["Func"].Rows[++i];
                }

                FuncList.Add(f);

                // 向下扫描
            }
            return count;
        }

        private string strCon;
        private OleDbConnection conn;
        private DataTable dtSheetName;
        public DataSet ds { get; private set; }
        private OleDbDataAdapter myCommand;
        private int[] Flist;
        public string file {get; private set;}
        public FileInfo fi {get; private set; }

        public List<Func> FuncList;

        Stopwatch stopwatch;

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ExMer
{
    class ExMerge
    {
        public void RefreshDetailList()
        {
            string path;
            
            // 两方对比文件
            FileInfo f1;
            FileInfo f2;
            private FileStyleUriParser
            /*
            // 判断详细设计说明书是否存在，不存在要求设置，退出程序
            if (!File.Exists(MAConf.instance.DetailFile))
            {
                MessageBox.Show("详细设计说明书不存在，请检查配置文件 DetailFile 节点配置。");
                return;
            }

            DateTime d = File.GetCreationTime(MAConf.instance.DetailFile);
            // 重组时间比较，否则直接用时间比较，因为带了毫秒级数据，会有问题
            DateTime ExcelTime = new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second);

            // 判断 xml 文件是否存在，不存在则创建，存在则判断是否需要更新
            bool bRefresh = false;
            if (File.Exists(MAConf.instance.DetailList))
            {
                XmlTextReader reader = null;
                try
                {
                    // 装载 xml 文件
                    reader = new XmlTextReader(MAConf.instance.DetailList);

                    // 读取 detailtime 属性
                    reader.MoveToContent();
                    string XmlTime = reader.GetAttribute("detailtime");
                    if (DateTime.Compare(ExcelTime, DateTime.Parse(XmlTime)) > 0)
                    {
                        bRefresh = true;
                    }

                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            else
            {
                bRefresh = true;
            }

            // 不需要刷新时直接退出
            if (bRefresh == false)
            {
                return;
            }

            // 需要刷新时，重建详细设计说明书文件
            string strCon = " Provider = Microsoft.Jet.OLEDB.4.0; " +
            "Data Source =" + DetailFile + ";" +
            "Extended Properties = 'Excel 8.0;HDR=No;IMEX=1' ";

            OleDbConnection conn = new OleDbConnection(strCon);
            try
            {
                conn.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            //返回Excel的架构，包括各个sheet表的名称,类型，创建时间和修改时间等 
            DataTable dtSheetName = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables,
                new object[] { null, null, null, "Table" });

            //包含excel中表名的字符串数组
            string[] sTables = new string[dtSheetName.Rows.Count];
            int k = 0;
            for (; k < dtSheetName.Rows.Count; k++)
            {
                sTables[k] = dtSheetName.Rows[k]["TABLE_NAME"].ToString();

                if (sTables[k] == "模块定义$")  // 数起来是 5， 其实是 23
                    break;
            }

            string sql = "select * from [" + sTables[k] + "C10:F200]";
            OleDbDataAdapter myCommand = new OleDbDataAdapter(sql, strCon);
            DataSet ds = new DataSet();
            try
            {
                myCommand.Fill(ds, "[" + sTables[k] + "]");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            // 完毕后关闭连接
            conn.Close();


            #region 保存模块信息到xml文件
            XmlTextWriter writer = new XmlTextWriter(MAConf.instance.DetailList, Encoding.Default);
            writer.Formatting = Formatting.Indented;

            // 写xml文件
            writer.WriteStartDocument();
            // 创建注释节点
            writer.WriteComment("excel详细设计说明书");

            // 创建根元素
            writer.WriteStartElement("detail");
            // 创建本地时间元素属性
            writer.WriteAttributeString("gentime", System.DateTime.Now.ToString());
            // 创建excel文件时间元素属性
            writer.WriteAttributeString("detailtime", File.GetCreationTime(MAConf.instance.DetailFile).ToString());

            // 获取数据到xml文件中，遍历 DataSet 的数据表，保存行列值
            foreach (DataTable table in ds.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    // 处理掉最后一行的空白和中间的空白行
                    if (row[0].ToString().Trim() == String.Empty)
                    {
                        continue;
                    }

                    // 创建节点
                    writer.WriteStartElement(row[0].ToString());
                    writer.WriteAttributeString("file", row[1].ToString());
                    writer.WriteAttributeString("sql", row[2].ToString().Trim());
                    writer.WriteAttributeString("pas", row[3].ToString().Trim());
                    writer.WriteEndElement();
                }
            }

            // 关闭根节点元素和文件
            writer.WriteEndElement();
            writer.WriteEndDocument();

            // 保存文件
            writer.Flush();
            writer.Close();
            #endregion
             *              * */
        }

    }
}

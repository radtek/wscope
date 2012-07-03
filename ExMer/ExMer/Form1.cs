using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;
using System.IO;
using DiffMatchPatch;
using System.Globalization;

namespace ExMer
{
    public partial class Form1 : Form
    {
        ExFunc ef;
        ExFunc ef1;

        public Form1()
        {
            InitializeComponent();
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog fdlg = new OpenFileDialog();

            fdlg.Title = "Select file";

            //fdlg.InitialDirectory = @"c:\";

            fdlg.FileName = txtFileName.Text;

            fdlg.Filter = "Excel Sheet(*.xls)|*.xls|All Files(*.*)|*.*";

            fdlg.FilterIndex = 1;

            fdlg.RestoreDirectory = true;

            //if (fdlg.ShowDialog() == DialogResult.OK)
            if (true)
            {

                txtFileName.Text = fdlg.FileName;

                ef = new ExFunc("金融产品销售系统_详细设计说明书_上海大宗交易.xls");

                //ef.ReadExcel2();
                ef.ReadExcel();

                //dgv1.DataSource = ef.ds.Tables[0];


                //int s = int.Parse(dgv1.Rows[1].Cells[5].Value.ToString(), NumberStyles.Integer);
                //dgv1.Rows[1].Cells[5].Style.Format = "N";

                //Application.DoEvents();

                ef.ReadFuncList();
                ef.SetParaTable();
                foreach (Func f in ef.FuncList)
                {
                    lstFunc.Items.Add(f.ObjectNo);
                }

                
                // 读取第二个 Excel

                ef1 = new ExFunc("金融产品销售系统_详细设计说明书_上海大宗交易1.xls");

                //ef.ReadExcel2();
                ef1.ReadExcel();

                //dgv1.DataSource = ef.ds.Tables[0];


                //int s = int.Parse(dgv1.Rows[1].Cells[5].Value.ToString(), NumberStyles.Integer);
                //dgv1.Rows[1].Cells[5].Style.Format = "N";

                //Application.DoEvents();

                ef1.ReadFuncList();
                ef1.SetParaTable();
            }
        }



        public bool ReadService()
        {

            return true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Func f = ef.FuncList[0];
            Func f1 = ef1.FuncList[0];

           
            diff_match_patch dfp = new diff_match_patch();
            List<Diff> dfresult = new List<Diff>();
            dfresult = dfp.diff_lineMode(f.BusinFlow.ToString(), f1.BusinFlow.ToString());
            //dfresult = dfp.diff_main("good well \r\ntest", "test well \r\ntest");
             foreach (Diff d in dfresult)
            {
                if (d.operation != Operation.EQUAL)
                {
                    rtbLog.AppendText(d.operation.ToString() + " " + d.text);
                }
            }
             

            //IDiffer df = new Differ();
            //DiffPlex.Model.DiffResult dfresult = df.CreateLineDiffs(f.BusinFlow.ToString(), f1.BusinFlow.ToString(), false);
            //DiffPlex.Model.d dfresult = df.CreateLineDiffs(f.BusinFlow.ToString(), f1.BusinFlow.ToString(), false);
            
            // 分析对比结果，怎么分析？

            return;
        }

        private void lstFunc_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstFunc.SelectedIndex != -1)
            {
                foreach (Func f in ef.FuncList)
                {
                    if(f.ObjectNo == int.Parse(lstFunc.SelectedItem.ToString()))
                    {
                        rtbBusiFlow.Text = f.BusinFlow.ToString();
                        dgv1.DataSource = ef.ds.Tables["para" + f.ObjectNo.ToString()];
                    }
                }
            }
        }



    }
}

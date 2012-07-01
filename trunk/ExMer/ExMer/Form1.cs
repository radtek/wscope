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

namespace ExMer
{
    public partial class Form1 : Form
    {
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
            //{

                txtFileName.Text = fdlg.FileName;

                ExFunc ef = new ExFunc("金融产品销售系统_详细设计说明书_证券日终.xls");

                ef.ReadExcel();

                dgv1.DataSource = ef.ds.Tables[0];

                //Application.DoEvents();

            //}
        }



        public bool ReadService()
        {

            return true;
        }


    }
}

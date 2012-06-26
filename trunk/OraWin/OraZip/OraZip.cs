using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Ionic.Zip;

namespace OraZip
{
    public partial class OraZip : Form
    {
        public OraZip()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Nhs.GetMacAddr();
            //Nhs.get2();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CVSArg cv = new CVSArg("06版", 1, 4, 1, 4);
            Nhs nhs = new Nhs("test1.sql");
            nhs.ProcessSql(cv, "hs_user", "test1.sql");
            MessageBox.Show("over");
        }

        private void OraZip_Load(object sender, EventArgs e)
        {

        }

        private void lstFile_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}

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
            CVSArg cv = new CVSArg("06版", 6, 11, 3, 4);
            Nhs nhs = new Nhs();
            nhs.TestEvent += new Nhs.TestEventHandler(nhs_TestEvent);
            DBUser d;
            foreach (ListViewItem l in lstFile.Items)
            {
                if (l.Checked && !l.SubItems[2].Text.Equals(string.Empty))
                {
                    d = l.Tag as DBUser;
                    nhs.file = d.file;
                    nhs.RunNhs();
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            CVSArg cv = new CVSArg("06版", 6, 11, 3, 4);
            Nhs nhs = new Nhs();
            nhs.TestEvent += new Nhs.TestEventHandler(nhs_TestEvent);
            DBUser d;
            foreach (ListViewItem l in lstFile.Items)
            {
                if (l.Checked && ! l.SubItems[2].Text.Equals(string.Empty))
                {
                    d = l.Tag as DBUser;
                    nhs.file = d.file;
                    nhs.ProcessSql(cv, d.name, nhs.sqlfile);
                }
            }
        }

        private void OraZip_Load(object sender, EventArgs e)
        {
            OraConf oraconf = OraConf.instance;
            foreach (DBUser d in OraConf.instance.DBs[0].Users)
            {
                ListViewItem l = new ListViewItem(d.name, -1);
                l.SubItems.Add(OraConf.instance.DBs[0].tnsname);
                l.SubItems.Add(d.file);
                l.Tag = d;
                lstFile.Items.AddRange(new ListViewItem[] { l });
            }
        }

        private void nhs_TestEvent(object sender, Nhs.TestEventArgs e)
        {
            textBox1.AppendText(e.info+"\n");
        }


        private void button3_Click(object sender, EventArgs e)
        {
            label1.Text = DBUser.EncPass(maskedTextBox1.Text);
        }
    }
}

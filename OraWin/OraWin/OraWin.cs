﻿using System;
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
using OraZip;

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
            Nhs nhs = new Nhs("test1.nhs");

            nhs.RunNhs();
        } 

        private void button2_Click(object sender, EventArgs e)
        {
            label1.Text = DBUser.EncPass(maskedTextBox1.Text);
        }

        private void OraWin_Load(object sender, EventArgs e)
        {
            OraConf oraconf = OraConf.instance;
        }


    }
}

namespace MakeAuto
{
    partial class frmMakeAuto
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMakeAuto));
            this.txtLog = new System.Windows.Forms.TextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.mniSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.mniRefresh = new System.Windows.Forms.ToolStripMenuItem();
            this.mniAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.nfnMake = new System.Windows.Forms.NotifyIcon(this.components);
            this.bgwProc = new System.ComponentModel.BackgroundWorker();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cmbEnd = new System.Windows.Forms.ComboBox();
            this.cmbBegin = new System.Windows.Forms.ComboBox();
            this.lblBegin = new System.Windows.Forms.Label();
            this.lblEnd = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnSql = new System.Windows.Forms.Button();
            this.btnHyper = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.btnSO = new System.Windows.Forms.Button();
            this.btnProC = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.cmbModule = new System.Windows.Forms.ComboBox();
            this.txtModule = new System.Windows.Forms.Label();
            this.btnModPre = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txtAmendList = new System.Windows.Forms.TextBox();
            this.cmsNotify = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.button2 = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtLog
            // 
            this.txtLog.BackColor = System.Drawing.SystemColors.Window;
            this.txtLog.Cursor = System.Windows.Forms.Cursors.Default;
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.txtLog.Location = new System.Drawing.Point(0, 220);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(524, 118);
            this.txtLog.TabIndex = 0;
            this.txtLog.TabStop = false;
            this.txtLog.TextChanged += new System.EventHandler(this.txtLog_TextChanged);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mniSettings,
            this.mniAbout});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(524, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // mniSettings
            // 
            this.mniSettings.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mniRefresh});
            this.mniSettings.Name = "mniSettings";
            this.mniSettings.Size = new System.Drawing.Size(41, 20);
            this.mniSettings.Text = "设置";
            // 
            // mniRefresh
            // 
            this.mniRefresh.Name = "mniRefresh";
            this.mniRefresh.Size = new System.Drawing.Size(118, 22);
            this.mniRefresh.Text = "重建模块";
            this.mniRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // mniAbout
            // 
            this.mniAbout.Name = "mniAbout";
            this.mniAbout.Size = new System.Drawing.Size(41, 20);
            this.mniAbout.Text = "关于";
            this.mniAbout.Click += new System.EventHandler(this.mniAbout_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 387);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(524, 22);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1});
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(524, 25);
            this.toolStrip1.TabIndex = 4;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton1.Text = "toolStripButton1";
            // 
            // nfnMake
            // 
            this.nfnMake.Icon = ((System.Drawing.Icon)(resources.GetObject("nfnMake.Icon")));
            this.nfnMake.Text = "本MakeAuto不具有超牛力！";
            this.nfnMake.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.nfnMake_MouseDoubleClick);
            // 
            // bgwProc
            // 
            this.bgwProc.WorkerReportsProgress = true;
            this.bgwProc.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgwProc_DoWork);
            this.bgwProc.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgwProc_ProgressChanged);
            this.bgwProc.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgwProc_RunWorkerCompleted);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.txtLog);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 49);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(524, 338);
            this.panel1.TabIndex = 21;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.tabControl1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(524, 220);
            this.panel2.TabIndex = 23;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(524, 220);
            this.tabControl1.TabIndex = 22;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.button2);
            this.tabPage1.Controls.Add(this.groupBox1);
            this.tabPage1.Controls.Add(this.groupBox2);
            this.tabPage1.Location = new System.Drawing.Point(4, 21);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(516, 195);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "编译集成";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cmbEnd);
            this.groupBox1.Controls.Add(this.cmbBegin);
            this.groupBox1.Controls.Add(this.lblBegin);
            this.groupBox1.Controls.Add(this.lblEnd);
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(275, 121);
            this.groupBox1.TabIndex = 21;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "模块";
            // 
            // cmbEnd
            // 
            this.cmbEnd.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cmbEnd.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbEnd.FormattingEnabled = true;
            this.cmbEnd.Location = new System.Drawing.Point(139, 35);
            this.cmbEnd.Name = "cmbEnd";
            this.cmbEnd.Size = new System.Drawing.Size(121, 20);
            this.cmbEnd.TabIndex = 2;
            // 
            // cmbBegin
            // 
            this.cmbBegin.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cmbBegin.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.cmbBegin.Location = new System.Drawing.Point(8, 35);
            this.cmbBegin.Name = "cmbBegin";
            this.cmbBegin.Size = new System.Drawing.Size(121, 20);
            this.cmbBegin.TabIndex = 1;
            this.cmbBegin.TextUpdate += new System.EventHandler(this.cmbBegin_TextUpdate);
            // 
            // lblBegin
            // 
            this.lblBegin.AutoSize = true;
            this.lblBegin.Location = new System.Drawing.Point(6, 20);
            this.lblBegin.Name = "lblBegin";
            this.lblBegin.Size = new System.Drawing.Size(53, 12);
            this.lblBegin.TabIndex = 10;
            this.lblBegin.Text = "起始编号";
            // 
            // lblEnd
            // 
            this.lblEnd.AutoSize = true;
            this.lblEnd.Location = new System.Drawing.Point(137, 20);
            this.lblEnd.Name = "lblEnd";
            this.lblEnd.Size = new System.Drawing.Size(53, 12);
            this.lblEnd.TabIndex = 11;
            this.lblEnd.Text = "结束编号";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnSql);
            this.groupBox2.Controls.Add(this.btnHyper);
            this.groupBox2.Controls.Add(this.button1);
            this.groupBox2.Controls.Add(this.button5);
            this.groupBox2.Controls.Add(this.btnSO);
            this.groupBox2.Controls.Add(this.btnProC);
            this.groupBox2.Location = new System.Drawing.Point(306, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(183, 121);
            this.groupBox2.TabIndex = 22;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "操作";
            // 
            // btnSql
            // 
            this.btnSql.Location = new System.Drawing.Point(91, 17);
            this.btnSql.Name = "btnSql";
            this.btnSql.Size = new System.Drawing.Size(75, 23);
            this.btnSql.TabIndex = 18;
            this.btnSql.Text = "编译Sql";
            this.btnSql.UseVisualStyleBackColor = true;
            this.btnSql.Click += new System.EventHandler(this.btnSql_Click);
            // 
            // btnHyper
            // 
            this.btnHyper.Location = new System.Drawing.Point(91, 46);
            this.btnHyper.Name = "btnHyper";
            this.btnHyper.Size = new System.Drawing.Size(75, 23);
            this.btnHyper.TabIndex = 5;
            this.btnHyper.Text = "超链接";
            this.btnHyper.UseVisualStyleBackColor = true;
            this.btnHyper.Click += new System.EventHandler(this.btnHyper_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(91, 75);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 17;
            this.button1.Text = "重建模块";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(10, 75);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(75, 23);
            this.button5.TabIndex = 6;
            this.button5.Text = "重启AS";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.btnAS_Click);
            // 
            // btnSO
            // 
            this.btnSO.Location = new System.Drawing.Point(10, 46);
            this.btnSO.Name = "btnSO";
            this.btnSO.Size = new System.Drawing.Size(75, 23);
            this.btnSO.TabIndex = 4;
            this.btnSO.Text = "编译SO";
            this.btnSO.UseVisualStyleBackColor = true;
            this.btnSO.Click += new System.EventHandler(this.btnSO_Click);
            // 
            // btnProC
            // 
            this.btnProC.Location = new System.Drawing.Point(10, 17);
            this.btnProC.Name = "btnProC";
            this.btnProC.Size = new System.Drawing.Size(75, 23);
            this.btnProC.TabIndex = 3;
            this.btnProC.Text = "编译Proc";
            this.btnProC.UseVisualStyleBackColor = true;
            this.btnProC.Click += new System.EventHandler(this.btnProc_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.groupBox3);
            this.tabPage2.Location = new System.Drawing.Point(4, 21);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(516, 195);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "修改递交";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.cmbModule);
            this.groupBox3.Controls.Add(this.txtModule);
            this.groupBox3.Controls.Add(this.btnModPre);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.txtAmendList);
            this.groupBox3.Location = new System.Drawing.Point(8, 6);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(275, 87);
            this.groupBox3.TabIndex = 23;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "修改单";
            // 
            // cmbModule
            // 
            this.cmbModule.AutoCompleteCustomSource.AddRange(new string[] {
            "ni",
            "wo",
            "dajia",
            "better"});
            this.cmbModule.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cmbModule.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbModule.FormattingEnabled = true;
            this.cmbModule.Location = new System.Drawing.Point(65, 55);
            this.cmbModule.Name = "cmbModule";
            this.cmbModule.Size = new System.Drawing.Size(121, 20);
            this.cmbModule.TabIndex = 20;
            this.cmbModule.Text = "12-周边管理";
            // 
            // txtModule
            // 
            this.txtModule.AutoSize = true;
            this.txtModule.Location = new System.Drawing.Point(6, 58);
            this.txtModule.Name = "txtModule";
            this.txtModule.Size = new System.Drawing.Size(53, 12);
            this.txtModule.TabIndex = 19;
            this.txtModule.Text = "递交模块";
            // 
            // btnModPre
            // 
            this.btnModPre.Location = new System.Drawing.Point(194, 52);
            this.btnModPre.Name = "btnModPre";
            this.btnModPre.Size = new System.Drawing.Size(75, 23);
            this.btnModPre.TabIndex = 18;
            this.btnModPre.Text = "组建";
            this.btnModPre.UseVisualStyleBackColor = true;
            this.btnModPre.Click += new System.EventHandler(this.btnModPre_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 13;
            this.label2.Text = "修改单号";
            // 
            // txtAmendList
            // 
            this.txtAmendList.Location = new System.Drawing.Point(65, 19);
            this.txtAmendList.Name = "txtAmendList";
            this.txtAmendList.Size = new System.Drawing.Size(121, 21);
            this.txtAmendList.TabIndex = 12;
            this.txtAmendList.Text = "20110509040";
            // 
            // cmsNotify
            // 
            this.cmsNotify.Name = "cmsNotify";
            this.cmsNotify.Size = new System.Drawing.Size(61, 4);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(362, 165);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 23;
            this.button2.Text = "button2";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // frmMakeAuto
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(524, 409);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "frmMakeAuto";
            this.Text = "MakeAuto";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmMakeAuto_FormClosed);
            this.Load += new System.EventHandler(this.frmMakeAuto_Load);
            this.SizeChanged += new System.EventHandler(this.frmMakeAuto_SizeChanged);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem mniSettings;
        private System.Windows.Forms.ToolStripMenuItem mniAbout;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.NotifyIcon nfnMake;
        private System.ComponentModel.BackgroundWorker bgwProc;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ToolStripMenuItem mniRefresh;
        private System.Windows.Forms.ContextMenuStrip cmsNotify;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnSql;
        private System.Windows.Forms.Button btnHyper;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button btnSO;
        private System.Windows.Forms.Button btnProC;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox cmbEnd;
        private System.Windows.Forms.ComboBox cmbBegin;
        private System.Windows.Forms.Label lblBegin;
        private System.Windows.Forms.Label lblEnd;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox cmbModule;
        private System.Windows.Forms.Label txtModule;
        private System.Windows.Forms.Button btnModPre;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtAmendList;
        private System.Windows.Forms.Button button2;
    }
}


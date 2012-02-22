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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.mniSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.mniRefresh = new System.Windows.Forms.ToolStripMenuItem();
            this.开发ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.集成ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mniAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.关于ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.nfnMake = new System.Windows.Forms.NotifyIcon(this.components);
            this.bgwProc = new System.ComponentModel.BackgroundWorker();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.rbLog = new System.Windows.Forms.RichTextBox();
            this.tcSCM = new System.Windows.Forms.TabControl();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.button2 = new System.Windows.Forms.Button();
            this.button12 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtScmVer = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtSubmitVer = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txbMainNo = new System.Windows.Forms.TextBox();
            this.txbCommitPath = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txbAmenNo = new System.Windows.Forms.TextBox();
            this.btnReadInfo = new System.Windows.Forms.Button();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tbModule = new System.Windows.Forms.TextBox();
            this.clbModule = new System.Windows.Forms.CheckedListBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnSql = new System.Windows.Forms.Button();
            this.btnHyper = new System.Windows.Forms.Button();
            this.btnSO = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.btnProC = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.cmbModule = new System.Windows.Forms.ComboBox();
            this.txtModule = new System.Windows.Forms.Label();
            this.btnModPre = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txtAmendList = new System.Windows.Forms.TextBox();
            this.cmsNotify = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tcSCM.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mniSettings,
            this.开发ToolStripMenuItem,
            this.集成ToolStripMenuItem,
            this.mniAbout});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(750, 24);
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
            this.mniRefresh.Text = "刷新模块";
            this.mniRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // 开发ToolStripMenuItem
            // 
            this.开发ToolStripMenuItem.Name = "开发ToolStripMenuItem";
            this.开发ToolStripMenuItem.Size = new System.Drawing.Size(41, 20);
            this.开发ToolStripMenuItem.Text = "质保";
            // 
            // 集成ToolStripMenuItem
            // 
            this.集成ToolStripMenuItem.Name = "集成ToolStripMenuItem";
            this.集成ToolStripMenuItem.Size = new System.Drawing.Size(41, 20);
            this.集成ToolStripMenuItem.Text = "集成";
            // 
            // mniAbout
            // 
            this.mniAbout.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.关于ToolStripMenuItem});
            this.mniAbout.Name = "mniAbout";
            this.mniAbout.Size = new System.Drawing.Size(41, 20);
            this.mniAbout.Text = "帮助";
            // 
            // 关于ToolStripMenuItem
            // 
            this.关于ToolStripMenuItem.Name = "关于ToolStripMenuItem";
            this.关于ToolStripMenuItem.Size = new System.Drawing.Size(94, 22);
            this.关于ToolStripMenuItem.Text = "关于";
            this.关于ToolStripMenuItem.Click += new System.EventHandler(this.关于ToolStripMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 415);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(750, 22);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1});
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(750, 25);
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
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 49);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(750, 366);
            this.panel1.TabIndex = 21;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.rbLog);
            this.panel2.Controls.Add(this.tcSCM);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(750, 366);
            this.panel2.TabIndex = 23;
            // 
            // rbLog
            // 
            this.rbLog.BackColor = System.Drawing.SystemColors.Window;
            this.rbLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rbLog.Location = new System.Drawing.Point(0, 227);
            this.rbLog.Name = "rbLog";
            this.rbLog.ReadOnly = true;
            this.rbLog.Size = new System.Drawing.Size(750, 139);
            this.rbLog.TabIndex = 24;
            this.rbLog.Text = "";
            this.rbLog.TextChanged += new System.EventHandler(this.rbLog_TextChanged);
            // 
            // tcSCM
            // 
            this.tcSCM.Controls.Add(this.tabPage3);
            this.tcSCM.Controls.Add(this.tabPage1);
            this.tcSCM.Controls.Add(this.tabPage2);
            this.tcSCM.Dock = System.Windows.Forms.DockStyle.Top;
            this.tcSCM.Location = new System.Drawing.Point(0, 0);
            this.tcSCM.Name = "tcSCM";
            this.tcSCM.SelectedIndex = 0;
            this.tcSCM.Size = new System.Drawing.Size(750, 227);
            this.tcSCM.TabIndex = 22;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.button2);
            this.tabPage3.Controls.Add(this.button12);
            this.tabPage3.Controls.Add(this.button9);
            this.tabPage3.Controls.Add(this.button8);
            this.tabPage3.Controls.Add(this.button7);
            this.tabPage3.Controls.Add(this.button6);
            this.tabPage3.Controls.Add(this.groupBox4);
            this.tabPage3.Controls.Add(this.btnReadInfo);
            this.tabPage3.Location = new System.Drawing.Point(4, 21);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(742, 202);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "集成打包";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(398, 129);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 35;
            this.button2.Text = "打包";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button12
            // 
            this.button12.Location = new System.Drawing.Point(398, 73);
            this.button12.Name = "button12";
            this.button12.Size = new System.Drawing.Size(75, 23);
            this.button12.TabIndex = 34;
            this.button12.Text = "检出代码";
            this.button12.UseVisualStyleBackColor = true;
            this.button12.Click += new System.EventHandler(this.button12_Click);
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(572, 177);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(75, 23);
            this.button9.TabIndex = 31;
            this.button9.Text = "test";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.button9_Click);
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(479, 17);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(75, 23);
            this.button8.TabIndex = 30;
            this.button8.Text = "版本校验";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(398, 100);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(75, 23);
            this.button7.TabIndex = 29;
            this.button7.Text = "编译";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(398, 44);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(75, 23);
            this.button6.TabIndex = 28;
            this.button6.Text = "下载";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label8);
            this.groupBox4.Controls.Add(this.txtScmVer);
            this.groupBox4.Controls.Add(this.label7);
            this.groupBox4.Controls.Add(this.txtSubmitVer);
            this.groupBox4.Controls.Add(this.label6);
            this.groupBox4.Controls.Add(this.txbMainNo);
            this.groupBox4.Controls.Add(this.txbCommitPath);
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Controls.Add(this.label5);
            this.groupBox4.Controls.Add(this.txbAmenNo);
            this.groupBox4.Location = new System.Drawing.Point(8, 6);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(332, 194);
            this.groupBox4.TabIndex = 27;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "修改单";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(140, 107);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(53, 12);
            this.label8.TabIndex = 26;
            this.label8.Text = "集成版本";
            // 
            // txtScmVer
            // 
            this.txtScmVer.Location = new System.Drawing.Point(194, 102);
            this.txtScmVer.Name = "txtScmVer";
            this.txtScmVer.ReadOnly = true;
            this.txtScmVer.Size = new System.Drawing.Size(66, 21);
            this.txtScmVer.TabIndex = 25;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(11, 105);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 12);
            this.label7.TabIndex = 24;
            this.label7.Text = "递交版本";
            // 
            // txtSubmitVer
            // 
            this.txtSubmitVer.Location = new System.Drawing.Point(65, 100);
            this.txtSubmitVer.Name = "txtSubmitVer";
            this.txtSubmitVer.ReadOnly = true;
            this.txtSubmitVer.Size = new System.Drawing.Size(66, 21);
            this.txtSubmitVer.TabIndex = 23;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 49);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 12);
            this.label6.TabIndex = 22;
            this.label6.Text = "主修改单";
            // 
            // txbMainNo
            // 
            this.txbMainNo.Location = new System.Drawing.Point(65, 46);
            this.txbMainNo.Name = "txbMainNo";
            this.txbMainNo.ReadOnly = true;
            this.txbMainNo.Size = new System.Drawing.Size(121, 21);
            this.txbMainNo.TabIndex = 21;
            // 
            // txbCommitPath
            // 
            this.txbCommitPath.Location = new System.Drawing.Point(65, 73);
            this.txbCommitPath.Name = "txbCommitPath";
            this.txbCommitPath.ReadOnly = true;
            this.txbCommitPath.Size = new System.Drawing.Size(261, 21);
            this.txbCommitPath.TabIndex = 20;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 76);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 19;
            this.label4.Text = "递交路径";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 22);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 13;
            this.label5.Text = "修改单号";
            // 
            // txbAmenNo
            // 
            this.txbAmenNo.Location = new System.Drawing.Point(65, 19);
            this.txbAmenNo.Name = "txbAmenNo";
            this.txbAmenNo.Size = new System.Drawing.Size(121, 21);
            this.txbAmenNo.TabIndex = 12;
            this.txbAmenNo.Text = "20111125026";
            // 
            // btnReadInfo
            // 
            this.btnReadInfo.Location = new System.Drawing.Point(398, 17);
            this.btnReadInfo.Name = "btnReadInfo";
            this.btnReadInfo.Size = new System.Drawing.Size(75, 23);
            this.btnReadInfo.TabIndex = 26;
            this.btnReadInfo.Text = "读取";
            this.btnReadInfo.UseVisualStyleBackColor = true;
            this.btnReadInfo.Click += new System.EventHandler(this.btnReadInfo_Click);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.tbModule);
            this.tabPage1.Controls.Add(this.clbModule);
            this.tabPage1.Controls.Add(this.groupBox2);
            this.tabPage1.Location = new System.Drawing.Point(4, 21);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(742, 202);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "编译集成";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tbModule
            // 
            this.tbModule.Location = new System.Drawing.Point(397, 6);
            this.tbModule.Name = "tbModule";
            this.tbModule.Size = new System.Drawing.Size(156, 21);
            this.tbModule.TabIndex = 12;
            this.tbModule.TextChanged += new System.EventHandler(this.tbModule_TextChanged);
            // 
            // clbModule
            // 
            this.clbModule.CheckOnClick = true;
            this.clbModule.FormattingEnabled = true;
            this.clbModule.Location = new System.Drawing.Point(6, 6);
            this.clbModule.Name = "clbModule";
            this.clbModule.Size = new System.Drawing.Size(375, 212);
            this.clbModule.TabIndex = 23;
            this.clbModule.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.clbModule_ItemCheck);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnSql);
            this.groupBox2.Controls.Add(this.btnHyper);
            this.groupBox2.Controls.Add(this.btnSO);
            this.groupBox2.Controls.Add(this.button1);
            this.groupBox2.Controls.Add(this.button5);
            this.groupBox2.Controls.Add(this.btnProC);
            this.groupBox2.Location = new System.Drawing.Point(387, 44);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(181, 172);
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
            this.btnHyper.Location = new System.Drawing.Point(10, 46);
            this.btnHyper.Name = "btnHyper";
            this.btnHyper.Size = new System.Drawing.Size(75, 23);
            this.btnHyper.TabIndex = 5;
            this.btnHyper.Text = "超链接";
            this.btnHyper.UseVisualStyleBackColor = true;
            this.btnHyper.Click += new System.EventHandler(this.btnHyper_Click);
            // 
            // btnSO
            // 
            this.btnSO.Location = new System.Drawing.Point(91, 46);
            this.btnSO.Name = "btnSO";
            this.btnSO.Size = new System.Drawing.Size(75, 23);
            this.btnSO.TabIndex = 4;
            this.btnSO.Text = "编译SO";
            this.btnSO.UseVisualStyleBackColor = true;
            this.btnSO.Click += new System.EventHandler(this.btnSO_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(91, 75);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 17;
            this.button1.Text = "刷新模块";
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
            this.tabPage2.Controls.Add(this.groupBox1);
            this.tabPage2.Controls.Add(this.groupBox3);
            this.tabPage2.Location = new System.Drawing.Point(4, 21);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(742, 202);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "修改递交";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.comboBox1);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.button3);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.textBox1);
            this.groupBox1.Location = new System.Drawing.Point(8, 110);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(275, 87);
            this.groupBox1.TabIndex = 24;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "修改单";
            // 
            // comboBox1
            // 
            this.comboBox1.AutoCompleteCustomSource.AddRange(new string[] {
            "ni",
            "wo",
            "dajia",
            "better"});
            this.comboBox1.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.comboBox1.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(65, 55);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(121, 20);
            this.comboBox1.TabIndex = 20;
            this.comboBox1.Text = "12-周边管理";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 58);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 19;
            this.label1.Text = "递交模块";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(194, 52);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 18;
            this.button3.Text = "集成";
            this.button3.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 13;
            this.label3.Text = "修改单号";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(65, 19);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(121, 21);
            this.textBox1.TabIndex = 12;
            this.textBox1.Text = "20110509040";
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
            // frmMakeAuto
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(750, 437);
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
            this.panel2.ResumeLayout(false);
            this.tcSCM.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

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
        private System.Windows.Forms.TabControl tcSCM;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnSql;
        private System.Windows.Forms.Button btnHyper;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button btnSO;
        private System.Windows.Forms.Button btnProC;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox cmbModule;
        private System.Windows.Forms.Label txtModule;
        private System.Windows.Forms.Button btnModPre;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtAmendList;
        private System.Windows.Forms.ToolStripMenuItem 关于ToolStripMenuItem;
        private System.Windows.Forms.CheckedListBox clbModule;
        private System.Windows.Forms.TextBox tbModule;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ToolStripMenuItem 开发ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 集成ToolStripMenuItem;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Button btnReadInfo;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox txbCommitPath;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txbAmenNo;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txbMainNo;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtScmVer;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtSubmitVer;
        private System.Windows.Forms.Button button12;
        private System.Windows.Forms.RichTextBox rbLog;
        private System.Windows.Forms.Button button2;
    }
}


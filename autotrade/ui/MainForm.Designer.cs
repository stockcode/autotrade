namespace autotrade
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.c1CommandHolder1 = new C1.Win.C1Command.C1CommandHolder();
            this.c1Command1 = new C1.Win.C1Command.C1Command();
            this.c1CommandMenu1 = new C1.Win.C1Command.C1CommandMenu();
            this.c1CommandLink2 = new C1.Win.C1Command.C1CommandLink();
            this.c1Command2 = new C1.Win.C1Command.C1Command();
            this.c1CommandMenu2 = new C1.Win.C1Command.C1CommandMenu();
            this.c1CommandLink4 = new C1.Win.C1Command.C1CommandLink();
            this.c1Command3 = new C1.Win.C1Command.C1Command();
            this.accountBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.c1MainMenu1 = new C1.Win.C1Command.C1MainMenu();
            this.c1CommandLink1 = new C1.Win.C1Command.C1CommandLink();
            this.c1CommandLink3 = new C1.Win.C1Command.C1CommandLink();
            this.c1DockingTab1 = new C1.Win.C1Command.C1DockingTab();
            this.c1DockingTabPage1 = new C1.Win.C1Command.C1DockingTabPage();
            this.c1List1 = new C1.Win.C1List.C1List();
            this.c1ComboBox2 = new C1.Win.C1Input.C1ComboBox();
            this.c1ComboBox1 = new C1.Win.C1Input.C1ComboBox();
            this.c1Button1 = new C1.Win.C1Input.C1Button();
            this.c1StatusBar1 = new C1.Win.C1Ribbon.C1StatusBar();
            this.c1DockingTab2 = new C1.Win.C1Command.C1DockingTab();
            this.c1DockingTabPage2 = new C1.Win.C1Command.C1DockingTabPage();
            this.c1DockingTab3 = new C1.Win.C1Command.C1DockingTab();
            this.c1DockingTabPage3 = new C1.Win.C1Command.C1DockingTabPage();
            this.c1List2 = new C1.Win.C1List.C1List();
            this.marketDataBindingSource = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.c1CommandHolder1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.accountBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1DockingTab1)).BeginInit();
            this.c1DockingTab1.SuspendLayout();
            this.c1DockingTabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1List1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1ComboBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1ComboBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1StatusBar1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1DockingTab2)).BeginInit();
            this.c1DockingTab2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1DockingTab3)).BeginInit();
            this.c1DockingTab3.SuspendLayout();
            this.c1DockingTabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1List2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.marketDataBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(195, 419);
            this.button3.Margin = new System.Windows.Forms.Padding(4);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(100, 25);
            this.button3.TabIndex = 2;
            this.button3.Text = "持仓情况";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(344, 421);
            this.button4.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 22);
            this.button4.TabIndex = 3;
            this.button4.Text = "button4";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // c1CommandHolder1
            // 
            this.c1CommandHolder1.Commands.Add(this.c1Command1);
            this.c1CommandHolder1.Commands.Add(this.c1CommandMenu1);
            this.c1CommandHolder1.Commands.Add(this.c1Command2);
            this.c1CommandHolder1.Commands.Add(this.c1CommandMenu2);
            this.c1CommandHolder1.Commands.Add(this.c1Command3);
            this.c1CommandHolder1.Owner = this;
            // 
            // c1Command1
            // 
            this.c1Command1.Image = ((System.Drawing.Image)(resources.GetObject("c1Command1.Image")));
            this.c1Command1.Name = "c1Command1";
            this.c1Command1.ShortcutText = "";
            this.c1Command1.Text = "打开（&O）";
            // 
            // c1CommandMenu1
            // 
            this.c1CommandMenu1.CommandLinks.AddRange(new C1.Win.C1Command.C1CommandLink[] {
            this.c1CommandLink2});
            this.c1CommandMenu1.HideNonRecentLinks = false;
            this.c1CommandMenu1.Name = "c1CommandMenu1";
            this.c1CommandMenu1.ShortcutText = "";
            this.c1CommandMenu1.Text = "open";
            // 
            // c1CommandLink2
            // 
            this.c1CommandLink2.Command = this.c1Command2;
            // 
            // c1Command2
            // 
            this.c1Command2.Name = "c1Command2";
            this.c1Command2.ShortcutText = "";
            this.c1Command2.Text = "close";
            this.c1Command2.Click += new C1.Win.C1Command.ClickEventHandler(this.c1Command2_Click);
            // 
            // c1CommandMenu2
            // 
            this.c1CommandMenu2.CommandLinks.AddRange(new C1.Win.C1Command.C1CommandLink[] {
            this.c1CommandLink4});
            this.c1CommandMenu2.HideNonRecentLinks = false;
            this.c1CommandMenu2.Name = "c1CommandMenu2";
            this.c1CommandMenu2.ShortcutText = "";
            this.c1CommandMenu2.Text = "market";
            // 
            // c1CommandLink4
            // 
            this.c1CommandLink4.Command = this.c1Command3;
            // 
            // c1Command3
            // 
            this.c1Command3.Name = "c1Command3";
            this.c1Command3.ShortcutText = "";
            this.c1Command3.Text = "sub";
            this.c1Command3.Click += new C1.Win.C1Command.ClickEventHandler(this.c1Command3_Click);
            // 
            // accountBindingSource
            // 
            this.accountBindingSource.DataSource = typeof(autotrade.model.Account);
            // 
            // c1MainMenu1
            // 
            this.c1MainMenu1.AccessibleName = "Menu Bar";
            this.c1MainMenu1.CommandHolder = this.c1CommandHolder1;
            this.c1MainMenu1.CommandLinks.AddRange(new C1.Win.C1Command.C1CommandLink[] {
            this.c1CommandLink1,
            this.c1CommandLink3});
            this.c1MainMenu1.Dock = System.Windows.Forms.DockStyle.Top;
            this.c1MainMenu1.Location = new System.Drawing.Point(0, 0);
            this.c1MainMenu1.Margin = new System.Windows.Forms.Padding(4);
            this.c1MainMenu1.Name = "c1MainMenu1";
            this.c1MainMenu1.Size = new System.Drawing.Size(1180, 24);
            // 
            // c1CommandLink1
            // 
            this.c1CommandLink1.Command = this.c1CommandMenu1;
            // 
            // c1CommandLink3
            // 
            this.c1CommandLink3.Command = this.c1CommandMenu2;
            this.c1CommandLink3.SortOrder = 1;
            // 
            // c1DockingTab1
            // 
            this.c1DockingTab1.Controls.Add(this.c1DockingTabPage1);
            this.c1DockingTab1.Dock = System.Windows.Forms.DockStyle.Top;
            this.c1DockingTab1.Location = new System.Drawing.Point(0, 24);
            this.c1DockingTab1.Margin = new System.Windows.Forms.Padding(4);
            this.c1DockingTab1.Name = "c1DockingTab1";
            this.c1DockingTab1.SelectedIndex = 1;
            this.c1DockingTab1.ShowTabs = false;
            this.c1DockingTab1.Size = new System.Drawing.Size(1180, 89);
            this.c1DockingTab1.TabIndex = 12;
            // 
            // c1DockingTabPage1
            // 
            this.c1DockingTabPage1.Controls.Add(this.c1List1);
            this.c1DockingTabPage1.Controls.Add(this.c1ComboBox2);
            this.c1DockingTabPage1.Controls.Add(this.c1ComboBox1);
            this.c1DockingTabPage1.Controls.Add(this.c1Button1);
            this.c1DockingTabPage1.Location = new System.Drawing.Point(1, 1);
            this.c1DockingTabPage1.Name = "c1DockingTabPage1";
            this.c1DockingTabPage1.Size = new System.Drawing.Size(1178, 87);
            this.c1DockingTabPage1.TabIndex = 0;
            this.c1DockingTabPage1.Text = "第1页";
            // 
            // c1List1
            // 
            this.c1List1.AddItemSeparator = ';';
            this.c1List1.Caption = "账户情况";
            this.c1List1.ColumnWidth = 100;
            this.c1List1.DataSource = this.accountBindingSource;
            this.c1List1.DeadAreaBackColor = System.Drawing.SystemColors.ControlDark;
            this.c1List1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.c1List1.Images.Add(((System.Drawing.Image)(resources.GetObject("c1List1.Images"))));
            this.c1List1.Location = new System.Drawing.Point(0, 0);
            this.c1List1.MatchEntryTimeout = ((long)(2000));
            this.c1List1.Name = "c1List1";
            this.c1List1.PreviewInfo.Location = new System.Drawing.Point(0, 0);
            this.c1List1.PreviewInfo.Size = new System.Drawing.Size(0, 0);
            this.c1List1.PreviewInfo.ZoomFactor = 75D;
            this.c1List1.PrintInfo.PageSettings = ((System.Drawing.Printing.PageSettings)(resources.GetObject("c1List1.PrintInfo.PageSettings")));
            this.c1List1.RowDivider.Style = C1.Win.C1List.LineStyleEnum.None;
            this.c1List1.RowSubDividerColor = System.Drawing.Color.DarkGray;
            this.c1List1.ShowHeaderCheckBox = false;
            this.c1List1.Size = new System.Drawing.Size(1178, 87);
            this.c1List1.TabIndex = 15;
            this.c1List1.Text = "c1List1";
            this.c1List1.PropBag = resources.GetString("c1List1.PropBag");
            // 
            // c1ComboBox2
            // 
            this.c1ComboBox2.AutoOpen = false;
            this.c1ComboBox2.GapHeight = 0;
            this.c1ComboBox2.ImagePadding = new System.Windows.Forms.Padding(0);
            this.c1ComboBox2.ItemsDisplayMember = "";
            this.c1ComboBox2.ItemsValueMember = "";
            this.c1ComboBox2.Location = new System.Drawing.Point(132, 8);
            this.c1ComboBox2.Name = "c1ComboBox2";
            this.c1ComboBox2.Size = new System.Drawing.Size(73, 23);
            this.c1ComboBox2.TabIndex = 14;
            this.c1ComboBox2.Tag = null;
            // 
            // c1ComboBox1
            // 
            this.c1ComboBox1.AutoOpen = false;
            this.c1ComboBox1.GapHeight = 0;
            this.c1ComboBox1.ImagePadding = new System.Windows.Forms.Padding(0);
            this.c1ComboBox1.ItemsDisplayMember = "";
            this.c1ComboBox1.ItemsValueMember = "";
            this.c1ComboBox1.Location = new System.Drawing.Point(67, 8);
            this.c1ComboBox1.Name = "c1ComboBox1";
            this.c1ComboBox1.Size = new System.Drawing.Size(49, 23);
            this.c1ComboBox1.TabIndex = 13;
            this.c1ComboBox1.Tag = null;
            // 
            // c1Button1
            // 
            this.c1Button1.Location = new System.Drawing.Point(223, 6);
            this.c1Button1.Name = "c1Button1";
            this.c1Button1.Size = new System.Drawing.Size(75, 23);
            this.c1Button1.TabIndex = 12;
            this.c1Button1.Text = "c1Button1";
            this.c1Button1.UseVisualStyleBackColor = true;
            // 
            // c1StatusBar1
            // 
            this.c1StatusBar1.Location = new System.Drawing.Point(0, 605);
            this.c1StatusBar1.Name = "c1StatusBar1";
            this.c1StatusBar1.Size = new System.Drawing.Size(1180, 22);
            // 
            // c1DockingTab2
            // 
            this.c1DockingTab2.Controls.Add(this.c1DockingTabPage2);
            this.c1DockingTab2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.c1DockingTab2.Location = new System.Drawing.Point(0, 405);
            this.c1DockingTab2.Name = "c1DockingTab2";
            this.c1DockingTab2.Size = new System.Drawing.Size(1180, 200);
            this.c1DockingTab2.TabIndex = 15;
            // 
            // c1DockingTabPage2
            // 
            this.c1DockingTabPage2.Location = new System.Drawing.Point(1, 29);
            this.c1DockingTabPage2.Name = "c1DockingTabPage2";
            this.c1DockingTabPage2.Size = new System.Drawing.Size(1178, 170);
            this.c1DockingTabPage2.TabIndex = 0;
            this.c1DockingTabPage2.Text = "第2页";
            // 
            // c1DockingTab3
            // 
            this.c1DockingTab3.Controls.Add(this.c1DockingTabPage3);
            this.c1DockingTab3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.c1DockingTab3.Location = new System.Drawing.Point(0, 113);
            this.c1DockingTab3.Name = "c1DockingTab3";
            this.c1DockingTab3.Size = new System.Drawing.Size(1180, 292);
            this.c1DockingTab3.TabIndex = 18;
            // 
            // c1DockingTabPage3
            // 
            this.c1DockingTabPage3.Controls.Add(this.c1List2);
            this.c1DockingTabPage3.Location = new System.Drawing.Point(1, 29);
            this.c1DockingTabPage3.Name = "c1DockingTabPage3";
            this.c1DockingTabPage3.Size = new System.Drawing.Size(1178, 262);
            this.c1DockingTabPage3.TabIndex = 0;
            this.c1DockingTabPage3.Text = "第3页";
            // 
            // c1List2
            // 
            this.c1List2.AddItemSeparator = ';';
            this.c1List2.AlternatingRows = true;
            this.c1List2.Caption = "合约";
            this.c1List2.ColumnWidth = 100;
            this.c1List2.DataSource = this.marketDataBindingSource;
            this.c1List2.DeadAreaBackColor = System.Drawing.SystemColors.ControlDark;
            this.c1List2.Dock = System.Windows.Forms.DockStyle.Top;
            this.c1List2.Images.Add(((System.Drawing.Image)(resources.GetObject("c1List2.Images"))));
            this.c1List2.Location = new System.Drawing.Point(0, 0);
            this.c1List2.MatchEntryTimeout = ((long)(2000));
            this.c1List2.Name = "c1List2";
            this.c1List2.PreviewInfo.Location = new System.Drawing.Point(0, 0);
            this.c1List2.PreviewInfo.Size = new System.Drawing.Size(0, 0);
            this.c1List2.PreviewInfo.ZoomFactor = 75D;
            this.c1List2.PrintInfo.PageSettings = ((System.Drawing.Printing.PageSettings)(resources.GetObject("c1List2.PrintInfo.PageSettings")));
            this.c1List2.RowDivider.Style = C1.Win.C1List.LineStyleEnum.None;
            this.c1List2.RowSubDividerColor = System.Drawing.Color.DarkGray;
            this.c1List2.ShowHeaderCheckBox = false;
            this.c1List2.Size = new System.Drawing.Size(1178, 143);
            this.c1List2.TabIndex = 0;
            this.c1List2.Text = "c1List2";
            this.c1List2.PropBag = resources.GetString("c1List2.PropBag");
            // 
            // marketDataBindingSource
            // 
            this.marketDataBindingSource.DataSource = typeof(autotrade.model.MarketData);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1180, 627);
            this.Controls.Add(this.c1DockingTab3);
            this.Controls.Add(this.c1DockingTab2);
            this.Controls.Add(this.c1StatusBar1);
            this.Controls.Add(this.c1DockingTab1);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.c1MainMenu1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainForm";
            this.Text = "天网自动交易系统";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.c1CommandHolder1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.accountBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1DockingTab1)).EndInit();
            this.c1DockingTab1.ResumeLayout(false);
            this.c1DockingTabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.c1List1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1ComboBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1ComboBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1StatusBar1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1DockingTab2)).EndInit();
            this.c1DockingTab2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.c1DockingTab3)).EndInit();
            this.c1DockingTab3.ResumeLayout(false);
            this.c1DockingTabPage3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.c1List2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.marketDataBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private C1.Win.C1Command.C1CommandHolder c1CommandHolder1;
        private System.Windows.Forms.BindingSource accountBindingSource;
        private C1.Win.C1Command.C1Command c1Command1;
        private C1.Win.C1Command.C1CommandMenu c1CommandMenu1;
        private C1.Win.C1Command.C1CommandLink c1CommandLink2;
        private C1.Win.C1Command.C1Command c1Command2;
        private C1.Win.C1Command.C1MainMenu c1MainMenu1;
        private C1.Win.C1Command.C1CommandLink c1CommandLink1;
        private C1.Win.C1Command.C1DockingTab c1DockingTab1;
        private C1.Win.C1Command.C1DockingTabPage c1DockingTabPage1;
        private C1.Win.C1List.C1List c1List1;
        private C1.Win.C1Input.C1ComboBox c1ComboBox2;
        private C1.Win.C1Input.C1ComboBox c1ComboBox1;
        private C1.Win.C1Input.C1Button c1Button1;
        private C1.Win.C1Command.C1DockingTab c1DockingTab2;
        private C1.Win.C1Command.C1DockingTabPage c1DockingTabPage2;
        private C1.Win.C1Ribbon.C1StatusBar c1StatusBar1;
        private C1.Win.C1Command.C1DockingTab c1DockingTab3;
        private C1.Win.C1Command.C1DockingTabPage c1DockingTabPage3;
        private C1.Win.C1List.C1List c1List2;
        private System.Windows.Forms.BindingSource marketDataBindingSource;
        private C1.Win.C1Command.C1CommandMenu c1CommandMenu2;
        private C1.Win.C1Command.C1CommandLink c1CommandLink4;
        private C1.Win.C1Command.C1Command c1Command3;
        private C1.Win.C1Command.C1CommandLink c1CommandLink3;
    }
}


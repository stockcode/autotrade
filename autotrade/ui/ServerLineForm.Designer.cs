namespace autotrade.ui
{
    partial class ServerLineForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServerLineForm));
            this.label1 = new System.Windows.Forms.Label();
            this.lblBrokerId = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.lblTrade = new System.Windows.Forms.Label();
            this.fgTrade = new C1.Win.C1FlexGrid.C1FlexGrid();
            this.fgMarket = new C1.Win.C1FlexGrid.C1FlexGrid();
            this.lblMarket = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.btnOK = new C1.Win.C1Input.C1Button();
            this.btnCancel = new C1.Win.C1Input.C1Button();
            this.c1CommandHolder1 = new C1.Win.C1Command.C1CommandHolder();
            ((System.ComponentModel.ISupportInitialize)(this.fgTrade)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fgMarket)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1CommandHolder1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(70, 31);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "BrokerId:";
            // 
            // lblBrokerId
            // 
            this.lblBrokerId.Enabled = false;
            this.lblBrokerId.Location = new System.Drawing.Point(134, 29);
            this.lblBrokerId.Margin = new System.Windows.Forms.Padding(2);
            this.lblBrokerId.Name = "lblBrokerId";
            this.lblBrokerId.Size = new System.Drawing.Size(76, 21);
            this.lblBrokerId.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 63);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "交易服务";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // lblTrade
            // 
            this.lblTrade.Location = new System.Drawing.Point(82, 63);
            this.lblTrade.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblTrade.Name = "lblTrade";
            this.lblTrade.Size = new System.Drawing.Size(188, 18);
            this.lblTrade.TabIndex = 3;
            this.lblTrade.Click += new System.EventHandler(this.label3_Click);
            // 
            // fgTrade
            // 
            this.fgTrade.AllowEditing = false;
            this.fgTrade.ColumnInfo = resources.GetString("fgTrade.ColumnInfo");
            this.fgTrade.Location = new System.Drawing.Point(22, 92);
            this.fgTrade.Margin = new System.Windows.Forms.Padding(2);
            this.fgTrade.Name = "fgTrade";
            this.fgTrade.Rows.DefaultSize = 24;
            this.fgTrade.SelectionMode = C1.Win.C1FlexGrid.SelectionModeEnum.ListBox;
            this.fgTrade.Size = new System.Drawing.Size(258, 121);
            this.fgTrade.TabIndex = 4;
            this.fgTrade.RowColChange += new System.EventHandler(this.fgTrade_RowColChange);
            // 
            // fgMarket
            // 
            this.fgMarket.AllowEditing = false;
            this.fgMarket.ColumnInfo = resources.GetString("fgMarket.ColumnInfo");
            this.fgMarket.Location = new System.Drawing.Point(22, 270);
            this.fgMarket.Margin = new System.Windows.Forms.Padding(2);
            this.fgMarket.Name = "fgMarket";
            this.fgMarket.Rows.DefaultSize = 24;
            this.fgMarket.SelectionMode = C1.Win.C1FlexGrid.SelectionModeEnum.ListBox;
            this.fgMarket.Size = new System.Drawing.Size(258, 121);
            this.fgMarket.TabIndex = 7;
            this.fgMarket.RowColChange += new System.EventHandler(this.fgMarket_RowColChange);
            // 
            // lblMarket
            // 
            this.lblMarket.Location = new System.Drawing.Point(82, 242);
            this.lblMarket.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblMarket.Name = "lblMarket";
            this.lblMarket.Size = new System.Drawing.Size(188, 18);
            this.lblMarket.TabIndex = 6;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(20, 242);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 5;
            this.label5.Text = "行情服务";
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(178, 413);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 8;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(44, 413);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 9;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // c1CommandHolder1
            // 
            this.c1CommandHolder1.Owner = this;
            // 
            // ServerLineForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(308, 452);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.fgMarket);
            this.Controls.Add(this.lblMarket);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.fgTrade);
            this.Controls.Add(this.lblTrade);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblBrokerId);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "ServerLineForm";
            this.Text = "ServerLineForm";
            this.Load += new System.EventHandler(this.ServerLineForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.fgTrade)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fgMarket)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1CommandHolder1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox lblBrokerId;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblTrade;
        private C1.Win.C1FlexGrid.C1FlexGrid fgTrade;
        private C1.Win.C1FlexGrid.C1FlexGrid fgMarket;
        private System.Windows.Forms.Label lblMarket;
        private System.Windows.Forms.Label label5;
        private C1.Win.C1Input.C1Button btnOK;
        private C1.Win.C1Input.C1Button btnCancel;
        private C1.Win.C1Command.C1CommandHolder c1CommandHolder1;
    }
}
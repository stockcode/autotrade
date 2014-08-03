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
            this.label1 = new System.Windows.Forms.Label();
            this.lblBrokerId = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.lblTrade = new System.Windows.Forms.Label();
            this.lblMarket = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.gvMarket = new Telerik.WinControls.UI.RadGridView();
            this.gvTrade = new Telerik.WinControls.UI.RadGridView();
            this.btnCancel = new Telerik.WinControls.UI.RadButton();
            this.btnOK = new Telerik.WinControls.UI.RadButton();
            ((System.ComponentModel.ISupportInitialize)(this.gvMarket)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvMarket.MasterTemplate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvTrade)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvTrade.MasterTemplate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnCancel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnOK)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(93, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "BrokerId:";
            // 
            // lblBrokerId
            // 
            this.lblBrokerId.Enabled = false;
            this.lblBrokerId.Location = new System.Drawing.Point(179, 36);
            this.lblBrokerId.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lblBrokerId.Name = "lblBrokerId";
            this.lblBrokerId.Size = new System.Drawing.Size(100, 25);
            this.lblBrokerId.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(27, 79);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "交易服务";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // lblTrade
            // 
            this.lblTrade.Location = new System.Drawing.Point(109, 79);
            this.lblTrade.Name = "lblTrade";
            this.lblTrade.Size = new System.Drawing.Size(251, 22);
            this.lblTrade.TabIndex = 3;
            this.lblTrade.Click += new System.EventHandler(this.label3_Click);
            // 
            // lblMarket
            // 
            this.lblMarket.Location = new System.Drawing.Point(109, 302);
            this.lblMarket.Name = "lblMarket";
            this.lblMarket.Size = new System.Drawing.Size(251, 22);
            this.lblMarket.TabIndex = 6;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(27, 302);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(67, 15);
            this.label5.TabIndex = 5;
            this.label5.Text = "行情服务";
            // 
            // gvMarket
            // 
            this.gvMarket.Location = new System.Drawing.Point(55, 105);
            this.gvMarket.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.gvMarket.Name = "gvMarket";
            this.gvMarket.Size = new System.Drawing.Size(320, 188);
            this.gvMarket.TabIndex = 7;
            this.gvMarket.Text = "gvMarket";
            this.gvMarket.SelectionChanged += new System.EventHandler(this.gvMarket_SelectionChanged);
            // 
            // gvTrade
            // 
            this.gvTrade.Location = new System.Drawing.Point(55, 329);
            this.gvTrade.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.gvTrade.Name = "gvTrade";
            this.gvTrade.Size = new System.Drawing.Size(320, 188);
            this.gvTrade.TabIndex = 8;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(268, 524);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(107, 30);
            this.btnCancel.TabIndex = 9;
            this.btnCancel.Text = "取消";
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(55, 524);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(103, 30);
            this.btnOK.TabIndex = 10;
            this.btnOK.Text = "确定";
            // 
            // ServerLineForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(411, 565);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.gvTrade);
            this.Controls.Add(this.gvMarket);
            this.Controls.Add(this.lblMarket);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.lblTrade);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblBrokerId);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "ServerLineForm";
            this.Text = "ServerLineForm";
            this.Load += new System.EventHandler(this.ServerLineForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gvMarket.MasterTemplate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvMarket)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvTrade.MasterTemplate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvTrade)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnCancel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnOK)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox lblBrokerId;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblTrade;
        private System.Windows.Forms.Label lblMarket;
        private System.Windows.Forms.Label label5;
        private Telerik.WinControls.UI.RadGridView gvMarket;
        private Telerik.WinControls.UI.RadGridView gvTrade;
        private Telerik.WinControls.UI.RadButton btnCancel;
        private Telerik.WinControls.UI.RadButton btnOK;
    }
}
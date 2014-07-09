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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.fgTrade = new C1.Win.C1FlexGrid.C1FlexGrid();
            this.fgMarket = new C1.Win.C1FlexGrid.C1FlexGrid();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.fgTrade)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fgMarket)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(94, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "BrokerId:";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(179, 36);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 25);
            this.textBox1.TabIndex = 1;
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
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(109, 79);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(251, 23);
            this.label3.TabIndex = 3;
            this.label3.Text = "lblTrade";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // fgTrade
            // 
            this.fgTrade.AllowEditing = false;
            this.fgTrade.ColumnInfo = resources.GetString("fgTrade.ColumnInfo");
            this.fgTrade.Location = new System.Drawing.Point(30, 115);
            this.fgTrade.Name = "fgTrade";
            this.fgTrade.Rows.DefaultSize = 24;
            this.fgTrade.SelectionMode = C1.Win.C1FlexGrid.SelectionModeEnum.ListBox;
            this.fgTrade.Size = new System.Drawing.Size(310, 150);
            this.fgTrade.TabIndex = 4;
            this.fgTrade.RowColChange += new System.EventHandler(this.fgTrade_RowColChange);
            // 
            // fgMarket
            // 
            this.fgMarket.AllowEditing = false;
            this.fgMarket.ColumnInfo = resources.GetString("fgMarket.ColumnInfo");
            this.fgMarket.Location = new System.Drawing.Point(30, 338);
            this.fgMarket.Name = "fgMarket";
            this.fgMarket.Rows.DefaultSize = 24;
            this.fgMarket.SelectionMode = C1.Win.C1FlexGrid.SelectionModeEnum.ListBox;
            this.fgMarket.Size = new System.Drawing.Size(310, 150);
            this.fgMarket.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(109, 302);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(251, 23);
            this.label4.TabIndex = 6;
            this.label4.Text = "lblMarket";
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
            // ServerLineForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(411, 525);
            this.Controls.Add(this.fgMarket);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.fgTrade);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label1);
            this.Name = "ServerLineForm";
            this.Text = "ServerLineForm";
            this.Load += new System.EventHandler(this.ServerLineForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.fgTrade)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fgMarket)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private C1.Win.C1FlexGrid.C1FlexGrid fgTrade;
        private C1.Win.C1FlexGrid.C1FlexGrid fgMarket;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
    }
}
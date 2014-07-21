namespace autotrade
{
    partial class LoginForm
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
            this.tbInvestorID = new System.Windows.Forms.TextBox();
            this.tbPasswd = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnLogin = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.cbServer = new System.Windows.Forms.ComboBox();
            this.btnDetail = new Telerik.WinControls.UI.RadButton();
            ((System.ComponentModel.ISupportInitialize)(this.btnDetail)).BeginInit();
            this.SuspendLayout();
            // 
            // tbInvestorID
            // 
            this.tbInvestorID.Location = new System.Drawing.Point(81, 69);
            this.tbInvestorID.Margin = new System.Windows.Forms.Padding(2);
            this.tbInvestorID.Name = "tbInvestorID";
            this.tbInvestorID.Size = new System.Drawing.Size(155, 21);
            this.tbInvestorID.TabIndex = 0;
            // 
            // tbPasswd
            // 
            this.tbPasswd.Location = new System.Drawing.Point(81, 105);
            this.tbPasswd.Margin = new System.Windows.Forms.Padding(2);
            this.tbPasswd.Name = "tbPasswd";
            this.tbPasswd.PasswordChar = '*';
            this.tbPasswd.Size = new System.Drawing.Size(155, 21);
            this.tbPasswd.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(38, 71);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "账号：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(38, 107);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "密码：";
            // 
            // btnLogin
            // 
            this.btnLogin.Location = new System.Drawing.Point(239, 88);
            this.btnLogin.Margin = new System.Windows.Forms.Padding(2);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(56, 18);
            this.btnLogin.TabIndex = 4;
            this.btnLogin.Text = "登录";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 140);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 12);
            this.label3.TabIndex = 6;
            this.label3.Text = "选择服务器：";
            // 
            // cbServer
            // 
            this.cbServer.FormattingEnabled = true;
            this.cbServer.Location = new System.Drawing.Point(81, 136);
            this.cbServer.Name = "cbServer";
            this.cbServer.Size = new System.Drawing.Size(152, 20);
            this.cbServer.TabIndex = 11;
            this.cbServer.SelectedIndexChanged += new System.EventHandler(this.cbServer_SelectedItemChanged);
            // 
            // btnDetail
            // 
            this.btnDetail.Location = new System.Drawing.Point(239, 136);
            this.btnDetail.Name = "btnDetail";
            this.btnDetail.Size = new System.Drawing.Size(69, 24);
            this.btnDetail.TabIndex = 12;
            this.btnDetail.Text = "详细配置";
            this.btnDetail.Click += new System.EventHandler(this.btnDetail_Click);
            // 
            // LoginForm
            // 
            this.AcceptButton = this.btnLogin;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(320, 175);
            this.Controls.Add(this.btnDetail);
            this.Controls.Add(this.cbServer);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnLogin);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbPasswd);
            this.Controls.Add(this.tbInvestorID);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "LoginForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "LoginForm";
            this.Load += new System.EventHandler(this.LoginForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.btnDetail)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbInvestorID;
        private System.Windows.Forms.TextBox tbPasswd;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cbServer;
        private Telerik.WinControls.UI.RadButton btnDetail;
    }
}
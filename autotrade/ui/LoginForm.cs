using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using autotrade.ui;

namespace autotrade
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            tbInvestorID.Text = Properties.Settings.Default.InvestorID;
            tbPasswd.Text = Properties.Settings.Default.Passwd;
            cbServer.Text = Properties.Settings.Default.ServerLine;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnDetail_Click(object sender, EventArgs e)
        {
            ServerLineForm serverLineForm = new ServerLineForm();
            serverLineForm.ShowDialog();
        }
    }
}

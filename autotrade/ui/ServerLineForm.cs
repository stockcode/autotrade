using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using C1.Win.C1FlexGrid;

namespace autotrade.ui
{
    public partial class ServerLineForm : Form
    {
        public string serverLine { get; set; }

        public ServerLineForm()
        {
            InitializeComponent();
        }

        private void ServerLineForm_Load(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;

            fgTrade[1,1] = settings.dlqh_dianxinIP1;
            fgTrade[1,2] = settings.dlqh_dianxinPort1;

            fgTrade[2,1] = settings.dlqh_dianxinIP2;
            fgTrade[2,2] = settings.dlqh_dianxinPort2;

            Row row = fgTrade.Rows[1];
            label3.Text = "tcp://" + row[1] + ":" + row[2];
        }

        private void fgTrade_RowColChange(object sender, EventArgs e)
        {
            Row row = fgTrade.Rows.Selected[0];
            label3.Text = "tcp://" + row[1] + ":" + row[2];
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }
}

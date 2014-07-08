using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace autotrade.ui
{
    public partial class ServerLineForm : Form
    {
        public ServerLineForm()
        {
            InitializeComponent();
        }

        private void ServerLineForm_Load(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;

            ListViewItem item = new ListViewItem(settings.dlqh_dianxinIP1);
            item.SubItems.Add(settings.dlqh_dianxinPort1);
            lvTradeAddr.Items.Add(item);

            item = new ListViewItem(settings.dlqh_dianxinIP2);
            item.SubItems.Add(settings.dlqh_dianxinPort2);
            lvTradeAddr.Items.Add(item);
        }
    }
}

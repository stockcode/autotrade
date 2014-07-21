using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
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
        Properties.Settings settings = Properties.Settings.Default;

        public string serverLine { get; set; }

        public ServerLineForm()
        {
            InitializeComponent();
        }

        private void ServerLineForm_Load(object sender, EventArgs e)
        {
            

            lblBrokerId.Text = settings[serverLine + "BrokerID"].ToString();
            string selectedTrade = settings["Selected" + serverLine + "Trade"].ToString();
            string selectedMarket = settings["Selected" + serverLine + "Market"].ToString();

            int index = 1;
            while (true)
            {
                try
                {
                    string line = settings[serverLine + "Trade" + index].ToString();
                    //fgTrade[index, 1] = line.Split(':')[0];
                    //fgTrade[index, 2] = line.Split(':')[1];

                    //if (line == selectedTrade) fgTrade.Rows[index].Selected = true;
                    //else fgTrade.Rows[index].Selected = false;

                    index++;
                }
                catch (SettingsPropertyNotFoundException)
                {
                    break;
                }
                
            }

            //Row row = fgTrade.Rows.Selected[0];
            //lblTrade.Text = "tcp://" + row[1] + ":" + row[2];

            index = 1;
            while (true)
            {
                try
                {
                    string line = settings[serverLine + "Market" + index].ToString();
                    //fgMarket[index, 1] = line.Split(':')[0];
                    //fgMarket[index, 2] = line.Split(':')[1];

                    //if (line == selectedMarket) fgMarket.Rows[index].Selected = true;
                    //else fgMarket.Rows[index].Selected = false;

                    index++;
                }
                catch (SettingsPropertyNotFoundException)
                {
                    break;
                }

            }

            //row = fgMarket.Rows.Selected[0];
            //lblMarket.Text = "tcp://" + row[1] + ":" + row[2];
        }

        private void fgTrade_RowColChange(object sender, EventArgs e)
        {
            //Row row = fgTrade.Rows.Selected[0];
            //lblTrade.Text = "tcp://" + row[1] + ":" + row[2];
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            //Row row = fgTrade.Rows.Selected[0];

            //settings["Selected" + serverLine + "Trade"] = row[1] + ":" + row[2];

            //row = fgMarket.Rows.Selected[0];

            //settings["Selected" + serverLine + "Market"] = row[1] + ":" + row[2];

            //settings.Save();
        }

        private void fgMarket_RowColChange(object sender, EventArgs e)
        {
            //Row row = fgMarket.Rows.Selected[0];
            //lblMarket.Text = "tcp://" + row[1] + ":" + row[2];
        }
    }
}

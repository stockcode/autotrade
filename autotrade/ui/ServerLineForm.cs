using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using autotrade.Config;
using log4net;

namespace autotrade.ui
{
    public partial class ServerLineForm : Form
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        
        public BrokerManager brokerManager { get; set; }

        public ServerLineForm()
        {
            InitializeComponent();
        }

        private void ServerLineForm_Load(object sender, EventArgs e)
        {
            BrokerManagerNetworkInfo broker = brokerManager.GetHotBroker();

            lblBrokerId.Text = broker.BrokerId.ToString();

            gvTrade.DataSource = broker.NetLine.Where(line => line.type == "TD");
            gvTrade.BestFitColumns();

            gvMarket.DataSource = broker.NetLine.Where(line => line.type == "MD");
            gvMarket.BestFitColumns();

//            string selectedTrade = settings["Selected" + serverLine + "Trade"].ToString();
//            string selectedMarket = settings["Selected" + serverLine + "Market"].ToString();
//
//            int index = 1;
//            while (true)
//            {
//                try
//                {
//                    string line = settings[serverLine + "Trade" + index].ToString();
//                    
//                    //gvTrade.Rows[index].Cells[1].Value = line.Split(':')[0];
//                    //gvTrade.Rows[index].Cells[2].Value = line.Split(':')[1];
//
//                    //if (line == selectedTrade) gvTrade.Rows[index].IsSelected = true;
//                    //else gvTrade.Rows[index].IsSelected = false;
//
//                    index++;
//                }
//                catch (SettingsPropertyNotFoundException)
//                {
//                    break;
//                }
//                
//            }
//
//            //Row row = fgTrade.Rows.Selected[0];
//            //lblTrade.Text = "tcp://" + row[1] + ":" + row[2];
//
//            index = 1;
//            while (true)
//            {
//                try
//                {
//                    string line = settings[serverLine + "Market" + index].ToString();
//                    //fgMarket[index, 1] = line.Split(':')[0];
//                    //fgMarket[index, 2] = line.Split(':')[1];
//
//                    //if (line == selectedMarket) fgMarket.Rows[index].Selected = true;
//                    //else fgMarket.Rows[index].Selected = false;
//
//                    index++;
//                }
//                catch (SettingsPropertyNotFoundException)
//                {
//                    break;
//                }
//
//            }

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

        private void gvMarket_SelectionChanged(object sender, EventArgs e)
        {
            log.Info(e.ToString());
        }

    }
}

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using autotrade.Config;
using autotrade.Properties;
using autotrade.ui;
using autotrade.util;
using log4net;
using QuantBox.CSharp2CTP;
using QuantBox.CSharp2CTP.Event;
using QuantBox.Library;
using Telerik.WinControls.UI;

namespace autotrade
{
    public partial class LoginForm : ShapedForm
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Settings settings = Settings.Default;

        private BrokerManager brokerManager;

        private string brokerFile;

        public LoginForm()
        {
            InitializeComponent();
        }

        public MdApiWrapper mdApi { get; set; }
        public TraderApiWrapper tradeApi { get; set; }

        private void btnLogin_Click(object sender, EventArgs e)
        {

            radProgressBar1.Value1 = 0;

            settings.InvestorID = tbInvestorID.Text;
            settings.Passwd = tbPasswd.Text;
            settings.Save();

            string brokerId = brokerManager.GetHotBroker().BrokerId.ToString();

            string url = brokerManager.GetHotMarketUrl();

            mdApi = new MdApiWrapper();

            mdApi.OnConnect += mdApi_OnConnect;
            mdApi.Connect("", url, brokerId, settings.InvestorID, settings.Passwd);
        }

        private void tradeApi_OnConnect(object sender, OnConnectArgs e)
        {
            log.Info("Trade Connected:" + e.pRspUserLogin);

            radProgressBar1.Text = Enumerations.GetEnumDescription(e.result);
            radProgressBar1.Value1++;

            if (e.result == ConnectionStatus.Logined)
            {
                tradeApi.MaxOrderRef = Convert.ToInt32(e.pRspUserLogin.MaxOrderRef);                
            }

            if (e.result == ConnectionStatus.Confirmed)
            {
                DialogResult = DialogResult.OK;
            }
        }

        private void mdApi_OnConnect(object sender, OnConnectArgs e)
        {
            log.Info("Market Connected:" + e.result);
            radProgressBar1.Text = Enumerations.GetEnumDescription(e.result); ;
            radProgressBar1.Value1++;

            if (e.result == ConnectionStatus.Logined)
            {
                string brokerId = brokerManager.GetHotBroker().BrokerId.ToString();
                string url = brokerManager.GetHotTradeUrl();

                tradeApi = new TraderApiWrapper();

                tradeApi.OnConnect += tradeApi_OnConnect;
                tradeApi.Connect("", url, brokerId, settings.InvestorID, settings.Passwd,
                    THOST_TE_RESUME_TYPE.THOST_TERT_RESUME, "vicky", "");
            }
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            brokerFile = Application.StartupPath + @"\Broker.xml";
            string xml = File.ReadAllText(brokerFile);
            brokerManager = xml.ParseXML<BrokerManager>();

            foreach (var broker in brokerManager.NetworkInfo)
            {
                var item = new RadListDataItem(broker.name);                
                if (broker.isHot == 1) item.Selected = true;

                cbServer.Items.Add(item);
            }
            
            tbInvestorID.Text = settings.InvestorID;
            tbPasswd.Text = settings.Passwd;
        }


        private void btnDetail_Click(object sender, EventArgs e)
        {
            var serverLineForm = new ServerLineForm();
            serverLineForm.brokerManager = brokerManager;
            if (serverLineForm.ShowDialog() == DialogResult.OK)
            {
                brokerManager.SaveToFile(brokerFile);
            }
        }

        private void cbServer_SelectedIndexChanged(object sender, Telerik.WinControls.UI.Data.PositionChangedEventArgs e)
        {
            brokerManager.SetHotBroker(cbServer.SelectedText);
            brokerManager.SaveToFile(brokerFile);
        }
    }
}
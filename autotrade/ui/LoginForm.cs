using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using autotrade.ui;
using QuantBox.CSharp2CTP;
using QuantBox.CSharp2CTP.Event;

namespace autotrade
{
    public partial class LoginForm : Telerik.WinControls.UI.ShapedForm
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        Properties.Settings settings = Properties.Settings.Default;

        public MdApiWrapper mdApi { get; set; }
        public TraderApiWrapper tradeApi { get; set; }

        public bool isMarketConnected = false;
        public bool isTradeConnected = false;

        public LoginForm()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            settings.InvestorID = tbInvestorID.Text;
            settings.Passwd = tbPasswd.Text;
            settings.Save();

            string brokerId = settings[settings.SelectedServerLine + "BrokerID"].ToString();

            string url = "tcp://" + settings["Selected" + settings.SelectedServerLine + "Market"];

            mdApi = new MdApiWrapper();

            mdApi.OnConnect += mdApi_OnConnect;
            mdApi.Connect(@"D:\", settings.InvestorID, settings.Passwd, brokerId, url);

            url = "tcp://" + settings["Selected" + settings.SelectedServerLine + "Trade"];

            tradeApi = new TraderApiWrapper();

            tradeApi.OnConnect += tradeApi_OnConnect;
            tradeApi.Connect(@"D:\",  settings.InvestorID, settings.Passwd, brokerId, url, THOST_TE_RESUME_TYPE.THOST_TERT_RESUME, "vicky", "");
        }

        void tradeApi_OnConnect(object sender, OnConnectArgs e)
        {
            log.Info("Trade Connected");

            isTradeConnected = true;
            startMainForm();
        }

        void mdApi_OnConnect(object sender, OnConnectArgs e)
        {
            log.Info("Market Connected");

            isMarketConnected = true;
            startMainForm();
        }

        

        private void LoginForm_Load(object sender, EventArgs e)
        {
            var lines = settings.ServerLines;
            string[] serverLines = new string[lines.Count];
            lines.CopyTo(serverLines, 0);
            cbServer.Items.AddRange(serverLines);

            tbInvestorID.Text = settings.InvestorID;
            tbPasswd.Text = settings.Passwd;
            cbServer.Text = settings.SelectedServerLine;
        }

        

        private void startMainForm()
        {
            if (isMarketConnected && isTradeConnected)
            {
                this.DialogResult = DialogResult.OK;
            }
        }


        private void btnDetail_Click(object sender, EventArgs e)
        {
            ServerLineForm serverLineForm = new ServerLineForm();
            serverLineForm.serverLine = cbServer.Text;
            serverLineForm.ShowDialog();
        }

        private void cbServer_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.SelectedServerLine = cbServer.SelectedItem.ToString();
            Properties.Settings.Default.Save();
        }
    }
}

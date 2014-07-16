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
using CTPMdApi;
using CTPTradeApi;

namespace autotrade
{
    public partial class LoginForm : Form
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        Properties.Settings settings = Properties.Settings.Default;

        public MdApi mdApi { get; set; }
        public TradeApi tradeApi { get; set; }

        public bool isMarketConnected = false;
        public bool isTradeConnected = false;

        public LoginForm()
        {
            InitializeComponent();
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

        private void btnLogin_Click(object sender, EventArgs e)
        {
            settings.InvestorID = tbInvestorID.Text;
            settings.Passwd = tbPasswd.Text;
            settings.Save();

            string brokerId = settings[settings.SelectedServerLine + "BrokerID"].ToString();

            string url = "tcp://" + settings["Selected" + settings.SelectedServerLine + "Market"];

            mdApi = new MdApi(settings.InvestorID, settings.Passwd, brokerId, url);
            
            mdApi.OnFrontConnected +=mdApi_OnFrontConnected;
            mdApi.OnRspUserLogin +=mdApi_OnRspUserLogin;
            mdApi.Connect();

            url = "tcp://" + settings["Selected" + settings.SelectedServerLine + "Trade"];

            tradeApi = new TradeApi(settings.InvestorID, settings.Passwd, brokerId, url);
            tradeApi.OnFrontConnect += tradeApi_OnFrontConnect;
            tradeApi.OnRspUserLogin += tradeApi_OnRspUserLogin;
            tradeApi.Connect();
        }

        void tradeApi_OnRspUserLogin(ref CTPTradeApi.CThostFtdcRspUserLoginField pRspUserLogin, ref CTPTradeApi.CThostFtdcRspInfoField pRspInfo, int nRequestID, bool bIsLast)
        {
            if (pRspInfo.ErrorID == 0)
            {
                isTradeConnected = true;
                startMainForm();
            }
            log.Info(pRspInfo);
            log.Info(pRspUserLogin);
        }

        void tradeApi_OnFrontConnect()
        {
            log.Info("Trade Connected");
            tradeApi.UserLogin();
        }

        void mdApi_OnRspUserLogin(ref CTPMdApi.CThostFtdcRspUserLoginField pRspUserLogin, ref CTPMdApi.CThostFtdcRspInfoField pRspInfo, int nRequestID, bool bIsLast)
        {
            if (pRspInfo.ErrorID == 0)
            {
                isMarketConnected = true;
                startMainForm();
            }
            log.Info(pRspInfo);
            log.Info(pRspUserLogin);
        }

        private void startMainForm()
        {
            if (isMarketConnected && isTradeConnected)
            {
                this.DialogResult = DialogResult.OK;
            }
        }

        void mdApi_OnFrontConnected()
        {
            log.Info("Market Connected");

            mdApi.UserLogin();
        }

        private void btnDetail_Click(object sender, EventArgs e)
        {
            ServerLineForm serverLineForm = new ServerLineForm();
            serverLineForm.serverLine = cbServer.Text;
            serverLineForm.ShowDialog();
        }

        private void cbServer_SelectedItemChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.SelectedServerLine = cbServer.SelectedItem.ToString();
            Properties.Settings.Default.Save();            
        }
    }
}

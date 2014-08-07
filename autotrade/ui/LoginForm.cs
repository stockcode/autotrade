using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using autotrade.business;
using autotrade.Config;
using autotrade.model;
using autotrade.Properties;
using autotrade.ui;
using autotrade.util;
using log4net;
using MongoDB.Driver.Linq;
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

        public MarketManager MarketManager { get; set; }

        public OrderManager OrderManager { get; set; }

        private string brokerFile;

        private bool noOrder = true;

        public LoginForm()
        {
            InitializeComponent();
        }

        public MdApiWrapper mdApi { get; set; }
        public TraderApiWrapper tradeApi { get; set; }

        public AccountManager AccountManager { get; set; }

        private void btnLogin_Click(object sender, EventArgs e)
        {

            radProgressBar1.Value1 = 0;

            settings.InvestorID = tbInvestorID.Text;
            settings.Passwd = tbPasswd.Text;
            settings.Save();

            string brokerId = brokerManager.GetHotBroker().BrokerId.ToString();

            string url = brokerManager.GetHotMarketUrl();

            mdApi.Connect("", url, brokerId, settings.InvestorID, settings.Passwd);
        }

        private void tradeApi_OnConnect(object sender, OnConnectArgs e)
        {
            log.Info("Trade Connected:" + e.pRspUserLogin);

            ShowProgress(Enumerations.GetEnumDescription(e.result));

            if (e.result == ConnectionStatus.Logined)
            {
                tradeApi.MaxOrderRef = Convert.ToInt32(e.pRspUserLogin.MaxOrderRef);
                tradeApi.FrontID = e.pRspUserLogin.FrontID;
                tradeApi.SessionID = e.pRspUserLogin.SessionID;
                tradeApi.TradingDay = e.pRspUserLogin.TradingDay;
            }

            if (e.result == ConnectionStatus.Confirmed)
            {
                //tradeApi.ReqQryInstrument("");
                tradeApi.ReqQryTradingAccount();
            }
        }

        private void ShowProgress(String info)
        {
            log.Info(info);
            radProgressBar1.Text = info;
            radProgressBar1.Value1++;
        }

        private void mdApi_OnConnect(object sender, OnConnectArgs e)
        {
            log.Info("Market Connected:" + e.result);
            ShowProgress(Enumerations.GetEnumDescription(e.result));

            if (e.result == ConnectionStatus.Logined)
            {
                string brokerId = brokerManager.GetHotBroker().BrokerId.ToString();
                string url = brokerManager.GetHotTradeUrl();

                
                tradeApi.Connect("", url, brokerId, settings.InvestorID, settings.Passwd,
                    THOST_TE_RESUME_TYPE.THOST_TERT_QUICK, "vicky", "");
            }
        }

        void tradeApi_OnRspQryInstrument(object sender, OnRspQryInstrumentArgs e)
        {
//            Instrument instrument = new Instrument();
//            ObjectUtils.CopyStruct(e.pInstrument, instrument);
//            if (instrument.InstrumentID.Contains(" ")) return;
            
//            if (!InstrumentManager.instruments.Any(o => o.InstrumentID == instrument.InstrumentID))
//            {
//                InstrumentManager.instruments.Add(instrument);
//                log.Info(e.pInstrument.InstrumentID + ":" + e.pInstrument.LongMarginRatio + ":" +
//                         e.pInstrument.ShortMarginRatio);
//            }

            if (e.bIsLast)
            {
                ShowProgress("查询合约成功");

                tradeApi.ReqQryTradingAccount();
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

            mdApi.OnConnect += mdApi_OnConnect;


            tradeApi.OnConnect += tradeApi_OnConnect;
            tradeApi.OnRspQryInstrument += tradeApi_OnRspQryInstrument;
            tradeApi.OnRspQryTradingAccount += tradeApi_OnRspQryTradingAccount;
            tradeApi.OnRspQryOrder += tradeApi_OnRspQryOrder;
            tradeApi.OnRspQryTrade += tradeApi_OnRspQryTrade;
        }

        void tradeApi_OnRspQryTrade(object sender, OnRspQryTradeArgs e)
        {
            TradeRecord tradeRecord = new TradeRecord();

            ObjectUtils.CopyStruct(e.pTrade, tradeRecord);

            OrderManager.AddTradeRecord(tradeRecord);

            OrderManager.OrderRepository.UpdateTradeID(e.pTrade);

            if (e.bIsLast)
            {
                ShowProgress("查询成交单成功");
                DialogResult = DialogResult.OK;
            }
        }

        void tradeApi_OnRspQryOrder(object sender, OnRspQryOrderArgs e)
        {
            if (e.pRspInfo.ErrorID == 0)
            {
                noOrder = false;

                OrderRecord orderRecord = new OrderRecord();

                ObjectUtils.CopyStruct(e.pOrder, orderRecord);

                OrderManager.AddOrderRecord(orderRecord);

                if (e.bIsLast)
                {                    
                    ShowProgress("查询委托单成功");

                    tradeApi.ReqQryTrade();
                } 
            }
        }

        void tradeApi_OnRspQryTradingAccount(object sender, OnRspQryTradingAccountArgs e)
        {
            if (e.pRspInfo.ErrorID == 0)
            {
                ObjectUtils.CopyStruct(e.pTradingAccount, AccountManager.Accounts[0]);

                ShowProgress("查询账户成功");

                tradeApi.ReqQryOrder();

                Thread.Sleep(3000);

                if (noOrder) DialogResult = DialogResult.OK;                
            }
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
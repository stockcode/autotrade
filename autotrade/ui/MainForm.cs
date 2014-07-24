using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telerik.WinControls.UI;
using autotrade.Strategies;
using autotrade.business;
using autotrade.converter;
using autotrade.model;
using CTPMdApi;
using CTPTradeApi;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using autotrade.ui;
using CThostFtdcContractBankField = CTPTradeApi.CThostFtdcContractBankField;
using CThostFtdcInputOrderField = CTPTradeApi.CThostFtdcInputOrderField;
using CThostFtdcOrderField = CTPTradeApi.CThostFtdcOrderField;
using CThostFtdcReqQueryAccountField = CTPTradeApi.CThostFtdcReqQueryAccountField;
using CThostFtdcRspInfoField = CTPTradeApi.CThostFtdcRspInfoField;
using CThostFtdcTradingAccountField = CTPTradeApi.CThostFtdcTradingAccountField;
using EnumDirectionType = CTPTradeApi.EnumDirectionType;
using EnumOffsetFlagType = CTPTradeApi.EnumOffsetFlagType;
using System.Threading;

namespace autotrade
{
    public partial class MainForm : RadForm
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        

        public MdApi mdApi { get; set; }
        public TradeApi tradeApi { get; set; }

        private AboveMAStrategy maReverseStrategy;

        private OrderManager _orderManager;
        private AccountManager _accountManager;
        private MarketManager _marketManager;
        private StrategyManager _strategyManager;
        private InstrumentManager _instrumentManager;
        private IndicatorManager _indicatorManager = new IndicatorManager();
        
        public MainForm()
        {
            InitializeComponent();
        }

        
        
        

        void tradeApi_OnRspQryInvestorPositionCombineDetail(ref CTPTradeApi.CThostFtdcInvestorPositionCombineDetailField pInvestorPositionCombineDetail, ref CThostFtdcRspInfoField pRspInfo, int nRequestID, bool bIsLast)
        {
            log.Info(pRspInfo);
            log.Info(pInvestorPositionCombineDetail);
        }
        

        private void tradeApi_OnRspQryContractBank(ref CThostFtdcContractBankField pcontractbank, ref CThostFtdcRspInfoField prspinfo, int nrequestid, bool bislast)
        {
            log.Info(prspinfo);
            log.Info(pcontractbank);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //tradeApi.QryInvestorPosition();
            //tradeApi.QryInvestorPositionDetail();
            tradeApi.QryInvestorPositionCombinaDetail();
        }

 
        private void MainForm_Load(object sender, EventArgs e)
        {
            //radGridView4.Columns["Direction"].DataTypeConverter = new DirectionConverter();

            LoginForm loginForm = new LoginForm();
            
            if (loginForm.ShowDialog() != DialogResult.OK) Close();

            mdApi = loginForm.mdApi;
            tradeApi = loginForm.tradeApi;

            if (tradeApi == null)
            {
                Application.Exit();
                return;
            }

            _instrumentManager = new InstrumentManager();
            _accountManager = new AccountManager(tradeApi);

            _orderManager = new OrderManager(tradeApi);
            _orderManager.InstrumentManager = _instrumentManager;
            _orderManager.AccountManager = _accountManager;
            
            
            _marketManager = new MarketManager(mdApi);

            _strategyManager = new StrategyManager(_orderManager);

            _strategyManager.indicatorManager = _indicatorManager;

            _marketManager.strategyManager = _strategyManager;
            _marketManager.orderManager = _orderManager;
            _marketManager.indicatorManager = _indicatorManager;

            _accountManager.OnQryTradingAccount += accountManager_OnQryTradingAccount;

            _orderManager.OnRtnTradeRecord += orderManager_OnRtnTradeRecord;
            _orderManager.OnRspQryPositionDetail += orderManager_OnRspQryPositionDetail;
            _orderManager.OnRspQryPositionRecord += _orderManager_OnRspQryPositionRecord;
            _orderManager.OnRspQryOrderRecord += _orderManager_OnRspQryOrderRecord;
            _orderManager.OnRspQryOrder += _orderManager_OnRspQryOrder;

            radGridView8.DataSource = _accountManager.Accounts;

            Task.Factory.StartNew(() => {
                
                _orderManager.QryOrder();

                Thread.Sleep(1000);

                _orderManager.QryTrade();
                
            }).ContinueWith(obj =>
            {
                    Thread.Sleep(1000);
                    _accountManager.QryTradingAccount();

            });



            string[] ppInstrumentID = new string[Properties.Settings.Default.INID.Count];
            
            ppInstrumentID[0] = "IF1408";

            for (int i = 1; i < ppInstrumentID.Count(); i++)
            {
                ppInstrumentID[i] = Properties.Settings.Default.INID[i].ToLower();                
            }

            foreach (string id in ppInstrumentID)
            {
                MarketData    marketData = new MarketData(id);
                _marketManager.marketDatas.Add(marketData);
                _marketManager.instrumentDictionary.Add(id, marketData);
                
            }

            _marketManager.SubMarketData(ppInstrumentID);            

            this.radGridView2.MasterTemplate.Columns.Clear();
            radGridView2.DataSource = _marketManager.marketDatas;
            radGridView2.BestFitColumns();

            radGridView2.Columns["InstrumentId"].HeaderText = "合约";
            radGridView2.Columns["LastPrice"].HeaderText = "最新价";
            radGridView2.Columns["BidPrice1"].HeaderText = "买价";
            radGridView2.Columns["BidVolume1"].HeaderText = "买量";
            radGridView2.Columns["AskPrice1"].HeaderText = "卖价";
            radGridView2.Columns["AskVolume1"].HeaderText = "卖量";
            radGridView2.Columns["Volume"].HeaderText = "成交量";
            radGridView2.Columns["OpenInterest"].HeaderText = "持仓量";
            radGridView2.Columns["UpperLimitPrice"].HeaderText = "涨停价";
            radGridView2.Columns["LowerLimitPrice"].HeaderText = "跌停价";
            radGridView2.Columns["OpenPrice"].HeaderText = "今开盘";
            radGridView2.Columns["PreSettlementPrice"].HeaderText = "昨结算";
            radGridView2.Columns["HighestPrice"].HeaderText = "最高价";
            radGridView2.Columns["LowestPrice"].HeaderText = "最低价";
            radGridView2.Columns["PreClosePrice"].HeaderText = "昨收盘";
            radGridView2.Columns["Turnover"].HeaderText = "成交额";
            radGridView2.Columns["UpdateTime"].HeaderText = "行情更新时间";
            //radGridView2.LoadLayout("c:\\columns.xml");            
            
        }

        void _orderManager_OnRspQryOrder(object sender, OrderEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(e.methodInvoker);
            }
        }


        private delegate void RecordRecordDelegate(BindingList<OrderRecord> orderRecords);
        private void ShowOrderRecord(BindingList<OrderRecord> orderRecords)
        {
            radGridView6.DataSource = orderRecords;


            radGridView7.DataSource = _orderManager.getOrders();

            radGridView7.Columns["PositionProfit"].HeaderText = "持仓盈亏";
            radGridView7.Columns["PositionProfit"].FormatString = "{0:F2}";

            radGridView7.Columns["CloseProfit"].HeaderText = "平仓盈亏";
            radGridView7.Columns["CloseProfit"].FormatString = "{0:F2}";

            radGridView7.BestFitColumns();
            
        }

        void _orderManager_OnRspQryOrderRecord(object sender, OrderRecordEventArgs e)
        {
            if (this.InvokeRequired)
            {
                RecordRecordDelegate d = new RecordRecordDelegate(ShowOrderRecord);
                object arg = e.OrderRecords;
                this.Invoke(d, arg);
            }
        }

        private delegate void PositionRecordDelegate(List<PositionRecord> positionRecords);
        private void ShowPositionRecord(List<PositionRecord> positionRecords)
        {
            //radGridView5.DataSource = positionRecords;
        }

        void _orderManager_OnRspQryPositionRecord(object sender, PositionRecordEventArgs e)
        {
            if (this.InvokeRequired)
            {
                PositionRecordDelegate d = new PositionRecordDelegate(ShowPositionRecord);
                object arg = e.PositionRecords;
                this.Invoke(d, arg);
            }
        }

        private delegate void PositionDetailDelegate(List<PositionDetail> positionDetails);
        private void ShowPositionDetail(List<PositionDetail> positionDetails)
        {
            foreach (var positionDetail in positionDetails)
            {
                positionDetailBindingSource.Add(positionDetail);
            }
        }
        void orderManager_OnRspQryPositionDetail(object sender, PositionDetailEventArgs e)
        {
            if (this.InvokeRequired)
            {
                PositionDetailDelegate d = new PositionDetailDelegate(ShowPositionDetail);
                object arg = e.PositionDetails;
                this.Invoke(d, arg);
            }
        }

        private delegate void TradeRecordDelegate(BindingList<TradeRecord> tradeRecords);


        private void ShowTradeRecord(BindingList<TradeRecord> tradeRecords)
        {
            radGridView9.DataSource = tradeRecords;            
        }

        void orderManager_OnRtnTradeRecord(object sender, TradeRecordEventArgs e)
        {
            if (this.InvokeRequired)
            {
                TradeRecordDelegate d = new TradeRecordDelegate(ShowTradeRecord);
                object arg = e.tradeRecords;
                this.Invoke(d, arg);
            }
        }

        private delegate void TradingAccountDelegate(Account account);

        private void ShowTradingAccount(Account account)
        {
            accountBindingSource.Clear();
            accountBindingSource.Add(account);
        }
        void accountManager_OnQryTradingAccount(object sender, AccountEventArgs e)
        {
            if (this.InvokeRequired)
            {
                TradingAccountDelegate d = new TradingAccountDelegate(ShowTradingAccount);
                object arg = e.account;
                this.Invoke(d, arg);
            }
            
        }


        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (tradeApi != null) tradeApi.UserLogout();
        }

        private void radRibbonBar1_Click(object sender, EventArgs e)
        {

        }

        private void radMenuItem2_Click(object sender, EventArgs e)
        {
            tradeApi.QryOrder();
        }

        private void radMenuItem3_Click(object sender, EventArgs e)
        {
            
        }

        private void radMenuItem10_Click(object sender, EventArgs e)
        {
            InstrumentForm form = new InstrumentForm();
            form.InstrumentManager = _instrumentManager;

            form.ShowDialog();
        }

        private void miClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void radMenuItem11_Click(object sender, EventArgs e)
        {
            _strategyManager.Start();
        }
    }
}

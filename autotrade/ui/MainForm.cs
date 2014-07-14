﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using autotrade.business;
using autotrade.converter;
using autotrade.model;
using autotrade.strategy;
using CTPMdApi;
using CTPTradeApi;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
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
    public partial class MainForm : Form
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public MdApi mdApi { get; set; }
        public TradeApi tradeApi { get; set; }

        private MAReverseStrategy maReverseStrategy;

        private OrderManager _orderManager;
        private AccountManager _accountManager;
        private MarketManager _marketManager;

        readonly string[] ppInstrumentID = { "IF1407" };	// 行情订阅列表
        public MainForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            maReverseStrategy = new MAReverseStrategy(20, "IF1407");

            log.Info("start login");

            mdApi.OnRspError += mdApi_OnRspError;
            mdApi.OnRspSubMarketData += mdApi_OnRspSubMarketData;
            mdApi.OnRtnDepthMarketData += mdApi_OnRtnDepthMarketData;
            log.Info("login");
        }

        void mdApi_OnRtnDepthMarketData(ref CTPMdApi.CThostFtdcDepthMarketDataField pDepthMarketData)
        {
            string s = string.Format("{0,-6} : UpdateTime = {1}.{2:D3},  LasPrice = {3}", pDepthMarketData.InstrumentID, pDepthMarketData.UpdateTime, pDepthMarketData.UpdateMillisec, pDepthMarketData.LastPrice);
            s += maReverseStrategy.Match(pDepthMarketData);
            log.Info(s);
        }

        void mdApi_OnRspSubMarketData(ref CTPMdApi.CThostFtdcSpecificInstrumentField pSpecificInstrument, ref CTPMdApi.CThostFtdcRspInfoField pRspInfo, int nRequestId, bool bIsLast)
        {
            log.Info(pSpecificInstrument.InstrumentID);
        }

        
        void mdApi_OnRspError(ref CTPMdApi.CThostFtdcRspInfoField pRspInfo, int nRequestId, bool bIsLast)
        {
            log.Info(pRspInfo.ErrorMsg);
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
            radGridView4.Columns["Direction"].DataTypeConverter = new DirectionConverter();

            LoginForm loginForm = new LoginForm();
            
            if (loginForm.ShowDialog() != DialogResult.OK) Close();

            mdApi = loginForm.mdApi;
            tradeApi = loginForm.tradeApi;


            _orderManager = new OrderManager(tradeApi);

            _accountManager = new AccountManager(tradeApi);
            _marketManager = new MarketManager(mdApi);

            _accountManager.OnQryTradingAccount += accountManager_OnQryTradingAccount;

            _marketManager.OnRtnMarketData += marketManager_OnRtnMarketData;

            _orderManager.OnRtnTradeRecord += orderManager_OnRtnTradeRecord;
            _orderManager.OnRspQryPositionDetail += orderManager_OnRspQryPositionDetail;
            _orderManager.OnRspQryPositionRecord += _orderManager_OnRspQryPositionRecord;
            _orderManager.OnRspQryOrderRecord += _orderManager_OnRspQryOrderRecord;


            Task.Factory.StartNew(() => {
                _orderManager.QryTrade();
                Thread.Sleep(1000);

                _orderManager.QryInvestorPosition();

                Thread.Sleep(1000);

                _orderManager.QryOrder();
                
            });

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    _accountManager.QryTradingAccount();

                }

            });
        }

        private delegate void RecordRecordDelegate(List<OrderRecord> orderRecords);
        private void ShowOrderRecord(List<OrderRecord> orderRecords)
        {
            orderRecordBindingSource.Clear();

            foreach (var orderRecord in orderRecords)
            {
                orderRecordBindingSource.Add(orderRecord);
            }            
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
            radGridView5.DataSource = positionRecords;
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

        private delegate void TradeRecordDelegate(List<TradeRecord> tradeRecords);


        private void ShowTradeRecord(List<TradeRecord> tradeRecords)
        {
            radGridView4.DataSource = tradeRecords;            
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

        private delegate void MarketDataDelegate(MarketData marketData);
        void marketManager_OnRtnMarketData(object sender, MarketDataEventArgs e)
        {
            if (this.InvokeRequired) {
                MarketDataDelegate d = new MarketDataDelegate(ShowMarketData);
                object arg = e.marketData;
                this.Invoke(d,arg); 
            }
        }

        private void ShowMarketData(MarketData marketData)
        {
            marketDataBindingSource.Clear();
            marketDataBindingSource.Add(marketData);
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

        private void c1Command2_Click(object sender, C1.Win.C1Command.ClickEventArgs e)
        {
            Order order = new Order();
            order.InstrumentId = "IF1407";
            order.OffsetFlag = EnumOffsetFlagType.Open;
            order.Direction = EnumDirectionType.Buy;
            order.Price = 10;
            order.Volume = 1;

            _orderManager.OrderInsert(order);
        }

        private void c1Command3_Click(object sender, C1.Win.C1Command.ClickEventArgs e)
        {
            _marketManager.SubMarketData("IF1407");
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            tradeApi.UserLogout();
        }

        private void c1Command4_Click(object sender, C1.Win.C1Command.ClickEventArgs e)
        {
            _accountManager.QryTradingAccount();
        }

        private void c1Command5_Click(object sender, C1.Win.C1Command.ClickEventArgs e)
        {
            _orderManager.QryTrade();
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
            _marketManager.SubMarketData("IF1407");
        }
    }
}

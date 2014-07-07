using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CTPMdApi;
using CTPTradeApi;

namespace autotrade
{
    public partial class Form1 : Form
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        MdApi mdApi = new MdApi("gengke", "123");
        TradeApi tradeApi = new TradeApi("gengke", "123");

        readonly string[] ppInstrumentID = { "IF1407" };	// 行情订阅列表
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            log.Info("start login");

            mdApi.Connect();

            mdApi.OnFrontConnected += mdApi_OnFrontConnected;
            mdApi.OnRspUserLogin += mdApi_OnRspUserLogin;
            mdApi.OnRspError += mdApi_OnRspError;
            mdApi.OnRspSubMarketData += mdApi_OnRspSubMarketData;
            mdApi.OnRtnDepthMarketData += mdApi_OnRtnDepthMarketData;
            log.Info("login");
        }

        void mdApi_OnRtnDepthMarketData(ref CTPMdApi.CThostFtdcDepthMarketDataField pDepthMarketData)
        {
            string s = string.Format("{0,-6} : UpdateTime = {1}.{2:D3},  LasPrice = {3}", pDepthMarketData.InstrumentID, pDepthMarketData.UpdateTime, pDepthMarketData.UpdateMillisec, pDepthMarketData.LastPrice);
            log.Info(s);
        }

        void mdApi_OnRspSubMarketData(ref CTPMdApi.CThostFtdcSpecificInstrumentField pSpecificInstrument, ref CTPMdApi.CThostFtdcRspInfoField pRspInfo, int nRequestId, bool bIsLast)
        {
            log.Info(pSpecificInstrument.InstrumentID);
        }

        void mdApi_OnRspUserLogin(ref CTPMdApi.CThostFtdcRspUserLoginField pRspUserLogin, ref CTPMdApi.CThostFtdcRspInfoField pRspInfo, int nRequestId, bool bIsLast)
        {
            log.Info(pRspInfo.ErrorMsg);
            mdApi.SubMarketData(ppInstrumentID);
        }

        void mdApi_OnRspError(ref CTPMdApi.CThostFtdcRspInfoField pRspInfo, int nRequestId, bool bIsLast)
        {
            log.Info(pRspInfo.ErrorMsg);
        }

        void mdApi_OnFrontConnected()
        {
            log.Info("connected");

            mdApi.UserLogin();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tradeApi.Connect();

            tradeApi.OnFrontConnect += tradeApi_OnFrontConnect;
        }

        void tradeApi_OnFrontConnect()
        {
            tradeApi.UserLogin();
        }
    }
}

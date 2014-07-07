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

namespace autotrade
{
    public partial class Form1 : Form
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        MdApi mdApi = new MdApi("gengke", "123");

        readonly string[] ppInstrumentID = { "IF1407" };	// 行情订阅列表
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Console.WriteLine("start login");
            log.Info("start login");

            mdApi.Connect();

            mdApi.OnFrontConnected += mdApi_OnFrontConnected;
            mdApi.OnRspUserLogin += mdApi_OnRspUserLogin;
            mdApi.OnRspError += mdApi_OnRspError;
            mdApi.OnRspSubMarketData += mdApi_OnRspSubMarketData;
            mdApi.OnRtnDepthMarketData += mdApi_OnRtnDepthMarketData;
            log.Info("login");
        }

        void mdApi_OnRtnDepthMarketData(ref CThostFtdcDepthMarketDataField pDepthMarketData)
        {
            string s = string.Format("{0,-6} : UpdateTime = {1}.{2:D3},  LasPrice = {3}", pDepthMarketData.InstrumentID, pDepthMarketData.UpdateTime, pDepthMarketData.UpdateMillisec, pDepthMarketData.LastPrice);
            Console.WriteLine(s);
        }

        void mdApi_OnRspSubMarketData(ref CThostFtdcSpecificInstrumentField pSpecificInstrument, ref CThostFtdcRspInfoField pRspInfo, int nRequestID, bool bIsLast)
        {
            Console.WriteLine(pSpecificInstrument.InstrumentID);
        }

        void mdApi_OnRspUserLogin(ref CThostFtdcRspUserLoginField pRspUserLogin, ref CThostFtdcRspInfoField pRspInfo, int nRequestID, bool bIsLast)
        {
            Console.WriteLine(pRspInfo.ErrorMsg);
            mdApi.SubMarketData(ppInstrumentID);
        }

        void mdApi_OnRspError(ref CThostFtdcRspInfoField pRspInfo, int nRequestID, bool bIsLast)
        {
            Console.WriteLine(pRspInfo.ErrorMsg);
        }

        void mdApi_OnFrontConnected()
        {
            Console.WriteLine("connected");

            mdApi.UserLogin();
        }
    }
}

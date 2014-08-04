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
using Autofac;
using QuantBox.CSharp2CTP;
using QuantBox.CSharp2CTP.Event;
using Telerik.WinControls.UI;
using autotrade.Strategies;
using autotrade.business;
using autotrade.converter;
using autotrade.model;
using autotrade.ui;
using System.Threading;
using IContainer = Autofac.IContainer;

namespace autotrade
{
    public partial class MainForm : RadForm
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        

        public MdApiWrapper mdApi { get; set; }
        public TraderApiWrapper tradeApi { get; set; }

        private AboveMAStrategy maReverseStrategy;

        public OrderManager _orderManager { get; set; }
        public IContainer Container { get; set; }

        public AccountManager _accountManager { get; set; }
        public MarketManager _marketManager { get; set; }
        public StrategyManager _strategyManager { get; set; }
        public InstrumentManager _instrumentManager { get; set; }
        public IndicatorManager _indicatorManager { get; set; }
        
        public MainForm()
        {
            InitializeComponent();
        }

        
        
        

        void tradeApi_OnRspQryInvestorPositionCombineDetail(ref CThostFtdcInvestorPositionCombineDetailField pInvestorPositionCombineDetail, ref CThostFtdcRspInfoField pRspInfo, int nRequestID, bool bIsLast)
        {
            log.Info(pRspInfo);
            log.Info(pInvestorPositionCombineDetail);
        }
        

        private void tradeApi_OnRspQryContractBank(ref CThostFtdcContractBankField pcontractbank, ref CThostFtdcRspInfoField prspinfo, int nrequestid, bool bislast)
        {
            log.Info(prspinfo);
            log.Info(pcontractbank);
        }
 
        private void MainForm_Load(object sender, EventArgs e)
        {
            //radGridView4.Columns["Direction"].DataTypeConverter = new DirectionConverter();


            var loginForm = Container.Resolve<LoginForm>();

            if (loginForm.ShowDialog() != DialogResult.OK)
            {
                Close();
                return;
            }


            radGridView8.DataSource = _accountManager.Accounts;


            radGridView6.DataSource = _orderManager.GetOrderRecords();


            radGridView7.DataSource = _orderManager.getOrders();

            radGridView9.DataSource = _orderManager.GetTradeRecords();


            radGridView7.Columns["PositionProfit"].HeaderText = "持仓盈亏";
            radGridView7.Columns["PositionProfit"].FormatString = "{0:F2}";

            radGridView7.Columns["CloseProfit"].HeaderText = "平仓盈亏";
            radGridView7.Columns["CloseProfit"].FormatString = "{0:F2}";

            radGridView7.BestFitColumns();

            _marketManager.Subscribe();

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

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            mdApi.Disconnect();

            tradeApi.Disconnect();
        }

        private void radRibbonBar1_Click(object sender, EventArgs e)
        {

        }

        private void radMenuItem2_Click(object sender, EventArgs e)
        {
            //tradeApi.QryOrder();
        }

        private void radMenuItem3_Click(object sender, EventArgs e)
        {
            
        }

        private void radMenuItem10_Click(object sender, EventArgs e)
        {
            
            var form = Container.Resolve<InstrumentForm>();
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

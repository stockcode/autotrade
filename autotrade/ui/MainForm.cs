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
using MongoDB.Driver.Linq;
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
            _strategyManager.Container = Container;

            //radGridView4.Columns["Direction"].DataTypeConverter = new DirectionConverter();


            var loginForm = Container.Resolve<LoginForm>();

            if (loginForm.ShowDialog() != DialogResult.OK)
            {
                Close();
                return;
            }

            _orderManager.OnRspQryOrder += _orderManager_OnRspQryOrder;

            radGridView8.DataSource = _accountManager.Accounts;


            radGridView6.DataSource = _orderManager.GetOrderRecords();


            gvOrder.DataSource = _orderManager.getOrders();

            radGridView3.DataSource = _orderManager.getOrderLogs();

            radGridView9.DataSource = _orderManager.GetTradeRecords();


            gvOrder.Columns["PositionProfit"].HeaderText = "持仓盈亏";
            gvOrder.Columns["PositionProfit"].FormatString = "{0:F2}";

            gvOrder.Columns["CloseProfit"].HeaderText = "平仓盈亏";
            gvOrder.Columns["CloseProfit"].FormatString = "{0:F2}";

            gvOrder.BestFitColumns();

            _marketManager.Subscribe();

            this.gvInstrument.MasterTemplate.Columns.Clear();
            gvInstrument.DataSource = _marketManager.marketDatas;
            gvInstrument.BestFitColumns();

            gvInstrument.Columns["InstrumentId"].HeaderText = "合约";
            gvInstrument.Columns["LastPrice"].HeaderText = "最新价";
            gvInstrument.Columns["BidPrice1"].HeaderText = "买价";
            gvInstrument.Columns["BidVolume1"].HeaderText = "买量";
            gvInstrument.Columns["AskPrice1"].HeaderText = "卖价";
            gvInstrument.Columns["AskVolume1"].HeaderText = "卖量";
            gvInstrument.Columns["Volume"].HeaderText = "成交量";
            gvInstrument.Columns["OpenInterest"].HeaderText = "持仓量";
            gvInstrument.Columns["UpperLimitPrice"].HeaderText = "涨停价";
            gvInstrument.Columns["LowerLimitPrice"].HeaderText = "跌停价";
            gvInstrument.Columns["OpenPrice"].HeaderText = "今开盘";
            gvInstrument.Columns["PreSettlementPrice"].HeaderText = "昨结算";
            gvInstrument.Columns["HighestPrice"].HeaderText = "最高价";
            gvInstrument.Columns["LowestPrice"].HeaderText = "最低价";
            gvInstrument.Columns["PreClosePrice"].HeaderText = "昨收盘";
            gvInstrument.Columns["Turnover"].HeaderText = "成交额";
            gvInstrument.Columns["UpdateTime"].HeaderText = "行情更新时间";
            //radGridView2.LoadLayout("c:\\columns.xml");            
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            mdApi.Disconnect();

            tradeApi.Disconnect();
        }

        void _orderManager_OnRspQryOrder(object sender, OrderEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(e.methodInvoker);
            }
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

        private void gvOrder_SelectionChanged(object sender, EventArgs e)
        {
            Order order = (Order) gvOrder.SelectedRows[0].DataBoundItem;

            int index = _marketManager.GetIndex(order.InstrumentId);
            if (index < 0) return;

            gvInstrument.Rows[index].IsSelected = true;
            gvInstrument.TableElement.ScrollToRow(index);
        }

        private void RadGridView_SelectionChanged(object sender, EventArgs e)
        {
            MarketData marketData = (MarketData) gvInstrument.SelectedRows[0].DataBoundItem;

            radPropertyGrid1.SelectedObject = _strategyManager.GetInstrumentStrategy(marketData.InstrumentId);
        }
    }
}

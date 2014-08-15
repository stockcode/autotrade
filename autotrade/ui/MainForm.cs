using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using Autofac;
using autotrade.business;
using autotrade.model;
using autotrade.Strategies;
using autotrade.ui;
using log4net;
using MongoDB.Driver.Linq;
using QuantBox.CSharp2CTP;
using QuantBox.CSharp2CTP.Event;
using Telerik.WinControls.UI;

namespace autotrade
{
    public partial class MainForm : RadForm
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public MainForm()
        {
            InitializeComponent();
        }


        public MdApiWrapper mdApi { get; set; }
        public TraderApiWrapper tradeApi { get; set; }

        public OrderManager OrderManager { get; set; }
        public IContainer Container { get; set; }

        public AccountManager AccountManager { get; set; }
        public MarketManager MarketManager { get; set; }
        public StrategyManager StrategyManager { get; set; }
        public IndicatorManager IndicatorManager { get; set; }


        private void tradeApi_OnRspQryInvestorPositionCombineDetail(
            ref CThostFtdcInvestorPositionCombineDetailField pInvestorPositionCombineDetail,
            ref CThostFtdcRspInfoField pRspInfo, int nRequestID, bool bIsLast)
        {
            log.Info(pRspInfo);
            log.Info(pInvestorPositionCombineDetail);
        }


        private void tradeApi_OnRspQryContractBank(ref CThostFtdcContractBankField pcontractbank,
            ref CThostFtdcRspInfoField prspinfo, int nrequestid, bool bislast)
        {
            log.Info(prspinfo);
            log.Info(pcontractbank);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            StrategyManager.Container = Container;

            //radGridView4.Columns["Direction"].DataTypeConverter = new DirectionConverter();

            dtTradingDay.Text = tradeApi.TradingDay;

            miBuyOrder.Click += miBuyOrder_Click;
            miSellOrder.Click += miSellOrder_Click;

            miDetail.Click += miDetail_Click;
            miCloseOrder.Click += miCloseOrder_Click;
            miCancelOrder.Click += miCancelOrder_Click;

            miOrderLogDetail.Click += miOrderLogDetail_Click;

            var loginForm = Container.Resolve<LoginForm>();

            if (loginForm.ShowDialog() != DialogResult.OK)
            {
                Close();
                return;
            }            

            OrderManager.Init();

            OrderManager.OnRspQryOrder += _orderManager_OnRspQryOrder;

            radGridView8.DataSource = AccountManager.Accounts;


            radGridView6.DataSource = OrderManager.GetOrderRecords();


            gvOrder.DataSource = OrderManager.getOrders();
            ConfigGridView(gvOrder, new String[]{"PositionProfit", "UseMargin"});

            OrderManager.ChangeOrderLogs(tradeApi.TradingDay);
            gvOrderLog.DataSource = OrderManager.GetOrderLogs();
            ConfigGridView(gvOrderLog, new String[] { "CloseProfit", "UseMargin" });

            radGridView9.DataSource = OrderManager.GetTradeRecords();



            MarketManager.Subscribe();

            gvInstrument.MasterTemplate.Columns.Clear();
            gvInstrument.DataSource = MarketManager.marketDatas;
            gvInstrument.BestFitColumns();


            IndicatorManager.Init(MarketManager.marketDatas);

            StrategyManager.Start();
        }

        void miSellOrder_Click(object sender, EventArgs e)
        {
            var marketData = (MarketData)gvInstrument.SelectedRows[0].DataBoundItem;
            OrderManager.OpenOrder(marketData.InstrumentId, TThostFtdcDirectionType.Sell);
        }

        void miBuyOrder_Click(object sender, EventArgs e)
        {
            var marketData = (MarketData)gvInstrument.SelectedRows[0].DataBoundItem;
            OrderManager.OpenOrder(marketData.InstrumentId, TThostFtdcDirectionType.Buy);
        }

        void miCancelOrder_Click(object sender, EventArgs e)
        {
            var order = (Order)gvOrder.SelectedRows[0].DataBoundItem;

            OrderManager.CancelOrder(order);
        }

        void miCloseOrder_Click(object sender, EventArgs e)
        {
            var order = (Order)gvOrder.SelectedRows[0].DataBoundItem;

            OrderManager.CloseOrder(order);
        }

        private void ConfigGridView(RadGridView radGridView, IEnumerable<string> columns)
        {
            foreach (var column in columns)
            {
                radGridView.Columns[column].FormatString = "{0:F2}";    
            }
            
            radGridView.BestFitColumns();
        }

        void miOrderLogDetail_Click(object sender, EventArgs e)
        {
            var orderLog = (OrderLog)gvOrderLog.SelectedRows[0].DataBoundItem;

            var orderDetailForm = new OrderDetailForm();
            orderDetailForm.OrderLog = orderLog;
            orderDetailForm.ShowDialog();
        }

        private void miDetail_Click(object sender, EventArgs e)
        {            
            var order = (Order) gvOrder.SelectedRows[0].DataBoundItem;

            var orderDetailForm = new OrderDetailForm();
            orderDetailForm.Order = order;
            orderDetailForm.ShowDialog();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            mdApi.Disconnect();

            tradeApi.Disconnect();
        }

        private void _orderManager_OnRspQryOrder(object sender, OrderEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(e.methodInvoker);
            }
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
            throw new NullReferenceException("null for test");
            //Close();
        }

        private void radMenuItem11_Click(object sender, EventArgs e)
        {
            StrategyManager.Start();
        }

        private void gvOrder_SelectionChanged(object sender, EventArgs e)
        {
//            Order order = (Order) gvOrder.SelectedRows[0].DataBoundItem;
//
//            int index = MarketManager.GetIndex(order.InstrumentId);
//            if (index < 0) return;
//
//            gvInstrument.Rows[index].IsSelected = true;
//            gvInstrument.TableElement.ScrollToRow(index);
        }

        private void RadGridView_SelectionChanged(object sender, EventArgs e)
        {
            var marketData = (MarketData) gvInstrument.SelectedRows[0].DataBoundItem;

            radPropertyGrid1.SelectedObject = StrategyManager.GetInstrumentStrategy(marketData.InstrumentId);
        }

        private void gvOrder_ContextMenuOpening(object sender, ContextMenuOpeningEventArgs e)
        {
            e.ContextMenu = cmOrder.DropDown;
        }

        private void gvOrderLog_ContextMenuOpening(object sender, ContextMenuOpeningEventArgs e)
        {
            e.ContextMenu = cmOrderLog.DropDown;
        }

        private void dtTradingDay_ValueChanged(object sender, EventArgs e)
        {
            

            OrderManager.ChangeOrderLogs(dtTradingDay.Value.ToString("yyyyMMdd"));                       
        }

        private void gvInstrument_ContextMenuOpening(object sender, ContextMenuOpeningEventArgs e)
        {
            e.ContextMenu = cmMarket.DropDown;
        }
    }
}
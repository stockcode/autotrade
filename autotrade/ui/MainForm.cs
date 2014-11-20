using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Autofac;
using autotrade.business;
using autotrade.model;
using autotrade.Strategies;
using autotrade.ui;
using log4net;
using MongoDB.Driver.Linq;
using QuantBox.CSharp2CTP;
using QuantBox.CSharp2CTP.Event;
using Schedule;
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

        ReportTimer _Timer = new ReportTimer();

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
            miCancelAll.Click += miCancelAll_Click;
            miDeleteOrder.Click += miDeleteOrder_Click;

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

            _Timer.Elapsed += new ReportEventHandler(_Timer_Elapsed);

            _Timer.AddReportEvent(new ScheduledTime("Daily", "15:30:00"), 1);

            _Timer.Start();
        }

        void miDeleteOrder_Click(object sender, EventArgs e)
        {
            var order = (Order)gvOrder.SelectedRows[0].DataBoundItem;

            OrderManager.DeleteOrder(order);
        }

        void miCancelAll_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("你确定要撤销所有挂单吗？", "温馨提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                OrderManager.CancelAllOrder();
            }
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

            _Timer.Stop();
        }

        private void _orderManager_OnRspQryOrder(object sender, OrderEventArgs e)
        {
            //if (InvokeRequired)
            //{
                Invoke(e.methodInvoker);
            //}
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

        private void radMenuItem6_Click(object sender, EventArgs e)
        {
            
        }

        private void _Timer_Elapsed(object sender, ReportEventArgs e)
        {
            var tickDir = Application.StartupPath + @"\Tick\";
            MarketManager.Export(tickDir);
        }

        private void radMenuItem8_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK) return;


            var filename = openFileDialog1.FileName;

            foreach (var line in File.ReadAllLines(filename))
            {
                string[] datas = line.Split(',');

                var data = new CThostFtdcDepthMarketDataField();
                data.InstrumentID = datas[0];
                data.LastPrice = Double.Parse(datas[1]);
                data.AveragePrice = Double.Parse(datas[2]);
                data.UpdateTime = datas[3];
                data.UpdateMillisec = Convert.ToInt32(datas[4]);

                MarketManager.AddSimData(data);
                Thread.Sleep(100);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autofac;
using autotrade.business;
using autotrade.model;
using autotrade.ui;
using autotrade.ui.Editor;
using log4net;
using QuantBox.CSharp2CTP;
using QuantBox.CSharp2CTP.Event;
using Schedule;
using Telerik.WinControls.UI;

namespace autotrade
{
    public partial class MainForm : RadForm
    {
        private readonly ReportTimer _Timer = new ReportTimer();
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
            OrderManager.OnStopTrade += OrderManager_OnStopTrade;
            rgAccount.DataSource = AccountManager.Accounts;

            ConfigAccountGrid();

            radGridView6.DataSource = OrderManager.GetOrderRecords();


            gvOrder.DataSource = OrderManager.getOrders();

            ConfigGridView(gvOrder, new[] {"PositionProfit", "CloseProfit", "UseMargin"});

            OrderManager.ChangeOrderLogs(tradeApi.TradingDay);
            gvOrderLog.DataSource = OrderManager.GetOrderLogs();
            ConfigGridView(gvOrderLog, new[] {"CloseProfit", "UseMargin"});

            radGridView9.DataSource = OrderManager.GetTradeRecords();


            MarketManager.Subscribe();

            gvInstrument.MasterTemplate.Columns.Clear();
            gvInstrument.DataSource = MarketManager.marketDatas;
            gvInstrument.BestFitColumns();


            IndicatorManager.Init(MarketManager.marketDatas);

            StrategyManager.Start();

            _Timer.Elapsed += _Timer_Elapsed;

            _Timer.AddReportEvent(new ScheduledTime("Daily", "15:30:00"), 1);

            _Timer.Start();
        }

        void OrderManager_OnStopTrade(object sender, AccountEventArgs e)
        {            
            StrategyManager.Stop();

            var client = new SmtpClient
            {
                Port = 25,
                Host = "smtp.139.com",
                EnableSsl = true,
                Timeout = 10000,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential("13613803575@139.com", "gk790624")
            };

            var body = String.Format("平仓盈亏:{0:F2}, 持仓盈亏:{1:F2}, 亏损比例:{2:P2}, 账户余额:{3:F2}", e.account.CloseProfit, e.account.PositionProfit, e.account.CloseProfit / e.account.Available, e.account.Available);

            var mm = new MailMessage()
            {
                BodyEncoding = Encoding.UTF8,
                DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure,
                From = new MailAddress("13613803575@139.com"),                
                Subject = "亏损提醒",
                Body = body
            };

            mm.To.Add(new MailAddress("13613803575@139.com"));
            mm.To.Add(new MailAddress("15003862166@139.com"));

            client.Send(mm);


            log.Info(e.account);
        }

        private void ConfigAccountGrid()
        {
            rgAccount.TableElement.RowHeight = 50; 
            ConfigGridView(rgAccount, new[] { "PositionProfit", "CloseProfit", "CurrMargin", "Available", "FrozenMargin" });
            rgAccount.Columns["StopPercent"].Width = 200; 
            rgAccount.Columns["StopPercent"].FormatString = "{0} %";
            rgAccount.Columns["StopLoss"].Width = 100; 
        }

        private void miDeleteOrder_Click(object sender, EventArgs e)
        {
            var order = (Order) gvOrder.SelectedRows[0].DataBoundItem;

            OrderManager.DeleteOrder(order);
        }

        private void miCancelAll_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("你确定要撤销所有挂单吗？", "温馨提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                OrderManager.CancelAllOrder();
            }
        }

        private void miSellOrder_Click(object sender, EventArgs e)
        {
            var marketData = (MarketData) gvInstrument.SelectedRows[0].DataBoundItem;
            OrderManager.OpenOrder(marketData.InstrumentId, TThostFtdcDirectionType.Sell);
        }

        private void miBuyOrder_Click(object sender, EventArgs e)
        {
            var marketData = (MarketData) gvInstrument.SelectedRows[0].DataBoundItem;
            OrderManager.OpenOrder(marketData.InstrumentId, TThostFtdcDirectionType.Buy);
        }

        private void miCancelOrder_Click(object sender, EventArgs e)
        {
            var order = (Order) gvOrder.SelectedRows[0].DataBoundItem;

            OrderManager.CancelOrder(order);
        }

        private void miCloseOrder_Click(object sender, EventArgs e)
        {
            var order = (Order) gvOrder.SelectedRows[0].DataBoundItem;

            OrderManager.CloseOrder(order);
        }

        private void ConfigGridView(RadGridView radGridView, IEnumerable<string> columns)
        {
            foreach (string column in columns)
            {
                radGridView.Columns[column].FormatString = "{0:F2}";
            }

            radGridView.BestFitColumns();
        }

        private void miOrderLogDetail_Click(object sender, EventArgs e)
        {
            var orderLog = (OrderLog) gvOrderLog.SelectedRows[0].DataBoundItem;

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
            string tickDir = Application.StartupPath + @"\Tick\";
            MarketManager.Export(tickDir);
        }

        private void radMenuItem8_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK) return;

            Task.Factory.StartNew(() =>
            {
                string filename = openFileDialog1.FileName;

                foreach (string line in File.ReadAllLines(filename))
                {
                    string[] datas = line.Split(',');

                    var data = new CThostFtdcDepthMarketDataField();
                    data.InstrumentID = datas[0];
                    data.LastPrice = Double.Parse(datas[1]);
                    data.AveragePrice = Double.Parse(datas[2])*300;
                    data.UpdateTime = datas[3];
                    data.UpdateMillisec = Convert.ToInt32(datas[4]);
                    data.ExchangeID = "CZCE";

                    MarketManager.AddSimData(data);
                    Thread.Sleep(100);
                }
            }
                );
        }

        private void rgAccount_ValueChanged(object sender, EventArgs e)
        {
            if (rgAccount.CurrentColumn.Name == "UseStopPercent" || rgAccount.CurrentColumn.Name == "UseStopLoss")
            {
                rgAccount.EndEdit();
                return;
            }

            var editor = sender as TrackBarEditor;

            if (editor == null) return;

            GridCellElement cell = rgAccount.TableElement.GetCellElement(rgAccount.CurrentRow,
                rgAccount.Columns["StopPercent"]);
            if (cell != null)
            {
                cell.Text = editor.Value + " %";
            }
        }

        private void rgAccount_EditorRequired(object sender, EditorRequiredEventArgs e)
        {
            if (e.EditorType == typeof (GridSpinEditor) && rgAccount.Columns["StopPercent"].IsCurrent)
            {
                e.EditorType = typeof (TrackBarEditor);
            }
        }

        private void miStart_Click(object sender, EventArgs e)
        {
            StrategyManager.Start();
        }
    }
}
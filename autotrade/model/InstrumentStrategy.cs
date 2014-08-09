using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using autotrade.business;
using autotrade.converter;
using autotrade.Stop.Loss;
using autotrade.Stop.Profit;
using autotrade.Strategies;
using MongoRepository;
using IContainer = Autofac.IContainer;

namespace autotrade.model
{
    public class InstrumentStrategy : Entity, INotifyPropertyChanged
    {
        public InstrumentStrategy()
        {
            Strategies = new BindingList<UserStrategy>();            
        }

        public void BindEvent(IContainer container)
        {
            Strategies.ListChanged += Strategies_ListChanged;
            foreach (var strategy in Strategies)
            {
                if (strategy.IndicatorManager == null)
                    strategy.IndicatorManager = container.Resolve<IndicatorManager>();

                if (strategy.OrderManager == null)
                    strategy.OrderManager = container.Resolve<OrderManager>();

                if (strategy.InstrumentStrategy == null)
                    strategy.InstrumentStrategy = this;


                strategy.StopLosses.ListChanged += Strategies_ListChanged;
                foreach (var stopLoss in strategy.StopLosses)
                {
                    if (stopLoss.IndicatorManager == null)
                        stopLoss.IndicatorManager = container.Resolve<IndicatorManager>();

                    if (stopLoss.OrderManager == null)
                        stopLoss.OrderManager = container.Resolve<OrderManager>();
                }

                strategy.StopProfits.ListChanged += Strategies_ListChanged;

                foreach (var stopProfit in strategy.StopProfits)
                {
                    if (stopProfit.IndicatorManager == null)
                        stopProfit.IndicatorManager = container.Resolve<IndicatorManager>();

                    if (stopProfit.OrderManager == null)
                        stopProfit.OrderManager = container.Resolve<OrderManager>();

                    if (stopProfit.InstrumentStrategy == null)
                        stopProfit.InstrumentStrategy = this;
                }
            }            
        }

        void Strategies_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemAdded) NotifyPropertyChanged();
        }

        [DisplayName("合约")]
        [ReadOnly(true)]
        public string InstrumentID { get; set; }

        [DisplayName("合约名称")]
        [ReadOnly(true)]
        public string InstrumentName { get; set; }

        [ReadOnly(true)]
        [DisplayName("合约数量乘数")]
        public int VolumeMultiple { get; set; }

        [ReadOnly(true)]
        [DisplayName("最小变动价位")]
        public double PriceTick { get; set; }


        [ReadOnly(true)]
        [DisplayName("多头保证金率")]
        public double LongMarginRatio { get; set; }

        [ReadOnly(true)]
        [DisplayName("空头保证金率")]
        public double ShortMarginRatio { get; set; }


        [DisplayName("策略列表")]
        [Editor(typeof(StrategyUIEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public BindingList<UserStrategy> Strategies { get; set; }
        

        private bool _startTrade = true;

        [DisplayName("是否交易")]
        public bool StartTrade 
        {
            get { return _startTrade; }
            set
            {
                if (this._startTrade != value)
                {
                    this._startTrade = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private int _volume = 1;

        [DisplayName("单笔手数")]
        public int Volume
        {
            get { return _volume; }
            set
            {
                if (this._volume != value)
                {
                    this._volume = value;
                    NotifyPropertyChanged();
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}

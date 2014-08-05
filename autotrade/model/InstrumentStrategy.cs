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
using autotrade.Strategies;
using MongoRepository;
using IContainer = Autofac.IContainer;

namespace autotrade.model
{
    public class InstrumentStrategy : Entity, INotifyPropertyChanged
    {
        public InstrumentStrategy()
        {
            Strategies = new BindingList<Strategy>();            
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
            }
        }

        void Strategies_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemAdded) NotifyPropertyChanged();
        }

        [ReadOnly(true)]
        public string InstrumentID { get; set; }

        [ReadOnly(true)]
        [DisplayName("合约数量乘数")]
        /// <summary>
        /// 合约数量乘数
        /// </summary>
        public int VolumeMultiple { get; set; }

        [Editor(typeof(StrategyUIEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public BindingList<Strategy> Strategies { get; set; }

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

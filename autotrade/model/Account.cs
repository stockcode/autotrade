using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MongoDB.Bson.Serialization.Attributes;
using MongoRepository;

namespace autotrade.model
{
    public class Account : Entity, INotifyPropertyChanged
    {
        public Account()
        {
            UseStopPercent = true;
            UseStopLoss = true;
            StopPercent = 2;
            StopLoss = 10000;
        }

        [DisplayName("投资者帐号")]
        public string AccountID { get; set; }

        private double closeProfit;

        [DisplayName("平仓盈亏")]
        [BsonIgnore]
        public double CloseProfit
        {
            get { return closeProfit; }
            set
            {
                    this.closeProfit = value;
                    NotifyPropertyChanged();
            }
        }

        private double positionProfit;

        [DisplayName("持仓盈亏")]
        [BsonIgnore]
        public double PositionProfit
        {
            get { return positionProfit; }
            set
            {
                    this.positionProfit = value;
                    NotifyPropertyChanged();
            }
        }

        [DisplayName("可用资金")]
        [BsonIgnore]
        public double Available { get; set; }

        [DisplayName("冻结的保证金")]
        [BsonIgnore]
        public double FrozenMargin { get; set; }

        [DisplayName("当前保证金总额")]
        [BsonIgnore]
        public double CurrMargin { get; set; }

        private bool useStopPercent;

        [DisplayName("使用止损百分比")]
        public bool UseStopPercent 
        {
            get { return useStopPercent; }
            set
            {
                if (this.useStopPercent == value) return;

                this.useStopPercent = value;
                NotifyPropertyChanged();
            }
        }

        private int stopPercent;

        [DisplayName("止损百分比")]
        public int StopPercent
        {
            get { return stopPercent; }
            set
            {
                if (this.stopPercent == value) return;

                this.stopPercent = value;
                NotifyPropertyChanged();
            }
        }

        private bool useStopLoss;

        [DisplayName("使用止损金额")]
        public bool UseStopLoss
        {
            get { return useStopLoss; }
            set
            {
                if (this.useStopLoss == value) return;

                this.useStopLoss = value;
                NotifyPropertyChanged();
            }
        }

        private double stopLoss;

        [DisplayName("止损金额")]
        public double StopLoss
        {
            get { return stopLoss; }
            set
            {
                this.stopLoss = value;
                NotifyPropertyChanged();
            }
        }

        public override string ToString()
        {
            return String.Format("投资者帐号={0}, 平仓盈亏={1}, 持仓盈亏={2}, 可用资金={3}, 冻结的保证金={4}", AccountID, CloseProfit,
                PositionProfit, Available, FrozenMargin);
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

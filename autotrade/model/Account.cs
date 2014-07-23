using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace autotrade.model
{
    public class Account : INotifyPropertyChanged
    {
        /// <summary>
        /// 投资者帐号
        /// </summary>
        public string AccountID { get; set; }

        private double closeProfit;
        /// <summary>
        /// 平仓盈亏
        /// </summary>
        public double CloseProfit
        {
            get { return closeProfit; }
            set
            {
                if (this.closeProfit != value)
                {
                    this.closeProfit = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private double positionProfit;

        /// <summary>
        /// 持仓盈亏
        /// </summary>
        public double PositionProfit
        {
            get { return positionProfit; }
            set
            {
                if (this.positionProfit != value)
                {
                    this.positionProfit = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 可用资金
        /// </summary>
        public double Available { get; set; }

        /// <summary>
        /// 冻结的保证金
        /// </summary>
        public double FrozenMargin { get; set; }

        /// <summary>
        /// 当前保证金总额
        /// </summary>
        public double CurrMargin { get; set; }

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

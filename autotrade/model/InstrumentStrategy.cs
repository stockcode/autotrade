using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using autotrade.business;
using autotrade.converter;
using autotrade.Strategies;
using MongoRepository;

namespace autotrade.model
{
    public class InstrumentStrategy : Entity, INotifyPropertyChanged
    {
        public InstrumentStrategy()
        {
            Strategies = new List<Strategy>();
        }

        [ReadOnly(true)]
        public string InstrumentID { get; set; }

        [Editor(typeof(StrategyUIEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public List<Strategy> Strategies { get; set; }


        [DisplayName("是否交易")]
        public bool StartTrade { get; set; }

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

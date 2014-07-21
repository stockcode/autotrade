using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MongoRepository;

namespace autotrade.model
{
    public class Instrument : Entity, INotifyPropertyChanged
    {
        public String Code { get; set; }

        public String Name { get; set; }

        public int Unit { get; set; }

        public double MinPrice { get; set; }

        private string exchangeID;

        public string ExchangeID
        {
            get { return exchangeID; }
            set
            {
                if (this.exchangeID != value)
                {
                    this.exchangeID = value;
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

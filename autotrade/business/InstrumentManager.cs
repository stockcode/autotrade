using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoRepository;
using autotrade.model;

namespace autotrade.business
{
    public class InstrumentManager
    {
        public BindingList<Instrument> instruments = new BindingList<Instrument>();

        private MongoRepository<Instrument> instrumentRepo = new MongoRepository<Instrument>();

        public InstrumentManager()
        {
            foreach (var instrument in instrumentRepo.Collection.FindAll())
            {
                instruments.Add(instrument);
            }

            instruments.ListChanged += instruments_ListChanged;
        }

        public int GetUnit(string instrumentId)
        {
            string str = instrumentId.Substring(0, instrumentId.Length - 4);

            foreach (var instrument in instruments)
            {
                if (instrument.Code == str) return instrument.Unit;
            }

            return 0;
        }

        void instruments_ListChanged(object sender, ListChangedEventArgs e)
        {
            switch (e.ListChangedType)
            {
                case ListChangedType.ItemAdded:
                    instrumentRepo.Add(instruments[e.NewIndex]);
                    break;
                case ListChangedType.ItemChanged:
                    instrumentRepo.Update(instruments[e.NewIndex]);
                    break;
                case ListChangedType.ItemDeleted:
                    instrumentRepo.Delete(instruments[e.NewIndex]);
                    break;
            }
        }
    }
}

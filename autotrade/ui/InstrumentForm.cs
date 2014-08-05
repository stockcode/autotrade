using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Telerik.WinControls;
using autotrade.business;

namespace autotrade.ui
{
    public partial class InstrumentForm : Telerik.WinControls.UI.RadForm
    {
        public MarketManager MarketManager { get; set; }

        public InstrumentForm()
        {
            InitializeComponent();
        }

        private void InstrumentForm_Load(object sender, EventArgs e)
        {
            radGridView1.DataSource = MarketManager.instruments;

            radGridView1.BestFitColumns();
        }

        private void radGridView1_ValueChanged(object sender, EventArgs e)
        {
            if (radGridView1.CurrentColumn.Name == "AutoTrade")
            {
                radGridView1.EndEdit();
            }
        }
    }
}

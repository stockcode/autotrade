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
        public InstrumentManager InstrumentManager { get; set;}

        public InstrumentForm()
        {
            InitializeComponent();
        }

        private void InstrumentForm_Load(object sender, EventArgs e)
        {
            radGridView1.DataSource = InstrumentManager.instruments;

            radGridView1.BestFitColumns();
        }
    }
}

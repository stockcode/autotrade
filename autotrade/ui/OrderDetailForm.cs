﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using autotrade.model;
using Telerik.WinControls;

namespace autotrade.ui
{
    public partial class OrderDetailForm : Telerik.WinControls.UI.RadForm
    {
        public Order Order { get; set; }

        public OrderLog OrderLog { get; set; }
        public OrderDetailForm()
        {
            InitializeComponent();            
        }

        private void OrderDetailForm_Load(object sender, EventArgs e)
        {
            if (OrderLog != null)
            {
                radGridView1.DataSource = OrderLog.StrategyLogs.OfType<object>();
                radGridView2.DataSource = OrderLog.CloseOrder.StrategyLogs.OfType<object>();
                radGridView2.Show();
            }
            else
            {
                radGridView1.DataSource = Order.StrategyLogs.OfType<object>();
                radGridView2.Hide();
                
            }

            radGridView1.BestFitColumns();
            radGridView2.BestFitColumns();
        }
    }
}
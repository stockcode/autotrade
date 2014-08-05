﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using autotrade.model;

namespace autotrade.business
{
    public interface IStrategy
    {
        List<Order> Match(MarketData marketData);
    }
}

﻿using System.ComponentModel;
using autotrade.model;
using autotrade.util;
using CTPTradeApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace autotrade.business
{
    class AccountManager
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private TradeApi tradeApi;

        public BindingList<Account> Accounts = new BindingList<Account>();

        public delegate void AccountNotifyHandler( object sender, AccountEventArgs e );
    
        public event AccountNotifyHandler OnQryTradingAccount;
        
        

        public AccountManager(TradeApi tradeApi)
        {
            this.tradeApi = tradeApi;
            this.tradeApi.OnRspQryTradingAccount += tradeApi_OnRspQryTradingAccount;

            Accounts.Add(new Account());
        }

        void tradeApi_OnRspQryTradingAccount(ref CThostFtdcTradingAccountField pTradingAccount, ref CThostFtdcRspInfoField pRspInfo, int nRequestID, bool bIsLast)
        {
            if (pRspInfo.ErrorID == 0)
            {
                ObjectUtils.Copy(pTradingAccount, Accounts[0]);
            }

            //log.Info(pTradingAccount + ":" + bIsLast);
        }

        public int QryTradingAccount()
        {
            return tradeApi.QryTradingAccount();
        }
    }

    internal class AccountEventArgs : EventArgs
    {
        public Account account { get; set; }
        public AccountEventArgs(Account account)
            : base()
        {
            this.account = account;
        }
    }
}

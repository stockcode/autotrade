using System.Threading;
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
        public delegate void AccountNotifyHandler( object sender, AccountEventArgs e );
    
        public event AccountNotifyHandler OnQryTradingAccount;

        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private TradeApi tradeApi;

        public AccountManager(TradeApi tradeApi)
        {
            this.tradeApi = tradeApi;
            this.tradeApi.OnRspQryTradingAccount += tradeApi_OnRspQryTradingAccount;
        }

        void tradeApi_OnRspQryTradingAccount(ref CThostFtdcTradingAccountField pTradingAccount, ref CThostFtdcRspInfoField pRspInfo, int nRequestID, bool bIsLast)
        {
            if (pRspInfo.ErrorID == 0)
            {
                Account account = new Account();

                ObjectUtils.Copy(pTradingAccount, account);

                OnQryTradingAccount(this, new AccountEventArgs(account));
                
            }
        }

        public void QryTradingAccount()
        {
            tradeApi.QryTradingAccount();                                    
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

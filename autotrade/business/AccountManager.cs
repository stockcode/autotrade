using System.ComponentModel;
using autotrade.model;
using autotrade.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantBox.CSharp2CTP;
using QuantBox.CSharp2CTP.Event;

namespace autotrade.business
{
    class AccountManager
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private TraderApiWrapper tradeApi;

        public BindingList<Account> Accounts = new BindingList<Account>();

        public delegate void AccountNotifyHandler( object sender, AccountEventArgs e );
    
        public event AccountNotifyHandler OnQryTradingAccount;



        public AccountManager(TraderApiWrapper tradeApi)
        {
            this.tradeApi = tradeApi;
            this.tradeApi.OnRspQryTradingAccount += tradeApi_OnRspQryTradingAccount;

            Accounts.Add(new Account());
        }

        void tradeApi_OnRspQryTradingAccount(object sender, OnRspQryTradingAccountArgs e)
        {
            if (e.pRspInfo.ErrorID == 0)
            {
                ObjectUtils.Copy(e.pTradingAccount, Accounts[0]);
            }
        }

        public void QryTradingAccount()
        {
            tradeApi.ReqQryTradingAccount();
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

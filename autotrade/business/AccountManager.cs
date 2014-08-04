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
    public class AccountManager
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public TraderApiWrapper tradeApi { get; set; }

        public BindingList<Account> Accounts = new BindingList<Account>();

        public delegate void AccountNotifyHandler( object sender, AccountEventArgs e );
    
        public event AccountNotifyHandler OnQryTradingAccount;



        public AccountManager()
        {
            Accounts.Add(new Account());
        }


        public void QryTradingAccount()
        {
            tradeApi.ReqQryTradingAccount();
        }
    }

    public class AccountEventArgs : EventArgs
    {
        public Account account { get; set; }
        public AccountEventArgs(Account account)
            : base()
        {
            this.account = account;
        }
    }
}

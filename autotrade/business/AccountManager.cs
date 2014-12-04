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
using MongoRepository;

namespace autotrade.business
{
    public class AccountManager
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public TraderApiWrapper tradeApi { get; set; }

        public BindingList<Account> Accounts = new BindingList<Account>();

        private MongoRepository<Account> accountRepo = new MongoRepository<Account>();

        public delegate void AccountNotifyHandler( object sender, AccountEventArgs e );
    
        public event AccountNotifyHandler OnQryTradingAccount;



        public AccountManager()
        {
            Accounts.ListChanged += Accounts_ListChanged;
        }

        void Accounts_ListChanged(object sender, ListChangedEventArgs e)
        {
            var account = Accounts[e.NewIndex];
            switch (e.ListChangedType)
            {
                case ListChangedType.ItemAdded:
                    account.Id = null;
                    accountRepo.Add(account);
                    break;
                case ListChangedType.ItemChanged:
                    accountRepo.Update(account);

                    break;
                case ListChangedType.ItemDeleted:
                    accountRepo.Delete(account);
                    break;
            }
        }


        public void QryTradingAccount()
        {
            tradeApi.ReqQryTradingAccount();
        }

        public void Init(CThostFtdcTradingAccountField pTradingAccount)
        {            
            var account = accountRepo.FirstOrDefault(o => o.AccountID == pTradingAccount.AccountID) ?? new Account();

            ObjectUtils.CopyStruct(pTradingAccount, account);

            Accounts.RaiseListChangedEvents = false;

            Accounts.Add(account);

            Accounts.RaiseListChangedEvents = true;
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

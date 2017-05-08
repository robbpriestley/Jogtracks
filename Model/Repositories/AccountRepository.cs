using System;
using System.Linq;
using System.Collections.Generic;
using DigitalWizardry.SPA_Template;

namespace DigitalWizardry.SPA_Template
{
	public class AccountRepository : DigitalWizardry.SPA_Template.IAccountRepository
	{		
		public MainContext Context { get; set; }
		
		public AccountRepository(MainContext context)
		{
			Context = context;
		}

		public void Add(Account account)
		{
			Context.Account.Add(account);
			Context.SaveChanges();
		}

		public void Update(Account account)
		{
			Context.Account.Update(account);
			Context.SaveChanges();
		}

		public Account GetByToken(Guid token)
		{
			return Context.Account.Where(x => x.Token == token).Single();
		}

		public Account GetByUserName(string userName)
		{
			return Context.Account.Where(x => x.UserName == userName).Single();
		}
	}
}
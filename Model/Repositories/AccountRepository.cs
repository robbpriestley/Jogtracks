using System;
using System.Linq;
using System.Collections.Generic;

namespace DigitalWizardry.Jogtracks
{
	public class AccountRepository: IAccountRepository
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

		public List<Account> GetCoaches()
		{
			return Context.Account.Where(x => x.AccountType.Equals("COACH")).OrderBy(x => x.UserName).ToList();
		}

		public List<Account> GetLinkedAccounts(Account user, bool includeSelf)
		{
			List<Account> accounts = null;

			if (user.AccountType == "JOGGER")
			{
				accounts = new List<Account>();
				accounts.Add(user);
			}
			else
			{
				if (!includeSelf)
				{
					accounts = Context.Account.Where(x => !x.UserName.Equals(user.UserName)).ToList();
				}
				else
				{
					accounts = Context.Account.ToList();
				}
			}

			return accounts.OrderBy(x => x.UserName).ToList();
		}

		public string GetUserColor(string userName)
		{
			Account user = GetByUserName(userName);
			return user.UserColor;
		}

		public void DeleteByUserName(string userName)
		{
			Account account = GetByUserName(userName);
			Context.Account.Remove(account);
			Context.SaveChanges();
		}
	}
}
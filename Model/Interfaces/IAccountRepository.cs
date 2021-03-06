using System;
using System.Collections.Generic;

namespace DigitalWizardry.Jogtracks
{
	public interface IAccountRepository
	{
		void Add(Account account);
		void Update(Account account);
		Account GetByToken(Guid token);
		Account GetByUserName(string userName);
		List<Account> GetCoaches();
		List<Account> GetLinkedAccounts(Account user, bool includeSelf);
		string GetUserColor(string userName);
		void DeleteByUserName(string userName);
	}
}
using System;
using System.Collections.Generic;

namespace DigitalWizardry.SPA_Template
{
	public interface IAccountRepository
	{
		void Add(Account account);
		void Update(Account account);
		Account GetByToken(Guid token);
		Account GetByUserName(string userName);
	}
}
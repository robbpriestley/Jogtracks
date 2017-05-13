using System;
using System.Collections.Generic;

namespace DigitalWizardry.Jogtracks
{
	public interface IJogRepository
	{
		int Count();
		void Add(Jog jog);
		int GetTotalByUserAccount(Account user);
		List<Jog> GetByUserName(string userName);
		List<Jog> GetAll(DateTime? fromDate, DateTime? toDate);
		List<Jog> GetByUserAccount(Account user, DateTime? fromDate, DateTime? toDate);
	}
}
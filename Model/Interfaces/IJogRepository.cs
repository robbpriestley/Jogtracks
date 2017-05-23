using System;
using System.Collections.Generic;

namespace DigitalWizardry.Jogtracks
{
	public interface IJogRepository
	{
		int Count();
		void Add(Jog jog);
		void Update(Jog jog);
		void Delete(int id);
		int GetTotalByUserAccount(Account user);
		Jog GetById(int id);
		List<Jog> GetByUserName(string userName);
		List<Jog> GetAll(DateTime? fromDate, DateTime? toDate);
		List<Jog> GetByUserAccount(Account user, DateTime? fromDate, DateTime? toDate);
		void DeleteByUserName(string userName);
	}
}
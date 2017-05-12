using System;
using System.Collections.Generic;

namespace DigitalWizardry.Jogtracks
{
	public interface IJogRepository
	{
		int Count();
		List<Jog> GetAll();
		List<Jog> GetByUserName(string userName);
		List<Jog> GetByUserAccount(Account user);
		void Add(Jog jog);
	}
}
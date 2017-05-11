using System;
using System.Collections.Generic;

namespace DigitalWizardry.Jogtracks
{
	public interface IJogRepository
	{
		int Count();
		List<Jog> GetAll();
		List<Jog> GetByUserName(string userName);
		void Add(Jog jog);
	}
}
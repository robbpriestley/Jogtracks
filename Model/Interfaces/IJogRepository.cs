using System;
using System.Collections.Generic;

namespace DigitalWizardry.Jogtracks
{
	public interface IJogRepository
	{
		int Count();
		List<Jog> GetAll();
		void Add(Jog jog);
	}
}
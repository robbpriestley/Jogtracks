using System;
using System.Collections.Generic;

namespace DigitalWizardry.Jogtracks
{
	public interface IItemRepository
	{
		int Count();
		List<Item> GetAll();
		void Add(Item item);
	}
}
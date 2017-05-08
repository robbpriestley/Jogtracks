using System;
using System.Collections.Generic;

namespace DigitalWizardry.SPA_Template
{
	public interface IItemRepository
	{
		int Count();
		List<Item> GetAll();
		void Add(Item item);
	}
}
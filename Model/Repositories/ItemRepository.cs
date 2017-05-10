using System;
using System.Linq;
using System.Collections.Generic;

namespace DigitalWizardry.Jogtracks
{
	public class ItemRepository: IItemRepository
	{		
		public MainContext Context { get; set; }
		
		public ItemRepository(MainContext context)
		{
			Context = context;
		}

		public int Count()
		{
			return Context.Item.Count();
		}
	
		public List<Item> GetAll()
		{
			List<Item> items = null;
			
			try
			{
				items = Context.Item.ToList();
			}
			catch (System.InvalidOperationException)
			{
				// There are none, I suppose.
			}

			return items;
		}

		public void Add(Item item)
		{
			Context.Item.Add(item);
			Context.SaveChanges();
		}
	}
}
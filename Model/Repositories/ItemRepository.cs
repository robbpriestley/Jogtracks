using System;
using System.Linq;
using System.Collections.Generic;

namespace DigitalWizardry.Jogtracks
{
	public class JogRepository: IJogRepository
	{		
		public MainContext Context { get; set; }
		
		public JogRepository(MainContext context)
		{
			Context = context;
		}

		public int Count()
		{
			return Context.Jog.Count();
		}
	
		public List<Jog> GetAll()
		{
			List<Jog> jogs = null;
			
			try
			{
				jogs = Context.Jog.ToList();
			}
			catch (System.InvalidOperationException)
			{
				// There are none, I suppose.
			}

			return jogs;
		}

		public void Add(Jog jog)
		{
			Context.Jog.Add(jog);
			Context.SaveChanges();
		}
	}
}
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

		public void Add(Jog jog)
		{
			Context.Jog.Add(jog);
			Context.SaveChanges();
		}

		public void Update(Jog jog)
		{
			Context.Jog.Update(jog);
			Context.SaveChanges();
		}

		public void Delete(int id)
		{
			Jog jog = Context.Jog.Where(x => x.Id == id).Single();
			Context.Jog.Remove(jog);
			Context.SaveChanges();
		}

		public Jog GetById(int id)
		{
			Jog jog = null;
			
			try
			{
				jog = Context.Jog.Where(x => x.Id == id).Single();
			}
			catch (System.InvalidOperationException)
			{
				// It doesn't exist, I suppose.
			}

			return jog;
		}

		public List<Jog> GetByUserName(string userName)
		{
			List<Jog> jogs = null;
			
			try
			{
				jogs = Context.Jog.Where(x => x.UserName == userName).OrderByDescending(x => x.Date).ToList();
			}
			catch (System.InvalidOperationException)
			{
				// There are none, I suppose.
			}

			return jogs;
		}

		public List<Jog> GetByUserAccount(Account user, DateTime? fromDate, DateTime? toDate)
		{
			List<Jog> jogs = null;

			if (user.AccountType.Equals("JOGGER"))
			{
				jogs = GetJogsByJogger(user, fromDate, toDate);
			}
			else
			{
				jogs = GetAll(fromDate, toDate);
			}

			// Don't return a null list.
			return jogs == null ? new List<Jog>() : jogs.OrderByDescending(x => x.UserName).ToList();
		}
	
		public List<Jog> GetAll(DateTime? fromDate, DateTime? toDate)
		{
			List<Jog> jogs = null;
			
			try
			{
				if (fromDate != null && toDate != null)
				{
					jogs = Context.Jog.Where(x => x.Date >= fromDate && x.Date < ((DateTime)toDate).AddDays(1)).ToList();
				}
				else
				{
					jogs = Context.Jog.ToList();
				}
			}
			catch (System.InvalidOperationException)
			{
				// There are none, I suppose.
			}

			return jogs;
		}

		public int GetTotalByUserAccount(Account user)
		{
			int total = 0;

			if (user.AccountType.Equals("JOGGER"))
			{
				total = GetTotalJogsByJogger(user);
			}
			else
			{
				total = Count();
			}

			// Don't return a null list. Also, postpone sorting due to efficiency gained as UserColor must still be obtained.
			return total;
		}

		#region Helper Methods

		private List<Jog> GetJogsByJogger(Account user, DateTime? fromDate, DateTime? toDate)
		{
			List<Jog> jogs = null;

			try
			{
				if (fromDate != null && toDate != null)
				{
					jogs = Context.Jog.Where(x => x.UserName == user.UserName && x.Date >= fromDate && x.Date < ((DateTime)toDate).AddDays(1)).ToList();
				}
				else
				{
					jogs = Context.Jog.Where(x => x.UserName == user.UserName).ToList();	
				}
			}
			catch (System.InvalidOperationException)
			{
				// There are none, I suppose.
			}

			return jogs;
		}

		private int GetTotalJogsByJogger(Account user)
		{
			int total = 0;

			try
			{
				total = Context.Jog.Where(x => x.UserName == user.UserName).Count();	
			}
			catch (System.InvalidOperationException)
			{
				// There are none, I suppose.
			}

			return total;
		}

		private List<Account> GetAllAccounts()
		{
			List<Account> accounts = null;
			
			try
			{
				accounts = Context.Account.ToList();
			}
			catch (System.InvalidOperationException)
			{
				// There are none, I suppose.
			}

			return accounts;
		}

		public void DeleteByUserName(string userName)
		{			
			Context.Jog.RemoveRange(Context.Jog.Where(x => x.UserName.Equals(userName)));
			Context.SaveChanges();
		}

		#endregion
	}
}
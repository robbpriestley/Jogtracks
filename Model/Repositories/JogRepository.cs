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
			
			switch (user.AccountType)
			{
				case "JOGGER":
					jogs = GetJogsByJogger(user, fromDate, toDate);
					break;

				case "COACH":
					jogs = GetJogsByCoach(user, fromDate, toDate);
					break;

				case "ADMIN":
					jogs = GetAll(fromDate, toDate);
					break;
				
				default:
					break;
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
					jogs = Context.Jog.Where(x => x.Date >= fromDate && x.Date <= ((DateTime)toDate).AddDays(1)).ToList();
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
			
			switch (user.AccountType)
			{
				case "JOGGER":
					total = GetTotalJogsByJogger(user);
					break;

				case "COACH":
					total = GetTotalJogsByCoach(user);
					break;

				case "ADMIN":
					total = Count();
					break;
				
				default:
					break;
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
					jogs = Context.Jog.Where(x => x.UserName == user.UserName && x.Date >= fromDate && x.Date <= ((DateTime)toDate).AddDays(1)).ToList();
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

		private List<Jog> GetJogsByCoach(Account coach, DateTime? fromDate, DateTime? toDate)
		{
			List<Jog> jogs = new List<Jog>();
			
			try
			{
				List<Account> joggers = GetAccountsByCoach(coach);
				joggers.Add(coach);

				foreach (Account jogger in joggers)
				{
					if (fromDate != null && toDate != null)
					{
						jogs.AddRange(Context.Jog.Where(x => x.UserName == jogger.UserName && x.Date >= fromDate && x.Date <= ((DateTime)toDate).AddDays(1)).ToList());
					}
					else
					{
						jogs.AddRange(Context.Jog.Where(x => x.UserName == jogger.UserName).ToList());
					}
				}
			}
			catch (System.InvalidOperationException)
			{
				// There are no jogs, I suppose.
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

		private int GetTotalJogsByCoach(Account coach)
		{
			int total = 0;
			
			try
			{
				List<Account> joggers = GetAccountsByCoach(coach);
				joggers.Add(coach);

				foreach (Account jogger in joggers)
				{
					total += Context.Jog.Where(x => x.UserName == jogger.UserName).Count();
				}
			}
			catch (System.InvalidOperationException)
			{
				// There are no jogs, I suppose.
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

		private List<Account> GetAccountsByCoach(Account coach)
		{
			List<Account> accounts = null;
			
			try
			{
				accounts = Context.Account.Where(x => x.Coach == coach.UserName).ToList();
			}
			catch (System.InvalidOperationException)
			{
				// There are none, I suppose.
			}

			return accounts;
		}

		#endregion
	}
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Linq;
using System.Text;

namespace DigitalWizardry.Jogtracks.Controllers
{
	[Route("api")]
	public class ApiController : Controller
	{	
		public Secrets Secrets { get; set; }
		public IServiceLogRepository ServiceLogs { get; set; }
		public IAccountRepository Accounts { get; set; }
		public IJogRepository Jogs { get; set; }
		
		public ApiController
		(
			IOptions<Secrets> secrets,
			IServiceLogRepository serviceLogs,
			IAccountRepository accounts,
			IJogRepository jogs
		)
		{
			Secrets = secrets.Value;
			ServiceLogs = serviceLogs;
			Accounts = accounts;
			Jogs = jogs;
		}

		#region Authentication Output

		public class AuthOutput
		{
			public string AccountType { get; set; }
			public string Coach { get; set; }
			public string Token { get; set; }
			public string UserName { get; set; }

			public AuthOutput(string token, string accountType, string userName, string coach)
			{
				Token = token;
				Coach = coach;
				UserName = userName;
				AccountType = accountType;
			}
		}

		#endregion
		#region Authentication: Sign Up
		
		public class SignUpInput
		{
			public string UserName { get; set; }
			public string Password { get; set; }
			public string AccountType { get; set; }
		}

		[HttpPost]
		[Route("auth/signup")]
		public IActionResult SignUp([FromBody] SignUpInput signUpData)
		{
			if (!Utility.BasicAuthentication(Secrets, Request))
			{
				return new UnauthorizedResult();
			}

			AuthOutput authOutput = null;

			try
			{				
				try
				{
					Accounts.GetByUserName(signUpData.UserName);
					// This is bad. The account already exists. Reject the sign up by returning 204 code.
					ServiceLogs.SignUp(Request, "ERROR: Account Exists", signUpData.UserName);
					return new StatusCodeResult(204);
				}
				catch (System.InvalidOperationException)
				{
					// This is good. The account doesn't exist yet. Create the account and return token.
					HashData hashData = HashPassword(signUpData.Password);
					
					Account user = new Account();
					user.UserName = signUpData.UserName;
					user.UserColor = RandomUserColor();
					user.Salt = hashData.Salt;
					user.Hash = hashData.Hash;
					user.Token = Guid.NewGuid();;
					user.AccountType = signUpData.AccountType;
					Accounts.Add(user);

					authOutput = new AuthOutput(user.Token.ToString(), user.AccountType, user.UserName, null);  // Coach is always null on new sign up.
					
					ServiceLogs.SignUp(Request, signUpData.AccountType, signUpData.UserName);
				}
			}
			catch (System.Exception e)
			{
				ServiceLogs.Error(Request, "[EXCEPTION] " + e.ToString(), "ApiController.SignUp()", signUpData == null ? null : signUpData.UserName);
				return new StatusCodeResult(500);
			}

			return Utility.JsonObjectResult(authOutput);
		}

		private string RandomUserColor()
		{
			List<string> colors = new List<string>();

			colors.Add("Chartreuse");
			colors.Add("DarkSeaGreen");
			colors.Add("GreenYellow");
			colors.Add("LawnGreen");
			colors.Add("LightGreen");
			colors.Add("Lime");
			colors.Add("LimeGreen");
			colors.Add("MediumAquamarine");
			colors.Add("MediumSpringGreen");
			colors.Add("PaleGreen");
			colors.Add("SpringGreen");
			colors.Add("YellowGreen");

			Random r = new Random();
			int i = r.Next(colors.Count);
			return colors[i];
		}

		#endregion
		#region Authentication: Sign In

		public class SignInInput
		{
			public string UserName { get; set; }
			public string Password { get; set; }
		}
		
		[HttpPost]
		[Route("auth/signin")]
		public IActionResult SignIn([FromBody] SignInInput signInData)
		{
			if (!Utility.BasicAuthentication(Secrets, Request))
			{
				return new UnauthorizedResult();
			}

			AuthOutput authOutput = null;

			try
			{				
				try
				{
					Account user = Accounts.GetByUserName(signInData.UserName);
					
					string hash = HashPassword(user.Salt, signInData.Password);

					if (user.Hash != hash)
					{
						// This is bad. The password doesn't match. Reject the sign in by returning 204 code.
						ServiceLogs.SignIn(Request, "ERROR: Bad Password", signInData.UserName);
						return new StatusCodeResult(204);
					}

					user.Token = Guid.NewGuid();
					Accounts.Update(user);

					authOutput = new AuthOutput(user.Token.ToString(), user.AccountType, user.UserName, user.Coach);
					
					ServiceLogs.SignIn(Request, user.Token.ToString(), signInData.UserName);
				}
				catch (System.InvalidOperationException)
				{
					// The user's account doesn't exist. Reject the request by returning 500 code.
					ServiceLogs.SignIn(Request, "ERROR: Account Doesn't Exist", signInData.UserName);
					return new StatusCodeResult(204);
				}
			}
			catch (System.Exception e)
			{
				ServiceLogs.Error(Request, "[EXCEPTION] " + e.ToString(), "ApiController.SignIn()", signInData == null ? null : signInData.UserName);
				return new StatusCodeResult(500);
			}

			return Utility.JsonObjectResult(authOutput);
		}

		#endregion
		#region Authentication: Change Password

		public class ChangePasswordInput
		{
			public string Token { get; set; }
			public string Password { get; set; }
		}

		[HttpPost]
		[Route("auth/changepassword")]
		public IActionResult ChangePassword([FromBody] ChangePasswordInput changePasswordData)
		{
			if (!Utility.BasicAuthentication(Secrets, Request))
			{
				return new UnauthorizedResult();
			}

			try
			{				
				try
				{
					HashData hashData = HashPassword(changePasswordData.Password);
					
					Account user = Accounts.GetByToken(Guid.Parse(changePasswordData.Token));
					user.Salt = hashData.Salt;
					user.Hash = hashData.Hash;
					Accounts.Update(user);

					ServiceLogs.Access(Request, null, user.UserName);
				}
				catch (System.InvalidOperationException)
				{
					// The user's account doesn't exist. Reject the request by returning 500 code.
					ServiceLogs.Access(Request, "ERROR: Account Doesn't Exist", changePasswordData.Token);
					return new StatusCodeResult(204);
				}
			}
			catch (System.Exception e)
			{
				ServiceLogs.Error(Request, "[EXCEPTION] " + e.ToString(), "ApiController.ChangePassword()", changePasswordData.Token);
				return new StatusCodeResult(500);
			}

			return new ObjectResult("SUCCESS");
		}

		#endregion
		#region Authentication: Utility

		public class HashData
		{
			public byte[] Salt { get; set; }
			public string Hash { get; set; }
		}

		private HashData HashPassword(string password)
		{
			byte[] salt = new byte[128 / 8];

			using (var r = RandomNumberGenerator.Create())
			{
				r.GetBytes(salt);
			}

			string hash = Convert.ToBase64String
			(
				KeyDerivation.Pbkdf2
				(
					password: password,
					salt: salt,
					prf: KeyDerivationPrf.HMACSHA1,
					iterationCount: 10000,
					numBytesRequested: 256 / 8
				)
			);

			HashData hashData = new HashData();
			hashData.Salt = salt;
			hashData.Hash = hash;

			return hashData;
		}

		private string HashPassword(byte[] salt, string password)
		{
			string hash = Convert.ToBase64String
			(
				KeyDerivation.Pbkdf2
				(
					password: password,
					salt: salt,
					prf: KeyDerivationPrf.HMACSHA1,
					iterationCount: 10000,
					numBytesRequested: 256 / 8
				)
			);

			return hash;
		}
		
		#endregion
		#region Jogs: Jog List

		[HttpGet]
		[Route("jogs")]
		public IActionResult JogsList(string token)
		{			
			if (!Utility.BasicAuthentication(Secrets, Request))
			{
				return new UnauthorizedResult();
			}
			
			Account user = GetUser(token);

			if (user == null)
			{
				return new StatusCodeResult(204);
			}
			
			ServiceLogs.Access(Request, null, user.UserName);
			
			List<JogOutput> jogOutputs = new List<JogOutput>();

			try
			{			
				string currentUser = null;
				string currentUserColor = null;
				List<Jog> jogs = Jogs.GetByUserAccount(user, null, null);

				foreach (Jog jog in jogs)
				{
					if (currentUser == null || !currentUser.Equals(jog.UserName))
					{
						currentUser = jog.UserName;
						currentUserColor = Accounts.GetUserColor(jog.UserName);
					}
					
					JogOutput jogOutput = new JogOutput();

					jogOutput.Id = jog.Id;
					jogOutput.UserName = currentUser;
					jogOutput.UserColor = currentUserColor;
					jogOutput.Date = JogOutput.DateStringCalc(jog.Date);
					jogOutput.Year = jog.Date.Year;
					jogOutput.Month = jog.Date.Month;
					jogOutput.Day = jog.Date.Day;
					jogOutput.Week = JogOutput.WeekOfYearCalc(jog.Date);
					jogOutput.Distance = jog.Distance;
					jogOutput.Time = jog.Time;
					jogOutput.TimeString = JogOutput.TimeStringCalc(jog.Time);
					jogOutput.AverageSpeed = jog.AverageSpeed;

					jogOutputs.Add(jogOutput);
				}

				// Final sort!
				jogOutputs = jogOutputs.OrderByDescending(x => x.Date).ThenBy(y => y.UserName).ToList();
			}
			catch (System.Exception e)
			{
				ServiceLogs.Error(Request, "[EXCEPTION] " + e.ToString(), "ApiController.JogsList()", token);
				return new StatusCodeResult(500);
			}

			return Utility.JsonObjectResult(jogOutputs);
		}

		[HttpGet]
		[Route("jogsfilter")]
		public IActionResult JogsListFilter(string token, string fromDate, string toDate)
		{			
			if (!Utility.BasicAuthentication(Secrets, Request))
			{
				return new UnauthorizedResult();
			}
			
			Account user = GetUser(token);

			if (user == null)
			{
				return new StatusCodeResult(204);
			}
			
			ServiceLogs.Access(Request, fromDate + " " + toDate, user.UserName);
			
			List<JogOutput> jogOutputs = new List<JogOutput>();

			try
			{			
				string currentUser = null;
				string currentUserColor = null;
				DateTime from = DateTime.Parse(fromDate);
				DateTime to = DateTime.Parse(toDate);
				
				List<Jog> jogs = Jogs.GetByUserAccount(user, from, to);

				foreach (Jog jog in jogs)
				{
					if (currentUser == null || !currentUser.Equals(jog.UserName))
					{
						currentUser = jog.UserName;
						currentUserColor = Accounts.GetUserColor(jog.UserName);
					}
					
					JogOutput jogOutput = new JogOutput();

					jogOutput.Id = jog.Id;
					jogOutput.UserName = currentUser;
					jogOutput.UserColor = currentUserColor;
					jogOutput.Date = JogOutput.DateStringCalc(jog.Date);
					jogOutput.Year = jog.Date.Year;
					jogOutput.Month = jog.Date.Month;
					jogOutput.Day = jog.Date.Day;
					jogOutput.Week = JogOutput.WeekOfYearCalc(jog.Date);
					jogOutput.Distance = jog.Distance;
					jogOutput.Time = jog.Time;
					jogOutput.TimeString = JogOutput.TimeStringCalc(jog.Time);
					jogOutput.AverageSpeed = jog.AverageSpeed;

					jogOutputs.Add(jogOutput);
				}

				// Final sort!
				jogOutputs = jogOutputs.OrderByDescending(x => x.Date).ThenBy(y => y.UserName).ToList();
			}
			catch (System.Exception e)
			{
				ServiceLogs.Error(Request, "[EXCEPTION] " + e.ToString(), "ApiController.JogsListFilter()", token);
				return new StatusCodeResult(500);
			}

			return Utility.JsonObjectResult(jogOutputs);
		}

		public class JogOutput
		{
			public int Id { get; set; }
			public string UserName { get; set; }
			public string UserColor { get; set; }
			public string Date { get; set; }
			public int Year { get; set; }
			public int Month { get; set; }
			public int Day { get; set; }
			public int Week { get; set; }
			public decimal Distance { get; set; }
			public int Time { get; set; }
			public string TimeString { get; set; }
			public decimal AverageSpeed { get; set; }

			public static string DateStringCalc(DateTime dt)
			{
				string month = dt.Month < 10 ? "0" + dt.Month.ToString() : dt.Month.ToString();
				string day = dt.Day < 10 ? "0" + dt.Day.ToString() : dt.Day.ToString();
				return dt.Year.ToString() + "-" + month + "-" + day;
			}

			public static int WeekOfYearCalc(DateTime dt)
			{
				DateTimeFormatInfo info = DateTimeFormatInfo.CurrentInfo;
				Calendar cal = info.Calendar;
				return cal.GetWeekOfYear(dt, info.CalendarWeekRule, info.FirstDayOfWeek);
			}

			public static string TimeStringCalc(int time)
			{
				int h = time / 3600;
				time = time - h * 3600;
				int m = time / 60;
				time = time - m * 60;
				int s = time;

				string hs = h < 10 ? "0" + h.ToString() : h.ToString();
				string ms = m < 10 ? "0" + m.ToString() : m.ToString();
				string ss = s < 10 ? "0" + s.ToString() : s.ToString();

				return hs + ":" + ms + ":" + ss;
			}
		}

		#endregion
		#region Jogs: Jog Total Count

		[HttpGet]
		[Route("jogstotal")]
		public IActionResult JogsTotal(string token)
		{			
			if (!Utility.BasicAuthentication(Secrets, Request))
			{
				return new UnauthorizedResult();
			}
			
			Account user = GetUser(token);

			if (user == null)
			{
				return new StatusCodeResult(204);
			}
			
			ServiceLogs.Access(Request, null, user.UserName);

			int total = 0;

			try
			{			
				total = Jogs.GetTotalByUserAccount(user);
			}
			catch (System.Exception e)
			{
				ServiceLogs.Error(Request, "[EXCEPTION] " + e.ToString(), "ApiController.JogsTotal()", token);
				return new StatusCodeResult(500);
			}

			return new ObjectResult(total);
		}

		#endregion
		#region Jogs: Jog Add

		public class JogAddInput
		{
			public string Token { get; set; }
			public string UserName { get; set; }
			public string Date { get; set; }
			public decimal Distance { get; set; }
			public int Time { get; set; }
		}

		[HttpPost]
		[Route("jogs/add")]
		public IActionResult JogAdd([FromBody] JogAddInput jogAddInput)
		{
			if (!Utility.BasicAuthentication(Secrets, Request))
			{
				return new UnauthorizedResult();
			}

			Account user = GetUser(jogAddInput.Token);

			if (user == null)
			{
				return new StatusCodeResult(204);
			}

			ServiceLogs.Access(Request, null, user.UserName);

			try
			{				
				Jog jog = new Jog();
				jog.UserName = jogAddInput.UserName;
				jog.AddedBy = user.UserName;
				jog.Date = DateTime.Parse(jogAddInput.Date);
				jog.Distance = jogAddInput.Distance;
				jog.Time = jogAddInput.Time;
				jog.AverageSpeed = (decimal)((double)jog.Distance / ((double)jog.Time / 3600.0d));
				Jogs.Add(jog);
			}
			catch (System.Exception e)
			{
				ServiceLogs.Error(Request, "[EXCEPTION] " + e.ToString(), "ApiController.JogAdd()", user.UserName);
				return new StatusCodeResult(500);
			}

			return new ObjectResult("SUCCESS");
		}

		#endregion
		#region Coaches: Coach List

		[HttpGet]
		[Route("coaches")]
		public IActionResult CoachList(string token)
		{			
			if (!Utility.BasicAuthentication(Secrets, Request))
			{
				return new UnauthorizedResult();
			}
			
			Account user = GetUser(token);

			if (user == null)
			{
				return new StatusCodeResult(204);
			}
			
			ServiceLogs.Access(Request, null, user.UserName);

			List<CoachOutput> coaches = new List<CoachOutput>();

			try
			{				
				List<Account> coachAccounts = Accounts.GetCoaches();

				// Convert full accounts list to basic list.
				foreach(Account coachAccount in coachAccounts)
				{
					coaches.Add(new CoachOutput(coachAccount.UserName));
				}
			}
			catch (System.Exception e)
			{
				ServiceLogs.Error(Request, "[EXCEPTION] " + e.ToString(), "ApiController.CoachList()", token);
				return new StatusCodeResult(500);
			}

			return Utility.JsonObjectResult(coaches);
		}

		public class CoachOutput
		{
			public string UserName { get; set; }
			
			public CoachOutput(string userName)
			{
				UserName = userName;
			}
		}

		#endregion
		#region Coaches: Coach Patch
			
		public class CoachPatchInput
		{
			public string Token { get; set; }
			public string Coach { get; set; }
		}
		
		[HttpPatch]
		[Route("coachpatch")]
		public IActionResult CoachPatch([FromBody] CoachPatchInput coachPatchInput)
		{			
			if (!Utility.BasicAuthentication(Secrets, Request))
			{
				return new UnauthorizedResult();
			}
			
			Account user = GetUser(coachPatchInput.Token);

			if (user == null)
			{
				return new StatusCodeResult(204);
			}
			
			ServiceLogs.Access(Request, null, user.UserName);

			try
			{				
				user.Coach = coachPatchInput.Coach == "null" ? null : coachPatchInput.Coach;
				Accounts.Update(user);
			}
			catch (System.Exception e)
			{
				ServiceLogs.Error(Request, "[EXCEPTION] " + e.ToString(), "ApiController.SetCoach()", coachPatchInput.Token);
				return new StatusCodeResult(500);
			}

			return new ObjectResult("SUCCESS");
		}

		#endregion
		#region Utility
			
		private Account GetUser(string token)
		{
			Account user = null;

			try
			{
				user = Accounts.GetByToken(Guid.Parse(token));  // Look up the current user by their token.
			}
			catch (System.InvalidOperationException)
			{
				ServiceLogs.Access(Request, "ERROR: Token not found, cannot get user", token);
			}

			return user;
		}

		#endregion
	}
}
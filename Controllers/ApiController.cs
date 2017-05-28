using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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

		public class JogInput
		{
			public int Id { get; set; }
			public string Token { get; set; }
			public string UserName { get; set; }
			public string Date { get; set; }
			public decimal Distance { get; set; }
			public int Time { get; set; }
		}

		public class AccountOutput
		{
			public string UserName { get; set; }
			public string AccountType { get; set; }
			
			public AccountOutput(string userName, string accountType)
			{
				UserName = userName;
				AccountType = accountType;
			}
		}

		#region Authentication: Check Token
		
		[HttpGet]
		[Route("auth")]
		public IActionResult CheckToken(string token)
		{
			if (!Utility.BasicAuthentication(Secrets, Request))
			{
				return new UnauthorizedResult();
			}

			bool result = false;

			try
			{				
				try
				{
					Account user = Accounts.GetByToken(Guid.Parse(token));
					result = true;
				}
				catch (System.InvalidOperationException)
				{
					// The token is not valid.
					result = false;
				}
			}
			catch (System.Exception e)
			{
				ServiceLogs.Error(Request, "[EXCEPTION] " + e.ToString(), "ApiController.CheckToken()", token);
				result = false;
			}

			return new ObjectResult(result);
		}
		
		#endregion
		#region Authentication Output

		public class AuthOutput
		{
			public string AccountType { get; set; }
			public string Token { get; set; }
			public string UserName { get; set; }
			public string ValidationMessage { get; set; }

			public AuthOutput(string token, string accountType, string userName)
			{
				Token = token;
				UserName = userName;
				AccountType = accountType;
			}
			
			public AuthOutput(string validationMessage)
			{
				ValidationMessage = validationMessage;
			}
		}

		#endregion
		#region Authentication: Sign Up
		
		public class SignUpInput
		{
			public string UserName { get; set; }
			public string Password { get; set; }
		}

		[HttpPost]
		[Route("auth/signup")]
		public IActionResult SignUp([FromBody] SignUpInput input)
		{
			if (!Utility.BasicAuthentication(Secrets, Request))
			{
				return new UnauthorizedResult();
			}

			AuthOutput authOutput = null;

			try
			{				
				ValidateSignUpInput(input);
				
				try
				{
					Accounts.GetByUserName(input.UserName);
					// This is bad. The account already exists. Reject the sign up by returning 204 code.
					ServiceLogs.SignUp(Request, "ERROR: Account Exists", input.UserName);
					return new StatusCodeResult(204);
				}
				catch (System.InvalidOperationException)
				{
					// This is good. The account doesn't exist yet. Create the account and return token.
					HashData hashData = HashPassword(input.Password);
					
					Account user = new Account();
					user.UserName = input.UserName;
					user.Salt = hashData.Salt;
					user.Hash = hashData.Hash;
					user.Token = Guid.NewGuid();
					user.AccountType = "JOGGER";  // Sign ups are always Joggers. They can be promoted by admins and by coaches.
					Accounts.Add(user);

					user.UserColor = DetermineUserColor(user.Id);  // User colour is determined by Id to enforce cycling of colors.
					Accounts.Update(user);

					authOutput = new AuthOutput(user.Token.ToString(), user.AccountType, user.UserName);
					
					ServiceLogs.SignUp(Request, "JOGGER", input.UserName);
				}
			}
			catch (ValidationException ve)
			{
				authOutput = new AuthOutput(ve.Message);
				ServiceLogs.Error(Request, "[VALIDATION] " + ve.ToString(), "ApiController.SignUp()", input == null ? null : input.UserName);
			}
			catch (System.Exception e)
			{
				ServiceLogs.Error(Request, "[EXCEPTION] " + e.ToString(), "ApiController.SignUp()", input == null ? null : input.UserName);
				return new StatusCodeResult(500);
			}

			return Utility.JsonObjectResult(authOutput);
		}

		private void ValidateSignUpInput(SignUpInput input)
		{
			Regex r = new Regex("^[a-zA-Z0-9]*$");
	
			if (input.UserName == null || input.Password == null)
			{
				throw new ValidationException("Both username and password must be provided.");
			}
			else if (!r.IsMatch(input.UserName))
			{
				throw new ValidationException("Username must be alphanumeric.");
			}
			else if (!r.IsMatch(input.Password))
			{
				throw new ValidationException("Password must be alphanumeric.");
			}
			else if (input.Password.Length < 8)
			{
				throw new ValidationException("Password must be at least 8 characters long.");
			}
		}

		private string DetermineUserColor(int id)
		{
			List<string> colors = new List<string>();

			colors.Add("Gainsboro");
			colors.Add("Khaki");
			colors.Add("Lavender");
			colors.Add("LemonChiffon");
			colors.Add("LightCyan");
			colors.Add("LightGoldenrodYellow");
			colors.Add("Moccasin");
			colors.Add("PaleGoldenrod");
			colors.Add("PapayaWhip");
			colors.Add("PeachPuff");

			return colors[id % 10];
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
		public IActionResult SignIn([FromBody] SignInInput input)
		{
			if (!Utility.BasicAuthentication(Secrets, Request))
			{
				return new UnauthorizedResult();
			}

			AuthOutput authOutput = null;

			try
			{				
				ValidateSignInInput(input);
				
				try
				{
					Account user = Accounts.GetByUserName(input.UserName);
					
					string hash = HashPassword(user.Salt, input.Password);

					if (user.Hash != hash)
					{
						// This is bad. The password doesn't match. Reject the sign in by returning 204 code.
						ServiceLogs.SignIn(Request, "ERROR: Bad Password", input.UserName);
						return new StatusCodeResult(204);
					}

					user.Token = Guid.NewGuid();
					Accounts.Update(user);

					authOutput = new AuthOutput(user.Token.ToString(), user.AccountType, user.UserName);
					
					ServiceLogs.SignIn(Request, user.Token.ToString(), input.UserName);
				}
				catch (System.InvalidOperationException)
				{
					// The user's account doesn't exist. Reject the request by returning 500 code.
					ServiceLogs.SignIn(Request, "ERROR: Account Doesn't Exist", input.UserName);
					return new StatusCodeResult(204);
				}
			}
			catch (ValidationException ve)
			{
				authOutput = new AuthOutput(ve.Message);
				ServiceLogs.Error(Request, "[VALIDATION] " + ve.ToString(), "ApiController.SignIn()", input == null ? null : input.UserName);
			}
			catch (System.Exception e)
			{
				ServiceLogs.Error(Request, "[EXCEPTION] " + e.ToString(), "ApiController.SignIn()", input == null ? null : input.UserName);
				return new StatusCodeResult(500);
			}

			return Utility.JsonObjectResult(authOutput);
		}

		private void ValidateSignInInput(SignInInput input)
		{
			Regex r = new Regex("^[a-zA-Z0-9]*$");
	
			if (input.UserName == null || input.Password == null)
			{
				throw new ValidationException("Both username and password must be provided.");
			}
			else if (!r.IsMatch(input.UserName))
			{
				throw new ValidationException("Username must be alphanumeric.");
			}
			else if (!r.IsMatch(input.Password))
			{
				throw new ValidationException("Password must be alphanumeric.");
			}
			else if (input.Password.Length < 8)
			{
				throw new ValidationException("Password must be at least 8 characters long.");
			}
		}

		#endregion
		#region Authentication: Change Password

		public class ChangePasswordInput
		{
			public string Token { get; set; }
			public string Password { get; set; }
			public string UserName { get; set; }
		}

		[HttpPost]
		[Route("auth/changepassword")]
		public IActionResult ChangePassword([FromBody] ChangePasswordInput input)
		{
			if (!Utility.BasicAuthentication(Secrets, Request))
			{
				return new UnauthorizedResult();
			}

			Account user = GetUser(input.Token);

			if (user == null)
			{
				return new StatusCodeResult(204);
			}
			
			ServiceLogs.Access(Request, null, user.UserName);
			
			try
			{				
				ValidateChangePasswordInput(input);
				
				try
				{
					HashData hashData = HashPassword(input.Password);
					
					Account target = Accounts.GetByUserName(input.UserName);
					target.Salt = hashData.Salt;
					target.Hash = hashData.Hash;
					Accounts.Update(target);
				}
				catch (System.InvalidOperationException)
				{
					// The user's account doesn't exist. Reject the request by returning 500 code.
					ServiceLogs.Access(Request, "ERROR: Account Doesn't Exist", input.UserName);
					return new StatusCodeResult(204);
				}
			}
			catch (ValidationException ve)
			{
				ServiceLogs.Error(Request, "[VALIDATION] " + ve.ToString(), "ApiController.ChangePassword()", null);
				return new StatusCodeResult(204);
			}
			catch (System.Exception e)
			{
				ServiceLogs.Error(Request, "[EXCEPTION] " + e.ToString(), "ApiController.ChangePassword()", input.Token);
				return new StatusCodeResult(500);
			}

			return new ObjectResult("SUCCESS");
		}

		private void ValidateChangePasswordInput(ChangePasswordInput input)
		{
			Regex r = new Regex("^[a-zA-Z0-9]*$");
	
			if (input.Password == null)
			{
				throw new ValidationException("Password must be provided.");
			}
			else if (!r.IsMatch(input.Password))
			{
				throw new ValidationException("Password must be alphanumeric.");
			}
			else if (input.Password.Length < 8)
			{
				throw new ValidationException("Password must be at least 8 characters long.");
			}
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
		#region Jogs

		[HttpGet]
		[Route("jog")]
		public IActionResult Jog(int id, string token)
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
			
			JogOutput jogOutput = null;

			try
			{			
				jogOutput = new JogOutput();
				
				Jog jog = Jogs.GetById(id);

				jogOutput.Id = jog.Id;
				jogOutput.UserName = user.UserName;
				jogOutput.UserColor = user.UserColor;
				jogOutput.Date = JogOutput.DateStringCalc(jog.Date);
				jogOutput.Year = jog.Date.Year;
				jogOutput.Month = jog.Date.Month;
				jogOutput.Day = jog.Date.Day;
				jogOutput.Week = JogOutput.WeekOfYearCalc(jog.Date);
				jogOutput.Distance = jog.Distance;
				jogOutput.Time = jog.Time;
				jogOutput.TimeString = JogOutput.TimeStringCalc(jog.Time);
				jogOutput.AverageSpeed = jog.AverageSpeed;
			}
			catch (System.Exception e)
			{
				ServiceLogs.Error(Request, "[EXCEPTION] " + e.ToString(), "ApiController.JogsList()", token);
				return new StatusCodeResult(500);
			}

			return Utility.JsonObjectResult(jogOutput);
		}

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
		[Route("jogs/filter")]
		public IActionResult JogsListFilter(string fromDate, string toDate, string token)
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
		[Route("jogs/total")]
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
		#region Jogs: Jog Add and Update

		[HttpPost]
		[Route("jog")]
		public IActionResult JogAdd([FromBody] JogInput input)
		{
			if (!Utility.BasicAuthentication(Secrets, Request))
			{
				return new UnauthorizedResult();
			}

			Account user = GetUser(input.Token);

			if (user == null)
			{
				return new StatusCodeResult(204);
			}

			ServiceLogs.Access(Request, null, user.UserName);

			try
			{				
				ValidateJogInput(input);
				
				Jog jog = new Jog();
				jog.UserName = input.UserName;
				jog.UpdatedBy = user.UserName;
				jog.Date = DateTime.Parse(input.Date);
				jog.Distance = input.Distance;
				jog.Time = input.Time;
				jog.AverageSpeed = (decimal)((double)jog.Distance / ((double)jog.Time / 3600.0d));
				Jogs.Add(jog);
			}
			catch (ValidationException ve)
			{
				ServiceLogs.Error(Request, "[VALIDATION] " + ve.ToString(), "ApiController.JogAdd()", user.UserName);
				return new ObjectResult(ve.Message);
			}
			catch (System.Exception e)
			{
				ServiceLogs.Error(Request, "[EXCEPTION] " + e.ToString(), "ApiController.JogAdd()", user.UserName);
				return new StatusCodeResult(500);
			}

			return new ObjectResult("SUCCESS");
		}

		[HttpPut]
		[Route("jog")]
		public IActionResult JogUpdate([FromBody] JogInput input)
		{
			if (!Utility.BasicAuthentication(Secrets, Request))
			{
				return new UnauthorizedResult();
			}

			Account user = GetUser(input.Token);

			if (user == null)
			{
				return new StatusCodeResult(204);
			}

			ServiceLogs.Access(Request, null, user.UserName);

			try
			{				
				ValidateJogInput(input);
			
				Jog jog = Jogs.GetById(input.Id);
				jog.UserName = input.UserName;
				jog.UpdatedBy = user.UserName;
				jog.Date = DateTime.Parse(input.Date);
				jog.Distance = input.Distance;
				jog.Time = input.Time;
				jog.AverageSpeed = (decimal)((double)jog.Distance / ((double)jog.Time / 3600.0d));
				Jogs.Update(jog);
			}
			catch (ValidationException ve)
			{
				ServiceLogs.Error(Request, "[VALIDATION] " + ve.ToString(), "ApiController.SignUp()", user.UserName);
				return new ObjectResult(ve.Message);
			}
			catch (System.Exception e)
			{
				ServiceLogs.Error(Request, "[EXCEPTION] " + e.ToString(), "ApiController.JogUpdate()", user.UserName);
				return new StatusCodeResult(500);
			}

			return new ObjectResult("SUCCESS");
		}

		private void ValidateJogInput(JogInput input)
		{
			if 
			(
				input.Date == null ||
				input.Token == null ||
				input.UserName == null
				// Distance, Id, and Time are non-nullable fields.
			)
			{
				throw new ValidationException("All jog data fields must be provided.");
			}
			else if (input.Distance < 0 || input.Distance > 1000)
			{
				throw new ValidationException("Distance must be in the range 0 to 1000.");
			}
			else if (input.Time < 0 || input.Time > 86400)
			{
				throw new ValidationException("Time in seconds must be in the range 0 to 86400.");
			}
		}


		#endregion
		#region Jogs: Jog Total Count

		[HttpDelete]
		[Route("jog")]
		public IActionResult JogDelete([FromBody] JogInput jogInput)
		{			
			if (!Utility.BasicAuthentication(Secrets, Request))
			{
				return new UnauthorizedResult();
			}
			
			Account user = GetUser(jogInput.Token);

			if (user == null)
			{
				return new StatusCodeResult(204);
			}
			
			ServiceLogs.Access(Request, "Jog Id: " + jogInput.Id, user.UserName);

			try
			{			
				Jogs.Delete(jogInput.Id);
			}
			catch (System.Exception e)
			{
				ServiceLogs.Error(Request, "[EXCEPTION] " + e.ToString(), "ApiController.JogDelete()", jogInput.Token);
				return new StatusCodeResult(500);
			}

			return new ObjectResult("SUCCESS");
		}

		#endregion
		#region Accounts

		public class AccountInput
		{
			public string Token { get; set; }
			public string UserName { get; set; }
			public string AccountType { get; set; }
		}

		#endregion
		#region Accounts: Accounts List

		[HttpGet]
		[Route("accounts")]
		public IActionResult AccountList(string token, bool includeSelf)
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

			List<AccountOutput> accounts = new List<AccountOutput>();

			try
			{				
				List<Account> userAccounts = Accounts.GetLinkedAccounts(user, includeSelf);

				// Convert full accounts list to basic list.
				foreach(Account userAccount in userAccounts)
				{
					string accountType = null;

					switch (userAccount.AccountType)
					{
						case "COACH":
							accountType = "Coach";
							break;
						
						case "ADMIN":
							accountType = "Admin";
							break;
						
						default:
							accountType = "Jogger";
							break;
					}

					accounts.Add(new AccountOutput(userAccount.UserName, accountType));
				}
			}
			catch (System.Exception e)
			{
				ServiceLogs.Error(Request, "[EXCEPTION] " + e.ToString(), "ApiController.AccountList()", token);
				return new StatusCodeResult(500);
			}

			return Utility.JsonObjectResult(accounts);
		}

		#endregion
		#region Accounts: Update Account Type

		[HttpPatch]
		[Route("account")]
		public IActionResult UpdateAccountType([FromBody] AccountInput input)
		{			
			if (!Utility.BasicAuthentication(Secrets, Request))
			{
				return new UnauthorizedResult();
			}
			
			Account user = GetUser(input.Token);

			if (user == null)
			{
				return new StatusCodeResult(204);
			}
			
			ServiceLogs.Access(Request, null, user.UserName);

			try
			{				
				Account account = Accounts.GetByUserName(input.UserName);
				account.AccountType = input.AccountType;
				Accounts.Update(account);
			}
			catch (System.Exception e)
			{
				ServiceLogs.Error(Request, "[EXCEPTION] " + e.ToString(), "ApiController.DeleteAccount()", input.UserName);
				return new StatusCodeResult(500);
			}

			return new ObjectResult("SUCCESS");
		}

		#endregion
		#region Accounts: Delete Account

		[HttpDelete]
		[Route("account")]
		public IActionResult DeleteAccount([FromBody] AccountInput input)
		{			
			if (!Utility.BasicAuthentication(Secrets, Request))
			{
				return new UnauthorizedResult();
			}
			
			Account user = GetUser(input.Token);

			if (user == null)
			{
				return new StatusCodeResult(204);
			}
			
			ServiceLogs.Access(Request, null, user.UserName);

			try
			{				
				Jogs.DeleteByUserName(input.UserName);
				Accounts.DeleteByUserName(input.UserName);
			}
			catch (System.Exception e)
			{
				ServiceLogs.Error(Request, "[EXCEPTION] " + e.ToString(), "ApiController.DeleteAccount()", input.UserName);
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

			List<AccountOutput> coaches = new List<AccountOutput>();

			try
			{				
				List<Account> coachAccounts = Accounts.GetCoaches();

				// Convert full accounts list to basic list.
				foreach(Account coachAccount in coachAccounts)
				{
					coaches.Add(new AccountOutput(coachAccount.UserName, "Coach"));
				}
			}
			catch (System.Exception e)
			{
				ServiceLogs.Error(Request, "[EXCEPTION] " + e.ToString(), "ApiController.CoachList()", token);
				return new StatusCodeResult(500);
			}

			return Utility.JsonObjectResult(coaches);
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

class ValidationException : Exception
{
	public ValidationException(string message) : base(message) {}
}
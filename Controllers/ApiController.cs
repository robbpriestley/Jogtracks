using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace DigitalWizardry.Jogtracks.Controllers
{
	[Route("api")]
	public class ApiController : Controller
	{	
		public Secrets Secrets { get; set; }
		public IServiceLogRepository ServiceLogs { get; set; }
		public IAccountRepository Accounts { get; set; }
		public IItemRepository Items { get; set; }
		
		public ApiController
		(
			IOptions<Secrets> secrets,
			IServiceLogRepository serviceLogs,
			IAccountRepository accounts,
			IItemRepository items
		)
		{
			Secrets = secrets.Value;
			ServiceLogs = serviceLogs;
			Accounts = accounts;
			Items = items;
		}

		#region Authentication

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

		public class HashData
		{
			public byte[] Salt { get; set; }
			public string Hash { get; set; }
		}

		#endregion

		[HttpGet]
		[Route("items")]
		public IActionResult ItemList(string token)
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
			
			List<Item> items = null;

			try
			{			
				items = Items.GetAll();
			}
			catch (System.Exception e)
			{
				ServiceLogs.Error(Request, "[EXCEPTION] " + e.ToString(), "ApiController.ItemList()", token);
				return new StatusCodeResult(500);
			}

			return Utility.JsonObjectResult(items);
		}

		[HttpPost]
		[Route("items/add")]
		public IActionResult ItemAdd(string userId)
		{
			if (!Utility.BasicAuthentication(Secrets, Request))
			{
				return new UnauthorizedResult();
			}

			try
			{				
				ServiceLogs.Access(Request, null, userId);
				
				string name = Request.Form["name"];
				int rating = Int32.Parse(Request.Form["rating"]);
				string description = Request.Form["description"];

				Item item = new Item();
				item.Name = name;
				item.Rating = rating;
				item.Description = description;
				Items.Add(item);
			}
			catch (System.Exception e)
			{
				ServiceLogs.Error(Request, "[EXCEPTION] " + e.ToString(), "ApiController.ItemAdd()", userId);
				return new StatusCodeResult(500);
			}

			return new ObjectResult("SUCCESS");
		}

		#region Coaches
			
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
				user.Coach = coachPatchInput.Coach;
				Accounts.Update(user);
			}
			catch (System.Exception e)
			{
				ServiceLogs.Error(Request, "[EXCEPTION] " + e.ToString(), "ApiController.SetCoach()", coachPatchInput.Token);
				return new StatusCodeResult(500);
			}

			return new ObjectResult("SUCCESS");
		}

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

			List<Coach> coaches = new List<Coach>();

			try
			{				
				List<Account> coachAccounts = Accounts.GetCoaches();

				// Convert full accounts list to basic list.
				foreach(Account coachAccount in coachAccounts)
				{
					coaches.Add(new Coach(coachAccount.UserName));
				}
			}
			catch (System.Exception e)
			{
				ServiceLogs.Error(Request, "[EXCEPTION] " + e.ToString(), "ApiController.CoachList()", token);
				return new StatusCodeResult(500);
			}

			return Utility.JsonObjectResult(coaches);
		}

		public class Coach
		{
			public string UserName { get; set; }
			
			public Coach(string userName)
			{
				UserName = userName;
			}
		}

		public class CoachPatchInput
		{
			public string Token { get; set; }
			public string Coach { get; set; }
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
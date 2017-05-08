using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace DigitalWizardry.SPA_Template.Controllers
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

		[HttpPost]
		[Route("auth/signup")]
		public IActionResult SignUp([FromBody] AuthData authData)
		{
			if (!Utility.BasicAuthentication(Secrets, Request))
			{
				return new UnauthorizedResult();
			}

			Guid? token = null;

			try
			{				
				try
				{
					Accounts.GetByUserName(authData.UserName);
					// This is bad. The account already exists. Reject the sign up by returning 204 code.
					ServiceLogs.SignUp(Request, "ERROR: Account Exists", authData.UserName);
					return new StatusCodeResult(204);
				}
				catch (System.InvalidOperationException)
				{
					// This is good. The account doesn't exist yet. Create the account and return token.
					token = Guid.NewGuid();
					HashData hashData = HashPassword(authData.Password);
					
					Account account = new Account();
					account.UserName = authData.UserName;
					account.Salt = hashData.Salt;
					account.Hash = hashData.Hash;
					account.Token = (Guid)token;
					Accounts.Add(account);
					
					ServiceLogs.SignUp(Request, null, authData.UserName);
				}
			}
			catch (System.Exception e)
			{
				ServiceLogs.Error(Request, "[EXCEPTION] " + e.ToString(), "ApiController.SignUp()", authData == null ? null : authData.UserName);
				return new StatusCodeResult(500);
			}

			return new ObjectResult(token);
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

		[HttpPost]
		[Route("auth/signin")]
		public IActionResult SignIn([FromBody] AuthData authData)
		{
			if (!Utility.BasicAuthentication(Secrets, Request))
			{
				return new UnauthorizedResult();
			}

			Guid? token = null;

			try
			{				
				try
				{
					Account account = Accounts.GetByUserName(authData.UserName);
					
					string hash = HashPassword(account.Salt, authData.Password);

					if (account.Hash != hash)
					{
						// This is bad. The password doesn't match. Reject the sign in by returning 204 code.
						ServiceLogs.SignIn(Request, "ERROR: Bad Password", authData.UserName);
						return new StatusCodeResult(204);
					}

					token = Guid.NewGuid();
					account.Token = (Guid)token;
					Accounts.Update(account);
					
					ServiceLogs.SignIn(Request, token.ToString(), authData.UserName);
				}
				catch (System.InvalidOperationException)
				{
					// This is bad. The account doesn't exist. Reject the sign in by returning 204 code.
					ServiceLogs.SignIn(Request, "ERROR: Account Doesn't Exist", authData.UserName);
					return new StatusCodeResult(204);
				}
			}
			catch (System.Exception e)
			{
				ServiceLogs.Error(Request, "[EXCEPTION] " + e.ToString(), "ApiController.SignIn()", authData == null ? null : authData.UserName);
				return new StatusCodeResult(500);
			}

			return new ObjectResult(token);
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

		public class AuthData
		{
			public string UserName { get; set; }
			public string Password { get; set; }
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
			
			Account account = null;

			try
			{
				account = Accounts.GetByToken(Guid.Parse(token));
			}
			catch (System.InvalidOperationException)
			{
				// This is bad. The account doesn't exist. Reject the request by returning 500 code.
				ServiceLogs.Access(Request, "ERROR: Token Doesn't Exist", token);
				return new StatusCodeResult(204);
			}
			
			List<Item> items = null;

			try
			{				
				ServiceLogs.Access(Request, null, account.UserName);

				try
				{
					items = Items.GetAll();
				}
				catch (System.InvalidOperationException)
				{
					ServiceLogs.Error(Request, "[ITEMS LOAD FAILURE] ", "ApiController.ItemList()", account.UserName);
					return new StatusCodeResult(500);
				}
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
	}
}
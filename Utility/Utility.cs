using System;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace DigitalWizardry.Jogtracks
{	
	public static class Utility
	{
		public static string Json(Object obj)
		{
			string json = 
			
			JsonConvert.SerializeObject
			(
				obj, 
				Formatting.Indented, 
				new JsonSerializerSettings 
				{ 
					ReferenceLoopHandling = ReferenceLoopHandling.Ignore
				}
			);
			
			return json;
		}
		
		public static ObjectResult JsonObjectResult(Object obj)
		{		
			return new ObjectResult(Json(obj));
		}

		public static bool BasicAuthentication(Secrets secrets, HttpRequest request)
		{
			bool authenticated = false;
			
			try
			{
				string header = request.Headers["Authorization"];
				
				if (header != null)
				{
					string[] userpass = ParseHeader(header);
					
					// Now check for a match
					if (secrets.BasicAuthUsername == userpass[0] && secrets.BasicAuthPassword == userpass[1])
					{
						authenticated = true;
					}
					else
					{
						// Log error (Bad credentials)
					}
				}
				else
				{
					// Log error (No authorization header in request)
				}
			}
			catch (System.Exception)
			{
				// Log error (Probable authorization header format problem)
			}
			
			return authenticated;
		}
		
		private static string[] ParseHeader(string header)
		{
			// Header should look like this: "Basic ZnVkZ2VidWNrZXQ6Y2h1bmRlcm11ZmZpbg=="
			// Ignore the "Basic" part and decode the payload aspect into plain-text
			string[] tokens = header.Split(' ');
			byte[] data = Convert.FromBase64String(tokens[1]);
			string decoded = Encoding.UTF8.GetString(data);
			return decoded.Split(':');
		}
	}
}
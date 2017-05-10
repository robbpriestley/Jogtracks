using System;
using Microsoft.AspNetCore.Http;

namespace DigitalWizardry.Jogtracks
{
    public class ServiceLogRepository: IServiceLogRepository
    {		
		public ServiceLogContext Context { get; set; }
		
		public ServiceLogRepository(ServiceLogContext context)
        {
            Context = context;
        }
		
		public void Access(HttpRequest request, string message, string userId)
        {
			Add(ServiceLogType.ACCESS, request, message, null, userId);
        }

		public void SignUp(HttpRequest request, string message, string userId)
		{
			Add(ServiceLogType.SIGNUP, request, message, null, userId);
		}

		public void SignIn(HttpRequest request, string message, string userId)
		{
			Add(ServiceLogType.SIGNIN, request, message, null, userId);
		}

		public void Error(HttpRequest request, string message, string caller, string userId)
        {
			Add(ServiceLogType.ERROR, request, message, caller, userId);
        }
		
		public void Test(HttpRequest request, string message, string userId)
		{
			Add(ServiceLogType.TEST, request, message, null, userId);
		}
		
		private void Add(ServiceLogType type, HttpRequest request, string message, string caller, string userId)
		{
			string path = request.Path;
			
			if (request.QueryString.HasValue)
			{
				path += request.QueryString.ToString();
			}
			
			if (caller != null)
			{
				message = caller + " *** " + message;	
			}

			ServiceLog log = new ServiceLog(type, path, message, userId);
			
			Context.ServiceLog.Add(log);
            Context.SaveChanges();
			
			Console.WriteLine("*** LOG MESSAGE *** UtcTime: " + log.UtcTime + " Type: " + type.ToString() + " Path: " + path + " Message: " + message + " UserId: " + userId);
		}
    }
}
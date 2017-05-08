using Microsoft.AspNetCore.Http;

namespace DigitalWizardry.SPA_Template
{
	public interface IServiceLogRepository
	{
		void Access(HttpRequest request, string message, string userId);
		void SignUp(HttpRequest request, string message, string userId);
		void SignIn(HttpRequest request, string message, string userId);
		void Error(HttpRequest request, string message, string caller, string userId);
		void Test(HttpRequest request, string message, string userId);
	}
}
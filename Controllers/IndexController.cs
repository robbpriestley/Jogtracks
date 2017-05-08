using Microsoft.AspNetCore.Mvc;

namespace DigitalWizardry.SPA_Template.Controllers
{
	public class IndexController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
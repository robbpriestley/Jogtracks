using Microsoft.AspNetCore.Mvc;

namespace DigitalWizardry.Jogtracks.Controllers
{
	public class IndexController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
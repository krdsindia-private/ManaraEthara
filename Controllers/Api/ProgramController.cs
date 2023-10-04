using Microsoft.AspNetCore.Mvc;

namespace ManarEthara.Controllers.Api {
    public class ProgramController : Controller {
        public IActionResult Index() {
            return View();
        }
    }
}

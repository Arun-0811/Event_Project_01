using Microsoft.AspNetCore.Mvc;

namespace AR_Events.Areas.Login.Controllers
{
    public class BaseController : Controller
    {
        protected string GetUserSession()
        {
            return HttpContext.Session.GetString("my-session");
        }

        protected bool IsLoggedIn()
        {
            return !string.IsNullOrEmpty(GetUserSession());
        }
    }

}

using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;

namespace CloudSOA.Portal.Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string returnUrl)
        {
            // Store properties to be picked up by owin authentication handlers
            var properties = new AuthenticationProperties { RedirectUri = returnUrl };
            AuthenticationManager.AuthenticationResponseChallenge = new AuthenticationResponseChallenge(null, properties);
            
            // Generate http 401 response. will be catched and handled by owin auth handler later in the response chain. 
            return new HttpUnauthorizedResult();
        }


        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            return RedirectToAction("Index", "Home");
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }
    }
}
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Zambo.Domain.Models.Common.Request;
using Zambo.Domain.Models.Security.Request;
using Zambo.Site.Extensions;

namespace Value.Site.Controllers
{
    public class LoginController : BaseController
    {
    
    // colocar no html
    // <script src="@System.Web.Optimization.BundleTable.Bundles.ResolveBundleUrl("~/bundles/value-newrequest")"></script>

        // GET: Login
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(TokenRequest param)
        {
            try
            {
                var isauthenticated = Request.IsAuthenticated;
                param.BrowserIpAdress = Request.UserHostAddress;
                param.ClientHostName = Request.UserHostName;
                var response = SecurityClient.GetToken(param);

                if (response.ContainsKey("error_message"))
                {
                    ViewBag.ErrorMessage = response["error_message"];
                    return View("Index");
                }
                else
                {
                    Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName,
                        FormsAuthentication.Encrypt(
                            new FormsAuthenticationTicket(
                                new Random().Next(), //version
                                                                response["userName"], // user name
                                                                Convert.ToDateTime(response[".issued"]),             //creation
                                                                Convert.ToDateTime(response[".expires"]), //Expiration
                                                                false, //Persistent
                                                                Guid.NewGuid().ToString()))));
                    AddCookie(response["userName"], response["access_token"], Convert.ToDateTime(response[".expires"]));

                    SessionManager.Current.Cookied = response;
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = string.Format("{0} {1}", ex.Message, ex.InnerException);
                return View("Index");
            }
        }

        [HttpGet]
        public ActionResult Logout()
        {
            if (SessionManager.Current.Cookied != null)
            {
                var response = SecurityClient.GetLogout(new GeneralRequest<object>() { Token = SessionManager.Current.AccessToken });
                try
                {
                    if (response.Success)
                    {
                        FormsAuthentication.SignOut();
                        RemoveCookie(SessionManager.Current.UserName);
                        Session.RemoveAll();
                        Session.Clear();
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = string.Format("{0} {1}", ex.Message, ex.InnerException);
                }
            }
            return View("Index");
        }
    }
}
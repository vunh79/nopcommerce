using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Nop.Spa.Models;
using Microsoft.AspNetCore.Http.Extensions;
using Nop.Spa.Managers;
using Microsoft.AspNetCore.Authorization;
using Nop.Spa.Parameters;
using Newtonsoft.Json;
using Nop.Spa.Helpers;

namespace Nop.Spa.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var baseUrl = UrlHelpers.GetBaseUrl(this.Request);

            ViewBag.domain = baseUrl;
            return View();
        }

        public IActionResult About()
        {
            ViewBag.customUri = $"Custom Uri={UrlHelpers.GetAbsoluteUri(this.Request, "/Home/GetAccessToken")}";
            ViewBag.absoluteUri = $"Absolute Uri = {UrlHelpers.GetAbsoluteUri(this.Request)}";
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Login()
        {
            var model = new UserAccessModel();

            var absoluteUri = UrlHelpers.GetAbsoluteUri(this.Request, "/Home/GetAccessToken");
            ViewData["Message"] = "Your contact page.";
            model.RedirectUrl = absoluteUri;

            return View(model);
        }
        [HttpPost]
        public IActionResult Login(UserAccessModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var nopAuthorizationManager = new AuthorizationManager(model.ClientId, model.ClientSecret, model.ServerUrl);

                    var redirectUrl = UrlHelpers.GetAbsoluteUri(this.Request, "/Home/GetAccessToken");
                    //var redirectUrl = Url.RouteUrl("GetAccessToken", null, Request.Scheme);
                    if (redirectUrl != model.RedirectUrl)
                    {
                        return BadRequest();
                    }

                    // For demo purposes this data is kept into the current Session, but in production environment you should keep it in your database
                    HttpContext.Session.SetString("clientId", model.ClientId);
                    HttpContext.Session.SetString("clientSecret", model.ClientSecret);
                    HttpContext.Session.SetString("serverUrl", model.ServerUrl);
                    HttpContext.Session.SetString("redirectUrl", model.RedirectUrl);

                    // // This should not be saved anywhere.
                    var state = Guid.NewGuid();
                    // Session["state"] = state;
                    HttpContext.Session.SetString("state", state.ToString());

                    string authUrl = nopAuthorizationManager.BuildAuthUrl(redirectUrl, new string[] { }, state.ToString());

                    return Redirect(authUrl);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            return BadRequest();
        }

        [HttpPost]
        public IActionResult GetAccessToken(AccessModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var nopAuthorizationManager = new AuthorizationManager(model.UserAccessModel.ClientId, model.UserAccessModel.ClientSecret, model.UserAccessModel.ServerUrl);

                    var redirectUrl = UrlHelpers.GetAbsoluteUri(this.Request, "/Home/GetAccessToken");
                    //var redirectUrl = Url.RouteUrl("GetAccessToken", null, Request.Scheme);
                    if (redirectUrl != model.UserAccessModel.RedirectUrl)
                    {
                        return BadRequest();
                    }

                    // For demo purposes this data is kept into the current Session, but in production environment you should keep it in your database
                    HttpContext.Session.SetString("clientId", model.UserAccessModel.ClientId);
                    HttpContext.Session.SetString("clientSecret", model.UserAccessModel.ClientSecret);
                    HttpContext.Session.SetString("serverUrl", model.UserAccessModel.ServerUrl);
                    HttpContext.Session.SetString("redirectUrl", model.UserAccessModel.RedirectUrl);

                    // // This should not be saved anywhere.
                    var state = Guid.NewGuid();
                    // Session["state"] = state;
                    HttpContext.Session.SetString("state", state.ToString());

                    string authUrl = nopAuthorizationManager.BuildAuthUrl(redirectUrl, new string[] { }, state.ToString());

                    return Redirect(authUrl);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            return BadRequest();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetAccessToken(string code, string state)
        {
            if (ModelState.IsValid && !string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(state))
            {
                if (state != HttpContext.Session.GetString("state"))
                {
                    return BadRequest();
                }

                var model = new AccessModel();
                ViewBag.refreshToken = UrlHelpers.GetAbsoluteUri(this.Request, "/Home/RefreshAccessToken");

                try
                {
                    // TODO: Here you should get the authorization user data from the database instead from the current Session.
                    string clientId = HttpContext.Session.GetString("clientId");
                    string clientSecret = HttpContext.Session.GetString("clientSecret");
                    string serverUrl = HttpContext.Session.GetString("serverUrl");
                    string redirectUrl = HttpContext.Session.GetString("redirectUrl");

                    var authParameters = new AuthParameters()
                    {
                        ClientId = clientId,
                        ClientSecret = clientSecret,
                        ServerUrl = serverUrl,
                        RedirectUrl = redirectUrl,
                        GrantType = "authorization_code",
                        Code = code
                    };

                    var nopAuthorizationManager = new AuthorizationManager(authParameters.ClientId, authParameters.ClientSecret, authParameters.ServerUrl);

                    string responseJson = nopAuthorizationManager.GetAuthorizationData(authParameters);

                    AuthorizationModel authorizationModel = JsonConvert.DeserializeObject<AuthorizationModel>(responseJson);

                    model.AuthorizationModel = authorizationModel;
                    model.UserAccessModel = new UserAccessModel()
                    {
                        ClientId = clientId,
                        ClientSecret = clientSecret,
                        ServerUrl = serverUrl,
                        RedirectUrl = redirectUrl
                    };

                    // TODO: Here you can save your access and refresh tokens in the database. For illustration purposes we will save them in the Session and show them in the view.
                    //  Session["accessToken"] = authorizationModel.AccessToken;
                    HttpContext.Session.SetString("accessToken", authorizationModel.AccessToken);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }

                return View(model);
            }

            return BadRequest();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult RefreshAccessToken(string refreshToken, string clientId, string clientSecret, string serverUrl)
        {
            string json = string.Empty;
            var model = new AccessModel();

            if (ModelState.IsValid &&
                !string.IsNullOrEmpty(refreshToken) &&
                !string.IsNullOrEmpty(clientId) &&
                 !string.IsNullOrEmpty(clientSecret) &&
                !string.IsNullOrEmpty(serverUrl))
            {


                try
                {
                    var authParameters = new AuthParameters()
                    {
                        ClientId = clientId,
                        ClientSecret = clientSecret,
                        ServerUrl = serverUrl,
                        RefreshToken = refreshToken,
                        GrantType = "refresh_token"
                    };

                    var nopAuthorizationManager = new AuthorizationManager(authParameters.ClientId,
                        authParameters.ClientSecret, authParameters.ServerUrl);

                    string responseJson = nopAuthorizationManager.RefreshAuthorizationData(authParameters);

                    //AuthorizationModel authorizationModel = JsonConvert.DeserializeObject<AuthorizationModel>(responseJson);

                    AuthorizationModel authorizationModel = JsonConvert.DeserializeObject<AuthorizationModel>(responseJson);

                    model.AuthorizationModel = authorizationModel;
                    model.UserAccessModel = new UserAccessModel()
                    {
                        ClientId = clientId,
                        ClientSecret = clientSecret,
                        ServerUrl = serverUrl,
                        RedirectUrl = HttpContext.Session.GetString("redirectUrl")
                    };

                    // Here we use the temp data because this method is called via ajax and here we can't hold a session.
                    // This is needed for the GetCustomers method in the CustomersController.
                    TempData["accessToken"] = authorizationModel.AccessToken;
                    TempData["serverUrl"] = serverUrl;
                }
                catch (Exception ex)
                {
                    json = string.Format("error: '{0}'", ex.Message);

                    return Json(json);
                }

                json = JsonConvert.SerializeObject(model.AuthorizationModel);
            }
            else
            {
                json = "error: 'something went wrong'";
            }

            return View("GetAccessToken", model);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Spa.Managers;
using Nop.Spa.Models;
using Nop.Spa.Parameters;

namespace Nop.Spa.Controllers
{
    public class AuthorizationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        //TODO: it is recommended to have an [Authorize] attribute set
        public ActionResult Submit(UserAccessModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var nopAuthorizationManager = new AuthorizationManager(model.ClientId, model.ClientSecret, model.ServerUrl);

                    var redirectUrl = Url.RouteUrl("GetAccessToken", null, Request.Scheme);

                    if (redirectUrl != model.RedirectUrl)
                    {
                        return BadRequest();
                    }

                    // // For demo purposes this data is kept into the current Session, but in production environment you should keep it in your database
                    //Session["clientId"] = model.ClientId;
                    //Session["clientSecret"] = model.ClientSecret;
                    //Session["serverUrl"] = model.ServerUrl;
                    //Session["redirectUrl"] = redirectUrl;

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

                return View("~/Views/AccessToken.cshtml", model);
            }

            return BadRequest();
        }

        [HttpGet]
        [AllowAnonymous]
        public JsonResult RefreshAccessToken(string refreshToken, string clientId, string clientSecret, string serverUrl)
        {
            string json = string.Empty;

            if (ModelState.IsValid &&
                !string.IsNullOrEmpty(refreshToken) &&
                !string.IsNullOrEmpty(clientId) &&
                 !string.IsNullOrEmpty(clientSecret) &&
                !string.IsNullOrEmpty(serverUrl))
            {
                var model = new AccessModel();

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

                    AuthorizationModel authorizationModel =
                        JsonConvert.DeserializeObject<AuthorizationModel>(responseJson);

                    model.AuthorizationModel = authorizationModel;
                    model.UserAccessModel = new UserAccessModel()
                    {
                        ClientId = clientId,
                        ServerUrl = serverUrl
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

            return Json(json);
        }

        private ActionResult BadRequest(string message = "Bad Request")
        {
            return BadRequest(message);
        }
    }
}
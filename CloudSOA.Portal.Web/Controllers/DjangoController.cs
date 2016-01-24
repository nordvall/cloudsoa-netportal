using CloudSOA.Portal.Web.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CloudSOA.Portal.Tokens;
using RestSharp;
using System.Net;

namespace CloudSOA.Portal.Web.Controllers
{
    [Authorize]
    public class DjangoController : Controller
    {
        // GET: Django
        public ActionResult Index()
        {
            var request = new RestRequest("", Method.GET);
            var items = SendRequest<DjangoListViewModel>(request, HttpStatusCode.OK);
            
            return View(items);
        }

        [HttpGet]
        public ActionResult Edit(int id = 0)
        {
            var item = new DjangoItemViewModel();

            if (id > 0)
            {
                var request = new RestRequest(id.ToString() + "/", Method.GET);
                item = SendRequest<DjangoItemViewModel>(request, HttpStatusCode.OK);
            }

            return View(item);
        }

        [HttpPost]
        public ActionResult Edit(DjangoItemViewModel item)
        {
            if (!ModelState.IsValid)
            {
                return View(item);
            }

            if (item.id > 0)
            {
                var request = new RestRequest(item.id.ToString() + "/", Method.PUT);
                request.AddJsonBody(item);
                SendRequest<DjangoItemViewModel>(request, HttpStatusCode.OK);
            }
            else
            {
                var request = new RestRequest("", Method.POST);
                request.AddJsonBody(item);
                SendRequest<DjangoItemViewModel>(request, HttpStatusCode.Created);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var request = new RestRequest(id.ToString() + "/", Method.DELETE);

            SendRequest(request, HttpStatusCode.NoContent);

            return RedirectToAction("Index");
        }

        private void SendRequest(RestRequest request, HttpStatusCode expectedStatus)
        {
            RestClient client = createClient();
            IRestResponse response = client.Execute(request);

            if (response.StatusCode != expectedStatus)
            {
                throw new Exception(string.Format("HTTP {0} {1}", response.StatusCode, response.StatusDescription));
            }
        }

        private T SendRequest<T>(RestRequest request, HttpStatusCode expectedStatus) where T : new()
        {
            RestClient client = createClient();
            IRestResponse<T> response = client.Execute<T>(request);

            if (response.StatusCode != expectedStatus)
            {
                throw new Exception(string.Format("HTTP {0} {1}", response.StatusCode, response.StatusDescription));
            }

            return response.Data;
        }

        private RestClient createClient()
        {
            string apiUrl = ReadConfigSettingOrThrow("django:apiurl");
            string resourceId = ReadConfigSettingOrThrow("django:resourceid");

            var tokenClient = CachingTokenClientFactory.CreateFromAppSettings(ConfigurationManager.AppSettings);
            string token = tokenClient.GetAccessTokenFromRefreshToken(User.Identity.Name, resourceId);

            var apiClient = new RestClient(apiUrl);
            apiClient.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(token, "Bearer");

            return apiClient;
        }

        private string ReadConfigSettingOrThrow(string configurationKey)
        {
            string configurationValue = ConfigurationManager.AppSettings[configurationKey];

            if (string.IsNullOrWhiteSpace(configurationValue))
            {
                throw new ConfigurationErrorsException("AppSetting missing: " + configurationKey);
            }

            return configurationValue;
        }
    }
}
using System;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;
using CloudSOA.Portal.Web.Models;
using System.Web.Helpers;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Threading.Tasks;
using System.Configuration;
using System.Web.Hosting;
using CloudSOA.Portal.Tokens;

namespace CloudSOA.Portal.Web
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            string clientId = ReadConfigSettingOrThrow("ida:ClientID");
            string authority = ReadConfigSettingOrThrow("ida:Authority");
            string redirectUri = ReadConfigSettingOrThrow("ida:RedirectUri");

            AntiForgeryConfig.UniqueClaimTypeIdentifier = System.IdentityModel.Claims.ClaimTypes.GivenName;
            
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                ClientId = clientId,
                Authority = authority,
                
                Notifications = new OpenIdConnectAuthenticationNotifications
                { 
                    AuthenticationFailed = context => 
                    {
                        context.HandleResponse();
                        context.Response.Redirect("/Error?message=" + context.Exception.Message);
                        return Task.FromResult(0);
                    },
                    AuthorizationCodeReceived = context =>
                    {
                        string userId = context.JwtSecurityToken.Claims.First(c => c.Type == "unique_name").Value;

                        // Exchange the received authorization code for an access token and a refresh token
                        // We don't need the tokens until future external api calls, so we use a background thread for this
                        HostingEnvironment.QueueBackgroundWorkItem(cancellationToken =>
                        {
                            CachingTokenClient tokenClient = CachingTokenClientFactory.CreateFromAppSettings(ConfigurationManager.AppSettings);
                            tokenClient.GetAccessTokenFromAuthorizationCode(userId, context.Code, redirectUri);
                        });

                        return Task.FromResult(0);
                    }
                }
            });
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
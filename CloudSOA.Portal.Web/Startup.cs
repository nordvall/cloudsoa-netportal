using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CloudSOA.Portal.Web.Startup))]
namespace CloudSOA.Portal.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

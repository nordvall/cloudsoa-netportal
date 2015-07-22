using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CloudSOA.NetPortal.Web.Startup))]
namespace CloudSOA.NetPortal.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

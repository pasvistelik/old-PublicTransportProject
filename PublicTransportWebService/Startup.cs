using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(PublicTransportWebService.Startup))]
namespace PublicTransportWebService
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Logistics.Startup))]
namespace Logistics
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

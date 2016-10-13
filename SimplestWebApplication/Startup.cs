using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SimplestWebApplication.Startup))]
namespace SimplestWebApplication
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

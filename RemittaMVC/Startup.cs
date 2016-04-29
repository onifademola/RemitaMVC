using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(RemittaMVC.Startup))]
namespace RemittaMVC
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

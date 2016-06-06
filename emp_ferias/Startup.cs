using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(emp_ferias.Startup))]
namespace emp_ferias
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

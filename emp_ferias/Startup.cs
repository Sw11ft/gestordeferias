using emp_ferias.lib.DAL;
using emp_ferias.Models;
using Microsoft.Owin;
using Owin;
using System.Data.Entity;

[assembly: OwinStartupAttribute(typeof(emp_ferias.Startup))]
namespace emp_ferias
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            ApplicationDbContext dbidentity = new ApplicationDbContext();
            dbidentity.Database.Initialize(false);
            EmpFeriasDbContext db = new EmpFeriasDbContext();
            
            db.Database.Initialize(true);
    }
    }
}

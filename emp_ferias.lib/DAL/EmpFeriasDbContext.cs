using emp_ferias.lib.Classes;
using emp_ferias.lib.Migrations;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace emp_ferias.lib.DAL
{
    public class EmpFeriasDbContext : DbContext
    {
        public DbSet<Marcacao> Marcacoes { get; set; }

        public EmpFeriasDbContext() : base("name=DefaultConnection")
        {

        }

        public System.Data.Entity.DbSet<emp_ferias.lib.Classes.User> Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<EmpFeriasDbContext, Configuration>());
            base.OnModelCreating(modelBuilder);
        }
    }
}

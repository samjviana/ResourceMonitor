using Server.DB.Modelos;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.DB.Conexao {
    public class DatabaseContext : DbContext {
        public DbSet<Computador> Computadores { get; set; }
        public DatabaseContext() : base("name=ResourceMonitorDBConnString") {
            //Database.SetInitializer<DatabaseContext>(new CreateDatabaseIfNotExists<DatabaseContext>());

            Database.SetInitializer<DatabaseContext>(new DropCreateDatabaseIfModelChanges<DatabaseContext>());
        }
    }
}

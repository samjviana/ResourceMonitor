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
        public DbSet<CPU> CPUs { get; set; }
        public DbSet<Armazenamento> Armazenamentos { get; set; }
        public DbSet<GPU> GPUs { get; set; }
        public DbSet<Memoria> Memorias { get; set; }

        public DatabaseContext() : base("name=ResourceMonitorDBConnString") {
            //Database.SetInitializer<DatabaseContext>(new CreateDatabaseIfNotExists<DatabaseContext>());

            Database.SetInitializer<DatabaseContext>(new DropCreateDatabaseIfModelChanges<DatabaseContext>());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder) {
            modelBuilder.Entity<Computador>()
                        .HasRequired(c => c.Memoria)
                        .WithOptional(m => m.Computador)
                        .Map(m => m.MapKey("Memoria_Id"));

            base.OnModelCreating(modelBuilder);
        }

    }
}

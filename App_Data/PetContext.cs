using System.Data.Entity;
using PetFinderAPI.Models;
using MySql.Data.EntityFramework;
using System.Data.Common;
using System.Data.Entity.Migrations.History;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.Migrations.Sql;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;
using System;

namespace PetFinderAPI.App_Data
{

    /// <summary>
    /// Inherit from MySqlMigrationSqlGenerator by fixing the Index generation method, set the default type in BTREE
    /// <para>(the MySql manual says that in the SQL script syntax it is not necessary to specify it for the innoDB engine)</para>
    /// </summary>
    public class MyCustomMigrationSQLGenerator : MySqlMigrationSqlGenerator
    {
        private string TrimSchemaPrefix(string table)
        {
            if (table.StartsWith("dbo."))
                return table.Replace("dbo.", "");
            return table;
        }

        protected override MigrationStatement Generate(CreateIndexOperation op)
        {
            StringBuilder sb = new StringBuilder();

            sb = sb.Append("CREATE ");

            if (op.IsUnique)
            {
                sb.Append("UNIQUE ");
            }

            sb.AppendFormat("index  `{0}` on `{1}` (", op.Name, TrimSchemaPrefix(op.Table));
            sb.Append(string.Join(",", op.Columns.Select(c => "`" + c + "`")) + ") ");

            return new MigrationStatement() { Sql = sb.ToString() };
        }
    }

    /// <summary>
    /// Inherit from DbConfiguration to set the new QSL script generator and set the conditions to work with MySQL
    /// </summary>
    public class MyCustomEFConfiguration : DbConfiguration
    {
        public MyCustomEFConfiguration()
        {
            AddDependencyResolver(new MySqlDependencyResolver());
            SetProviderFactory(MySqlProviderInvariantName.ProviderName, new MySqlClientFactory());
            SetProviderServices(MySqlProviderInvariantName.ProviderName, new MySqlProviderServices());
            SetDefaultConnectionFactory(new MySqlConnectionFactory());
            SetMigrationSqlGenerator(MySqlProviderInvariantName.ProviderName, () => new MyCustomMigrationSQLGenerator());
            SetManifestTokenResolver(new MySqlManifestTokenResolver());
            SetHistoryContext(MySqlProviderInvariantName.ProviderName,
                (existingConnection, defaultSchema) => new MyCustomHistoryContext(existingConnection, defaultSchema));
        }
    }

    /// <summary>
    /// Read and write the migration history of the database during the first code migrations. This class must be in the same assembly as the EF configuration
    /// </summary>
    public class MyCustomHistoryContext : HistoryContext
    {
        public MyCustomHistoryContext(DbConnection existingConnection, string defaultSchema) : base(existingConnection, defaultSchema) { }

        protected override void OnModelCreating(System.Data.Entity.DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<HistoryRow>().HasKey(h => new { h.MigrationId });
        }
    }


    [DbConfigurationType(typeof(MyCustomEFConfiguration))]
    public class PetContext : DbContext
    {
        public PetContext() : base("name=petfinder")
        {
            Database.SetInitializer<PetContext>(new CreateDatabaseIfNotExists<PetContext>());
            //Database.SetInitializer<PetContext>(new DropCreateDatabaseAlways<PetContext>());
        }
        public override int SaveChanges()
        {
            AddTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync()
        {
            AddTimestamps();
            return await base.SaveChangesAsync();
        }

        private void AddTimestamps()
        {
            var entities = ChangeTracker.Entries()
                .Where(x => (x.State == EntityState.Added || x.State == EntityState.Modified));

            foreach (var entity in entities)
            {
                var now = DateTime.UtcNow; // current datetime

                if (entity.State == EntityState.Added)
                {
                    if (entity.Entity is Post)
                    {
                        ((Post)entity.Entity).CreatedAt = now;
                    }
                }
            }
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Pet> Pets { get; set; }

        public DbSet<Role> Roles { get; set; }
        public DbSet<Image> Images { get; set; }


        public DbSet<PostCategory> PostCategories { get; set; }

        public DbSet<PetCategory> PetCategories { get; set; }

        public DbSet<Post> Posts { get; set; }
        public DbSet<FcmToken> Tokens { get; set; }


        public DbSet<PetLikes> PetLikes { get; set; }
        public DbSet<PostLikes> PostLikes { get; set; }



    }
}

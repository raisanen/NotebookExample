using Entities;
using Microsoft.EntityFrameworkCore;

namespace Data {
    /// <summary>
    /// Represents the Entity Framework Core database context for the Notebook application.
    /// This context manages entity objects during runtime and coordinates database operations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This context is typically used through the <see cref="UnitOfWork"/> class, which
    /// coordinates multiple repositories sharing the same context instance to ensure
    /// atomic transactions.
    /// </para>
    /// <para>
    /// The context is configured to use SQL Server LocalDB and includes initial seed data
    /// for development and testing purposes.
    /// </para>
    /// </remarks>
    public class NotebookDbContext : DbContext {
        /// <summary>
        /// Gets the <see cref="DbSet{User}"/> for querying and managing <see cref="User"/> entities.
        /// </summary>
        /// <value>
        /// A <see cref="DbSet{User}"/> that can be used to query and save instances of <see cref="User"/>.
        /// </value>
        public DbSet<User> Users => Set<User>();

        /// <summary>
        /// Gets the <see cref="DbSet{Note}"/> for querying and managing <see cref="Note"/> entities.
        /// </summary>
        /// <value>
        /// A <see cref="DbSet{Note}"/> that can be used to query and save instances of <see cref="Note"/>.
        /// </value>
        public DbSet<Note> Notes => Set<Note>();

        /// <summary>
        /// Configures the database connection and provider options for this context.
        /// </summary>
        /// <param name="optionsBuilder">
        /// A builder used to create or modify options for this context.
        /// </param>
        /// <remarks>
        /// This method configures the context to use SQL Server LocalDB with the database name "NotebookDB".
        /// In production scenarios, consider using constructor injection to pass in <see cref="DbContextOptions"/>
        /// for better flexibility and testability.
        /// </remarks>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=NotebookDB");
        }

        /// <summary>
        /// Configures the entity model and relationships using the Fluent API.
        /// </summary>
        /// <param name="modelBuilder">
        /// The builder being used to construct the model for this context.
        /// </param>
        /// <remarks>
        /// <para>
        /// This method configures:
        /// <list type="bullet">
        /// <item><description>Primary keys for User and Note entities</description></item>
        /// <item><description>A one-to-many relationship between User and Notes with cascade delete</description></item>
        /// <item><description>Initial seed data for development and testing</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// The relationship configuration ensures that when a User is deleted, all associated Notes
        /// are automatically deleted (cascade delete behavior).
        /// </para>
        /// </remarks>
        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<User>(u => {
                u.HasKey(x => x.Id);
            });
            modelBuilder.Entity<Note>(n => {
                n.HasKey(x => x.Id);
                n.HasOne(x => x.User)
                    .WithMany(u => u.Notes)
                    .HasForeignKey(x => x.UserId);
            });
            Seed(modelBuilder);
        }

        /// <summary>
        /// Seeds the database with initial data for development and testing purposes.
        /// </summary>
        /// <param name="modelBuilder">
        /// The builder being used to construct the model for this context.
        /// </param>
        /// <remarks>
        /// <para>
        /// This method adds the following seed data:
        /// <list type="bullet">
        /// <item><description>One test user with username "kalle" and password "password"</description></item>
        /// <item><description>One test note titled "test" associated with the test user</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// Warning: This seed data includes a plaintext password and should not be used in production.
        /// Consider using a proper password hashing mechanism for production environments.
        /// </para>
        /// </remarks>
        private static void Seed(ModelBuilder modelBuilder) {
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "kalle", Password = "password" }
            );
            modelBuilder.Entity<Note>().HasData(
                new Note { Id = 1, Title = "test", Text = "This is a test", UserId = 1 }
            );

        }
    }
}

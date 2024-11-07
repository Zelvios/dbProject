using Microsoft.EntityFrameworkCore;
using dbProject.Models;
using Task = dbProject.Models.Task;

namespace dbProject.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Todo> Todos { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<Worker> Workers { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamWorker> TeamWorkers { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Todo>()
                .HasKey(t => t.TodoId);

            modelBuilder.Entity<Task>()
                .HasKey(t => t.TaskId);

            modelBuilder.Entity<Team>()
                .HasKey(t => t.TeamId);

            modelBuilder.Entity<TeamWorker>()
                .Property(tw => tw.TeamWorkerId)
                .ValueGeneratedOnAdd();


            // Configure relationships
            modelBuilder.Entity<Task>()
                .HasMany(t => t.Todos)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
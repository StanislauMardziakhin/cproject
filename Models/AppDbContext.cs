using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace CourseProject.Models
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Template> Templates { get; set; }
        //public DbSet<Question> Questions { get; set; }
        //public DbSet<Form> Forms { get; set; }
        //public DbSet<Response> Responses { get; set; }
        //public DbSet<Comment> Comments { get; set; }
        //public DbSet<Like> Likes { get; set; }


        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Template>()
                .Property(t => t.Tags)
                .HasColumnType("text[]");

            modelBuilder.Entity<Template>()
                .HasGeneratedTsVectorColumn(
                    t => t.SearchVector,
                    "english", 
                    t => new { t.Name, t.Description })
                .HasIndex(t => t.SearchVector)
                .HasMethod("GIN");

            modelBuilder.Entity<Template>()
                .HasIndex(t => t.Tags)
                .HasMethod("gin")
                .HasOperators("gin_trgm_ops");

            //modelBuilder.Entity<Template>()
                //.HasMany(t => t.Questions)
                //.WithOne(q => q.Template)
                //.HasForeignKey(q => q.TemplateId);
        }
    }
}
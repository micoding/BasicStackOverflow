using Microsoft.EntityFrameworkCore;

namespace BasicStackOverflow.Entities;

public class BasicStackOverflowContext : DbContext
{
    public BasicStackOverflowContext(DbContextOptions<BasicStackOverflowContext> options) : base(options)
    {
    }

    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Vote> Votes { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Answer> Answers { get; set; }
    public DbSet<Role> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Comment>(c =>
        {
            c.HasKey(x => x.Id);
            c.Property(x => x.CreationDate).HasPrecision(0);
            c.Property(x => x.CreationDate).HasDefaultValueSql("CURRENT_TIMESTAMP");

            c.Property(x => x.AuthorId).IsRequired();
            c.Property(x => x.PostId).IsRequired();

            c.HasOne(co => co.Post).WithMany(p => p.Comments).HasForeignKey(p => p.PostId);
            c.HasOne(co => co.Author).WithMany(u => u.Comments).HasForeignKey(co => co.AuthorId);
        });

        modelBuilder.Entity<Tag>(t =>
        {
            t.HasKey(x => x.Id);
            t.HasIndex(x => x.Name).IsUnique();
            t.Property(x => x.Name).IsRequired();
            t.HasMany(ta => ta.Questions).WithMany(q => q.Tags);
        });

        modelBuilder.Entity<User>(u =>
        {
            u.HasKey(x => x.Id);
            u.Property(x => x.Username).IsRequired();
            u.Property(x => x.RoleId).IsRequired();
            u.Property(x => x.PasswordHash).IsRequired();

            u.HasMany(us => us.Posts).WithOne(p => p.Author).HasForeignKey(post => post.AuthorId);
        });

        modelBuilder.Entity<Question>(q =>
        {
            q.Property(x => x.Title).IsRequired();
            q.HasMany(qu => qu.Answers).WithOne(a => a.Question).HasForeignKey(a => a.QuestionId);
        });

        modelBuilder.Entity<Vote>(v =>
        {
            v.HasKey(x => new { x.UserId, x.AnswerId });

            v.Property(x => x.AnswerId).IsRequired();

            v.HasOne(vo => vo.UserVoted).WithMany(u => u.Votes).HasForeignKey(x => x.UserId);
            v.HasOne(vo => vo.Answer).WithMany(a => a.Votes).HasForeignKey(x => x.AnswerId);
            v.Property(x => x.Value).IsRequired();
        });

        modelBuilder.Entity<Post>(p =>
        {
            p.UseTphMappingStrategy();
            p.HasKey(x => x.Id);

            p.Property(x => x.CreationDate).HasPrecision(0);
            p.Property(x => x.CreationDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            p.Property(x => x.AuthorId).IsRequired();
            p.Property(x => x.Content).IsRequired();
        });

        modelBuilder.Entity<Answer>(a => { a.Property(x => x.QuestionId).IsRequired(); });

        modelBuilder.Entity<Role>(r =>
        {
            r.HasKey(x => x.Id);
            r.Property(x => x.Name).IsRequired();
        });
    }
}
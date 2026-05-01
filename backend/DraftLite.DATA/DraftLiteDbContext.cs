using Microsoft.EntityFrameworkCore;
using DraftLite.Data.Entities;

namespace DraftLite.Data;

public class DraftLiteDbContext : DbContext
{
    public DraftLiteDbContext(DbContextOptions<DraftLiteDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<ProjectRole> ProjectRoles => Set<ProjectRole>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Page> Pages => Set<Page>();
    public DbSet<ProjectCollaborator> ProjectCollaborators => Set<ProjectCollaborator>();
    public DbSet<ProjectHistory> ProjectHistories => Set<ProjectHistory>();
    public DbSet<AuditLogEntry> AuditLog => Set<AuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Role>(b =>
        {
            b.ToTable("role");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(64).IsRequired();
            b.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<ProjectRole>(b =>
        {
            b.ToTable("projectRole");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(64).IsRequired();
            b.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<User>(b =>
        {
            b.ToTable("users");
            b.HasKey(x => x.Id);
            b.Property(x => x.GoogleId).HasMaxLength(256);
            b.Property(x => x.Email).HasMaxLength(320).IsRequired();
            b.Property(x => x.Pseudo).HasMaxLength(128).IsRequired();
            b.Property(x => x.Theme).HasMaxLength(16).HasDefaultValue("light").IsRequired();
            b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
            b.Property(x => x.IsActive).HasDefaultValue(true);
            b.Property(x => x.BanReason).HasMaxLength(512);

            b.HasIndex(x => x.Email).IsUnique();
            b.HasIndex(x => x.GoogleId).IsUnique();

            b.HasOne(x => x.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Project>(b =>
        {
            b.ToTable("projects");
            b.HasKey(x => x.Id);
            b.Property(x => x.Title).HasMaxLength(256).IsRequired();
            b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
            b.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");

            b.HasOne(x => x.Owner)
                .WithMany(u => u.OwnedProjects)
                .HasForeignKey(x => x.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.OwnerId);
        });

        modelBuilder.Entity<Page>(b =>
        {
            b.ToTable("pages");
            b.HasKey(x => x.Id);
            b.Property(x => x.Title).HasMaxLength(256).IsRequired();
            b.Property(x => x.Blocks).HasColumnType("jsonb").IsRequired();
            b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
            b.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");

            b.HasOne(x => x.Project)
                .WithMany(p => p.Pages)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => new { x.ProjectId, x.OrderIndex });
        });

        modelBuilder.Entity<ProjectCollaborator>(b =>
        {
            b.ToTable("project_collaborators");
            b.HasKey(x => new { x.ProjectId, x.UserId });

            b.HasOne(x => x.Project)
                .WithMany(p => p.Collaborators)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.User)
                .WithMany(u => u.ProjectCollaborations)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Role)
                .WithMany(r => r.ProjectCollaborators)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.InvitedBy)
                .WithMany(u => u.SentProjectInvites)
                .HasForeignKey(x => x.InvitedById)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.RoleId);
            b.HasIndex(x => x.InvitedById);
        });

        modelBuilder.Entity<ProjectHistory>(b =>
        {
            b.ToTable("projectHistory");
            b.HasKey(x => x.Id);
            b.Property(x => x.Action).HasMaxLength(64).IsRequired();
            b.Property(x => x.Patch).HasColumnType("jsonb").IsRequired();
            b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");

            b.HasOne(x => x.Project)
                .WithMany(p => p.HistoryEntries)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Page)
                .WithMany(p => p.HistoryEntries)
                .HasForeignKey(x => x.PageId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.User)
                .WithMany(u => u.ProjectHistoryEntries)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasIndex(x => new { x.PageId, x.Version }).IsUnique();
            b.HasIndex(x => x.ProjectId);
        });

        modelBuilder.Entity<AuditLogEntry>(b =>
        {
            b.ToTable("audit_log");
            b.HasKey(x => x.Id);
            b.Property(x => x.EntityType).HasMaxLength(128).IsRequired();
            b.Property(x => x.Action).HasMaxLength(64).IsRequired();
            b.Property(x => x.ChangedAt).HasDefaultValueSql("now()");

            b.HasOne(x => x.User)
                .WithMany(u => u.AuditLogEntries)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasIndex(x => new { x.EntityType, x.EntityId });
            b.HasIndex(x => x.UserId);
        });
    }
}


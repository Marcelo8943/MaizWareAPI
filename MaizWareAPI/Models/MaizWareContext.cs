using Microsoft.EntityFrameworkCore;

namespace MaizWareAPI.Models;

public partial class MaizWareContext : DbContext
{
    public MaizWareContext(DbContextOptions<MaizWareContext> options) : base(options) { }

    public virtual DbSet<AiConversation> AiConversations => Set<AiConversation>();
    public virtual DbSet<AiMessage> AiMessages => Set<AiMessage>();
    public virtual DbSet<Emotion> Emotions => Set<Emotion>();
    public virtual DbSet<MoodEntry> MoodEntries => Set<MoodEntry>();
    public virtual DbSet<Role> Roles => Set<Role>();
    public virtual DbSet<User> Users => Set<User>();
    public virtual DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public virtual DbSet<UserRole> UserRoles => Set<UserRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AiConversation>(entity =>
        {
            entity.HasKey(e => e.AiConversationId);
            entity.Property(e => e.Title).HasMaxLength(120);
            entity.Property(e => e.StartedAt).HasDefaultValueSql("sysutcdatetime()");
            entity.HasOne(e => e.User).WithMany(e => e.AiConversations).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AiMessage>(entity =>
        {
            entity.HasKey(e => e.AiMessageId);
            entity.Property(e => e.SenderType).HasMaxLength(20);
            entity.Property(e => e.SentAt).HasDefaultValueSql("sysutcdatetime()");
            entity.HasOne(e => e.AiConversation).WithMany(e => e.AiMessages).HasForeignKey(e => e.AiConversationId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Emotion>(entity =>
        {
            entity.HasKey(e => e.EmotionId);
            entity.HasIndex(e => e.EmotionName).IsUnique();
            entity.Property(e => e.EmotionName).HasMaxLength(50);
            entity.Property(e => e.Emoji).HasMaxLength(10);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<MoodEntry>(entity =>
        {
            entity.HasKey(e => e.MoodEntryId);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.RegisteredAt).HasDefaultValueSql("sysutcdatetime()");
            entity.HasOne(e => e.User).WithMany(e => e.MoodEntries).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Emotion).WithMany(e => e.MoodEntries).HasForeignKey(e => e.EmotionId);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId);
            entity.HasIndex(e => e.RoleName).IsUnique();
            entity.Property(e => e.RoleName).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("sysutcdatetime()");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(120);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("sysutcdatetime()");
        });

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.UserProfileId);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.Property(e => e.FirstName).HasMaxLength(80);
            entity.Property(e => e.LastName).HasMaxLength(80);
            entity.Property(e => e.Phone).HasMaxLength(25);
            entity.Property(e => e.Gender).HasMaxLength(30);
            entity.Property(e => e.EmergencyContactName).HasMaxLength(120);
            entity.Property(e => e.EmergencyContactPhone).HasMaxLength(25);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("sysutcdatetime()");
            entity.HasOne(e => e.User).WithOne(e => e.UserProfile).HasForeignKey<UserProfile>(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId });
            entity.Property(e => e.AssignedAt).HasDefaultValueSql("sysutcdatetime()");
            entity.HasOne(e => e.User).WithMany(e => e.UserRoles).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Role).WithMany(e => e.UserRoles).HasForeignKey(e => e.RoleId).OnDelete(DeleteBehavior.Cascade);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

namespace MaizWareAPI.Models;

public partial class User
{
    public int UserId { get; set; }
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public virtual UserProfile? UserProfile { get; set; }
    public virtual ICollection<AiConversation> AiConversations { get; set; } = new List<AiConversation>();
    public virtual ICollection<MoodEntry> MoodEntries { get; set; } = new List<MoodEntry>();
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

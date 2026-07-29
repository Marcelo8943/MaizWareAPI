namespace MaizWareAPI.Models;

public partial class AiConversation
{
    public int AiConversationId { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = null!;
    public DateTime StartedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public virtual ICollection<AiMessage> AiMessages { get; set; } = new List<AiMessage>();
    public virtual User User { get; set; } = null!;
}

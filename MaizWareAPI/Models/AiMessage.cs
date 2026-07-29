namespace MaizWareAPI.Models;

public partial class AiMessage
{
    public int AiMessageId { get; set; }
    public int AiConversationId { get; set; }
    public string SenderType { get; set; } = null!;
    public string MessageText { get; set; } = null!;
    public DateTime SentAt { get; set; }
    public virtual AiConversation AiConversation { get; set; } = null!;
}

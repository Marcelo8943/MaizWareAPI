namespace MaizWareAPI.Models;

public partial class MoodEntry
{
    public int MoodEntryId { get; set; }
    public int UserId { get; set; }
    public int EmotionId { get; set; }
    public byte Intensity { get; set; }
    public string? Notes { get; set; }
    public DateTime RegisteredAt { get; set; }
    public virtual Emotion Emotion { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}

namespace MaizWareAPI.Models;

public partial class Emotion
{
    public int EmotionId { get; set; }
    public string EmotionName { get; set; } = null!;
    public string? Emoji { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public virtual ICollection<MoodEntry> MoodEntries { get; set; } = new List<MoodEntry>();
}

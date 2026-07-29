using MaizWareAPI.DTOs;
using MaizWareAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MaizWareAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoodEntriesController : ControllerBase
{
    private readonly MaizWareContext _context;

    public MoodEntriesController(MaizWareContext context)
    {
        _context = context;
    }

    [HttpGet("emotions")]
    public async Task<ActionResult<IEnumerable<EmotionDto>>> GetEmotions()
    {
        var emotions = await _context.Emotions
            .AsNoTracking()
            .OrderBy(emotion => emotion.SortOrder)
            .ThenBy(emotion => emotion.EmotionName)
            .Select(emotion => new EmotionDto(
                emotion.EmotionId,
                emotion.EmotionName,
                emotion.Emoji,
                emotion.SortOrder,
                emotion.IsActive))
            .ToListAsync();

        return Ok(emotions);
    }

    [HttpGet("user/{userId:int}")]
    public async Task<ActionResult<IEnumerable<MoodEntryDto>>> GetUserMoodEntries(int userId)
    {
        var userExists = await _context.Users.AnyAsync(user => user.UserId == userId);

        if (!userExists)
        {
            return NotFound(new { message = "Usuario no encontrado." });
        }

        var entries = await MoodEntryQuery()
            .Where(entry => entry.UserId == userId)
            .OrderByDescending(entry => entry.RegisteredAt)
            .ToListAsync();

        return Ok(entries.Select(MapMoodEntry));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MoodEntryDto>> GetMoodEntry(int id)
    {
        var entry = await MoodEntryQuery().FirstOrDefaultAsync(item => item.MoodEntryId == id);

        if (entry is null)
        {
            return NotFound(new { message = "Registro emocional no encontrado." });
        }

        return Ok(MapMoodEntry(entry));
    }

    [HttpPost]
    public async Task<ActionResult<MoodEntryDto>> CreateMoodEntry(CreateMoodEntryRequest request)
    {
        if (request.Intensity is < 1 or > 10)
        {
            return BadRequest(new { message = "La intensidad debe estar entre 1 y 10." });
        }

        var userExists = await _context.Users.AnyAsync(user => user.UserId == request.UserId);
        var emotionExists = await _context.Emotions.AnyAsync(emotion => emotion.EmotionId == request.EmotionId && emotion.IsActive);

        if (!userExists)
        {
            return BadRequest(new { message = "El usuario indicado no existe." });
        }

        if (!emotionExists)
        {
            return BadRequest(new { message = "La emocion indicada no existe o no esta activa." });
        }

        var entry = new MoodEntry
        {
            UserId = request.UserId,
            EmotionId = request.EmotionId,
            Intensity = request.Intensity,
            Notes = request.Notes
        };

        _context.MoodEntries.Add(entry);
        await _context.SaveChangesAsync();

        var createdEntry = await MoodEntryQuery().FirstAsync(item => item.MoodEntryId == entry.MoodEntryId);
        return CreatedAtAction(nameof(GetMoodEntry), new { id = entry.MoodEntryId }, MapMoodEntry(createdEntry));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<MoodEntryDto>> UpdateMoodEntry(int id, UpdateMoodEntryRequest request)
    {
        if (request.Intensity is < 1 or > 10)
        {
            return BadRequest(new { message = "La intensidad debe estar entre 1 y 10." });
        }

        var entry = await _context.MoodEntries.FindAsync(id);

        if (entry is null)
        {
            return NotFound(new { message = "Registro emocional no encontrado." });
        }

        var emotionExists = await _context.Emotions.AnyAsync(emotion => emotion.EmotionId == request.EmotionId && emotion.IsActive);

        if (!emotionExists)
        {
            return BadRequest(new { message = "La emocion indicada no existe o no esta activa." });
        }

        entry.EmotionId = request.EmotionId;
        entry.Intensity = request.Intensity;
        entry.Notes = request.Notes;

        await _context.SaveChangesAsync();

        var updatedEntry = await MoodEntryQuery().FirstAsync(item => item.MoodEntryId == id);
        return Ok(MapMoodEntry(updatedEntry));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteMoodEntry(int id)
    {
        var entry = await _context.MoodEntries.FindAsync(id);

        if (entry is null)
        {
            return NotFound(new { message = "Registro emocional no encontrado." });
        }

        _context.MoodEntries.Remove(entry);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private IQueryable<MoodEntry> MoodEntryQuery() =>
        _context.MoodEntries
            .AsNoTracking()
            .Include(entry => entry.Emotion);

    private static MoodEntryDto MapMoodEntry(MoodEntry entry) =>
        new(
            entry.MoodEntryId,
            entry.UserId,
            entry.EmotionId,
            entry.Emotion.EmotionName,
            entry.Emotion.Emoji,
            entry.Intensity,
            entry.Notes,
            entry.RegisteredAt);
}

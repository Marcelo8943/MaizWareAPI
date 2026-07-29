using MaizWareAPI.DTOs;
using MaizWareAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MaizWareAPI.Controllers;

[ApiController]
[Route("api/ai-chat")]
public class AiChatController : ControllerBase
{
    private readonly MaizWareContext _context;

    public AiChatController(MaizWareContext context)
    {
        _context = context;
    }

    [HttpGet("user/{userId:int}/conversations")]
    public async Task<ActionResult<IEnumerable<AiConversationDto>>> GetUserConversations(int userId)
    {
        var userExists = await _context.Users.AnyAsync(user => user.UserId == userId);

        if (!userExists)
        {
            return NotFound(new { message = "Usuario no encontrado." });
        }

        var conversations = await ConversationQuery()
            .Where(conversation => conversation.UserId == userId)
            .OrderByDescending(conversation => conversation.StartedAt)
            .ToListAsync();

        return Ok(conversations.Select(MapConversation));
    }

    [HttpGet("conversations/{conversationId:int}")]
    public async Task<ActionResult<AiConversationDto>> GetConversation(int conversationId)
    {
        var conversation = await ConversationQuery()
            .FirstOrDefaultAsync(item => item.AiConversationId == conversationId);

        if (conversation is null)
        {
            return NotFound(new { message = "Conversacion no encontrada." });
        }

        return Ok(MapConversation(conversation));
    }

    [HttpPost("conversations")]
    public async Task<ActionResult<AiConversationDto>> CreateConversation(CreateAiConversationRequest request)
    {
        var userExists = await _context.Users.AnyAsync(user => user.UserId == request.UserId);

        if (!userExists)
        {
            return BadRequest(new { message = "El usuario indicado no existe." });
        }

        var conversation = new AiConversation
        {
            UserId = request.UserId,
            Title = string.IsNullOrWhiteSpace(request.Title) ? "Nueva conversacion" : request.Title.Trim()
        };

        if (!string.IsNullOrWhiteSpace(request.InitialMessage))
        {
            conversation.AiMessages.Add(new AiMessage
            {
                SenderType = "Usuario",
                MessageText = request.InitialMessage.Trim()
            });

            conversation.AiMessages.Add(new AiMessage
            {
                SenderType = "Asistente",
                MessageText = BuildAssistantReply(request.InitialMessage)
            });
        }

        _context.AiConversations.Add(conversation);
        await _context.SaveChangesAsync();

        var createdConversation = await ConversationQuery()
            .FirstAsync(item => item.AiConversationId == conversation.AiConversationId);

        return CreatedAtAction(
            nameof(GetConversation),
            new { conversationId = conversation.AiConversationId },
            MapConversation(createdConversation));
    }

    [HttpPost("conversations/{conversationId:int}/messages")]
    public async Task<ActionResult<AiConversationDto>> AddMessage(int conversationId, CreateAiMessageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.MessageText))
        {
            return BadRequest(new { message = "El mensaje no puede estar vacio." });
        }

        var conversation = await _context.AiConversations
            .Include(item => item.AiMessages)
            .FirstOrDefaultAsync(item => item.AiConversationId == conversationId);

        if (conversation is null)
        {
            return NotFound(new { message = "Conversacion no encontrada." });
        }

        if (conversation.ClosedAt is not null)
        {
            return BadRequest(new { message = "La conversacion ya esta cerrada." });
        }

        conversation.AiMessages.Add(new AiMessage
        {
            SenderType = "Usuario",
            MessageText = request.MessageText.Trim()
        });

        if (request.AutoReply)
        {
            conversation.AiMessages.Add(new AiMessage
            {
                SenderType = "Asistente",
                MessageText = BuildAssistantReply(request.MessageText)
            });
        }

        await _context.SaveChangesAsync();

        var updatedConversation = await ConversationQuery()
            .FirstAsync(item => item.AiConversationId == conversationId);

        return CreatedAtAction(
            nameof(GetConversation),
            new { conversationId },
            MapConversation(updatedConversation));
    }

    [HttpPut("conversations/{conversationId:int}/close")]
    public async Task<ActionResult<AiConversationDto>> CloseConversation(int conversationId)
    {
        var conversation = await _context.AiConversations.FindAsync(conversationId);

        if (conversation is null)
        {
            return NotFound(new { message = "Conversacion no encontrada." });
        }

        conversation.ClosedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var closedConversation = await ConversationQuery()
            .FirstAsync(item => item.AiConversationId == conversationId);

        return Ok(MapConversation(closedConversation));
    }

    [HttpDelete("conversations/{conversationId:int}")]
    public async Task<IActionResult> DeleteConversation(int conversationId)
    {
        var conversation = await _context.AiConversations.FindAsync(conversationId);

        if (conversation is null)
        {
            return NotFound(new { message = "Conversacion no encontrada." });
        }

        _context.AiConversations.Remove(conversation);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private IQueryable<AiConversation> ConversationQuery() =>
        _context.AiConversations
            .AsNoTracking()
            .Include(conversation => conversation.AiMessages);

    private static AiConversationDto MapConversation(AiConversation conversation) =>
        new(
            conversation.AiConversationId,
            conversation.UserId,
            conversation.Title,
            conversation.StartedAt,
            conversation.ClosedAt,
            conversation.AiMessages
                .OrderBy(message => message.SentAt)
                .ThenBy(message => message.AiMessageId)
                .Select(MapMessage)
                .ToList());

    private static AiMessageDto MapMessage(AiMessage message) =>
        new(
            message.AiMessageId,
            message.AiConversationId,
            message.SenderType,
            message.MessageText,
            message.SentAt);

    private static string BuildAssistantReply(string userMessage)
    {
        var text = userMessage.ToLowerInvariant();

        if (text.Contains("ansiedad") || text.Contains("abrum") || text.Contains("estres"))
        {
            return "Te entiendo, respira hondo. Estoy aqui para apoyarte. Podemos intentar una respiracion guiada de un minuto.";
        }

        if (text.Contains("triste") || text.Contains("solo") || text.Contains("mal"))
        {
            return "Siento que estes pasando por eso. No tienes que cargarlo todo a solas; cuentame un poco mas y vamos paso a paso.";
        }

        return "Gracias por contarmelo. Estoy aqui para escucharte y acompanarte. Que sientes con mas fuerza en este momento?";
    }
}

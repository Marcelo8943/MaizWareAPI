namespace MaizWareAPI.DTOs;

public sealed record RoleDto(int RoleId, string RoleName, string? Description);

public sealed record UserProfileDto(
    int UserProfileId,
    string FirstName,
    string LastName,
    string? Phone,
    DateOnly? BirthDate,
    string? Gender,
    string? EmergencyContactName,
    string? EmergencyContactPhone);

public sealed record UserDto(
    int UserId,
    string Email,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    UserProfileDto? Profile,
    IReadOnlyCollection<RoleDto> Roles);

public sealed record UpsertUserProfileRequest(
    string FirstName,
    string LastName,
    string? Phone,
    DateOnly? BirthDate,
    string? Gender,
    string? EmergencyContactName,
    string? EmergencyContactPhone);

public sealed record RegisterUserRequest(
    string Email,
    string? Password,
    UpsertUserProfileRequest Profile,
    IReadOnlyCollection<int>? RoleIds);

public sealed record UpdateUserRequest(
    string Email,
    bool IsActive,
    UpsertUserProfileRequest Profile,
    IReadOnlyCollection<int>? RoleIds);

public sealed record AssignRolesRequest(IReadOnlyCollection<int> RoleIds);

public sealed record EmotionDto(int EmotionId, string EmotionName, string? Emoji, int SortOrder, bool IsActive);

public sealed record MoodEntryDto(
    int MoodEntryId,
    int UserId,
    int EmotionId,
    string EmotionName,
    string? Emoji,
    byte Intensity,
    string? Notes,
    DateTime RegisteredAt);

public sealed record CreateMoodEntryRequest(int UserId, int EmotionId, byte Intensity, string? Notes);
public sealed record UpdateMoodEntryRequest(int EmotionId, byte Intensity, string? Notes);

public sealed record AiMessageDto(
    int AiMessageId,
    int AiConversationId,
    string SenderType,
    string MessageText,
    DateTime SentAt);

public sealed record AiConversationDto(
    int AiConversationId,
    int UserId,
    string Title,
    DateTime StartedAt,
    DateTime? ClosedAt,
    IReadOnlyCollection<AiMessageDto> Messages);

public sealed record CreateAiConversationRequest(int UserId, string? Title, string? InitialMessage);
public sealed record CreateAiMessageRequest(string MessageText, bool AutoReply = true);

public sealed record LoginRequest(string Email, string Password);

public sealed record RegisterRequest(string Email, string Password, string FirstName, string LastName, string? Phone);

public sealed record AuthResponse(UserDto User);



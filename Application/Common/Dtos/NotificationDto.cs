namespace Application.Common.Dtos
{
    public record NotificationDto(Guid Id, string Type, bool IsRead, string Title, string Message, object Data, DateTime CreatedAt);
}

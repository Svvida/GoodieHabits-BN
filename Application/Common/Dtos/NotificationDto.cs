namespace Application.Common.Dtos
{
    public record NotificationDto(Guid Id, string Type, string Title, string Message, object Data);
}

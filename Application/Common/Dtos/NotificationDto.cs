namespace Application.Common.Dtos
{
    public record NotificationDto(int Id, string Type, string Title, string Message, object Data);
}

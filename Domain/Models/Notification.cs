using Domain.Enums;

namespace Domain.Models
{
    public class Notification
    {
        public Guid Id { get; private set; }
        public int UserProfileId { get; private set; }
        public NotificationTypeEnum Type { get; private set; }
        public string Title { get; private set; } = null!;
        public string Message { get; private set; } = null!;
        public string PayloadJson { get; private set; } = null!;
        public bool IsRead { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public UserProfile UserProfile { get; private set; } = null!;

        protected Notification() { } // For EF Core

        private Notification(Guid id, int userProfileId, NotificationTypeEnum type, string title, string message, string payloadJson, DateTime utcNow)
        {
            Id = id;
            UserProfileId = userProfileId;
            Type = type;
            Title = title;
            Message = message;
            PayloadJson = payloadJson;
            IsRead = false;
            CreatedAt = utcNow;
        }

        public static Notification Create(Guid id, int userProfileId, NotificationTypeEnum type, string title, string message, string payloadJson, DateTime utcNow)
        {
            return new Notification(id, userProfileId, type, title, message, payloadJson, utcNow);
        }

        public void MarkAsRead()
        {
            IsRead = true;
        }
    }
}

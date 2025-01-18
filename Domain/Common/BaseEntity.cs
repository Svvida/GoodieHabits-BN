namespace Domain.Common
{
    public abstract class BaseEntity
    {
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        public void SetCreatedAt(DateTime createdAt)
        {
            if (createdAt > DateTime.UtcNow)
            {
                throw new ArgumentException("CreatedAt cannot be in the future.");
            }

            CreatedAt = createdAt;
        }

        public void SetUpdatedAt(DateTime? updatedAt)
        {
            if (updatedAt.HasValue && updatedAt > DateTime.UtcNow)
            {
                throw new ArgumentException("UpdatedAt cannot be in the future.");
            }

            UpdatedAt = updatedAt;
        }
    }
}

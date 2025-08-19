using Domain.Exceptions;

namespace Domain.Common
{
    public abstract class EntityBase : AggregateRoot
    {
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public void SetCreatedAt(DateTime createdAt)
        {
            if (createdAt > DateTime.UtcNow)
            {
                throw new InvalidArgumentException("CreatedAt cannot be in the future.");
            }

            CreatedAt = createdAt;
        }

        public void SetUpdatedAt(DateTime? updatedAt)
        {
            if (updatedAt.HasValue && updatedAt > DateTime.UtcNow)
            {
                throw new InvalidArgumentException("UpdatedAt cannot be in the future.");
            }

            UpdatedAt = updatedAt;
        }
    }
}

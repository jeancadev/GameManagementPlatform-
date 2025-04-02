
using System;

namespace GameManagement.Domain.Entities
{
    public class ModeratorLogEntry
    {
        public Guid Id { get; private set; }
        public Guid RoomId { get; private set; }
        public Guid ModeratorId { get; private set; }
        public Guid? TargetUserId { get; private set; }
        public string Action { get; private set; }
        public string Details { get; private set; }
        public DateTime Timestamp { get; private set; }

        // Propiedades de navegación
        public virtual GameRoom Room { get; private set; }
        public virtual User Moderator { get; private set; }
        public virtual User TargetUser { get; private set; }

        private ModeratorLogEntry() { }

        public static ModeratorLogEntry Create(
            Guid roomId,
            Guid moderatorId,
            string action,
            string details,
            Guid? targetUserId = null)
        {
            if (string.IsNullOrWhiteSpace(action))
                throw new ArgumentException("Action is required", nameof(action));

            if (string.IsNullOrWhiteSpace(details))
                throw new ArgumentException("Details are required", nameof(details));

            return new ModeratorLogEntry
            {
                Id = Guid.NewGuid(),
                RoomId = roomId,
                ModeratorId = moderatorId,
                TargetUserId = targetUserId,
                Action = action,
                Details = details,
                Timestamp = DateTime.UtcNow
            };
        }
    }
}
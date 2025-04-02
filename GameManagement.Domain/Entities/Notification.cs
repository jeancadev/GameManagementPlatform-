using GameManagement.Domain.Notifications;

namespace GameManagement.Domain.Entities
{
    public class Notification
    {
        public Guid Id { get; private set; }
        public Guid RoomId { get; private set; }
        public Guid? SenderId { get; private set; }
        public Guid? ReceiverId { get; private set; }
        public NotificationType Type { get; private set; }
        public string Message { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public bool IsRead { get; private set; }
        public virtual GameRoom Room { get; private set; }
        public virtual User Sender { get; private set; }
        public virtual User Receiver { get; private set; }

        private Notification() { }

        public static Notification Create(
            Guid roomId,
            NotificationType type,
            string message,
            Guid? senderId = null,
            Guid? receiverId = null)
        {
            return new Notification
            {
                Id = Guid.NewGuid(),
                RoomId = roomId,
                SenderId = senderId,
                ReceiverId = receiverId,
                Type = type,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };
        }

        public void MarkAsRead()
        {
            IsRead = true;
        }
    }
}
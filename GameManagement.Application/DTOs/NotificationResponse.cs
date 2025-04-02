namespace GameManagement.Application.DTOs
{
    public class NotificationResponse
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public Guid RoomId { get; set; }
        public string RoomName { get; set; }
        public string SenderUsername { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}
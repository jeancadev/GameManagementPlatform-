
namespace GameManagement.Application.DTOs.Moderation
{
    public class KickPlayerRequest
    {
        public Guid PlayerId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}

using System;

namespace GameManagement.Application.DTOs.Moderation
{
    public class MutePlayerRequest
    {
        public Guid PlayerId { get; set; }
        public int DurationMinutes { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
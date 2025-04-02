
using System;

namespace GameManagement.Application.DTOs.Moderation
{
    public class WarnPlayerRequest
    {
        public Guid PlayerId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
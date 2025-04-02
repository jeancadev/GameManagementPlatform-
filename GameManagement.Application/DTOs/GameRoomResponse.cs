using GameManagement.Application.Services;
using System;
using System.Collections.Generic;

namespace GameManagement.Application.DTOs
{
    public class GameRoomResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int MaxPlayers { get; set; }
        public int CurrentPlayers { get; set; }
        public string Status { get; set; }
        public string OwnerUsername { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public List<PlayerInfo> Players { get; set; }
    }
}
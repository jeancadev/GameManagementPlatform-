namespace GameManagement.Application.DTOs
{
    public class CreateGameRoomRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int MaxPlayers { get; set; }
    }
}
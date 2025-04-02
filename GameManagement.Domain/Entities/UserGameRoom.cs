using GameManagement.Domain.Enums;

namespace GameManagement.Domain.Entities
{
    public class UserGameRoom
    {
        public Guid UserId { get; private set; }
        public Guid GameRoomId { get; private set; }
        public PlayerRole Role { get; private set; }
        public DateTime JoinedAt { get; private set; }
        public virtual User User { get; private set; }
        public virtual GameRoom GameRoom { get; private set; }

        private UserGameRoom() { }

        public static UserGameRoom Create(User user, GameRoom gameRoom, PlayerRole role)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (gameRoom == null)
                throw new ArgumentNullException(nameof(gameRoom));

            return new UserGameRoom
            {
                UserId = user.Id,
                GameRoomId = gameRoom.Id,
                Role = role,
                JoinedAt = DateTime.UtcNow,
                User = user,
                GameRoom = gameRoom
            };
        }

        public void UpdateRole(PlayerRole newRole)
        {
            // Elimine la validación que impide degradar al propietario
            Role = newRole;
        }
    }
}
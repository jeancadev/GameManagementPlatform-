using GameManagement.Domain.Enums;
using System;
using System.Collections.Generic;

namespace GameManagement.Domain.Entities
{
    public class GameRoom
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public int MaxPlayers { get; private set; }
        public int MinPlayersToStart { get; private set; }
        public Guid OwnerId { get; private set; }
        public GameRoomStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? StartedAt { get; private set; }
        public DateTime? EndedAt { get; private set; }
        public TimeSpan MaxWaitTimeToStart { get; private set; }
        public virtual User Owner { get; private set; }
        public virtual ICollection<UserGameRoom> UserRooms { get; private set; }

        private GameRoom()
        {
            UserRooms = new HashSet<UserGameRoom>();
            MaxWaitTimeToStart = TimeSpan.FromMinutes(15);
        }

        public static GameRoom Create(string name, string description, int maxPlayers, Guid ownerId)
        {
            ValidateRoomCreation(name, maxPlayers);

            var room = new GameRoom
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                MaxPlayers = maxPlayers,
                MinPlayersToStart = CalculateMinPlayers(maxPlayers),
                OwnerId = ownerId,
                Status = GameRoomStatus.Created,
                CreatedAt = DateTime.UtcNow
            };

            return room;
        }

        private static void ValidateRoomCreation(string name, int maxPlayers)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("El nombre de la sala no puede estar vacío", nameof(name));

            if (name.Length < 3 || name.Length > 50)
                throw new ArgumentException("El nombre de la sala debe tener entre 3 y 50 caracteres", nameof(name));

            if (maxPlayers < 2)
                throw new ArgumentException("La sala debe permitir al menos 2 jugadores", nameof(maxPlayers));

            if (maxPlayers > 10)
                throw new ArgumentException("La sala no puede tener más de 10 jugadores", nameof(maxPlayers));
        }

        private static int CalculateMinPlayers(int maxPlayers)
        {
            return Math.Max(2, maxPlayers / 2);
        }

        public void Start()
        {
            if (Status != GameRoomStatus.Created)
                throw new InvalidOperationException("La sala solo puede iniciarse desde el estado Created");

            if (GetPlayerCount() < MinPlayersToStart)
                throw new InvalidOperationException($"Se requieren al menos {MinPlayersToStart} jugadores para iniciar");

            if (DateTime.UtcNow - CreatedAt > MaxWaitTimeToStart)
                throw new InvalidOperationException($"Se ha excedido el tiempo máximo de espera ({MaxWaitTimeToStart.TotalMinutes} minutos) para iniciar la sala");

            Status = GameRoomStatus.InProgress;
            StartedAt = DateTime.UtcNow;
        }

        public void End()
        {
            if (Status != GameRoomStatus.InProgress)
                throw new InvalidOperationException("Solo se pueden finalizar salas que estén en progreso");

            Status = GameRoomStatus.Ended;
            EndedAt = DateTime.UtcNow;
        }

        public void AddPlayer(User user, PlayerRole role = PlayerRole.Player)
        {
            ValidatePlayerJoin(user);

            var userRoom = UserGameRoom.Create(user, this, role);
            UserRooms.Add(userRoom);
        }

        private void ValidatePlayerJoin(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user), "El jugador no puede ser nulo");

            if (Status != GameRoomStatus.Created)
                throw new InvalidOperationException("Solo se pueden unir jugadores a salas en estado Created");

            if (GetPlayerCount() >= MaxPlayers)
                throw new InvalidOperationException("La sala está llena");

            if (UserRooms.Any(ur => ur.UserId == user.Id))
                throw new InvalidOperationException("El jugador ya está en la sala");
        }

        public void RemovePlayer(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user), "El jugador no puede ser nulo");

            var userRoom = UserRooms.FirstOrDefault(ur => ur.UserId == user.Id);

            if (userRoom == null)
                throw new InvalidOperationException("El jugador no se encuentra en esta sala");

            if (userRoom.Role == PlayerRole.Owner)
                throw new InvalidOperationException("El propietario no puede abandonar la sala");

            if (Status != GameRoomStatus.Created)
                throw new InvalidOperationException("Solo se pueden remover jugadores de salas en estado Created");

            UserRooms.Remove(userRoom);
        }

        public void UpdatePlayerRole(Guid userId, PlayerRole newRole, Guid requestingUserId)
        {
            var userRoom = UserRooms.FirstOrDefault(ur => ur.UserId == userId);
            if (userRoom == null)
                throw new InvalidOperationException("El jugador no se encuentra en esta sala");

            var requestingUserRoom = UserRooms.FirstOrDefault(ur => ur.UserId == requestingUserId);
            if (requestingUserRoom?.Role != PlayerRole.Owner)
                throw new InvalidOperationException("Solo el propietario puede cambiar roles");

            if (userRoom.Role == PlayerRole.Owner && newRole != PlayerRole.Owner)
                throw new InvalidOperationException("No se puede degradar al propietario de la sala");

            userRoom.UpdateRole(newRole);
        }

        public void KickPlayer(Guid targetUserId, Guid requestingUserId)
        {
            var requestingUserRoom = UserRooms.FirstOrDefault(ur => ur.UserId == requestingUserId);
            if (requestingUserRoom?.Role != PlayerRole.Owner && requestingUserRoom?.Role != PlayerRole.Moderator)
                throw new InvalidOperationException("Solo el propietario y moderadores pueden expulsar jugadores");

            var targetUserRoom = UserRooms.FirstOrDefault(ur => ur.UserId == targetUserId);
            if (targetUserRoom == null)
                throw new InvalidOperationException("El jugador no se encuentra en la sala");

            if (targetUserRoom.Role == PlayerRole.Owner)
                throw new InvalidOperationException("No se puede expulsar al propietario de la sala");

            if (targetUserRoom.Role == PlayerRole.Moderator && requestingUserRoom.Role != PlayerRole.Owner)
                throw new InvalidOperationException("Solo el propietario puede expulsar moderadores");

            UserRooms.Remove(targetUserRoom);
        }

        public void TransferOwnership(Guid newOwnerId, Guid currentOwnerId)
        {
            if (Status != GameRoomStatus.Created)
                throw new InvalidOperationException("Solo se puede transferir la propiedad en salas que no han iniciado");

            var currentOwnerRoom = UserRooms.FirstOrDefault(ur => ur.UserId == currentOwnerId);
            if (currentOwnerRoom?.Role != PlayerRole.Owner)
                throw new InvalidOperationException("Solo el propietario puede transferir la propiedad");

            var newOwnerRoom = UserRooms.FirstOrDefault(ur => ur.UserId == newOwnerId);
            if (newOwnerRoom == null)
                throw new InvalidOperationException("El nuevo propietario debe estar en la sala");

            // Primero asingo al nuevo propietario
            newOwnerRoom.UpdateRole(PlayerRole.Owner);
            // Luego se degrada al propietario actual
            currentOwnerRoom.UpdateRole(PlayerRole.Player);
            OwnerId = newOwnerId;
        }

        private int GetPlayerCount()
        {
            return UserRooms.Count;
        }
    }
}
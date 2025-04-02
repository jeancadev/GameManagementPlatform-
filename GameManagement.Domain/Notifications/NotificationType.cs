namespace GameManagement.Domain.Notifications
{
    public enum NotificationType
    {
        PlayerJoined,
        PlayerLeft,
        GameStarted,
        GameEnded,
        PlayerKicked,
        RoomCreated,
        PlayerWarned,   // Nuevo tipo para advertencias
        PlayerMuted,    // Nuevo tipo para silenciamientos
        PlayerUnmuted,  // Opcional: para cuando se levanta el silencio
        ModeratorAssigned, // Opcional: para cuando se asigna un nuevo moderador
        RoleChanged      // Opcional: para cambios de rol en general
    }
}
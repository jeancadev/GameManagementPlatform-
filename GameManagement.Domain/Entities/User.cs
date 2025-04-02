using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameManagement.Domain.Entities
{
    public class User
    {
        public Guid Id { get; private set; }
        public string Username { get; private set; }
        public string Email { get; private set; }
        public byte[] PasswordHash { get; private set; }
        public byte[] PasswordSalt { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? LastLogin { get; private set; }
        public bool IsActive { get; private set; }

        private User() { } // Para Entity Framework

        public static User Create(string username, string email)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be empty", nameof(username));

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));

            return new User
            {
                Id = Guid.NewGuid(),
                Username = username,
                Email = email.ToLower(),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
        }

        public void SetPassword(byte[] passwordHash, byte[] passwordSalt)
        {
            PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
            PasswordSalt = passwordSalt ?? throw new ArgumentNullException(nameof(passwordSalt));
        }

        public void UpdateLastLogin()
        {
            LastLogin = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void Activate()
        {
            IsActive = true;
        }
    }
}

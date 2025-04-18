﻿namespace FinanceSystem.Domain.Entities
{
    public class User
    {
        public Guid Id { get; protected set; }
        public string Username { get; protected set; }
        public string Email { get; protected set; }
        public string PasswordHash { get; protected set; }
        public bool IsActive { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public DateTime? LastLogin { get; protected set; }
        public ICollection<UserRole> UserRoles { get; protected set; }

        protected User()
        {

            UserRoles = [];
        }

        public User(string username, string email, string passwordHash)
        {
            Id = Guid.NewGuid();
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UserRoles = new List<UserRole>();
        }

        public void UpdateUsername(string username)
        {
            Username = username;
        }

        public void UpdateEmail(string email)
        {
            Email = email;
        }

        public void UpdatePassword(string passwordHash)
        {
            PasswordHash = passwordHash;
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void SetLastLogin()
        {
            LastLogin = DateTime.UtcNow;
        }

        public void AddRole(Role role)
        {
            UserRoles.Add(new UserRole(this, role));
        }

        public void RemoveRole(Role role)
        {
            var userRole = UserRoles.FirstOrDefault(ur => ur.RoleId == role.Id);
            if (userRole != null)
                UserRoles.Remove(userRole);
        }
    }
}

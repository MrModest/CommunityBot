using System;
using Dapper.Contrib.Extensions;

namespace CommunityBot.Contracts
{
    [Table("Users")]
    public class AppUser : IEntity
    {
        [ExplicitKey]
        public long Id { get; set; }
        
        public string? Username { get; set; }

        public string? FirstName { get; set; }
        
        public string? LastName { get; set; }
        
        public DateTime Joined { get; set; } 

        public long? InvitedBy { get; set; }
        
        public string? InviteComment { get; set; }
        
        public UserAccessType AccessType { get; set; }
        
        public string? PasswordHash { get; set; }

        public override string ToString()
        {
            return $"[Id: '{Id}' | UserName: '{Username}' | FirstName: '{FirstName}' | LastName: '{LastName}']";
        }
    }

    public enum UserAccessType
    {
        Unknown = 0,
        Allow,
        Block
    }
}

namespace CommunityBot.Contracts
{
    [Dapper.Contrib.Extensions.Table("Users")]
    public class AppUser : EntityBase
    {
        public string? Username { get; set; }

        public string? FirstName { get; set; }
        
        public string? LastName { get; set; }

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
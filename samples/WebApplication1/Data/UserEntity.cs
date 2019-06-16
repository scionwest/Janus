using System;

namespace WebApplication1.Data
{
    public class UserEntity
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public AccountEntity Account { get; set; }
    }
}

using System;

namespace WebApplication1.Data
{
    public class AccountEntity
    {
        public Guid Id { get; set; }
        public string Number { get; set; }
        public UserEntity User { get; set; }
    }
}

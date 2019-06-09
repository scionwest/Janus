using System;
using System.Collections.Generic;

namespace WebApplication1
{
    public class UserEntity
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }

        public List<TaskEntity> Tasks { get; set; } = new List<TaskEntity>();
    }
}

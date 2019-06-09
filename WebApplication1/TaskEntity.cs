using System;

namespace WebApplication1
{
    public class TaskEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public DateTime DueDate { get; set; }
    }
}

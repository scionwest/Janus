using System;
using System.Collections.Generic;

namespace WebApplication1.Data
{
    public class ProjectEntity
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public List<TaskEntity> Tasks { get; set; }
    }

}

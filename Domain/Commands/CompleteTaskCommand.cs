using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Commands
{
    public class CompleteTaskCommand
    {
        public Guid TaskId { get; set; }
    }
}

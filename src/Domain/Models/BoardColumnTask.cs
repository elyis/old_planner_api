using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace old_planner_api.src.Domain.Models
{
    public class BoardColumnTask
    {
        public BoardColumn Column { get; set; }
        public Guid ColumnId { get; set; }

        public TaskModel Task { get; set; }
        public Guid TaskId { get; set; }
    }
}
using System.Collections.Generic;

namespace dbProject.Models
{
    public class Worker
    {
        public int WorkerId { get; set; }
        public string Name { get; set; }
        public List<TeamWorker>? Teams { get; set; }
        public Todo? CurrentTodo { get; set; }
        public List<Todo>? Todos { get; set; }
    }
}
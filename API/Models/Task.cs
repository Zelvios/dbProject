namespace dbProject.Models
{
    public class Task
    {
        public int TaskId { get; set; }
        public string Name { get; set; }
        public List<Todo> Todos { get; set; } = new List<Todo>();
    }
}
namespace dbProject.Models
{
    public class Team
    {
        public int TeamId { get; set; }
        public string Name { get; set; }
        public List<TeamWorker>? Workers { get; set; }
        public Task? CurrentTask { get; set; }
        public List<Task>? Tasks { get; set; } = new List<Task>();
    }
}
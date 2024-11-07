namespace dbProject.DTOs
{
    public class CreateTaskDto
    {
        public string Name { get; set; }
        public List<int> TodoIds { get; set; } = new List<int>();
    }
}
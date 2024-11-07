namespace dbProject.DTOs
{
    public class CreateTeamDto
    {
        public string Name { get; set; }
        public List<int> WorkerIds { get; set; }
    }
}
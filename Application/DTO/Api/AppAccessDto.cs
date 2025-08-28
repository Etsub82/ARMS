namespace Application.DTO.Api
{
    public class AppAccessDto
    {
        public string? AppName { get; set; }
        public bool IsApproved { get; set; }
        public GroupDto? Group { get; set; }
    }
}

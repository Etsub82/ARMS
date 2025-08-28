namespace Application.DTO.Admin
{
    public class AssignRolesToGroupDto
    {
        public int ApplicationGroupId { get; set; }
        public List<int> RoleIds { get; set; }
    }
}

using System.Collections.Generic;

namespace Application.DTO.Api
{
    public class GroupDto
    {
        public string? Name { get; set; }
        public List<RoleDto>? Roles { get; set; }
    }
}

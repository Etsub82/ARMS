using Domain.common;
using System.Collections.Generic;

namespace Domain
{
    public class RoleModel : BaseDomainEntity
    {
        public string Name { get; set; }
        public ICollection<GroupRole> GroupRoles { get; set; }
    }
}

using Domain.common;
using System.Collections.Generic;

namespace Domain
{
    public class ApplicationGroup : BaseDomainEntity
    {
        public string Name { get; set; }
        public ICollection<ApplicationModel> Applications { get; set; }
        public ICollection<GroupRole> GroupRoles { get; set; }
    }
}

using Domain.common;

namespace Domain
{
    public class GroupRole : BaseDomainEntity
    {
        public int ApplicationGroupId { get; set; }
        public ApplicationGroup ApplicationGroup { get; set; }

        public int RoleModelId { get; set; }
        public RoleModel RoleModel { get; set; }
    }
}

using Domain.common;
using System.Collections.Generic;

namespace Domain
{
    public class ApplicationModel : BaseDomainEntity
    {
        public string AppId { get; set; }
        public string AppKey { get; set; }
        public string Name { get; set; }
        public string Status { get; set; } // e.g., "Pending", "Approved", "Rejected"
        public int? ApplicationGroupId { get; set; }
        public ApplicationGroup ApplicationGroup { get; set; }
    }
}

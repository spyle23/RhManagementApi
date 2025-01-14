using System.ComponentModel;

namespace RhManagementApi.Enums
{
    public enum RHStatus
    {
        [Description("Approved")]
        Approved,
        
        [Description("Pending")]
        Pending,
        
        [Description("Rejected")]
        Rejected
    }
} 
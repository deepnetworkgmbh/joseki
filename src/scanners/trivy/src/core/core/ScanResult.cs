using System.Runtime.Serialization;

namespace core.core
{
    public enum ScanResult
    {
        [EnumMember(Value = "NOT_FOUND")]
        NotFound = 0,

        [EnumMember(Value = "FAILED")]
        Failed = 1,

        [EnumMember(Value = "SUCCEEDED")]
        Succeeded = 2,
    }
}
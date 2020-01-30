using System.Runtime.Serialization;

namespace webapp.Models
{
    /// <summary>
    /// Enumerates possible infrastructure categories.
    /// </summary>
    public enum InfrastructureCategory
    {
        /// <summary>
        /// Overall infrastructure.
        /// </summary>
        [EnumMember(Value = "overall")]
        Overall,

        /// <summary>
        /// Cloud Subscription.
        /// </summary>
        [EnumMember(Value = "subscription")]
        Subscription,

        /// <summary>
        /// Kubernetes cluster.
        /// </summary>
        [EnumMember(Value = "kubernetes")]
        Kubernetes,
    }
}
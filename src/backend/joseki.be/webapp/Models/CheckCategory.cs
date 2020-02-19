using System;

namespace webapp.Models
{
    /// <summary>
    /// polaris category.
    /// azks feature name.
    /// </summary>
    public class CheckCategory
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

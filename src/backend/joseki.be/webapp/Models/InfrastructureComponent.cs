using System;

namespace webapp.Models
{
    /// <summary>
    /// The class for Infrastructure Component object.
    /// </summary>
    public class InfrastructureComponent
    {
        /// <summary>
        /// The unique Id of a component.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The name of the component: dev-cluster, subscription-1, etc.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The bucket of infrastructure component: Cloud Subscription, Kubernetes cluster, etc.
        /// </summary>
        public InfrastructureCategory Category { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InfrastructureComponent"/> class.
        /// Creates random id on constructor.
        /// </summary>
        public InfrastructureComponent()
        {
            this.Id = Guid.NewGuid().ToString();
        }
    }
}

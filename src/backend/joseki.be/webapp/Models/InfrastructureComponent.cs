using System;
using System.Web;

namespace webapp.Models
{
    /// <summary>
    /// The class for Infrastructure Component object.
    /// </summary>
    public class InfrastructureComponent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InfrastructureComponent"/> class.
        /// Creates random id on constructor.
        /// </summary>
        public InfrastructureComponent()
            : this(Guid.NewGuid().ToString())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InfrastructureComponent"/> class.
        /// </summary>
        /// <param name="id">Component identifier.</param>
        public InfrastructureComponent(string id)
        {
            this.IdRaw = id;
            this.Id = HttpUtility.UrlEncode(id);
        }

        /// <summary>
        /// Url encoded component unique Id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Raw representation of the component unique id.
        /// </summary>
        public string IdRaw { get; internal set; }

        /// <summary>
        /// The name of the component: dev-cluster, subscription-1, etc.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The bucket of infrastructure component: Cloud Subscription, Kubernetes cluster, etc.
        /// </summary>
        public InfrastructureCategory Category { get; set; }
    }
}

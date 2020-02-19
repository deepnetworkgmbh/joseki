using System;

namespace webapp.Models
{
    /// <summary>
    /// Check against a component.
    /// </summary>
    public class Check
    {
        /// <summary>
        /// unique id of the check.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// the date of the check.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Name of the collection.
        /// kubernetes: namespace.
        /// azks: resource-group.
        /// </summary>
        public Collection Collection { get; set; }

        /// <summary>
        /// category of the check
        /// kubernetes: polaris/trivy category.
        /// azks: feature name.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// The object to be checked.
        /// k8s: object (deployment, pod, service etc).
        /// azks: resource (keyvault etc).
        /// </summary>
        public InfrastructureComponent Object { get; set; }

        /// <summary>
        /// The control name of the check.
        /// k8s: polaris `check` name.
        /// azks: azks `control` name.
        /// </summary>
        public CheckControl Control { get; set; }

        /// <summary>
        /// Result of the check.
        /// </summary>
        public CheckSeverity Result { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Check"/> class.
        /// create unique id on constructor.
        /// </summary>
        public Check(InfrastructureComponent component, DateTime date, Collection collection, string category, CheckControl control, CheckSeverity severity)
        {
            this.Id = Guid.NewGuid().ToString();
            this.Object = component;
            this.Date = date;
            this.Category = category;
            this.Collection = collection;
            this.Control = control;
            this.Result = severity;
        }
    }
}

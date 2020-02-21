using System;

namespace webapp.Models
{
    /// <summary>
    /// Control tag and description of a check.
    /// (eg: namespace: default, resource group: common).
    /// </summary>
    public class CheckControl
    {
        /// <summary>
        /// Id of the control.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Message of the control.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Category of the control.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckControl"/> class.
        /// control constructor.
        /// </summary>
        /// <param name="id">id of control.</param>
        /// <param name="category">category of control.</param>
        /// <param name="message">message of control.</param>
        public CheckControl(string category, string id, string message)
        {
            this.Id = id;
            this.Message = message;
            this.Category = category;
        }
    }
}

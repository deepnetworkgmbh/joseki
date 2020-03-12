namespace webapp.Exceptions
{
    /// <summary>
    /// Thrown, when there is no audit to satisfy requested parameters.
    /// </summary>
    public class AuditNotFoundException : JosekiException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuditNotFoundException"/> class.
        /// </summary>
        /// <param name="message">Custom exception message.</param>
        public AuditNotFoundException(string message)
            : base(message)
        {
        }
    }
}
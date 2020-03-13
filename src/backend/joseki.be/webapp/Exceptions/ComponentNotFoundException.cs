namespace webapp.Exceptions
{
    /// <summary>
    /// Thrown, when there is no audit to satisfy requested parameters.
    /// </summary>
    public class ComponentNotFoundException : JosekiException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentNotFoundException"/> class.
        /// </summary>
        /// <param name="message">Custom exception message.</param>
        public ComponentNotFoundException(string message)
            : base(message)
        {
        }
    }
}
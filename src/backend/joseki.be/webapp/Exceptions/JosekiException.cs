using System;

namespace webapp.Exceptions
{
    /// <summary>
    /// General Joseki exception.
    /// </summary>
    public class JosekiException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JosekiException"/> class.
        /// </summary>
        /// <param name="ex">Inner exception object.</param>
        /// <param name="message">Custom exception message.</param>
        public JosekiException(Exception ex, string message)
            : base(message, ex)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JosekiException"/> class.
        /// </summary>
        /// <param name="message">Custom exception message.</param>
        public JosekiException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JosekiException"/> class.
        /// </summary>
        public JosekiException()
            : base()
        {
        }
    }
}
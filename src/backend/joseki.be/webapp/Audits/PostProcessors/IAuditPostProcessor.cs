using System;
using System.Threading;
using System.Threading.Tasks;
using webapp.Database.Models;

namespace webapp.Audits.PostProcessors
{
    /// <summary>
    /// A base interface for audit post-processors.
    /// </summary>
    public interface IAuditPostProcessor
    {
        /// <summary>
        /// After being saved to database, process audit data for extracting any meta.
        /// </summary>
        /// <param name="audit">The audit to post-process.</param>
        /// <param name="token">A signal to stop processing.</param>
        /// <returns>A task object, which indicates the end of the processing.</returns>
        Task Process(Audit audit, CancellationToken token);
    }
}

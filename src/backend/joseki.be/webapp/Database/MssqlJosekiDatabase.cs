using System.Threading.Tasks;

using Serilog;

using webapp.Database.Models;

namespace webapp.Database
{
    /// <summary>
    /// P-sql implementation of Database.
    /// </summary>
    public class MssqlJosekiDatabase : IJosekiDatabase
    {
        private static readonly ILogger Logger = Log.ForContext<MssqlJosekiDatabase>();

        /// <inheritdoc />
        public Task SaveAuditResult(Audit audit)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task SaveImageScanResult(ImageScanResult imageScanResult)
        {
            return Task.CompletedTask;
        }
    }
}
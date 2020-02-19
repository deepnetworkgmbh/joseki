using System.Threading.Tasks;

using Serilog;

using webapp.Database.Models;

namespace webapp.Database
{
    /// <summary>
    /// P-sql implementation of Database.
    /// </summary>
    public class PsqlJosekiDatabase : IJosekiDatabase
    {
        private static readonly ILogger Logger = Log.ForContext<PsqlJosekiDatabase>();

        /// <inheritdoc />
        public Task SaveAuditResult(Audit audit)
        {
            return Task.CompletedTask;
        }
    }
}